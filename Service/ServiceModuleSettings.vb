Partial Public Class ModuleController
	Private Const ServiceRegistryPathTemplate As String = "SYSTEM\CurrentControlSet\Services\{0}"
	Private ServiceRegistryRootPath As String
	Private DebugMode As Boolean = False
	Private DebugModeRegistryController As RegistryController

End Class
