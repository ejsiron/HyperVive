﻿Imports System.Threading
Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure

Namespace CIMitar.Virtualization
	Public Module Strings
		Public Const NamespaceVirtualization As String = "root/virtualization/v2"
		Public Const ClassNameVirtualizationJob As String = "Msvm_ConcreteJob"
		Public Const PropertyNameInstanceID As String = "InstanceID"
		Public Const PropertyNameCaption As String = "Caption"
		Public Const PropertyNameJobState As String = "JobState"
		Public Const PropertyNameJobStatus As String = "JobStatus"
		Public Const PropertyNameJobType As String = "JobType"
		Public Const PropertyNameErrorCode As String = "ErrorCode"
		Public Const PropertyNameOwner As String = "Owner"
		Public Const PropertyNameAddress As String = "Address"
		Public Const PropertyNameEnabledState As String = "EnabledState"
		Public Const PropertyNameElementName As String = "ElementName"
		Public Const PropertyNameOperationalStatus As String = "OperationalStatus"
		Public Const PropertyNameStatusDescriptions As String = "StatusDescriptions"
		Public Const PropertyNameStatus As String = "Status"
		Public Const PropertyNameHealthState As String = "HealthState"
		Public Const PropertyNameTimeSubmitted As String = "TimeSubmitted"
		Public Const PropertyNameScheduledStartTime As String = "ScheduledStartTime"
		Public Const PropertyNameStartTime As String = "StartTime"
		Public Const PropertyNameElapsedTime As String = "ElapsedTime"
		Public Const PropertyNameJobRunTimes As String = "JobRunTimes"
		Public Const PropertyNameLocalOrUtcTime As String = "LocalOrUtcTime"
		Public Const PropertyNamePriority As String = "Priority"
		Public Const PropertyNamePercentComplete As String = "PercentComplete"
		Public Const PropertyNameDeleteOnCompletion As String = "DeleteOnCompletion"
		Public Const PropertyNameErrorDescription As String = "ErrorDescription"
		Public Const PropertyNameErrorSummaryDescription As String = "ErrorSummaryDescription"
		Public Const PropertyNameRecoveryAction As String = "RecoveryAction"
		Public Const PropertyNameTimeOfLastStateChange As String = "TimeOfLastStateChange"
		Public Const PropertyNameTimeBeforeRemoval As String = "TimeBeforeRemoval"
		Public Const PropertyNameCancellable As String = "Cancellable"

		Public Const QueryTemplateMsvmConcreteJob As String = "SELECT * FROM Msvm_ConcreteJob WHERE InstanceID='{0}'"
	End Module

	Public Module CustomCimVirtualizationEvents
		Public Class VirtualizationJobNotFoundEventArgs
			Inherits CimEventArgs

			Public Property InstanceID As String
		End Class

		Public Class VirtualizationJobCompletedArgs
			Inherits CimEventArgs

			Public Property InstanceID As CimInstance
		End Class
	End Module

	Public Class VirtualizationJobCompletionController
		Public Sub New(ByVal Session As CimSession)
			Me.Session = Session
		End Sub

		Public Event JobNotFound(ByVal sender As Object, ByVal e As VirtualizationJobNotFoundEventArgs)
		Public Event JobCompleted(ByVal sender As Object, ByVal e As VirtualizationJobCompletedArgs)

		Public Property InstanceID As String

		Public Async Function StartAsync(ByVal JobInstanceID As String) As Task(Of CimInstance)
			Dim Job As CimInstance = Nothing
			Using JobWatcher As New CimAsyncQueryInstancesController(Session, NamespaceVirtualization) With {
				.QueryText = String.Format(QueryTemplateMsvmConcreteJob, JobInstanceID)
				}
				Dim JobList As CimInstanceList = Await JobWatcher.StartAsync
				If JobList.Count > 0 Then
					Job = JobList.First
					While JobIsRunning(Job)
						Thread.Sleep(RecheckDelay)
						Job.Refresh(Session)
					End While
				End If
				Return Job.Clone
			End Using
		End Function

		Public Sub Start(ByVal JobInstanceID As String)
			JobSubscriber?.Cancel()
			JobSubscriber?.Dispose()
			JobSubscriber = New CimAsyncQueryInstancesController(Session, NamespaceVirtualization) With {
				.QueryText = String.Format(QueryTemplateMsvmConcreteJob, JobInstanceID)}
			JobSubscriber.StartAsync.ContinueWith(AddressOf WatcherCallback)
		End Sub

		Private Const RecheckDelay As Integer = 250

		Private Function JobIsRunning(ByRef JobInstance As CimInstance) As Boolean
			Dim JobState As JobStates = CType(JobInstance.InstancePropertyUInt16(PropertyNameJobState), JobStates)
			Return JobState = JobStates.Running OrElse JobState = JobStates.[New] OrElse JobState = JobStates.Starting
		End Function

		Private Sub WatcherCallback(ControllerTask As Task(Of CimInstanceList))
			If ControllerTask.Result.Count > 0 Then
				If JobIsRunning(ControllerTask.Result.First) Then
					Thread.Sleep(RecheckDelay)
					JobSubscriber.RefreshAsync.ContinueWith(AddressOf WatcherCallback)
					Return
				Else
					RaiseEvent JobCompleted(Me, New VirtualizationJobCompletedArgs With {
						.Session = Session,
						.InstanceID = ControllerTask.Result.First.Clone
					})
				End If
			Else
				RaiseEvent JobNotFound(Me, New VirtualizationJobNotFoundEventArgs With {.Session = Session, .InstanceID = InstanceID})
			End If
			JobSubscriber.Dispose()
			JobSubscriber = Nothing
		End Sub

		Private Session As CimSession
		Private JobSubscriber As CimAsyncQueryInstancesController
	End Class
End Namespace
