Partial Public Class ModuleController
	Private Const ServiceRegistryPathTemplate As String = "SYSTEM\CurrentControlSet\Services\{0}"
	Private ReadOnly Property ServiceRegistryPath As String
		Get
			Static CombinedPath As String = String.Format(ServiceRegistryPathTemplate, Service.ServiceName)
			Return CombinedPath
		End Get
	End Property
	Private ServiceRegistryRootPath As String
	Private DebugMode As Boolean = False
	Private DebugModeRegistryController As RegistryController

End Class
