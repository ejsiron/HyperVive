Imports Microsoft.Management.Infrastructure
Imports System.Runtime.CompilerServices
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
	Public Sub New(ByVal MainService As HyperViveService)
		Service = MainService
	End Sub

	Public Sub Start()
		If Not IsRunning() Then
			LocalCimSession = CimSession.Create(Nothing)
			LogControllerInstance = LogController.GetControllerInstance(LocalCimSession, Service)
			AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnAppError
			If New WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) Then
				AdapterInventoryModule = New VMNetAdapterInventory(LocalCimSession, LogControllerInstance, LogControllerInstance)
				WOLListenerModule = New WakeOnLanListener(LogControllerInstance, LogControllerInstance, AddressOf ProcessMagicPacket)
				CheckpointWatcherModule = New CheckpointJobWatcher(LocalCimSession, LogControllerInstance, LogControllerInstance)
				WOLListenerModule.Start()
				CheckpointWatcherModule.Start()
				_IsRunning = True
			Else
				LogControllerInstance.LogElevationError()
				Service.Kill(5)
			End If
		End If
	End Sub

	Public ReadOnly Property IsRunning As Boolean = False

	Public Sub [Stop]()
		AdapterInventoryModule?.Dispose()
		AdapterInventoryModule = Nothing
		WOLListenerModule?.Dispose()
		CheckpointWatcherModule?.Dispose()
		LocalCimSession.Close()
		LocalCimSession.Dispose()
		If LogController.IsValid Then
			LogController.CloseAll()
		End If
		_IsRunning = False
	End Sub

	Private Service As HyperViveService
	Private LocalCimSession As CimSession
	Private LogControllerInstance As LogController
	Private AdapterInventoryModule As VMNetAdapterInventory
	Private WOLListenerModule As WakeOnLanListener
	Private CheckpointWatcherModule As CheckpointJobWatcher

	Private Sub ProcessMagicPacket(ByVal MAC As String, ByVal RequestorIP As String)
		Dim VmIDs As List(Of String) = AdapterInventoryModule.GetVmIDFromMac(MAC)
		Dim VMStarter As New VMStartController(LocalCimSession, LogControllerInstance, LogControllerInstance)
		VMStarter.StartVM(MAC, VmIDs, RequestorIP)
	End Sub

	Private Sub OnAppError(ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs)
		Dim UnknownError As Exception = CType(e.ExceptionObject, Exception)
		LogControllerInstance.LogApplicationHaltError(UnknownError)
		Service.Kill(CType(IIf(UnknownError.HResult = 0, -1, UnknownError.HResult), Integer))
		If TypeOf UnknownError Is IDisposable Then   ' CIM exceptions are disposable
			CType(UnknownError, IDisposable).Dispose()
		End If
	End Sub
End Class