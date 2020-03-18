Imports Microsoft.Management.Infrastructure
Public Class WOLService
	Private LocalCimSession As CimSession
	Private WithEvents AdapterInventory As VMNetAdapterInventory
	Private WithEvents WOLListener As WakeOnLanListener

	Protected Overrides Sub OnStart(ByVal args() As String)
		' Add code here to start your service. This method should set things
		' in motion so your service can do its work.
		LocalCimSession = CimSession.Create(Nothing)
		AdapterInventory = New VMNetAdapterInventory(LocalCimSession, EventLog)
		WOLListener = New WakeOnLanListener
		WOLListener.Start()
	End Sub

	Protected Overrides Sub OnStop()
		' Add code here to perform any tear-down necessary to stop your service.
		If WOLListener IsNot Nothing Then
			WOLListener.Dispose()
			WOLListener = Nothing
		End If
		LocalCimSession.Dispose()
	End Sub

	Private Sub MagicPacketReceived(ByVal SenderIP As String, ByVal MacAddress As String) Handles WOLListener.MagicPacketReceived
		EventLog.WriteEntry(String.Format("Received WOL packet from {0} for {1}", SenderIP, MacAddress), EventLogEntryType.Information)
	End Sub

	Private Sub ListenerCanceled() Handles WOLListener.OperationCanceled
		EventLog.WriteEntry("Listener canceled")
	End Sub

	Private Sub ReceiverException(ByVal ExceptionType As String) Handles WOLListener.ReceiverException
		EventLog.WriteEntry(String.Format("Listener error type: {0}", ExceptionType), EventLogEntryType.Error)
	End Sub

	Private Sub ReceiverDebugMessage(ByVal Message As String) Handles WOLListener.DebugMessage
		EventLog.WriteEntry(String.Format("Debug: {0}", Message))
	End Sub

End Class
