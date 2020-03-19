Module GlobalConstants
#Region "Cim Globals"
	Public Const CimNamespaceVirtualization As String = "root/virtualization/v2"
	Public Const CimInstanceCreationClassName As String = "CIM_InstCreation"
	Public Const CimInstanceModificationClassName As String = "CIM_InstModification"
	Public Const CimInstanceDeletionClassName As String = "CIM_InstDeletion"
	Public Const CimSelectRegistyChangeEventTemplate As String = "SELECT * FROM RegistryKeyChangeEvent WHERE HIVE='{0}' AND KeyPath='{1}'"
	Public Const CimSelectEventTimedTemplate As String = "SELECT * FROM {0} WITHIN {1} WHERE SourceInstance ISA '{2}'"
	Public Const CimClassNameSyntheticAdapterSettingData As String = "Msvm_SyntheticEthernetPortSettingData"
	Public Const CimClassNameEmulatedAdapterSettingData As String = "Msvm_EmulatedEthernetPortSettingData"
	Public Const CimPropertyNameInstanceID As String = "InstanceID"
	Public Const CimPropertyNameAddress As String = "Address"
#End Region 'Cim Globals

#Region "Registry Globals"
	Public Const RegistryKeyName As String = "SYSTEM\CurrentControlSet\Services\HyperVive\"
	Public Const RegistryDebugKVPName As String = "DebugMode"
#End Region
End Module
