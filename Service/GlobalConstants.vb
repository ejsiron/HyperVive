Module GlobalConstants
#Region "Cim Globals"
	Public Const CimNamespaceVirtualization As String = "root/virtualization/v2"
	Public Const CimNamespaceRootDefault As String = "root/DEFAULT"

	Public Const CimClassNameSyntheticAdapterSettingData As String = "Msvm_SyntheticEthernetPortSettingData"
	Public Const CimClassNameEmulatedAdapterSettingData As String = "Msvm_EmulatedEthernetPortSettingData"
	Public Const CimClassNameInstanceCreation As String = "CIM_InstCreation"
	Public Const CimClassNameInstanceModification As String = "CIM_InstModification"
	Public Const CimClassNameInstanceDeletion As String = "CIM_InstDeletion"
	Public Const CimClassNameVirtualizationJob As String = "Msvm_ConcreteJob"

	Public Const CimMethodNameRequestStateChange As String = "RequestStateChange"

	Public Const CimParameterNameRequestedState As String = "RequestedState"
	Public Const CimParameterNameTimeoutPeriod As String = "TimeoutPeriod"
	Public Const CimParameterNameJob As String = "Job"

	Public Const CimPropertyNameInstanceID As String = "InstanceID"
	Public Const CimPropertyNameAddress As String = "Address"
	Public Const CimPropertyNameEnabledState As String = "EnabledState"
	Public Const CimPropertyNameJobState As String = "JobState"
	Public Const CimPropertyNameJobStatus As String = "JobStatus"
	Public Const CimPropertyNameErrorCode As String = "ErrorCode"
	Public Const CimPropertyNameName As String = "Name"
	Public Const CimPropertyNameElementName As String = "ElementName"

	Public Const CimQueryTemplateRegistryValueChange As String = "SELECT * FROM RegistryValueChangeEvent WHERE HIVE='{0}' AND KeyPath='{1}' AND ValueName='{2}'"
	Public Const CimQueryTemplateTimedEvent As String = "SELECT * FROM {0} WITHIN {1} WHERE SourceInstance ISA '{2}'"
	Public Const CimQueryTemplateMsvmConcreteJob As String = "SELECT * FROM Msvm_ConcreteJob WHERE InstanceID='{0}'"
	Public Const CimQueryTemplateVirtualMachine As String = "SELECT * FROM Msvm_ComputerSystem {0}"

	Public Enum JobStates As UShort
		[New] = 2
		Starting = 3
		Running = 4
		ShuttingDown = 6
		Completed = 7
		Terminated = 8
		Killed = 9
		Exception = 10
		Service = 11
	End Enum
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
End Module
