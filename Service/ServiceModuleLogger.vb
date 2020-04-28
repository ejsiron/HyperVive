Partial Public Class ModuleController
	Public Enum LogVirtualAdapterAction
		Changed
		Added
		Deleted
		AddedFromUpdate
	End Enum

	Private Shared ReadOnly EventTemplate As New EventInstance(0L, 0)
	Private Shared ReadOnly EventLock As New Object

	Private Const EventCategoryApplicationError As Integer = 1
	Private Const EventCategoryModuleError As Integer = 2
	Private Const EventCategoryDebugMessage As Integer = 3
	Private Const EventCategoryMagicPacket As Integer = 4
	Private Const EventCategoryVMStarter As Integer = 5
	Private Const EventCategoryCheckpoint As Integer = 6

	Private Const EventIdApplicationHaltError As Integer = 1000
	Private Const EventIdModuleError As Integer = 1001
	Private Const EventIdRegistryAccessError As Integer = 1011
	Private Const EventIdRegistryOpenKeyError As Integer = 1012
	Private Const EventIdInvalidVirtualAdapter As Integer = 1021
	Private Const EventIdMagicPacketProcessed As Integer = 2000
	Private Const EventIdVirtualMachineStartSuccess As Integer = 3000
	Private Const EventIdVirtualMachineStartFail As Integer = 3001
	Private Const EventIdCheckpointActionStarted As Integer = 4000
	Private Const EventIdCheckpointActionSuccess As Integer = 4001
	Private Const EventIdCheckpointActionFail As Integer = 4002
	Private Const EventIdDebugMessageGeneric As Integer = 9000
	Private Const EventIdDebugModeChanged As Integer = 9001
	Private Const EventIdDebugRegistryKVPNotFound As Integer = 9002
	Private Const EventIdDebugVirtualAdapterEnumeratedCount As Integer = 9003
	Private Const EventIdDebugVirtualAdapterNew As Integer = 9004
	Private Const EventIdDebugVirtualAdapterChanged As Integer = 9005
	Private Const EventIdDebugVirtualAdapterNewFromUpdate As Integer = 9006
	Private Const EventIdDebugVirtualAdapterDeleted As Integer = 9007
	Private Const EventIdDebugInitiatedVMStart As Integer = 9008
	Private Const EventIdDebugMagicPacketInvalidFormat As Integer = 9009
	Private Const EventIdDebugMagicPacketDuplicate As Integer = 9010
	Private Const EventIdDebugMagicPacketExclusionEnded As Integer = 9011
	Private Const EventIdDebugVirtualizationJobReceived As Integer = 9012

	Private Sub WriteEventLogEntry(ByVal EventId As Long, ByVal EventCategory As Integer, Parameters As Object(), Optional EventType As EventLogEntryType = EventLogEntryType.Information)
		SyncLock EventLock
			EventTemplate.InstanceId = EventId
			EventTemplate.CategoryId = EventCategory
			EventTemplate.EntryType = EventType
			Service.EventLog.WriteEvent(EventTemplate, Parameters)
		End SyncLock
	End Sub

	Private Sub LogApplicationHaltError(ByVal [Error] As Exception)
		WriteEventLogEntry(EventIdApplicationHaltError, EventCategoryApplicationError, {[Error].Message, [Error].GetType.FullName}, EventLogEntryType.Error)
	End Sub

	Private Sub LogModuleError(ByVal ModuleName As String, ByVal [Error] As Exception)
		WriteEventLogEntry(EventIdModuleError, EventCategoryModuleError, {ModuleName, [Error].Message, [Error].GetType.FullName}, EventLogEntryType.Error)
	End Sub

	Private Sub LogRegistryAccessError(ByVal RegistryPath As String, ByVal [Error] As Exception)
		WriteEventLogEntry(EventIdRegistryAccessError, EventCategoryModuleError, {RegistryPath, [Error].Message, [Error].GetType.FullName}, EventLogEntryType.Error)
	End Sub

	Private Sub LogRegistryOpenKeyError(ByVal RegistryPath As String, ByVal [Error] As Exception)
		WriteEventLogEntry(EventIdRegistryOpenKeyError, EventCategoryModuleError, {RegistryPath, [Error].Message, [Error].GetType.FullName}, EventLogEntryType.Error)
	End Sub

	Private Sub LogInvalidVirtualAdapter(ByVal InstanceId As String)
		WriteEventLogEntry(EventIdInvalidVirtualAdapter, EventCategoryModuleError, {InstanceId}, EventLogEntryType.Error)
	End Sub

	Private Sub LogMagicPacketProcessed(ByVal TargetMAC As String, ByVal RequestorIP As String)
		WriteEventLogEntry(EventIdMagicPacketProcessed, EventCategoryMagicPacket, {TargetMAC, RequestorIP})
	End Sub

	Private Sub LogVirtualMachineStart(ByVal Name As String, ByVal ID As String, ByVal MAC As String, ByVal RequestorIP As String, ByVal Success As Boolean, ByVal ResultCode As Integer, Optional ByVal ResultMessage As String = "")
		Dim EventId As Long = EventIdVirtualMachineStartSuccess
		Dim ParametersList As List(Of Object) = New List(Of Object)({Name, ID, MAC, RequestorIP})
		Dim EntryType As EventLogEntryType = CType(IIf(Success, EventLogEntryType.Information, EventLogEntryType.Error), EventLogEntryType)
		If ResultCode <> 0 Then
			EventId = EventIdVirtualMachineStartFail
			ParametersList.AddRange({ResultCode, ResultMessage})
		End If
		WriteEventLogEntry(EventId, EventCategoryVMStarter, ParametersList.ToArray, EntryType)
	End Sub

	Private Sub LogCheckpointActionStarted(ByVal ActionName As String, ByVal VMName As String, ByVal UserName As String, ByVal VMID As String, ByVal JobID As String, ByVal Completed As Boolean, ByVal ResultCode As Integer, Optional ByVal ResultMessage As String = "")
		Dim EventId As Long = EventIdCheckpointActionStarted
		Dim EntryType As EventLogEntryType = EventLogEntryType.Information
		If Completed Then
			EventId = CLng(IIf(ResultCode = 0, EventIdCheckpointActionSuccess, EventIdCheckpointActionFail))
		End If
		Dim ParametersList As List(Of Object) = New List(Of Object)({ActionName, VMName, UserName, VMID, JobID})
		If ResultCode <> 0 Then
			ParametersList.AddRange({ResultCode, ResultMessage})
			EntryType = EventLogEntryType.Error
		End If
		WriteEventLogEntry(EventId, EventCategoryCheckpoint, ParametersList.ToArray, EntryType)
	End Sub

	Private Sub LogBaseDebugMessage(ByVal EventId As Long, ByVal Parameters As Object())
		WriteEventLogEntry(EventId, EventCategoryDebugMessage, Parameters)
	End Sub

	Private Sub LogDebugMessageGeneric(ByVal Message As String)
		LogBaseDebugMessage(EventIdDebugMessageGeneric, {Message})
	End Sub

	Private Sub LogDebugModeChanged(ByVal Mode As Boolean)
		LogBaseDebugMessage(EventIdDebugModeChanged, {Mode})
	End Sub

	Private Sub LogDebugRegistryKVPNotFound(ByVal KVPName As String, ByVal Path As String)
		LogBaseDebugMessage(EventIdDebugRegistryKVPNotFound, {KVPName, Path})
	End Sub

	Private Sub LogDebugVirtualAdapterEnumeratedCount(ByVal Count As Integer)
		LogBaseDebugMessage(EventIdDebugVirtualAdapterEnumeratedCount, {Count})
	End Sub

	Private Sub LogDebugVirtualAdapterEvent(ByVal MAC As String, ByVal Action As LogVirtualAdapterAction, ByVal IsEmulated As Boolean)
		Dim EventId As Long
		Select Case Action
			Case LogVirtualAdapterAction.Added
				EventId = EventIdDebugVirtualAdapterNew
			Case LogVirtualAdapterAction.AddedFromUpdate
				EventId = EventIdDebugVirtualAdapterNewFromUpdate
			Case LogVirtualAdapterAction.Deleted
				EventId = EventIdDebugVirtualAdapterDeleted
			Case Else
				EventId = EventIdDebugVirtualAdapterChanged
		End Select
		'todo: this can't just be a string in the code file
		Dim AdapterType As String = IIf(IsEmulated, "Emulated", "Synthetic").ToString
		LogBaseDebugMessage(EventId, {MAC, AdapterType})
	End Sub

	Public Sub LogDebugVMStart(ByVal VMName As String, ByVal VMID As String, ByVal JobID As String)
		LogBaseDebugMessage(EventIdDebugInitiatedVMStart, {VMName, VMID, JobID})
	End Sub

	Public Sub LogDebugMagicPacketInvalidFormat()
		LogBaseDebugMessage(EventIdDebugMagicPacketInvalidFormat, Array.Empty(Of Object))
	End Sub

	Public Sub LogDebugMagicPacketDuplicate(ByVal MAC As String)
		LogBaseDebugMessage(EventIdDebugMagicPacketDuplicate, {MAC})
	End Sub

	Public Sub LogDebugMagicPacketExclusionEnded(ByVal MAC As String)
		LogBaseDebugMessage(EventIdDebugMagicPacketExclusionEnded, {MAC})
	End Sub

	Public Sub LogDebugVirtualizationJobReceived(ByVal TypeCode As Integer, ByVal JobID As String)
		LogBaseDebugMessage(EventIdDebugVirtualizationJobReceived, {TypeCode, JobID})
	End Sub
End Class
