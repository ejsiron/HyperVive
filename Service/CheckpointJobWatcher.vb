Imports HyperVive.CIMitar
Imports HyperVive.CIMitar.Virtualization
Imports Microsoft.Management.Infrastructure

Public Module CustomCheckpointActionEvents
	''' <summary>
	''' Raised when a checkpoint action is intercepted
	''' </summary>
	Public Class CheckpointActionEventArgs
		''' <summary>
		''' The CIM session where the action occurred
		''' </summary>
		''' <returns><see cref="CimSession"/></returns>
		Public Property Session As CimSession
		''' <summary>
		''' The instance ID of the Msvm_ConcreteJob that tracked the action, in <see cref="String"/> form.
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property JobInstanceID As String
		''' <summary>
		''' The type code for the job. See the official documentation for Msvm_ConcreteJob.
		''' </summary>
		''' <returns><see cref="UShort"/></returns>
		Public Property JobType As UShort
		''' <summary>
		''' The type of job in <see cref="String"/> form. Not localized.
		''' </summary>
		''' <returns></returns>
		Public Property JobTypeName As String
		''' <summary>
		''' The user name that initiated the job.
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property UserName As String
		''' <summary>
		''' The ID of the virtual machine connected to this checkpoint action, in <see cref="String"/> form.
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property VMID As String
		''' <summary>
		''' The name of the virtual machine connected to this checkpoint action.
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property VMName As String
		''' <summary>
		''' Indicates if the action has completed.
		''' </summary>
		''' <returns><see cref="Boolean"/></returns>
		Public Property IsCompleted As Boolean = False
		''' <summary>
		''' Returns the final error code reported by Msvm_ConcreteJob.
		''' </summary>
		''' <returns><see cref="UShort"/></returns>
		Public Property CompletionCode As UShort = 0US
		''' <summary>
		''' Returns the final error status reported by the CIM system.
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property CompletionStatus As String = String.Empty
	End Class
End Module

''' <summary>
''' Watches for the creation and completion of checkpoint-related Msvm_ConcreteJob items. Reports actions and results.
''' </summary>
Public Class CheckpointJobWatcher
	Implements IDisposable

	Public Event CheckpointJobStarted(ByVal sender As Object, ByVal e As CheckpointActionEventArgs)
	Public Event CheckpointJobCompleted(ByVal sender As Object, ByVal e As CheckpointActionEventArgs)
	Public Event DebugMessageGenerated(ByVal sender As Object, ByVal e As DebugMessageEventArgs)
	Public Event CheckpointWatcherErrorOccurred(ByVal sender As Object, ByVal e As ModuleExceptionEventArgs)

	''' <summary>
	''' Starts a new checkpoint watcher on the indicated CIM session.
	''' </summary>
	''' <param name="Session">The <see cref="CimSession"/> to watch for checkpoint jobs.</param>
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

	''' <summary>
	''' Handles the creation of new Msvm_ConcreteJob objects.
	''' </summary>
	''' <param name="sender"></param>
	''' <param name="e"></param>
	''' <remarks>Passes off the job as quickly as possible to avoid holding up the eventing system.</remarks>
	Private Sub JobHandler(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles JobSubscriber.EventReceived
		Dim Session As CimSession = e.Session
		Dim JobInstance As CimInstance = e.SubscribedEvent.GetSourceInstance.Clone
		e.Dispose()
		Task.Run(Sub() ProcessJob(JobInstance))
	End Sub

	''' <summary>
	''' Receives newly-created Msvm_ConcreteJob items. If the job is checkpoint-related, watches it to completion.
	''' </summary>
	''' <param name="JobInstance">The <see cref="CimInstance"/> that represents the Msvm_ConcreteJob to process.</param>
	Private Sub ProcessJob(ByRef JobInstance As CimInstance)
		Dim Report As New CheckpointActionEventArgs With {.Session = Session}
		Using JobInstance
			Report.JobType = JobInstance.InstancePropertyUInt16(PropertyNameJobType)
			RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs(String.Format("Virtualization job type {0} created", Report.JobType), EventIdDebugVirtualizationJobReceived))
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
					GetAssociatedVM.Wait()
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
				CheckpointCompletionWatcher.Wait()
				Report.IsCompleted = True
				Using CompletedInstance As CimInstance = CheckpointCompletionWatcher.Result
					If CompletedInstance IsNot Nothing Then
						Report.CompletionCode = CompletedInstance.InstancePropertyUInt16(PropertyNameErrorCode)
						Report.CompletionStatus = CompletedInstance.InstancePropertyString(PropertyNameJobStatus)
					Else
						Report.CompletionCode = USUnknownError
						Report.CompletionStatus = StrUnknownError
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