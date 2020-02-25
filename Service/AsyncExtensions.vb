Imports System.Runtime.CompilerServices
Imports System.Threading

' converted from https://stackoverflow.com/questions/19404199/how-to-to-make-udpclient-receiveasync-cancelable
Module AsyncExtensions
	<Extension>
	Public Async Function WithCancellation(Of T)(ByVal ThisTask As Task(Of T), ByVal ct As CancellationToken) As Task(Of T)
		Dim tcs As New TaskCompletionSource(Of Boolean)
		Using ct.Register(Sub(s As Object) CType(s, TaskCompletionSource(Of Boolean)).TrySetResult(True), tcs)
			If ThisTask IsNot Await Task.WhenAny(New Task() {ThisTask, tcs.Task}) Then
				Throw New OperationCanceledException(ct)
			End If
		End Using
		Return ThisTask.Result
	End Function
End Module
