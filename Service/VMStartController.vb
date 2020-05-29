Imports HyperVive.CIMitar
Imports HyperVive.CIMitar.Virtualization
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

''' <summary>
''' Starts all virtual machines with an InstanceID in a supplied list
''' </summary>
Public Class VMStartController
	Inherits ModuleWithCimBase

	''' <summary>
	''' Creates a new VM Start Controller
	''' </summary>
	''' <param name="Session">A <see cref="CimSession"/> on the system that owns the target virtual machines</param>
	Public Sub New(ByVal Session As CimSession, ByVal ModuleLogger As IModuleLogger, ByVal VirtualMachineStartLogger As IVirtualMachineStartLogger)
		MyBase.New(Session, ModuleLogger)
		VMStartLogger = VirtualMachineStartLogger
	End Sub

	''' <summary>
	''' Attempts to start VMs that match the supplied IDs.
	''' </summary>
	''' <remarks>If possible, runs each start operation in parallel. Hyper-V should always attempt to start a background job. The operation will watch the output of such jobs.</remarks>
	''' <param name="MacAddress">The MAC address owned by the VM(s). Unvalidated, passes through to events.</param>
	''' <param name="VmIDs">A <see cref="List(Of String)"/> of virtual machine IDs, in <see cref="String"/> format</param>
	''' <param name="SourceIP">A <see cref="String"/> that contains the source IP of the WOL frame. Unvalidated, passes through to events.</param>
	Public Async Sub StartVM(ByVal MacAddress As String, ByVal VmIDs As List(Of String), SourceIP As String)
		Using VMLister As New CimAsyncQueryInstancesController(Session, NamespaceVirtualization) With {
			.QueryText = String.Format(CimQueryTemplateVirtualMachine, ConvertVMListToQueryFilter(VmIDs)),
			.KeysOnly = True
		}
			Using TargetVMs As CimInstanceCollection = Await VMLister.StartAsync
				Parallel.ForEach(TargetVMs,
					Async Sub(ByVal VM As CimInstance)
						Dim CurrentState As VirtualMachineStates = CType(VM.CimInstanceProperties(PropertyNameEnabledState).Value, VirtualMachineStates)
						Dim Info As New VMInfo With {
							.ID = VM.InstancePropertyString(PropertyNameName),
							.Name = VM.InstancePropertyString(PropertyNameElementName),
							.MacAddress = MacAddress,
							.SourceIP = SourceIP
						}
						Dim ResultCode As UShort = VirtualizationMethodErrors.InvalidState
						Dim Job As CimInstance = Nothing
						Dim JobId As String = String.Empty
						If IsVmStartable(CurrentState) Then
							Using VMStartController As New CimAsyncInvokeInstanceMethodController(Session, NamespaceVirtualization) With {.Instance = VM}
								VMStartController.MethodName = CimMethodNameRequestStateChange
								VMStartController.InputParameters.Add(CimMethodParameter.Create(CimParameterNameRequestedState, VirtualMachineStates.Running, CimType.UInt16, 0))
								Using StartResult As CimMethodResult = Await VMStartController.StartAsync
									ResultCode = CUShort(StartResult.ReturnValue.Value)
									If ResultCode = VirtualizationMethodErrors.JobStarted Then
										Dim JobReference As CimInstance = CType(StartResult.OutParameters(CimParameterNameJob).Value, CimInstance)
										JobId = JobReference.InstancePropertyString(PropertyNameInstanceID)
										VMStartLogger.LogDebugVMStart(Info.Name, Info.ID, JobId)
										Job = Await VirtualizationJobCompletionController.WatchAsync(Session, JobId)
									End If
								End Using
							End Using
						End If
						ProcessVMStartResult(ResultCode, Info, Job)
					End Sub)
			End Using
		End Using
	End Sub

	Public Overrides ReadOnly Property ModuleName As String = "VM Starter"
	Private Const DefaultFailureReason As String = "Code not recognized"

	Private ReadOnly VMStartLogger As IVirtualMachineStartLogger

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

	Private Sub ProcessVMStartResult(ByVal ResultCode As UShort, ByVal Info As VMInfo, ByRef JobInstance As CimInstance)
		Dim ResultText As String = DefaultFailureReason
		If JobInstance IsNot Nothing Then
			ResultCode = JobInstance.InstancePropertyUInt16(PropertyNameErrorCode)
			ResultText = JobInstance.InstancePropertyString(PropertyNameJobStatus)
			JobInstance.Dispose()
		Else
			If [Enum].IsDefined(GetType(VirtualizationMethodErrors), ResultCode) Then
				ResultText = CType(ResultCode, VirtualizationMethodErrors).ToString
			End If
		End If
		VMStartLogger?.LogVirtualMachineStart(Info.Name, Info.ID, Info.MacAddress, Info.SourceIP, ResultCode = 0, ResultCode, ResultText)
	End Sub
End Class
