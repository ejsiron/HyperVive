Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure

Public Class VMStarter
	Private Session As CimSession

	Private TargetVMs As List(Of CimInstance)
	Private VMStarters As List(Of CimAsyncInvokeInstanceMethodController)

	Public Sub New(ByVal Session As CimSession)
		Me.Session = Session
	End Sub

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

	Public Async Sub Start(ByVal VmIDs As List(Of String))
		Dim VMLister As New CimAsyncQueryInstancesController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format("SELECT * FROM Msvm_ComputerSystem {0}", ConvertVMListToQueryFilter(VmIDs)),
			.KeysOnly = True
		}
		TargetVMs = Await VMLister.StartAsync
		VMStarters = New List(Of CimAsyncInvokeInstanceMethodController)(TargetVMs.Count)
		For Each VM As CimInstance In TargetVMs
			Dim CurrentState As VirtualMachineStates = CType(VM.CimInstanceProperties(CimPropertyNameEnabledState).Value, VirtualMachineStates)
			If IsVmStartable(CurrentState) Then
				Dim VMStartController As New CimAsyncInvokeInstanceMethodController(Session, CimNamespaceVirtualization)
				VMStartController.MethodName = "Request"
				VMStarters.Add(VMStartController)
			End If
		Next
	End Sub
End Class
