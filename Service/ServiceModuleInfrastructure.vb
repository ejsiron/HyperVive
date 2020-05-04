Imports Microsoft.Management.Infrastructure

''' <summary>
''' Use for modules that run full-time
''' </summary>
Public Interface IRunningModule
	Sub Start()

	ReadOnly Property IsRunning As Boolean

	Sub [Stop]()
End Interface

Public MustInherit Class ModuleBase
	Protected Sub New(ByVal ModuleLogController As IModuleLogger)
		GenericLogger = ModuleLogController
	End Sub
	Public MustOverride ReadOnly Property ModuleName As String

	Private GenericLogger As IModuleLogger

	Protected Sub ReportError(ByVal [Error] As Exception)
		GenericLogger.LogModuleError(ModuleName, [Error])
	End Sub

	Protected Sub ReportError(ByVal [Error] As CimException)
		GenericLogger.LogCimError([Error], ModuleName)
	End Sub

	Protected Sub ReportDebugMessage(ByVal Message As String)
		GenericLogger.LogDebugMessageGeneric(Message, ModuleName)
	End Sub
End Class

Public MustInherit Class ModuleWithCimBase
	Inherits ModuleBase

	Protected ReadOnly Session As CimSession

	Protected Sub New(ByVal Session As CimSession, ByVal ModuleLogController As IModuleLogger)
		MyBase.New(ModuleLogController)
		Me.Session = Session
	End Sub
End Class

Public Class ModuleController
	Public Shared Function Start(ByVal MainService As HyperViveService) As ModuleController
		If ControllerInstance Is Nothing Then
			ControllerInstance = New ModuleController(MainService)
		End If
		Return ControllerInstance
	End Function

	Public Shared Function IsRunning() As Boolean
		Return ControllerInstance IsNot Nothing
	End Function

	Public Sub [Stop]()
		If ControllerInstance IsNot Nothing Then
			ControllerInstance = Nothing
		End If
	End Sub

	Private Shared Service As HyperViveService
	Private Shared LocalCimSession As CimSession
	Private Shared ControllerInstance As ModuleController = Nothing

	Private Sub New(ByVal MainService As HyperViveService)
		Service = MainService
		LocalCimSession = CimSession.Create(Nothing)
	End Sub
End Class