Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure

''' <summary>
''' Information structure for start notifications
''' </summary>
Public Class VMInfo
	''' <summary>
	''' The MAC address of the started VM
	''' </summary>
	''' <returns>The MAC address in unformatted <see cref="String"/> form</returns>
	Public Property MacAddress As String
	''' <summary>
	''' The virtual machine ID
	''' </summary>
	''' <returns>The virtual machine GUID in <see cref="String"/> form</returns>
	Public Property ID As String
	''' <summary>
	''' The virtual machine name
	''' </summary>
	''' <returns><see cref="String"/></returns>
	Public Property Name As String
	''' <summary>
	''' The IP address that sent the wake-on-LAN request
	''' </summary>
	''' <returns>The IP address in <see cref="String"/> format</returns>
	Public Property SourceIP As String
End Class

Public Module VMStartControllerEvents
	''' <summary>
	''' Indicates the result of a virtual machine start
	''' </summary>
	Public Class VMStartResultEventArgs
		Inherits EventArgs
		''' <summary>
		''' Data about the virtual machine start
		''' </summary>
		''' <returns><see cref="VMInfo"/></returns>
		Public Property VirtualMachineInfo As VMInfo
		''' <summary>
		''' Indicates procedure success
		''' </summary>
		''' <returns><see cref="Boolean"/></returns>
		Public Property Success As Boolean
		''' <summary>
		''' Numerical code for the result
		''' </summary>
		''' <returns><see cref="UShort"/></returns>
		Public Property ResultCode As UShort
		''' <summary>
		''' Result explanation
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property ResultText As String
	End Class
End Module

''' <summary>
''' Starts all virtual machines with an InstanceID in a supplied list
''' </summary>
Public Class VMStartController
	Private Session As CimSession

	''' <summary>
	''' Raised when the controller successfully starts a virtual machine
	''' </summary>
	''' <param name="sender">The <see cref="VMStartController"/> object that started the virtual machine</param>
	''' <param name="e">A <see cref="VMStartResultEventArgs"/> with information about the virtual machine</param>
	Public Event StartResult(ByVal sender As Object, ByVal e As VMStartResultEventArgs)
	''' <summary>
	''' Sends debug-level messages
	''' </summary>
	''' <param name="sender">The <see cref="VMStartController"/> reporting the message</param>
	''' <param name="e">A <see cref="DebugMessageEventArgs"/> with message details</param>
	Public Event DebugMessageGenerated(ByVal sender As Object, ByVal e As DebugMessageEventArgs)
	''' <summary>
	''' Reports processing errors in a VM Start Controller
	''' </summary>
	''' <param name="sender">The <see cref="VMStartController"/> instance reporting the problem</param>
	''' <param name="e">A <see cref="ModuleExceptionEventArgs"/> with </param>
	Public Event StarterError(ByVal sender As Object, ByVal e As ModuleExceptionEventArgs)

	''' <summary>
	''' Creates a new VM Start Controller
	''' </summary>
	''' <param name="Session">A <see cref="CimSession"/> on the system that owns the target virtual machines</param>
	Public Sub New(ByVal Session As CimSession)
		Me.Session = Session
	End Sub

	''' <summary>
	''' Attempts to start VMs that match the supplied IDs.
	''' </summary>
	''' <remarks>If possible, runs each start operation in parallel. Hyper-V should always attempt to start a background job. The operation will watch the output of such jobs.</remarks>
	''' <param name="MacAddress">The MAC address owned by the VM(s). Unvalidated, passes through to events.</param>
	''' <param name="VmIDs">A <see cref="List(Of String)"/> of virtual machine IDs, in <see cref="String"/> format</param>
	''' <param name="SourceIP">A <see cref="String"/> that contains the source IP of the WOL frame. Unvalidated, passes through to events.</param>
	Public Async Sub Start(ByVal MacAddress As String, ByVal VmIDs As List(Of String), SourceIP As String)
		Using VMLister As New CimAsyncQueryInstancesController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimQueryTemplateVirtualMachine, ConvertVMListToQueryFilter(VmIDs)),
			.KeysOnly = True
		}
			Using TargetVMs As CimInstanceList = Await VMLister.StartAsync
				Parallel.ForEach(TargetVMs,
					Async Sub(ByVal VM As CimInstance)
						Dim CurrentState As VirtualMachineStates = CType(VM.CimInstanceProperties(CimPropertyNameEnabledState).Value, VirtualMachineStates)
						Dim Info As New VMInfo With {
							.ID = VM.GetInstancePropertyValueString(CimPropertyNameName),
							.Name = VM.GetInstancePropertyValueString(CimPropertyNameElementName),
							.MacAddress = MacAddress,
							.SourceIP = SourceIP
						}
						Dim AsJob As Boolean = False
						Dim ResultCode As UShort = VirtualizationMethodErrors.InvalidState
						Dim JobID As String = String.Empty
						Dim JobStatus As String = String.Empty
						If IsVmStartable(CurrentState) Then
							Using VMStartController As New CimAsyncInvokeInstanceMethodController(Session, CimNamespaceVirtualization) With {.Instance = VM}
								VMStartController.MethodName = CimMethodNameRequestStateChange
								VMStartController.InputParameters.Add(CimMethodParameter.Create(CimParameterNameRequestedState, VirtualMachineStates.Running, CimType.UInt16, 0))
								Using StartResult As CimMethodResult = Await VMStartController.StartAsync
									ResultCode = CUShort(StartResult.ReturnValue.Value)
									If ResultCode = VirtualizationMethodErrors.JobStarted Then
										AsJob = True
										Dim JobResult As KeyValuePair(Of UShort, String)
										Dim JobReference As CimInstance = CType(StartResult.OutParameters(CimParameterNameJob).Value, CimInstance)
										JobID = JobReference.GetInstancePropertyValueString(CimPropertyNameInstanceID)
										RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {.Message = String.Format(JobCreatedMessageTemplate, JobID, Info.Name, Info.ID)})
										JobResult = Await WatchVMStartJob(JobID)
										ResultCode = JobResult.Key
										JobStatus = JobResult.Value
									End If
								End Using
							End Using
						End If
						ProcessVMStartResult(ResultCode, Info, AsJob, JobStatus)
					End Sub)
			End Using
		End Using
	End Sub

	Private Const ModuleName As String = "VM Starter"
	Private Const DefaultFailureReason As String = "Code not recognized"
	Private Const JobNotFoundErrorTemplate As String = "Attemped to process a job with instance ID {0} but the job was not found."
	Private Const JobCreatedMessageTemplate As String = "Created start job with instance ID {0} for VM {1} with GUID {0}"
	Private Const JobNotFoundMessage As String = "Job not found"

	Private Shared Function ConvertVMListToQueryFilter(ByVal VmIDs As List(Of String)) As String
		Dim ListEntries As New List(Of String)(VmIDs.Count)
		For Each VmID As String In VmIDs
			ListEntries.Add(String.Format(" Name='{0}'", VmID))
		Next
		Return "WHERE" & String.Join(" OR", ListEntries)
	End Function

	Private Shared Function IsVmStartable(ByVal CurrentState As VirtualMachineStates) As Boolean
		Return CurrentState = VirtualMachineStates.Off OrElse CurrentState = VirtualMachineStates.Saved
	End Function

	Private Sub ProcessVMStartResult(ByVal ResultCode As UShort, ByVal Info As VMInfo, ByVal AsJob As Boolean, Optional ByVal JobStatus As String = "")
		Dim ResultMessage As New VMStartResultEventArgs With {.VirtualMachineInfo = Info, .ResultCode = ResultCode, .Success = ResultCode = 0}
		If AsJob Then
			ResultMessage.ResultText = JobStatus
		Else
			If [Enum].IsDefined(GetType(VirtualizationMethodErrors), ResultCode) Then
				ResultMessage.ResultText = CType(ResultCode, VirtualizationMethodErrors).ToString
			Else
				ResultMessage.ResultText = DefaultFailureReason
			End If
		End If
		RaiseEvent StartResult(Me, ResultMessage)
	End Sub

	Private Async Function WatchVMStartJob(ByVal JobInstanceID As String) As Task(Of KeyValuePair(Of UShort, String))
		Dim Job As CimInstance
		Dim JobList As CimInstanceList
		Dim JobState As JobStates = JobStates.New
		Dim ErrorCode As UShort
		Dim JobStatus As String = JobNotFoundMessage
		Using JobChecker As New CimAsyncQueryInstancesController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimQueryTemplateMsvmConcreteJob, JobInstanceID)
			}
			JobList = Await JobChecker.StartAsync
			If JobList.Count > 0 Then
				Job = JobList.First
				While JobState = JobStates.Running OrElse JobState = JobStates.New OrElse JobState = JobStates.Starting
					Job.Refresh(Session)
					JobState = CType(Job.CimInstanceProperties(CimPropertyNameJobState).Value, JobStates)
				End While
				ErrorCode = CUShort(Job.CimInstanceProperties(CimPropertyNameErrorCode).Value)
				JobStatus = Job.GetInstancePropertyValueString(CimPropertyNameJobStatus)
			Else
				RaiseEvent StarterError(Me, New ModuleExceptionEventArgs With {.ModuleName = ModuleName, .[Error] =
					New Exception(String.Format(JobNotFoundErrorTemplate, JobInstanceID))})
				JobState = JobStates.Exception
			End If
			Return New KeyValuePair(Of UShort, String)(ErrorCode, JobStatus)
		End Using
	End Function
End Class
