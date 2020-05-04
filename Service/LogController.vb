Imports System.Windows.Forms
Imports Microsoft.Management.Infrastructure

Public Interface IModuleLogger
	Sub LogModuleError(ByVal ModuleName As String, ByVal [Error] As Exception)
	Sub LogCimError(ByVal [Error] As CimException, ByVal ModuleName As String)
	Sub LogDebugMessageGeneric(ByVal Message As String, ByVal ModuleName As String)
End Interface

Public Interface IRegistryLogger
	Sub LogRegistryAccessError(ByVal RegistryPath As String, ByVal [Error] As Exception)
	Sub LogRegistryOpenKeyError(ByVal RegistryPath As String, ByVal [Error] As Exception)
	Sub LogDebugRegistryKVPNotFound(ByVal KVPName As String, ByVal Path As String)
End Interface

Public Interface IVirtualNetAdapterLogger
	Sub LogInvalidVirtualAdapter(ByVal InstanceId As String)
	Sub LogDebugVirtualAdapterEnumeratedCount(ByVal Count As Integer)
	Sub LogDebugVirtualAdapterEvent(ByVal MAC As String, ByVal Action As VirtualAdapterAction, ByVal IsEmulated As Boolean)

	Enum VirtualAdapterAction
		Changed
		Added
		Deleted
		AddedFromUpdate
	End Enum
End Interface

Public Interface IVirtualMachineStartLogger
	Sub LogVirtualMachineStart(ByVal Name As String, ByVal ID As String, ByVal MAC As String, ByVal RequestorIP As String, ByVal Success As Boolean, ByVal ResultCode As Integer, Optional ByVal ResultMessage As String = "")
	Sub LogDebugVMStart(ByVal VMName As String, ByVal VMID As String, ByVal JobID As String)
End Interface

Public Interface IMagicPacketLogger
	Sub LogMagicPacketProcessed(ByVal TargetMAC As String, ByVal RequestorIP As String)
	Sub LogDebugMagicPacketInvalidFormat()
	Sub LogDebugMagicPacketDuplicate(ByVal MAC As String)
	Sub LogDebugMagicPacketExclusionEnded(ByVal MAC As String)
End Interface

Public Interface ICheckpointLogger
	Sub LogCheckpointActionReport(ByVal ActionName As String, ByVal VMName As String, ByVal UserName As String, ByVal VMID As String, ByVal JobID As String, ByVal Completed As Boolean, ByVal ResultCode As Integer, Optional ByVal ResultMessage As String = "")
	Sub LogDebugVirtualizationJobReceived(ByVal TypeCode As Integer, ByVal JobID As String)
End Interface

Public Class LogController
	Implements IModuleLogger
	Implements IRegistryLogger
	Implements IVirtualNetAdapterLogger
	Implements IVirtualMachineStartLogger
	Implements IMagicPacketLogger
	Implements ICheckpointLogger

	Private Shared ControllerInstance As LogController
	Private Shared ServiceInstance As HyperViveService

	Public Shared Function GetControllerInstance(ByVal OwningService As HyperViveService) As LogController
		If Not IsValid Then
			ControllerInstance = New LogController(OwningService)
		End If
		Return ControllerInstance
	End Function

	Public Shared ReadOnly Property IsValid As Boolean
		Get
			Return ControllerInstance IsNot Nothing AndAlso ServiceInstance IsNot Nothing
		End Get
	End Property

	Public Shared Sub CloseAll()
		ServiceInstance = Nothing
		ControllerInstance = Nothing
	End Sub

	Private Sub New(ByVal OwningService As HyperViveService)
		ServiceInstance = OwningService
	End Sub

	Private Shared ReadOnly EventTemplate As New EventInstance(0L, 0)
	Private Shared ReadOnly EventLock As New Object

	Private Const EventCategoryApplicationError As Integer = 1
	Private Const EventCategoryModuleError As Integer = 2
	Private Const EventCategoryDebugMessage As Integer = 3
	Private Const EventCategoryMagicPacket As Integer = 4
	Private Const EventCategoryVMStarter As Integer = 5
	Private Const EventCategoryCheckpoint As Integer = 6
	Private Const EventCategoryCimError As Integer = 7
	Private Const EventIdApplicationHaltError As Integer = 1000
	Private Const EventIdModuleError As Integer = 1001
	Private Const EventIdCimError As Integer = 1002
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
			ServiceInstance.EventLog.WriteEvent(EventTemplate, Parameters)
		End SyncLock
	End Sub

	Public Sub LogApplicationHaltError(ByVal [Error] As Exception)
		WriteEventLogEntry(EventIdApplicationHaltError, EventCategoryApplicationError, {[Error].Message, [Error].GetType.FullName}, EventLogEntryType.Error)
	End Sub

	Public Sub LogModuleError(ByVal ModuleName As String, ByVal [Error] As Exception) Implements IModuleLogger.LogModuleError
		WriteEventLogEntry(EventIdModuleError, EventCategoryModuleError, {ModuleName, [Error].Message, [Error].GetType.FullName}, EventLogEntryType.Error)
	End Sub

	Public Sub LogCimError(ByVal [Error] As CimException, ByVal ModuleName As String) Implements IModuleLogger.LogCimError
		WriteEventLogEntry(EventIdCimError, EventCategoryCimError, {[Error].Message, ModuleName})
		[Error].Dispose()
	End Sub

	Public Sub LogRegistryAccessError(ByVal RegistryPath As String, ByVal [Error] As Exception) Implements IRegistryLogger.LogRegistryAccessError
		WriteEventLogEntry(EventIdRegistryAccessError, EventCategoryModuleError, {RegistryPath, [Error].Message, [Error].GetType.FullName}, EventLogEntryType.Error)
	End Sub

	Public Sub LogRegistryOpenKeyError(ByVal RegistryPath As String, ByVal [Error] As Exception) Implements IRegistryLogger.LogRegistryOpenKeyError
		WriteEventLogEntry(EventIdRegistryOpenKeyError, EventCategoryModuleError, {RegistryPath, [Error].Message, [Error].GetType.FullName}, EventLogEntryType.Error)
	End Sub

	Public Sub LogInvalidVirtualAdapter(ByVal InstanceId As String) Implements IVirtualNetAdapterLogger.LogInvalidVirtualAdapter
		WriteEventLogEntry(EventIdInvalidVirtualAdapter, EventCategoryModuleError, {InstanceId}, EventLogEntryType.Error)
	End Sub

	Public Sub LogMagicPacketProcessed(ByVal TargetMAC As String, ByVal RequestorIP As String) Implements IMagicPacketLogger.LogMagicPacketProcessed
		WriteEventLogEntry(EventIdMagicPacketProcessed, EventCategoryMagicPacket, {TargetMAC, RequestorIP})
	End Sub

	Public Sub LogVirtualMachineStart(ByVal Name As String, ByVal ID As String, ByVal MAC As String, ByVal RequestorIP As String, ByVal Success As Boolean, ByVal ResultCode As Integer, Optional ByVal ResultMessage As String = "") Implements IVirtualMachineStartLogger.LogVirtualMachineStart
		Dim EventId As Long = EventIdVirtualMachineStartSuccess
		Dim ParametersList As List(Of Object) = New List(Of Object)({Name, ID, MAC, RequestorIP})
		Dim EntryType As EventLogEntryType = CType(IIf(Success, EventLogEntryType.Information, EventLogEntryType.Error), EventLogEntryType)
		If ResultCode <> 0 Then
			EventId = EventIdVirtualMachineStartFail
			ParametersList.AddRange({ResultCode, ResultMessage})
		End If
		WriteEventLogEntry(EventId, EventCategoryVMStarter, ParametersList.ToArray, EntryType)
	End Sub

	Public Sub LogCheckpointActionReport(ByVal ActionName As String, ByVal VMName As String, ByVal UserName As String, ByVal VMID As String, ByVal JobID As String, ByVal Completed As Boolean, ByVal ResultCode As Integer, Optional ByVal ResultMessage As String = "") Implements ICheckpointLogger.LogCheckpointActionReport
		Dim EventId As Long = EventIdCheckpointActionStarted
		Dim EntryType As EventLogEntryType = EventLogEntryType.Information
		Dim ParametersList As List(Of Object) = New List(Of Object)({ActionName, VMName, UserName, VMID, JobID})
		If Completed Then
			EventId = CLng(IIf(ResultCode = 0, EventIdCheckpointActionSuccess, EventIdCheckpointActionFail))
		ElseIf ResultCode = 4096 Then
			ResultCode = 0 ' 4096 for a non-completed job means that it's still in-progress
		End If
		If ResultCode <> 0 Then
			ParametersList.AddRange({ResultCode, ResultMessage})
			EntryType = EventLogEntryType.Error
		End If
		WriteEventLogEntry(EventId, EventCategoryCheckpoint, ParametersList.ToArray, EntryType)
	End Sub

	Private Sub LogBaseDebugMessage(ByVal EventId As Long, ByVal Parameters As Object())
		WriteEventLogEntry(EventId, EventCategoryDebugMessage, Parameters)
	End Sub

	Public Sub LogDebugMessageGeneric(ByVal Message As String, ByVal ModuleName As String) Implements IModuleLogger.LogDebugMessageGeneric
		LogBaseDebugMessage(EventIdDebugMessageGeneric, {Message, ModuleName})
	End Sub

	Public Sub LogDebugModeChanged(ByVal Mode As Boolean)
		LogBaseDebugMessage(EventIdDebugModeChanged, {Mode})
	End Sub

	Public Sub LogDebugRegistryKVPNotFound(ByVal KVPName As String, ByVal Path As String) Implements IRegistryLogger.LogDebugRegistryKVPNotFound
		LogBaseDebugMessage(EventIdDebugRegistryKVPNotFound, {KVPName, Path})
	End Sub

	Public Sub LogDebugVirtualAdapterEnumeratedCount(ByVal Count As Integer) Implements IVirtualNetAdapterLogger.LogDebugVirtualAdapterEnumeratedCount
		LogBaseDebugMessage(EventIdDebugVirtualAdapterEnumeratedCount, {Count})
	End Sub

	Public Sub LogDebugVirtualAdapterEvent(ByVal MAC As String, ByVal Action As IVirtualNetAdapterLogger.VirtualAdapterAction, ByVal IsEmulated As Boolean) Implements IVirtualNetAdapterLogger.LogDebugVirtualAdapterEvent
		Dim EventId As Long

		Select Case Action
			Case IVirtualNetAdapterLogger.VirtualAdapterAction.Added
				EventId = EventIdDebugVirtualAdapterNew
			Case IVirtualNetAdapterLogger.VirtualAdapterAction.AddedFromUpdate
				EventId = EventIdDebugVirtualAdapterNewFromUpdate
			Case IVirtualNetAdapterLogger.VirtualAdapterAction.Deleted
				EventId = EventIdDebugVirtualAdapterDeleted
			Case Else
				EventId = EventIdDebugVirtualAdapterChanged
		End Select
		'todo: this can't just be a string in the code file
		Dim AdapterType As String = IIf(IsEmulated, "Emulated", "Synthetic").ToString
		LogBaseDebugMessage(EventId, {MAC, AdapterType})
	End Sub

	Public Sub LogDebugVMStart(ByVal VMName As String, ByVal VMID As String, ByVal JobID As String) Implements IVirtualMachineStartLogger.LogDebugVMStart
		LogBaseDebugMessage(EventIdDebugInitiatedVMStart, {VMName, VMID, JobID})
	End Sub

	Public Sub LogDebugMagicPacketInvalidFormat() Implements IMagicPacketLogger.LogDebugMagicPacketInvalidFormat
		LogBaseDebugMessage(EventIdDebugMagicPacketInvalidFormat, Array.Empty(Of Object))
	End Sub

	Public Sub LogDebugMagicPacketDuplicate(ByVal MAC As String) Implements IMagicPacketLogger.LogDebugMagicPacketDuplicate
		LogBaseDebugMessage(EventIdDebugMagicPacketDuplicate, {MAC})
	End Sub

	Public Sub LogDebugMagicPacketExclusionEnded(ByVal MAC As String) Implements IMagicPacketLogger.LogDebugMagicPacketExclusionEnded
		LogBaseDebugMessage(EventIdDebugMagicPacketExclusionEnded, {MAC})
	End Sub

	Public Sub LogDebugVirtualizationJobReceived(ByVal TypeCode As Integer, ByVal JobID As String) Implements ICheckpointLogger.LogDebugVirtualizationJobReceived
		LogBaseDebugMessage(EventIdDebugVirtualizationJobReceived, {TypeCode, JobID})
	End Sub
End Class
