Imports System.Runtime.CompilerServices
Imports System.Threading

' converted from https://stackoverflow.com/questions/19404199/how-to-to-make-udpclient-receiveasync-cancelable
Module AsyncExtensions
	''' <summary>
	''' Adds token-based cancellation capability to a task
	''' </summary>
	''' <typeparam name="T">Task return type</typeparam>
	''' <param name="ThisTask">Source task</param>
	''' <param name="Token">Cancellation token that will signal the task</param>
	''' <returns></returns>
	<Extension>
	Public Async Function WithCancellation(Of T)(ByVal ThisTask As Task(Of T), ByVal Token As CancellationToken) As Task(Of T)
		Dim CompletionSource As New TaskCompletionSource(Of Boolean)
		Using Token.Register(Sub(s As Object) CType(s, TaskCompletionSource(Of Boolean)).TrySetResult(True), CompletionSource)
			If ThisTask IsNot Await Task.WhenAny(New Task() {ThisTask, CompletionSource.Task}) Then
				Throw New OperationCanceledException(Token)
			End If
		End Using
		Return ThisTask.Result
	End Function
End Module
