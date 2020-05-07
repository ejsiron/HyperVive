Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

''' <summary>
''' Listens for Wake-On-LAN frames.
''' </summary>
Public Class WakeOnLanListener
	Inherits ModuleBase
	Implements IRunningModule
	Implements IDisposable

	Public Enum Ports As Integer
		DefaultWOL = 9
		LegacyWOL = 7
	End Enum

	Public Sub New(ByVal ModuleLogger As IModuleLogger, ByVal MagicPacketLogger As IMagicPacketLogger)
		MyBase.New(ModuleLogger)
		Me.MagicPacketLogger = MagicPacketLogger
	End Sub

	''' <summary>
	''' Starts the WOL listener
	''' </summary>
	Public Async Sub Start() Implements IRunningModule.Start
		RecentMACs = New List(Of String)
		Dim CancelToken As CancellationToken
		Canceller = New CancellationTokenSource
		CancelToken = Canceller.Token
		Dim ListenerEndpoint As New IPEndPoint(IPAddress.Any, Port)
		Dim Listener As New UdpClient(ListenerEndpoint)
		Dim ReceivedData As UdpReceiveResult
		Dim ReceivedMac As String

		_IsRunning = True
		While Not CancelToken.IsCancellationRequested
			Try
				ReceivedData = Await Listener.ReceiveAsync().WithCancellation(CancelToken)
			Catch opcancelex As OperationCanceledException
				' report? this is part of normal operation
				Exit While
			Catch ex As Exception
				ReportError(ex)
				Continue While
			End Try
			ReceivedMac = ExtractMACFromMagicPacket(ReceivedData.Buffer)
			If Not String.IsNullOrEmpty(ReceivedMac) Then
				ProcessReceivedMAC(ReceivedMac, ReceivedData.RemoteEndPoint.Address)
			End If
		End While
		_IsRunning = False
	End Sub

	Public ReadOnly Property IsRunning As Boolean Implements IRunningModule.IsRunning

	''' <summary>
	''' Stops the WOL listener
	''' </summary>
	Public Sub [Stop]() Implements IRunningModule.Stop
		If Canceller IsNot Nothing Then
			Canceller.Cancel()
			Canceller.Dispose()
			Canceller = Nothing
		End If
		RecentMACs.Clear()
		RecentMACs = Nothing
	End Sub

	Public Overrides ReadOnly Property ModuleName As String = "Wake-On-LAN Listener"
	Public Property Port As Ports = Ports.DefaultWOL
	Private Const MACWatchTimeoutSeconds As Integer = 5

	Private MagicPacketLogger As IMagicPacketLogger
	Private Canceller As CancellationTokenSource = Nothing
	Private ListLock As New Object
	Private RecentMACs As List(Of String)

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
					MagicPacketLogger.LogDebugMagicPacketInvalidFormat()
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
				MagicPacketLogger.LogDebugMagicPacketDuplicate(MAC)
				Return
			End If

			MagicPacketLogger.LogMagicPacketProcessed(MAC, SenderIP.ToString)

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
						MagicPacketLogger.LogDebugMagicPacketExclusionEnded(MAC)
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
