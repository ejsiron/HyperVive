Imports HyperVive.CIMitar
Imports HyperVive.CIMitar.Virtualization
Imports Microsoft.Management.Infrastructure

''' <summary>
''' Watches for the creation and completion of checkpoint-related Msvm_ConcreteJob items. Reports actions and results.
''' </summary>
Public Class CheckpointJobWatcher
	Inherits ModuleWithCimBase
	Implements IRunningModule
	Implements IDisposable

	''' <summary>
	''' Starts a new checkpoint watcher on the indicated CIM session.
	''' </summary>
	''' <param name="Session">The <see cref="CimSession"/> to watch for checkpoint jobs.</param>
	Public Sub New(ByVal Session As CimSession, ByVal ModuleLogger As IModuleLogger, ByVal CheckpointLogger As ICheckpointLogger)
		MyBase.New(Session, ModuleLogger)
		Me.CheckpointLogger = CheckpointLogger
		JobSubscriber = New InstanceCreationController(Session, NamespaceVirtualization, ClassNameVirtualizationJob, AddressOf ProcessJob, AddressOf ReportError)
	End Sub

	Public Sub Start() Implements IRunningModule.Start
		JobSubscriber.Start()
	End Sub

	Public ReadOnly Property IsRunning As Boolean Implements IRunningModule.IsRunning
		Get
			Return JobSubscriber.IsRunning
		End Get
	End Property

	Public Sub [Stop]() Implements IRunningModule.Stop
		JobSubscriber.Cancel()
	End Sub

	Public Overrides ReadOnly Property ModuleName As String = "Checkpoint Watcher"
	Private CheckpointLogger As ICheckpointLogger
	Private JobSubscriber As InstanceCreationController
	Private Const ApplySnapshotAction As String = "Apply"
	Private Const ClearSnapshotStateAction As String = "Clear state"
	Private Const DeleteSnapshotAction As String = "Delete"
	Private Const NewSnapshotAction As String = "Create"
	Private Const UnexpectedJobClassIntercepted As String = "Checkpoint watcher received unexpected event"

	''' <summary>
	''' Raised when a checkpoint action is intercepted
	''' </summary>
	Private Class CheckpointActionReport
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
		''' Current state of the job
		''' </summary>
		''' <returns><see cref="UShort"/></returns>
		Public Property JobState As UShort = 0

		''' <summary>
		''' The user name that initiated the job.
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property UserName As String
		''' <summary>
		''' The ID of the virtual machine connected to this checkpoint action, in <see cref="String"/> form.
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property VMID As String = Guid.NewGuid.ToString
		''' <summary>
		''' The name of the virtual machine connected to this checkpoint action.
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property VMName As String = String.Empty
		''' <summary>
		''' Indicates if the action has completed.
		''' </summary>
		''' <returns><see cref="Boolean"/></returns>
		Public Property IsCompleted As Boolean = False
		''' <summary>
		''' Returns the final error code reported by Msvm_ConcreteJob.
		''' </summary>
		''' <returns><see cref="UShort"/></returns>
		Public Property ResultCode As UShort = 0US
		''' <summary>
		''' Returns the final error status reported by the CIM system.
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property ResultMessage As String = String.Empty
	End Class

	''' <summary>
	''' Receives newly-created Msvm_ConcreteJob items. If the job is checkpoint-related, watches it to completion.
	''' </summary>
	''' <param name="Result">A <see cref="CimSubscriptionResult"/> that contains the Msvm_ConcreteJob to process.</param>
	Private Async Sub ProcessJob(ByVal Result As CimSubscriptionResult)
		Dim Report As New CheckpointActionReport
		Using Result
			Dim JobInstance As CimInstance = Result.GetSourceInstance
			Report.JobType = JobInstance.InstancePropertyUInt16(PropertyNameJobType)
			Report.JobInstanceID = JobInstance.InstancePropertyString(PropertyNameInstanceID)
			CheckpointLogger.LogDebugVirtualizationJobReceived(Report.JobType, Report.JobInstanceID)
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
			Using AssociatedVMController As New AsyncAssociatedInstancesController(Session, NamespaceVirtualization) With {.SourceInstance = JobInstance, .ResultClass = ClassNameVirtualMachine}
				Using GetAssociatedVM As Task(Of CimInstanceCollection) = AssociatedVMController.StartAsync
					GetAssociatedVM.Wait()
					Using VMInstances As CimInstanceCollection = GetAssociatedVM.Result
						If VMInstances IsNot Nothing AndAlso VMInstances.Count > 0 Then
							With VMInstances.First
								Report.VMID = .InstancePropertyString(PropertyNameName)
								Report.VMName = .InstancePropertyString(PropertyNameElementName)
								Report.JobState = .InstancePropertyUInt16(PropertyNameJobState)
							End With
						End If
					End Using
				End Using
			End Using
			Report.IsCompleted = Report.JobState < JobStates.Completed
			WriteCheckpointAction(Report)
			If Report.IsCompleted Then
				Using CompletedInstance As CimInstance = Await VirtualizationJobCompletionController.WatchAsync(Session, Report.JobInstanceID)
					Report.IsCompleted = True
					If CompletedInstance IsNot Nothing Then
						Report.ResultCode = CompletedInstance.InstancePropertyUInt16(PropertyNameErrorCode)
						Report.ResultMessage = CompletedInstance.InstancePropertyString(PropertyNameJobStatus)
					Else
						Report.ResultCode = USUnknownError
						Report.ResultMessage = StrUnknownError
					End If
				End Using
				WriteCheckpointAction(Report)
			End If
		End Using
	End Sub

	Private Sub WriteCheckpointAction(ByVal Report As CheckpointActionReport)
		CheckpointLogger.LogCheckpointActionReport(Report.JobTypeName, Report.VMName, Report.UserName, Report.VMID, Report.JobInstanceID, Report.IsCompleted, Report.ResultCode, Report.ResultMessage)
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