Public Class HyperViveService
	Public ReadOnly Property ServiceRegistryRootPath As String = String.Format(ServiceRegistryPathTemplate, ServiceName)

	Protected Overrides Sub OnStart(ByVal args() As String)
		ServiceModuleController = New ModuleController(Me)
	End Sub

	Protected Overrides Sub OnStop()
		LogController.CloseAll()
	End Sub

	Public Sub Kill(ByVal ExitCode As Integer)
		Me.ExitCode = ExitCode
		[Stop]()
	End Sub

	Private Const ServiceRegistryPathTemplate As String = "SYSTEM\CurrentControlSet\Services\{0}"
	Private ServiceModuleController As ModuleController
End Class
