Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Public Class WakeOnLanListener
	Implements IDisposable

	Public Enum Ports As Integer
		DefaultWOL = 9
		LegacyWOL = 7
	End Enum

	Private Const MACWatchTimeoutSeconds As Integer = 5

	Private Canceller As CancellationTokenSource = Nothing

	Public Event MagicPacketReceived(ByVal SenderIP As String, ByVal MacAddress As String)
	Public Event OperationCanceled()
	Public Event ReceiverException(ByVal ExceptionType As String)
	Public Event DebugMessage(ByVal Message As String)

	Private Shared RecentMACs As List(Of String)

	Public Async Sub Start(Optional ByVal DesiredPort As Integer = Ports.DefaultWOL)
		RecentMACs = New List(Of String)
		Dim CancelToken As CancellationToken
		Canceller = New CancellationTokenSource
		CancelToken = Canceller.Token
		Dim ListenerEndpoint As New IPEndPoint(IPAddress.Any, DesiredPort)
		Dim Listener As New UdpClient(ListenerEndpoint)
		Dim ReceivedData As UdpReceiveResult
		Dim ReceivedMac As String = String.Empty

		While Not CancelToken.IsCancellationRequested
			Try
				ReceivedData = Await Listener.ReceiveAsync().WithCancellation(CancelToken)
			Catch opcancelex As OperationCanceledException
				RaiseEvent OperationCanceled()
			Catch ex As Exception
				RaiseEvent ReceiverException(TypeName(ex))
			End Try
			ExtractMACFromMagicPacket(ReceivedData.Buffer, ReceivedMac)
			If Not String.IsNullOrEmpty(ReceivedMac) Then
				ProcessReceivedMAC(ReceivedData.RemoteEndPoint.Address.ToString, ReceivedMac)
			End If
		End While
	End Sub

	Public Sub [Stop]()
		If Canceller IsNot Nothing Then
			Canceller.Cancel()
			Canceller.Dispose()
			Canceller = Nothing
		End If
		RecentMACs.Clear()
		RecentMACs = Nothing
	End Sub

	Private Sub ExtractMACFromMagicPacket(ByRef DataBuffer As Byte(), ByRef ReceivedMAC As String)
		ReceivedMAC = String.Empty
		' magic packet contents should be 102 bytes with an optional password
		If DataBuffer Is Nothing OrElse DataBuffer.Length < 102 Then
			Return
		End If

		' first six bytes must be 0xFF
		For Each i As Byte In DataBuffer.Take(6)
			If i <> &HFF Then
				Return
			End If
		Next

		' next, the target system's MAC (6 bytes) is repeated 16 times
		Dim TargetMac As String = String.Empty
		For InitialMacPosition As Integer = 6 To 11 ' starting one past the initial 6 bytes, looking at next 6 bytes
			For MirroredMacOffset As Integer = 1 To 15 ' verify that the same char appears 15 more times, in 6 byte jumps
				If DataBuffer(InitialMacPosition) <> DataBuffer(InitialMacPosition + (6 * MirroredMacOffset)) Then
					Return
				End If
			Next
			TargetMac += DataBuffer(InitialMacPosition).ToString("X2")
		Next

		' can only reach this point if the MAC was repeated 16 times
		ReceivedMAC = TargetMac
	End Sub

	Private Sub ProcessReceivedMAC(ByVal SenderIP As String, ByVal MAC As String)
		If RecentMACs?.Contains(MAC) Then
			Return
		End If
		RaiseEvent MagicPacketReceived(SenderIP, MAC)

		' when binding to IPAny, will receive the same MAC at least twice (once on each IP that receives the broadcast, once on the loopback)
		' also, a VM cannot fully start instantly -- pointless to process the same MAC too rapidly, so good to ignore repeats for a time
		' Add the MAC to a watch list and remove it after a countdown
		RecentMACs.Add(MAC)
		Task.Run(Sub()
						Thread.Sleep(MACWatchTimeoutSeconds * 1000)
						RecentMACs?.Remove(MAC)
					End Sub)
	End Sub

#Region "IDisposable Support"
	Private disposedValue As Boolean ' To detect redundant calls

	' IDisposable
	Protected Overridable Sub Dispose(disposing As Boolean)
		If Not disposedValue Then
			If disposing Then
				[Stop]()
			End If
		End If
		disposedValue = True
	End Sub

	' This code added by Visual Basic to correctly implement the disposable pattern.
	Public Sub Dispose() Implements IDisposable.Dispose
		Dispose(True)
	End Sub
#End Region
End Class
