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
		JobSubscriber = New CimSubscriptionController(Session, NamespaceVirtualization)
	End Sub

	Public Sub Start()
		JobSubscriber.Start()
	End Sub

	Public Sub Cancel()
		JobSubscriber.Cancel()
	End Sub

	Private Session As CimSession
	Private Const ModuleName As String = "Checkpoint Watcher"
	Private WithEvents JobSubscriber As CimSubscriptionController
	Private Const ApplySnapshotAction As String = "Apply"
	Private Const ClearSnapshotStateAction As String = "Clear state"
	Private Const DeleteSnapshotAction As String = "Delete"
	Private Const NewSnapshotAction As String = "Create"
	Private Const UnexpectedJobClassIntercepted As String = "Checkpoint watcher received unexpected event"

	Private Sub JobHandler(ByVal sender As Object, ByVal e As CimEventArgs) Handles JobSubscriber.EventReceived
		Dim CheckpointAction As String = String.Empty
		Dim UserName As String
		Dim InstanceID As String
		Dim JobType As UShort = 0US
		Dim JobInstance As CimInstance
		Dim IsCompleted As Boolean = False
		Dim CompletionCode As UShort = 0US
		Dim CompletionStatus As String = String.Empty
		If TypeOf e Is CimSubscribedEventReceivedArgs Then
			JobInstance = CType(e, CimSubscribedEventReceivedArgs).SubscribedEvent.GetSourceInstance
		ElseIf TypeOf e Is VirtualizationJobCompletedArgs Then
			JobInstance = CType(e, VirtualizationJobCompletedArgs).InstanceID
			CompletionCode = JobInstance.InstancePropertyUInt16(PropertyNameErrorCode)
			CompletionStatus = JobInstance.InstancePropertyString(PropertyNameJobStatus)
			IsCompleted = True
		Else
			RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {.Message = UnexpectedJobClassIntercepted})
			Return
		End If
		Using JobInstance
			JobType = JobInstance.InstancePropertyUInt16(PropertyNameJobType)
			Select Case JobType
				Case VirtualizationJobTypes.ApplySnapshot
					CheckpointAction = ApplySnapshotAction
				Case VirtualizationJobTypes.ClearSnapshotState
					CheckpointAction = ClearSnapshotStateAction
				Case VirtualizationJobTypes.DeleteSnapshot
					CheckpointAction = DeleteSnapshotAction
				Case VirtualizationJobTypes.NewSnapshot
					CheckpointAction = NewSnapshotAction
				Case Else
					' some non-checkpoint related job type, let it pass
					Return
			End Select
			UserName = JobInstance.InstancePropertyString(PropertyNameOwner)
			InstanceID = JobInstance.InstancePropertyString(PropertyNameInstanceID)
			RaiseEvent CheckpointJobStarted(Me, New CheckpointActionEventArgs With {.JobInstanceID = InstanceID, .JobType = JobType, .JobTypeName = CheckpointAction, .Session = Session, .UserName = UserName})
		End Using
		If IsCompleted Then
			RaiseEvent CheckpointJobCompleted(Me, New CheckpointActionCompletedEventArgs With {
				.Session = Session,
				.JobInstanceID = InstanceID,
				.JobType = JobType,
				.JobTypeName = CheckpointAction,
				.UserName = UserName,
				.CompletionCode = CompletionCode,
				.CompletionStatus = CompletionStatus})
		Else
			RaiseEvent CheckpointJobStarted(Me, New CheckpointActionEventArgs With {
				.Session = Session,
				.JobInstanceID = InstanceID,
				.JobType = JobType,
				.JobTypeName = CheckpointAction,
				.UserName = UserName})
		End If
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