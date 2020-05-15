Public Class HyperViveService
	Public ReadOnly Property ServiceRegistryRootPath As String
		Get
			Return String.Format(ServiceRegistryPathTemplate, ServiceName)
		End Get
	End Property

	Protected Overrides Sub OnStart(ByVal args() As String)
		ServiceModuleController = New ModuleController(Me)
		ServiceModuleController.Start()
	End Sub

	Protected Overrides Sub OnStop()
		ServiceModuleController.Stop()
		ServiceModuleController = Nothing
	End Sub

	Public Sub Kill(ByVal ExitCode As Integer)
		Me.ExitCode = ExitCode
		[Stop]()
	End Sub

	Private Const ServiceRegistryPathTemplate As String = "SYSTEM\CurrentControlSet\Services\{0}"
	Private ServiceModuleController As ModuleController
End Class
