Module GlobalConstants
#Region "Cim Globals"
	Public Const CimNamespaceVirtualization As String = "root/virtualization/v2"
	Public Const CimInstanceCreationClassName As String = "CIM_InstCreation"
	Public Const CimInstanceModificationClassName As String = "CIM_InstModification"
	Public Const CimInstanceDeletionClassName As String = "CIM_InstDeletion"
	Public Const CimSelectRegistryValueChangeTemplate As String = "SELECT * FROM RegistryValueChangeEvent WHERE HIVE='{0}' AND KeyPath='{1}' AND ValueName='{2}'"
	Public Const CimSelectEventTimedTemplate As String = "SELECT * FROM {0} WITHIN {1} WHERE SourceInstance ISA '{2}'"
	Public Const CimClassNameSyntheticAdapterSettingData As String = "Msvm_SyntheticEthernetPortSettingData"
	Public Const CimClassNameEmulatedAdapterSettingData As String = "Msvm_EmulatedEthernetPortSettingData"
	Public Const CimPropertyNameInstanceID As String = "InstanceID"
	Public Const CimPropertyNameAddress As String = "Address"
	Public Const CimPropertyNameEnabledState As String = "EnabledState"
	Public Const CimMethodNameRequestStateChange As String = "RequestStateChange"
	Public Const CimParameterNameRequestedState As String = "RequestedState"
	Public Const CimParameterNameTimeoutPeriod As String = "TimeoutPeriod"
	Public Const CimMethodName As String = "RequestStateChange"
#End Region 'Cim Globals

#Region "Virtual Machine Globals"
	Public Enum VirtualMachineStates As UInt16
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
#End Region

#Region "Registry Globals"
	Public Const RegistryKeyName As String = "SYSTEM\CurrentControlSet\Services\HyperVive\"
	Public Const RegistryDebugKVPName As String = "DebugMode"
#End Region
End Module
