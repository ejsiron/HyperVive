Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Public Class WakeOnLanListener
	Implements IDisposable

	Public Enum Ports As Integer
		DefaultWOL = 9
		LegacyWOL = 7
	End Enum

	Private Canceller As CancellationTokenSource = Nothing

	Public Event MagicPacketReceived(ByVal SenderIP As String, ByVal MacAddress As String)
	Public Event OperationCanceled()
	Public Event ReceiverException(ByVal ExceptionType As String)
	Public Event DebugMessage(ByVal Message As String)

	Public Async Sub Start(Optional ByVal DesiredPort As Integer = Ports.DefaultWOL)
		Dim CancelToken As CancellationToken
		Canceller = New CancellationTokenSource
		CancelToken = Canceller.Token
		Dim ListenerEndpoint As New IPEndPoint(IPAddress.Any, DesiredPort)
		Dim Listener As New UdpClient(ListenerEndpoint)
		Dim ReceivedData As UdpReceiveResult

		While Not CancelToken.IsCancellationRequested
			Try
				ReceivedData = Await Listener.ReceiveAsync().WithCancellation(CancelToken)
				ExtractMACFromMagicPacket(ReceivedData.Buffer, ReceivedData.RemoteEndPoint.Address.ToString)
			Catch opcancelex As OperationCanceledException
				RaiseEvent OperationCanceled()
			Catch ex As Exception
				RaiseEvent ReceiverException(TypeName(ex))
			End Try
		End While

		[Stop]()
		CancelToken = Nothing
	End Sub

	Public Sub [Stop]()
		If Canceller IsNot Nothing Then
			Canceller.Cancel()
			Canceller.Dispose()
			Canceller = Nothing
		End If
	End Sub

	Private Sub ExtractMACFromMagicPacket(ByRef DataBuffer As Byte(), ByRef SenderIP As String)
		' magic packet contents should be 102 bytes with an optional password
		If DataBuffer.Length < 102 Then
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
		RaiseEvent MagicPacketReceived(SenderIP, TargetMac)
	End Sub

#Region "IDisposable Support"
	Private disposedValue As Boolean ' To detect redundant calls

	' IDisposable
	Protected Overridable Sub Dispose(disposing As Boolean)
		If Not disposedValue Then
			If disposing Then
				[Stop]()
				' TODO: dispose managed state (managed objects).
			End If

			' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
			' TODO: set large fields to null.
		End If
		disposedValue = True
	End Sub

	' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
	'Protected Overrides Sub Finalize()
	'    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
	'    Dispose(False)
	'    MyBase.Finalize()
	'End Sub

	' This code added by Visual Basic to correctly implement the disposable pattern.
	Public Sub Dispose() Implements IDisposable.Dispose
		' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
		Dispose(True)
		' TODO: uncomment the following line if Finalize() is overridden above.
		' GC.SuppressFinalize(Me)
	End Sub
#End Region
End Class
