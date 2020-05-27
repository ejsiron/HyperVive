Module GlobalConstants
#Region "Cim Globals" ' CIM-related strings with wide application, but not widespread enough for CIMitar
	Public Const CimNamespaceRootDefault As String = "root/DEFAULT"

	Public Const CimClassNameSyntheticAdapterSettingData As String = "Msvm_SyntheticEthernetPortSettingData"
	Public Const CimClassNameEmulatedAdapterSettingData As String = "Msvm_EmulatedEthernetPortSettingData"

	Public Const CimMethodNameRequestStateChange As String = "RequestStateChange"

	Public Const CimParameterNameRequestedState As String = "RequestedState"
	Public Const CimParameterNameTimeoutPeriod As String = "TimeoutPeriod"
	Public Const CimParameterNameJob As String = "Job"

	Public Const CimQueryTemplateRegistryValueChange As String = "SELECT * FROM RegistryValueChangeEvent WHERE HIVE='{0}' AND KeyPath='{1}' AND ValueName='{2}'"
#End Region 'Cim Globals
End Module
