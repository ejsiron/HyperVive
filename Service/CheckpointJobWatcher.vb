Imports HyperVive.CIMitar
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

	Public Sub New(ByVal Session As CimSession)
		Me.Session = Session
		EventSubscriber = JobSubscriber.GetCreationSubscriber(Session, CimNamespaceVirtualization, CimClassNameVirtualizationJob)
	End Sub

	Private Session As CimSession
	Private Const ModuleName As String = "Checkpoint Watcher"
	Private WithEvents EventSubscriber As JobSubscriber
	Private Const ApplySnapshotAction As String = "Apply"
	Private Const ClearSnapshotStateAction As String = "Clear state"
	Private Const DeleteSnapshotAction As String = "Delete"
	Private Const NewSnapshotAction As String = "Create"

	Private Async Sub JobHandler(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles EventSubscriber.SubscribedJobCreated
		Dim CheckpointAction As String = String.Empty
		Dim UserName As String
		Dim InstanceID As String
		Dim JobType As UShort = 0US
		Using e
			With e.SubscribedEvent.GetSourceInstance
				JobType = .GetInstancePropertyUInt16Value(CimPropertyNameJobType)
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
				UserName = .GetInstancePropertyStringValue(CimPropertyNameOwner)
				InstanceID = .GetInstancePropertyStringValue(CimPropertyNameInstanceID)
			End With
			RaiseEvent CheckpointJobStarted(Me, New CheckpointActionEventArgs With {.JobInstanceID = InstanceID, .JobType = JobType, .JobTypeName = CheckpointAction, .Session = Session, .UserName = UserName})
		End Using
		Dim JobCompletion As KeyValuePair(Of UShort, String) = Await JobSubscriber.WatchVirtualizationJobCompletionAsync(Me, Session, InstanceID)
		RaiseEvent CheckpointJobCompleted(Me, New CheckpointActionCompletedEventArgs With {.JobInstanceID = InstanceID, .JobType = JobType, .JobTypeName = CheckpointAction, .Session = Session, .UserName = UserName, .CompletionCode = JobCompletion.Key, .CompletionStatus = JobCompletion.Value})
	End Sub

#Region "IDisposable Support"

	Private disposedValue As Boolean

	Protected Overridable Sub Dispose(disposing As Boolean)
		If Not disposedValue Then
			If disposing Then
				EventSubscriber.Dispose()
			End If
		End If
		disposedValue = True
	End Sub

	Public Sub Dispose() Implements IDisposable.Dispose
		Dispose(True)
	End Sub

#End Region

End Class