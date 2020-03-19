Imports Microsoft.Management.Infrastructure
Imports System.Security.Principal

Public Module HyperViveEvents
	Public Class DebugMessageEventArgs
		Inherits EventArgs
		Public Property Message As String = String.Empty
	End Class
End Module

Public Class HyperViveService
	Private LocalCimSession As CimSession
	Private WithEvents AdapterInventory As VMNetAdapterInventory
	Private WithEvents WOLListener As WakeOnLanListener

	Protected Overrides Sub OnStart(ByVal args() As String)
		AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf AppErrorReceived
		If New WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) Then
			LocalCimSession = CimSession.Create(Nothing)
			AdapterInventory = New VMNetAdapterInventory(LocalCimSession, EventLog)
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
		LocalCimSession?.Dispose()
	End Sub

	Private Sub AppErrorReceived(ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs)
		Dim UnknownError As Exception = CType(e.ExceptionObject, Exception)
		EventLog.WriteEntry(String.Format("Halting due to unexpected error ""{0}"" of type {1}", UnknownError.Message, UnknownError.GetType.Name), EventLogEntryType.Error)
		Kill(CType(IIf(UnknownError.HResult = 0, -1, UnknownError.HResult), Integer))
		If TypeOf UnknownError Is IDisposable Then   ' CIM exceptions are disposable
			CType(UnknownError, IDisposable).Dispose()
		End If
	End Sub

	Private Sub MagicPacketReceived(ByVal sender As Object, ByVal e As WOLEvents.MagicPacketReceivedEventArgs) Handles WOLListener.MagicPacketReceived
		EventLog.WriteEntry(String.Format("Received WOL packet from {0} for {1}", e.SenderIP.ToString, e.MacAddress))
		Dim VmIDs As List(Of String) = AdapterInventory.GetVmIDFromMac(e.MacAddress)
		For Each ID As String In VmIDs
			EventLog.WriteEntry("Matches with VM ID {0}", ID)
		Next
	End Sub

	Private Sub ListenerCanceled() Handles WOLListener.OperationCanceled
		EventLog.WriteEntry("Wake-on-LAN listener canceled")
	End Sub

	Private Sub ReceiverException(ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs) Handles WOLListener.ReceiverException
		EventLog.WriteEntry("Wake-on-LAN listener encountered an unexpected error: {0}", CType(e.ExceptionObject, Exception).Message, EventLogEntryType.Error)
	End Sub

	Private Sub WriteDebugMessage(ByVal sender As Object, ByVal e As DebugMessageEventArgs) Handles WOLListener.DebugMessageGenerated
		EventLog.WriteEntry(String.Format("Debug: {0}", e.Message))
	End Sub

	Private Sub Kill(ByVal ExitCode As Integer)
		Me.ExitCode = ExitCode
		[Stop]()
	End Sub
End Class
