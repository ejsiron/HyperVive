Imports Microsoft.Management.Infrastructure
Imports System.Security.Principal

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
		If Not IsRunning() Then
			LocalCimSession = CimSession.Create(Nothing)
			LogControllerInstance = LogController.GetControllerInstance(LocalCimSession, MainService)
			AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnAppError
			If New WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) Then
				ModuleControllerInstance = New ModuleController(MainService)
				AdapterInventoryModule = New VMNetAdapterInventory(LocalCimSession, LogControllerInstance, LogControllerInstance)
				VMStarterModule = New VMStartController(LocalCimSession, LogControllerInstance, LogControllerInstance)
				WOLListenerModule = New WakeOnLanListener(LogControllerInstance, LogControllerInstance)
				WOLListenerModule.Start()
				CheckpointWatcherModule = New CheckpointJobWatcher(LocalCimSession, LogControllerInstance, LogControllerInstance)
				CheckpointWatcherModule.Start()
			Else
				LogControllerInstance.LogElevationError()
				Service.Kill(5)
			End If
		End If
		Service = MainService
		Return ModuleControllerInstance
	End Function

	Public Shared Function IsRunning() As Boolean
		Return ModuleControllerInstance IsNot Nothing
	End Function

	Public Shared Sub [Stop]()
		LocalCimSession.Close()
		LocalCimSession.Dispose()
		If ModuleControllerInstance IsNot Nothing Then
			ModuleControllerInstance = Nothing
		End If
		If LogController.IsValid Then
			LogController.CloseAll()
		End If
	End Sub

	Private Shared Service As HyperViveService
	Private Shared LocalCimSession As CimSession
	Private Shared ModuleControllerInstance As ModuleController = Nothing
	Private Shared LogControllerInstance As LogController
	Private Shared AdapterInventoryModule As VMNetAdapterInventory
	Private Shared VMStarterModule As VMStartController
	Private Shared WOLListenerModule As WakeOnLanListener
	Private Shared CheckpointWatcherModule As CheckpointJobWatcher

	Private Sub New(ByVal MainService As HyperViveService)
		Service = MainService
		LocalCimSession = CimSession.Create(Nothing)
	End Sub

	Private Shared Sub OnAppError(ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs)
		Dim UnknownError As Exception = CType(e.ExceptionObject, Exception)
		LogControllerInstance.LogApplicationHaltError(UnknownError)
		Service.Kill(CType(IIf(UnknownError.HResult = 0, -1, UnknownError.HResult), Integer))
		If TypeOf UnknownError Is IDisposable Then   ' CIM exceptions are disposable
			CType(UnknownError, IDisposable).Dispose()
		End If
	End Sub
End Class