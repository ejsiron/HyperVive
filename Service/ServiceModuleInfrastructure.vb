Public Interface IServiceModule
	Property ModuleName As String
	Property IsRunning As Boolean
End Interface

Public Class ModuleController
	Private ReadOnly Service As HyperViveService
	Public Sub New(ByRef OwningService As HyperViveService)
		Service = OwningService
	End Sub
End Class