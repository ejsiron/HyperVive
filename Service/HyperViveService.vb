Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure
Imports System.Security.Principal

Public Module HyperViveEvents
	Public Class DebugMessageEventArgs
		Inherits EventArgs
		Public Property Message As String = String.Empty
	End Class

	Public Class ModuleExceptionEventArgs
		Inherits EventArgs

		Public Property ModuleName As String
		Public Property [Error] As Exception
	End Class
End Module

Public Class HyperViveService
	Private LocalCimSession As CimSession
	Private WithEvents DebugModeSettingReader As RegistryController
	Private WithEvents AdapterInventory As VMNetAdapterInventory
	Private WithEvents WOLListener As WakeOnLanListener
	Private WithEvents VMStarter As VMStartController

	Private ReadOnly Property ServiceRegistryPath As String
		Get
			Return String.Format("SYSTEM\CurrentControlSet\Services\{0}", ServiceName)
		End Get
	End Property

	Private DebugMode As Boolean = False

	Protected Overrides Sub OnStart(ByVal args() As String)
		AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf AppErrorReceived
		If New WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) Then
			LocalCimSession = CimSession.Create(Nothing)
			DebugModeSettingReader = New RegistryController(LocalCimSession) With {.RootRegistry = Microsoft.Win32.Registry.LocalMachine, .KeyPath = ServiceRegistryPath, .ValueName = "DebugMode"}
			UpdateDebugMode(Nothing, Nothing)
			DebugModeSettingReader.Start()
			AdapterInventory = New VMNetAdapterInventory(LocalCimSession, EventLog)
			VMStarter = New VMStartController(LocalCimSession)
			WOLListener = New WakeOnLanListener
			WOLListener.Start()
		Else
			EventLog.WriteEntry("Must run as an elevated user", EventLogEntryType.Error)
			Kill(5)
		End If
	End Sub

	Protected Overrides Sub OnStop()
		RemoveHandler AppDomain.CurrentDomain.UnhandledException, AddressOf AppErrorReceived
		WOLListener?.Dispose()
		DebugModeSettingReader.Stop()
		VMStarter = Nothing
		LocalCimSession?.Dispose()
	End Sub

	Private Sub UpdateDebugMode(ByVal sender As Object, ByVal e As RegistryValueChangedEventArgs) Handles DebugModeSettingReader.RegistryValueChanged
		Try
			DebugMode = CBool(DebugModeSettingReader.Value)
		Catch ex As Exception
			DebugMode = False
		End Try
		WriteDebugMessage(Me, New DebugMessageEventArgs With {.Message = String.Format("Debug mode set to {0}", DebugMode)})
	End Sub

	Private Sub AppErrorReceived(ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs)
		Dim UnknownError As Exception = CType(e.ExceptionObject, Exception)
		EventLog.WriteEntry(String.Format("Halting due to unexpected error ""{0}"" of type {1}", UnknownError.Message, UnknownError.GetType.FullName), EventLogEntryType.Error)
		Kill(CType(IIf(UnknownError.HResult = 0, -1, UnknownError.HResult), Integer))
		If TypeOf UnknownError Is IDisposable Then   ' CIM exceptions are disposable
			CType(UnknownError, IDisposable).Dispose()
		End If
	End Sub

	Private Sub ModuleErrorReceived(ByVal sender As Object, ByVal e As ModuleExceptionEventArgs) Handles DebugModeSettingReader.RegistryAccessError, WOLListener.ReceiverError
		EventLog.WriteEntry(String.Format("Unexpected error of type ""{0}"" in module ""{1}"": {2}", e.Error.GetType.FullName(), e.ModuleName, e.Error.Message), EventLogEntryType.Error)
	End Sub

	Private Sub MagicPacketReceived(ByVal sender As Object, ByVal e As WOLEvents.MagicPacketReceivedEventArgs) Handles WOLListener.MagicPacketReceived
		WriteDebugMessage(Me, New DebugMessageEventArgs With {.Message = String.Format("Received WOL frame from {0} for {1}", e.SenderIP.ToString, e.MacAddress)})
		Dim VmIDs As List(Of String) = AdapterInventory.GetVmIDFromMac(e.MacAddress)
		VMStarter.Start(e.MacAddress, VmIDs)
	End Sub

	Private Sub WriteDebugMessage(ByVal sender As Object, ByVal e As DebugMessageEventArgs) Handles WOLListener.DebugMessageGenerated, AdapterInventory.DebugMessageGenerated, VMStarter.VMStartDebugMessage, DebugModeSettingReader.RegistryDebugMessage
		If DebugMode Then
			EventLog.WriteEntry(String.Format("Debug: {0}", e.Message))
		End If
	End Sub

	Private Sub Kill(ByVal ExitCode As Integer)
		Me.ExitCode = ExitCode
		[Stop]()
	End Sub
End Class
