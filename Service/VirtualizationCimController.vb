Imports System.Threading
Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure

Namespace CIMitar.Virtualization
	Public Module Strings
		Public Const NamespaceVirtualization As String = "root/virtualization/v2"
		Public Const ClassNameVirtualizationJob As String = "Msvm_ConcreteJob"
		Public Const ClassNameVirtualMachine As String = "Msvm_ComputerSystem"
		Public Const PropertyNameInstanceID As String = "InstanceID"
		Public Const PropertyNameJobState As String = "JobState"
		Public Const PropertyNameJobStatus As String = "JobStatus"
		Public Const PropertyNameJobType As String = "JobType"
		Public Const PropertyNameErrorCode As String = "ErrorCode"
		Public Const PropertyNameOwner As String = "Owner"
		Public Const PropertyNameAddress As String = "Address"
		Public Const PropertyNameEnabledState As String = "EnabledState"
		Public Const PropertyNameElementName As String = "ElementName"
		Public Const PropertyNameHealthState As String = "HealthState"

		Public ReadOnly CimQueryTemplateVirtualMachine As String = String.Format("SELECT * FROM {0} {{0}}", ClassNameVirtualMachine)

		Public Const QueryTemplateMsvmConcreteJobById As String = "SELECT * FROM Msvm_ConcreteJob WHERE InstanceID='{0}'"
	End Module

	Public Module Enums
		Public Enum JobStates As UShort
			[New] = 2
			Starting = 3
			Running = 4
			ShuttingDown = 6
			Completed = 7
			Terminated = 8
			Killed = 9
			Exception = 10
			Service = 11
		End Enum

		Public Enum VirtualizationJobTypes As UShort
			NewSnapshot = 70
			ApplySnapshot = 71
			DeleteSnapshot = 72
			ClearSnapshotState = 73
		End Enum
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

		Public Shared Async Function WatchAsync(ByVal Session As CimSession, ByVal JobInstanceID As String) As Task(Of CimInstance)
			Dim Job As CimInstance = Nothing
			Using JobWatcher As New CimAsyncQueryInstancesController(Session, NamespaceVirtualization) With {
				.QueryText = String.Format(QueryTemplateMsvmConcreteJobById, JobInstanceID)
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

		Public Sub Start()
			JobSubscriber?.Cancel()
			JobSubscriber?.Dispose()
			JobSubscriber = New CimAsyncQueryInstancesController(Session, NamespaceVirtualization) With {
				.QueryText = String.Format(QueryTemplateMsvmConcreteJobById, InstanceID)}
			JobSubscriber.StartAsync.ContinueWith(AddressOf WatcherCallback)
		End Sub

		Private Const RecheckDelay As Integer = 250

		Private Shared Function JobIsRunning(ByRef JobInstance As CimInstance) As Boolean
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
