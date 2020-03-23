Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure

Public Class VMInfo
	Public Property MacAddress As String
	Public Property ID As String
	Public Property Name As String
End Class

Public Module VMEvents
	Public Class VMStartedEventArgs
		Inherits EventArgs
		Public Property VirtualMachineInfo As VMInfo
	End Class

	Public Class VMStartFailureEventArgs
		Inherits VMStartedEventArgs
		Public Property ErrorCode As UInt16
		Public Property Reason As String
	End Class
End Module

Public Class VMStarter
	Private Session As CimSession

	Public Event VMStarted(ByVal sender As Object, ByVal e As VMStartedEventArgs)
	Public Event VMStartFailure(ByVal sender As Object, ByVal e As VMStartFailureEventArgs)
	Public Event VMStartDebugMessage(ByVal sender As Object, ByVal e As DebugMessageEventArgs)


	Public Sub New(ByVal Session As CimSession)
		Me.Session = Session
	End Sub

	Public Async Sub Start(ByVal MacAddress As String, ByVal VmIDs As List(Of String))
		Using VMLister As New CimAsyncQueryInstancesController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format("SELECT * FROM Msvm_ComputerSystem {0}", ConvertVMListToQueryFilter(VmIDs)),
			.KeysOnly = True
		}
			Using TargetVMs As CimInstanceList = Await VMLister.StartAsync
				Parallel.ForEach(TargetVMs,
					Async Sub(ByVal VM As CimInstance)
						Dim CurrentState As VirtualMachineStates = CType(VM.CimInstanceProperties(CimPropertyNameEnabledState).Value, VirtualMachineStates)
						Dim Info As New VMInfo With {
							.ID = VM.GetInstancePropertyValueString("Name"),
							.Name = VM.GetInstancePropertyValueString("Name"),
							.MacAddress = MacAddress
						}
						Dim ResultCode As UInt16 = VirtualizationMethodErrors.InvalidState
						Dim JobID As String = String.Empty
						If IsVmStartable(CurrentState) Then
							Using VMStartController As New CimAsyncInvokeInstanceMethodController(Session, CimNamespaceVirtualization)
								VMStartController.MethodName = CimMethodNameRequestStateChange
								VMStartController.InputParameters.Add(CimMethodParameter.Create(CimParameterNameRequestedState, VirtualMachineStates.Running, 0))
								Using StartResult As CimMethodResult = Await VMStartController.StartAsync
									ResultCode = CUShort(StartResult.ReturnValue.Value)
									If ResultCode = VirtualizationMethodErrors.JobStarted Then
										Dim JobReference As CimInstance = CType(StartResult.OutParameters("Job").Value, CimInstance)
										JobID = JobReference.GetInstancePropertyValueString(CimPropertyNameInstanceID)
										RaiseEvent VMStartDebugMessage(Me, New DebugMessageEventArgs With {.Message = String.Format("Created start job with instance ID {0} for VM {1} with GUID {0}", JobID, Info.Name, Info.ID)})
										WatchVMStartJob(JobID, Info).Start()
									Else
										ProcessVMStartResult(ResultCode, Info)
									End If
								End Using
							End Using
						End If
					End Sub)
			End Using
		End Using
	End Sub

	Private ReadOnly Property ModuleName As String = "VM Starter"

	Private Shared Function ConvertVMListToQueryFilter(ByVal VmIDs As List(Of String)) As String
		Dim ListEntries As New List(Of String)(VmIDs.Count)
		For Each VmID As String In VmIDs
			ListEntries.Add(String.Format(" Name='{0}'", VmID))
		Next
		Return "WHERE" & String.Join(" AND", ListEntries)
	End Function

	Private Shared Function IsVmStartable(ByVal CurrentState As VirtualMachineStates) As Boolean
		Return CurrentState = VirtualMachineStates.Off OrElse CurrentState = VirtualMachineStates.Saved
	End Function

	Private Sub ProcessVMStartResult(ByVal ResultCode As UInt16, ByVal Info As VMInfo)
		If ResultCode = VirtualizationMethodErrors.NoError Then
			RaiseEvent VMStarted(Me, New VMStartedEventArgs With {.VirtualMachineInfo = Info})
		Else  ' take care not to call with a 4096
			Dim FailureReason As String
			If [Enum].IsDefined(GetType(VirtualizationMethodErrors), ResultCode) Then
				FailureReason = CType(ResultCode, VirtualizationMethodErrors).ToString
			Else
				FailureReason = "Code not recognized"
			End If
			RaiseEvent VMStartFailure(Me, New VMStartFailureEventArgs With {.VirtualMachineInfo = Info, .ErrorCode = ResultCode, .Reason = ResultCode.ToString})
		End If
	End Sub

	Private Function WatchVMStartJob(ByVal JobInstanceID As String, ByVal Info As VMInfo) As Task

	End Function
End Class
