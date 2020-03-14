Module GlobalConstants
	Public Const CimNamespaceVirtualization As String = "root/virtualization/v2"
	Public Const CimInstanceCreationClassName As String = "CIM_InstCreation"
	Public Const CimInstanceModificationClassName As String = "CIM_InstModification"
	Public Const CimInstanceDeletionClassName As String = "CIM_InstDeletion"
	Public Const CimSelectEventTemplate As String = "SELECT * FROM {0} WITHIN {1} WHERE SourceInstance ISA '{2}'"
	Public Const CimClassNameSyntheticAdapter As String = "Msvm_SyntheticEthernetPort"
	Public Const CimClassNameEmulatedAdapter As String = "Msvm_EmulatedEthernetPort"
	Public Const CimPropertyNameDeviceID As String = "DeviceID"
	Public Const CimPropertyNamePermanentAddress As String = "PermanentAddress"
	Public Const CimPropertyNameSystemName As String = "SystemName"
End Module
