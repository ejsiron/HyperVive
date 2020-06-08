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
			DestroyVirtualMachine = 3
			NewSnapshot = 70
			ApplySnapshot = 71
			DeleteSnapshot = 72
			ClearSnapshotState = 73
		End Enum

		Public Enum VirtualMachineStates As UShort
			Unknown = 0
			Other = 1
			Running = 2
			Off = 3
			Stopping = 4
			EnabledOffline = 5
			Saved = 6
			InTest = 7
			Deferred = 8
			Quiesced = 9
			Starting = 10
		End Enum

		Public Enum VirtualizationMethodErrors As UShort
			NoError = 0
			JobStarted = 4096
			AccessDenied = 32769
			InvalidState = 32775
		End Enum
	End Module

	'Public Module CustomCimVirtualizationEvents
	'	Public Class VirtualizationJobNotFoundEventArgs
	'		Inherits CimEventArgs

	'		Public Property InstanceID As String
	'	End Class

	'	Public Class VirtualizationJobCompletedArgs
	'		Inherits CimEventArgs

	'		Public Property JobInstance As CimInstance
	'	End Class
	'End Module

	''' <summary>
	''' Watches Msvm_ConcreteJob objects for completion. Resets itself upon job completion.
	''' <para>Runs in instance mode (Start()) or ad hoc mode (StartAsync()). In instance mode, always set the InstanceID before starting.</para>
	''' </summary>
	Public Class VirtualizationJobCompletionController
		Public Sub New(ByVal Session As CimSession)
			Me.Session = Session
		End Sub

		Private ReadOnly JobNotFoundCallback As Action(Of CimSession, String)   ' InstanceID
		Private ReadOnly JobCompletedCallback As Action(Of CimSession, CimInstance)

		''' <summary>
		''' Instance ID of the target Msvm_ConcreteJob, in <see cref="String"/> form.
		''' </summary>
		''' <returns>Current job Instance ID in <see cref="String"/> form.</returns>
		Public Property InstanceID As String

		''' <summary>
		''' Ad hoc function to watch an Msvm_ConcreteJob asynchronously.
		''' </summary>
		''' <param name="Session"><see cref="CimSession"/> that contains the job to watch.</param>
		''' <param name="JobInstanceID">The <see cref="String"/> form of the target Msvm_ConcreteJob ID.</param>
		''' <returns></returns>
		Public Shared Async Function WatchAsync(ByVal Session As CimSession, ByVal JobInstanceID As String) As Task(Of CimInstance)
			Dim Job As CimInstance = Nothing
			Using JobWatcher As New CimAsyncQueryInstancesController(Session, NamespaceVirtualization) With {
				.QueryText = String.Format(QueryTemplateMsvmConcreteJobById, JobInstanceID)
				}
				Dim JobList As CimInstanceCollection = Await JobWatcher.StartAsync
				If JobList.Count > 0 Then
					Job = JobList.First
					While JobIsRunning(Job)
						Thread.Sleep(RecheckDelay)
						Job.Refresh(Session)
					End While
				End If
				Return Job?.Clone
			End Using
		End Function

		''' <summary>
		''' Starts watching the Msvm_ConcreteJob indicated in InstanceID. Uses events to report completion.
		''' </summary>
		Public Sub Start()
			JobSubscriber?.Cancel()
			JobSubscriber?.Dispose()
			JobSubscriber = New CimAsyncQueryInstancesController(Session, NamespaceVirtualization) With {
				.QueryText = String.Format(QueryTemplateMsvmConcreteJobById, InstanceID)}
			JobSubscriber.StartAsync.ContinueWith(AddressOf WatcherCallback)
		End Sub

		Private Const RecheckDelay As Integer = 250

		''' <summary>
		''' Checks the JobState of an Msvm_ConcreteJob object to determine if it is still running.
		''' </summary>
		''' <param name="JobInstance">A <see cref="CimInstance"/> that represents the Msvm_ConcreteJob object</param>
		''' <returns>A <see cref="Boolean"/> value that indicates the job running condition.</returns>
		Private Shared Function JobIsRunning(ByRef JobInstance As CimInstance) As Boolean
			Dim JobState As JobStates = CType(JobInstance.InstancePropertyUInt16(PropertyNameJobState), JobStates)
			Return JobState = JobStates.Running OrElse JobState = JobStates.[New] OrElse JobState = JobStates.Starting
		End Function

		''' <summary>
		''' Recursively watches for an Msvm_ConcreteJob to complete.
		''' </summary>
		''' <param name="ControllerTask"></param>
		Private Sub WatcherCallback(ControllerTask As Task(Of CimInstanceCollection))
			If ControllerTask.Result.Count > 0 Then
				If JobIsRunning(ControllerTask.Result.First) Then
					Thread.Sleep(RecheckDelay)
					JobSubscriber.RefreshAsync.ContinueWith(AddressOf WatcherCallback)
					Return
				Else
					JobCompletedCallback(Session, ControllerTask.Result.First.Clone)
				End If
			Else
				JobNotFoundCallback(Session, InstanceID)
			End If
			JobSubscriber.Dispose()
			JobSubscriber = Nothing
		End Sub

		Private ReadOnly Session As CimSession
		Private JobSubscriber As CimAsyncQueryInstancesController
	End Class
End Namespace
