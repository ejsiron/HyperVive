Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure

Public Module VMEvents
	Public Class VMStartedEventArgs
		Inherits EventArgs

		Public Property ID As String
		Public Property Name As String
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

	Public Sub New(ByVal Session As CimSession)
		Me.Session = Session
	End Sub

	Public Async Sub Start(ByVal VmIDs As List(Of String))
		Dim VMLister As New CimAsyncQueryInstancesController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format("SELECT * FROM Msvm_ComputerSystem {0}", ConvertVMListToQueryFilter(VmIDs)),
			.KeysOnly = True
		}
		TargetVMs = Await VMLister.StartAsync
		VMStarters = New List(Of CimAsyncInvokeInstanceMethodController)(TargetVMs.Count)
		For Each VM As CimInstance In TargetVMs
			Dim CurrentState As VirtualMachineStates = CType(VM.CimInstanceProperties(CimPropertyNameEnabledState).Value, VirtualMachineStates)
			Dim VMName As String = VM.GetInstancePropertyValueString("ElementName")
			Dim VmID As String = VM.GetInstancePropertyValueString("Name")
			If IsVmStartable(CurrentState) Then
				Dim VMStartController As New CimAsyncInvokeInstanceMethodController(Session, CimNamespaceVirtualization)
				VMStartController.MethodName = CimMethodNameRequestStateChange
				VMStartController.InputParameters.Add(CimMethodParameter.Create(CimParameterNameRequestedState, VirtualMachineStates.Running, 0))
				Dim StartResult As CimMethodResult = Await VMStartController.StartAsync
				Dim ResultCode As VirtualizationMethodErrors = CType(StartResult.ReturnValue.Value, VirtualizationMethodErrors)
				If ResultCode = VirtualizationMethodErrors.NoError Then
					RaiseEvent VMStarted(Me, New VMStartedEventArgs With {.ID = VmID, .Name = VMName})
				ElseIf ResultCode = VirtualizationMethodErrors.JobStarted Then
				Else
					Dim Reason As String
					Try
						Reason = ResultCode.ToString
					Catch ex As Exception
						Reason = "Code not recognized"
					End Try
					RaiseEvent VMStartFailure(Me, New VMStartFailureEventArgs With {.ID = VmID, .Name = VMName, .ErrorCode = ResultCode, .Reason = ResultCode.ToString})
				End If
				VMStarters.Add(VMStartController)
			End If
		Next
	End Sub

	Private TargetVMs As List(Of CimInstance)
	Private VMStarters As List(Of CimAsyncInvokeInstanceMethodController)

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

	Private Sub ProcessVMStarter(ByVal RunningTask As Task(Of CimMethodResult))

	End Sub
End Class
