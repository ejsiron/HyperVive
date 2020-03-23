Module GlobalConstants
#Region "Cim Globals"
	Public Const CimNamespaceVirtualization As String = "root/virtualization/v2"

	Public Const CimClassNameSyntheticAdapterSettingData As String = "Msvm_SyntheticEthernetPortSettingData"
	Public Const CimClassNameEmulatedAdapterSettingData As String = "Msvm_EmulatedEthernetPortSettingData"
	Public Const CimClassNameInstanceCreation As String = "CIM_InstCreation"
	Public Const CimClassNameInstanceModification As String = "CIM_InstModification"
	Public Const CimClassNameInstanceDeletion As String = "CIM_InstDeletion"

	Public Const CimMethodNameRequestStateChange As String = "RequestStateChange"

	Public Const CimParameterNameRequestedState As String = "RequestedState"
	Public Const CimParameterNameTimeoutPeriod As String = "TimeoutPeriod"
	Public Const CimParameterNameJob As String = "Job"

	Public Const CimPropertyNameInstanceID As String = "InstanceID"
	Public Const CimPropertyNameAddress As String = "Address"
	Public Const CimPropertyNameEnabledState As String = "EnabledState"

	Public Const CimQueryTemplateRegistryValueChange As String = "SELECT * FROM RegistryValueChangeEvent WHERE HIVE='{0}' AND KeyPath='{1}' AND ValueName='{2}'"
	Public Const CimQueryTemplateTimedEvent As String = "SELECT * FROM {0} WITHIN {1} WHERE SourceInstance ISA '{2}'"

#End Region 'Cim Globals

#Region "Virtualization Globals"
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

	Public Enum VirtualizationMethodErrors As UInt16
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
End Module
