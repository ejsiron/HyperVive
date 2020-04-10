Module GlobalConstants
#Region "Cim Globals"
	Public Const Unknown As String = "Unknown"
	Public Const CimNamespaceRootDefault As String = "root/DEFAULT"

	Public Const CimClassNameSyntheticAdapterSettingData As String = "Msvm_SyntheticEthernetPortSettingData"
	Public Const CimClassNameEmulatedAdapterSettingData As String = "Msvm_EmulatedEthernetPortSettingData"

	Public Const CimMethodNameRequestStateChange As String = "RequestStateChange"

	Public Const CimParameterNameRequestedState As String = "RequestedState"
	Public Const CimParameterNameTimeoutPeriod As String = "TimeoutPeriod"
	Public Const CimParameterNameJob As String = "Job"

	Public Const CimQueryTemplateRegistryValueChange As String = "SELECT * FROM RegistryValueChangeEvent WHERE HIVE='{0}' AND KeyPath='{1}' AND ValueName='{2}'"
#End Region 'Cim Globals

#Region "Virtualization Globals"
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
#End Region

#Region "Registry Globals"
	Public Const RegistryKeyName As String = "SYSTEM\CurrentControlSet\Services\HyperVive\"
	Public Const RegistryDebugKVPName As String = "DebugMode"
#End Region

#Region "Event Log Globals"
	Public Const EventIdAppError As Integer = 1000
	Public Const EventIdModuleErrorGeneral As Integer = 1001
	Public Const EventIdErrorRegistryAccess As Integer = 1011
	Public Const EventIdErrorRegistryKeyOpen As Integer = 1012
	Public Const EventIdErrorInvalidVirtualAdapter As Integer = 1021
	Public Const EventIdErrorVirtualAdapterSubscriber As Integer = 1022
	Public Const EventIdErrorWOLReceiver As Integer = 1031
	Public Const EventIdMagicPacketReceived As Integer = 2000
	Public Const EventIdVMStartSuccess As Integer = 3000
	Public Const EventIdVMStartFailed As Integer = 3001
	Public Const EventIdCheckpointActionStarted As Integer = 4000
	Public Const EventIdCheckpointActionSucceeded As Integer = 4001
	Public Const EventIdCheckpointActionFailed As Integer = 4002
	Public Const EventIdDebugGeneral As Integer = 9000
	Public Const EventIdDebugModeChanged As Integer = 9001
	Public Const EventIdDebugRegistryKVPNotFound As Integer = 9002
	Public Const EventIdEnumeratedVirtualAdapters As Integer = 9003
	Public Const EventIdDebugNewVirtualAdapter As Integer = 9004
	Public Const EventIdDebugChangedVirtualAdapter As Integer = 9005
	Public Const EventIdDebugNewVirtualAdapterFromUpdate As Integer = 9006
	Public Const EventIdDebugDeletedVirtualAdapter As Integer = 9007
	Public Const EventIdDebugInitiatedVMStart As Integer = 9008
	Public Const EventIdDebugInvalidMagicPacketFormat As Integer = 9009
	Public Const EventIdDebugMagicPacketReceived As Integer = 9010
	Public Const EventIdDebugEndingMagicPacketExclusionPeriod As Integer = 9011
	Public Const EventIdDebugVirtualizationJobReceived As Integer = 9012
#End Region ' Event Log Globals
End Module
