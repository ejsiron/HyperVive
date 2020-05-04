Imports Microsoft.Win32

Partial Public Class ModuleController
	Private Const ServiceRegistryPathTemplate As String = "SYSTEM\CurrentControlSet\Services\{0}"
	Private Const SettingNameDebugMode As String = "DebugMode"
	Private ReadOnly Property ServiceRegistryPath As String
		Get
			Static CombinedPath As String = String.Format(ServiceRegistryPathTemplate, Service.ServiceName)
			Return CombinedPath
		End Get
	End Property
	Private ServiceRegistryRootPath As String
	Private DebugMode As Boolean = False
	Private DebugModeRegistryController As RegistryController

	Private Sub StartDebugModeSettingsController()
		DebugModeRegistryController = New RegistryController(LocalCimSession) With
		{
			.RootRegistry = Microsoft.Win32.Registry.LocalMachine,
			.KeySubPath = ServiceRegistryPath,
			.ValueName = SettingNameDebugMode
		}

	End Sub
End Class
