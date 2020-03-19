Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Public Module WOLEvents
	Public Class MagicPacketReceivedEventArgs
		Inherits EventArgs

		Public Property MacAddress As String
		Public Property SenderIP As IPAddress
	End Class
End Module

Public Class WakeOnLanListener
	Implements IDisposable

	Public Enum Ports As Integer
		DefaultWOL = 9
		LegacyWOL = 7
	End Enum

	Private Const MACWatchTimeoutSeconds As Integer = 5

	Private Canceller As CancellationTokenSource = Nothing

	Public Event MagicPacketReceived(ByVal sender As Object, ByVal e As MagicPacketReceivedEventArgs)
	Public Event OperationCanceled(ByVal sender As Object, ByVal e As EventArgs)
	Public Event ReceiverException(ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs)
	Public Event DebugMessageGenerated(ByVal sender As Object, ByVal e As DebugMessageEventArgs)

	Private ListLock As Object
	Private RecentMACs As List(Of String)

	Public Async Sub Start(Optional ByVal DesiredPort As Integer = Ports.DefaultWOL)
		ListLock = New Object
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
				RaiseEvent OperationCanceled(Me, New EventArgs)
				Return
			Catch ex As Exception
				RaiseEvent ReceiverException(Me, New UnhandledExceptionEventArgs(ex, False))
				Continue While
			End Try
			ReceivedMac = ExtractMACFromMagicPacket(ReceivedData.Buffer)
			If Not String.IsNullOrEmpty(ReceivedMac) Then
				ProcessReceivedMAC(ReceivedMac, ReceivedData.RemoteEndPoint.Address)
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
		ListLock = Nothing
	End Sub

	Private Function ExtractMACFromMagicPacket(ByRef DataBuffer As Byte()) As String
		' magic packet contents should be 102 bytes with an optional password
		If DataBuffer Is Nothing OrElse DataBuffer.Length < 102 Then
			Return String.Empty
		End If

		' first six bytes must be 0xFF
		For Each i As Byte In DataBuffer.Take(6)
			If i <> &HFF Then
				Return String.Empty
			End If
		Next

		' next, the target system's MAC (6 bytes) is repeated 16 times
		Dim TargetMac As String = String.Empty
		For InitialMacPosition As Integer = 6 To 11 ' starting one past the initial 6 bytes, looking at next 6 bytes
			For MirroredMacOffset As Integer = 1 To 15 ' verify that the same char appears 15 more times, in 6 byte jumps
				If DataBuffer(InitialMacPosition) <> DataBuffer(InitialMacPosition + (6 * MirroredMacOffset)) Then
					RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {.Message = String.Format("Magic packet received with an invalid format")})
					Return String.Empty
				End If
			Next
			TargetMac += DataBuffer(InitialMacPosition).ToString("X2")
		Next

		' can only reach this point if the MAC was repeated 16 times
		Return TargetMac
	End Function

	Private Sub ProcessReceivedMAC(ByVal MAC As String, ByVal SenderIP As IPAddress)
		SyncLock ListLock
			If RecentMACs?.Contains(MAC) Then
				RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {.Message = String.Format("Received duplicate request for MAC {0}", MAC)})
				Return
			End If

			RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {.Message = String.Format("Received new request for MAC {0}", MAC)})

			RaiseEvent MagicPacketReceived(Me, New MagicPacketReceivedEventArgs With {.MacAddress = MAC, .SenderIP = SenderIP})

			' when binding to IPAny, will receive the same MAC at least twice (once on each IP that receives the broadcast, once on the loopback)
			' also, a VM cannot fully start instantly -- pointless to process the same MAC too rapidly, so good to ignore repeats for a time
			' Add the MAC to a watch list and remove it after a countdown
			RecentMACs.Add(MAC)
		End SyncLock
		Task.Run(Sub()
						Thread.Sleep(MACWatchTimeoutSeconds * 1000)
						SyncLock ListLock
							RecentMACs?.Remove(MAC)
						End SyncLock
						RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {.Message = String.Format("End exclusion period for MAC {0}", MAC)})
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
