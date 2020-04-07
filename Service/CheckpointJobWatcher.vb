Imports HyperVive.CIMitar
Imports HyperVive.CIMitar.Virtualization
Imports Microsoft.Management.Infrastructure

Public Class CheckpointJobWatcher
	Implements IDisposable

	Public Class CheckpointActionEventArgs
		Inherits CimEventArgs

		Public Property JobInstanceID As String
		Public Property JobType As UShort
		Public Property JobTypeName As String
		Public Property UserName As String
		Public Property VMID As String
		Public Property VMName As String
	End Class

	Public Class CheckpointActionCompletedEventArgs
		Inherits CheckpointActionEventArgs

		Public Property CompletionCode As UShort
		Public Property CompletionStatus As String
	End Class

	Public Event CheckpointJobStarted(ByVal sender As Object, ByVal e As CheckpointActionEventArgs)
	Public Event CheckpointJobCompleted(ByVal sender As Object, ByVal e As CheckpointActionCompletedEventArgs)
	Public Event DebugMessageGenerated(ByVal sender As Object, ByVal e As DebugMessageEventArgs)
	Public Event CheckpointWatcherErrorOccurred(ByVal sender As Object, ByVal e As ModuleExceptionEventArgs)

	Public Sub New(ByVal Session As CimSession)
		Me.Session = Session
		JobSubscriber = New InstanceCreationController(Session, NamespaceVirtualization, ClassNameVirtualizationJob)
	End Sub

	Public Sub Start()
		JobSubscriber.Start()
	End Sub

	Public Sub Cancel()
		JobSubscriber.Cancel()
	End Sub

	Private Session As CimSession
	Private Const ModuleName As String = "Checkpoint Watcher"
	Private WithEvents JobSubscriber As InstanceCreationController
	Private Const ApplySnapshotAction As String = "Apply"
	Private Const ClearSnapshotStateAction As String = "Clear state"
	Private Const DeleteSnapshotAction As String = "Delete"
	Private Const NewSnapshotAction As String = "Create"
	Private Const UnexpectedJobClassIntercepted As String = "Checkpoint watcher received unexpected event"

	Private Sub JobHandler(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles JobSubscriber.EventReceived
		Task.Run(Sub() ProcessJob(e.Session, e.SubscribedEvent.GetSourceInstance.Clone))
		e.Dispose()
	End Sub

	Private Sub ProcessJob(ByVal Session As CimSession, ByRef JobInstance As CimInstance)
		Dim Report As New CheckpointActionCompletedEventArgs With {.Session = Session}
		Using JobInstance
			Report.JobType = JobInstance.InstancePropertyUInt16(PropertyNameJobType)
			Select Case Report.JobType
				Case VirtualizationJobTypes.ApplySnapshot
					Report.JobTypeName = ApplySnapshotAction
				Case VirtualizationJobTypes.ClearSnapshotState
					Report.JobTypeName = ClearSnapshotStateAction
				Case VirtualizationJobTypes.DeleteSnapshot
					Report.JobTypeName = DeleteSnapshotAction
				Case VirtualizationJobTypes.NewSnapshot
					Report.JobTypeName = NewSnapshotAction
				Case Else
					' some non-checkpoint related job type, let it pass
					Return
			End Select
			Report.UserName = JobInstance.InstancePropertyString(PropertyNameOwner)
			Report.JobInstanceID = JobInstance.InstancePropertyString(PropertyNameInstanceID)
			Using AssociatedVMController As New AsyncAssociatedInstancesController(Session, NamespaceVirtualization) With {.SourceInstance = JobInstance, .ResultClass = ClassNameVirtualMachine}
				Using GetAssociatedVM As Task(Of CimInstanceList) = AssociatedVMController.StartAsync
					GetAssociatedVM.RunSynchronously()
					Using VMInstances As CimInstanceList = GetAssociatedVM.Result
						If VMInstances IsNot Nothing AndAlso VMInstances.Count > 0 Then
							With VMInstances.First
								Report.VMID = .InstancePropertyString(PropertyNameName)
								Report.VMName = .InstancePropertyString(PropertyNameElementName)
							End With
						End If
					End Using
				End Using
			End Using
			RaiseEvent CheckpointJobStarted(Me, Report)
			Using CheckpointCompletionWatcher As Task(Of CimInstance) = VirtualizationJobCompletionController.WatchAsync(Session, Report.JobInstanceID)
				CheckpointCompletionWatcher.RunSynchronously()
				Using CompletedInstance As CimInstance = CheckpointCompletionWatcher.Result
					If CompletedInstance IsNot Nothing Then
						Report.CompletionCode = CompletedInstance.InstancePropertyUInt16(PropertyNameErrorCode)
						Report.CompletionStatus = CompletedInstance.InstancePropertyString(PropertyNameJobStatus)
					End If
				End Using
			End Using
			RaiseEvent CheckpointJobCompleted(Me, Report)
		End Using
	End Sub

	Private Sub SubscriberError(ByVal sender As Object, ByVal e As CimErrorEventArgs) Handles JobSubscriber.ErrorOccurred
		RaiseEvent CheckpointWatcherErrorOccurred(Me, New ModuleExceptionEventArgs With {.ModuleName = ModuleName, .[Error] = e.ErrorInstance})
	End Sub

#Region "IDisposable Support"

	Private disposedValue As Boolean

	Protected Overridable Sub Dispose(disposing As Boolean)
		If Not disposedValue Then
			If disposing Then
				JobSubscriber?.Cancel()
				JobSubscriber?.Dispose()
			End If
		End If
		disposedValue = True
	End Sub

	Public Sub Dispose() Implements IDisposable.Dispose
		Dispose(True)
	End Sub

#End Region

End Class