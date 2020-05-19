Imports HyperVive.CIMitar
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
	Sub LogDebugMagicPacketReceived(ByVal TargetMAC As String, ByVal RequestorIP As String)
	Sub LogDebugMagicPacketExclusionEnded(ByVal MAC As String)
	Sub LogDebugMagicPacketDuplicate(ByVal MAC As String)
	Sub LogDebugMagicPacketNotInExclusionList(ByVal MAC As String)
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

	Public Shared Function GetControllerInstance(ByVal LocalCimSession As CimSession, ByVal OwningService As HyperViveService) As LogController
		If Not IsValid Then
			ControllerInstance = New LogController(LocalCimSession, OwningService)
		End If
		Return ControllerInstance
	End Function

	Public Shared ReadOnly Property IsValid As Boolean
		Get
			Return ControllerInstance IsNot Nothing AndAlso ServiceInstance IsNot Nothing
		End Get
	End Property

	Public Shared Sub CloseAll()
		DebugModeSettingController?.Dispose()
		DebugModeSettingController = Nothing
		ServiceInstance = Nothing
		ControllerInstance = Nothing
	End Sub

	Private Sub New(ByVal LocalCimSession As CimSession, ByVal OwningService As HyperViveService)
		Session = LocalCimSession
		ServiceInstance = OwningService
		DebugModeSettingController = New RegistryController(Session, AddressOf UpdateSettingMode, Me, Me) With {.RootRegistry = Microsoft.Win32.Registry.LocalMachine, .KeySubPath = ServiceInstance.ServiceRegistryRootPath, .ValueName = DebugModeKVPName}
		DebugModeSettingController.Start()
	End Sub

	Private Shared ReadOnly EventTemplate As New EventInstance(0L, 0)
	Private Shared ReadOnly EventLock As New Object

	Private Shared Session As CimSession
	Private Shared ControllerInstance As LogController = Nothing
	Private Shared ServiceInstance As HyperViveService = Nothing
	Private Shared DebugModeSettingController As RegistryController

	Private Const EventCategoryApplicationError As UInteger = 1
	Private Const EventCategoryModuleError As UInteger = 2
	Private Const EventCategoryDebugMessage As UInteger = 3
	Private Const EventCategoryMagicPacket As UInteger = 4
	Private Const EventCategoryVMStarter As UInteger = 5
	Private Const EventCategoryCheckpoint As UInteger = 6
	Private Const EventCategoryCimError As UInteger = 7
	Private Const EventIdApplicationHaltError As UInteger = 1000
	Private Const EventIdModuleError As UInteger = 1001
	Private Const EventIdCimError As UInteger = 1002
	Private Const EventIdElevationError As UInteger = 1003
	Private Const EventIdRegistryAccessError As UInteger = 1011
	Private Const EventIdRegistryOpenKeyError As UInteger = 1012
	Private Const EventIdInvalidVirtualAdapter As UInteger = 1021
	Private Const EventIdMagicPacketProcessed As UInteger = 2000
	Private Const EventIdVirtualMachineStartSuccess As UInteger = 3000
	Private Const EventIdVirtualMachineStartFail As UInteger = 3001
	Private Const EventIdCheckpointActionStarted As UInteger = 4000
	Private Const EventIdCheckpointActionSuccess As UInteger = 4001
	Private Const EventIdCheckpointActionFail As UInteger = 4002
	Private Const EventIdDebugMessageGeneric As UInteger = 9000
	Private Const EventIdDebugModeChanged As UInteger = 9001
	Private Const EventIdDebugRegistryKVPNotFound As UInteger = 9002
	Private Const EventIdDebugVirtualAdapterEnumeratedCount As UInteger = 9003
	Private Const EventIdDebugVirtualAdapterNew As UInteger = 9004
	Private Const EventIdDebugVirtualAdapterChanged As UInteger = 9005
	Private Const EventIdDebugVirtualAdapterNewFromUpdate As UInteger = 9006
	Private Const EventIdDebugVirtualAdapterDeleted As UInteger = 9007
	Private Const EventIdDebugInitiatedVMStart As UInteger = 9008
	Private Const EventIdDebugMagicPacketInvalidFormat As UInteger = 9009
	Private Const EventIdDebugMagicPacketReceived As UInteger = 9010
	Private Const EventIdDebugMagicPacketExclusionEnded As UInteger = 9011
	Private Const EventIdDebugVirtualizationJobReceived As UInteger = 9012
	Private Const EventIdDebugMagicPacketDuplicate As UInteger = 9013
	Private Const EventIdDebugMagicPacketNotInExclusionList As UInteger = 9014

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
		WriteEventLogEntry(EventIdCimError, EventCategoryCimError, {[Error].Message, ModuleName}, EventLogEntryType.Error)
		[Error].Dispose()
	End Sub

	Public Sub LogElevationError()
		WriteEventLogEntry(EventIdElevationError, EventCategoryApplicationError, Array.Empty(Of Object), EventLogEntryType.Error)
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
		End If
		If Not {0, 4096}.Contains(ResultCode) Then   ' 0 = Success, 4096 = job started
			ParametersList.AddRange({ResultCode, ResultMessage})
			EntryType = EventLogEntryType.Error
		End If
		WriteEventLogEntry(EventId, EventCategoryCheckpoint, ParametersList.ToArray, EntryType)
	End Sub

	Private Sub LogBaseDebugMessage(ByVal EventId As Long, ByVal Parameters As Object())
		If DebugMode Then
			WriteEventLogEntry(EventId, EventCategoryDebugMessage, Parameters)
		End If
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

	Public Sub LogDebugMagicPacketReceived(ByVal TargetMAC As String, ByVal RequestorIP As String) Implements IMagicPacketLogger.LogDebugMagicPacketReceived
		LogBaseDebugMessage(EventIdDebugMagicPacketReceived, {TargetMAC, RequestorIP})
	End Sub

	Public Sub LogDebugMagicPacketExclusionEnded(ByVal MAC As String) Implements IMagicPacketLogger.LogDebugMagicPacketExclusionEnded
		LogBaseDebugMessage(EventIdDebugMagicPacketExclusionEnded, {MAC})
	End Sub

	Public Sub LogDebugVirtualizationJobReceived(ByVal TypeCode As Integer, ByVal JobID As String) Implements ICheckpointLogger.LogDebugVirtualizationJobReceived
		LogBaseDebugMessage(EventIdDebugVirtualizationJobReceived, {TypeCode, JobID})
	End Sub

	Public Sub LogDebugMagicPacketDuplicate(ByVal MAC As String) Implements IMagicPacketLogger.LogDebugMagicPacketDuplicate
		LogBaseDebugMessage(EventIdDebugMagicPacketDuplicate, {MAC})
	End Sub

	Public Sub LogDebugMagicPacketNotInExclusionList(ByVal MAC As String) Implements IMagicPacketLogger.LogDebugMagicPacketNotInExclusionList
		LogBaseDebugMessage(EventIdDebugMagicPacketNotInExclusionList, {MAC})
	End Sub

	Private DebugMode As Boolean = False

	Private Const DebugModeKVPName As String = "DebugMode"

	Private Sub UpdateSettingMode()
		Try
			DebugMode = CBool(DebugModeSettingController.Value)
		Catch ex As Exception
			DebugMode = True
		End Try
		LogDebugModeChanged(DebugMode)
	End Sub
End Class
