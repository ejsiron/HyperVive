Imports Microsoft.Management.Infrastructure
Imports Microsoft.Management.Infrastructure.Generic
Imports System.Threading

Public Module CustomCimEventArgs
	Public Class CimErrorEventArgs
		Inherits EventArgs
		Public Property ErrorInstance As CimException
	End Class

	Public Delegate Sub CimErrorEventHandler(ByVal sender As Object, ByVal e As CimErrorEventArgs)
End Module

Public Class CIMController
	Public Class CimErrorEventArgs
		Inherits EventArgs
		Sub New()

		End Sub
	End Class
	Public Event CimErrorOccurred As CimErrorEventHandler

	Public Const DefaultNamespace As String = "root/CIMV2"
	Public Property [Namespace] As String = DefaultNamespace

	Private LocalSession As CimSession

	Private Class CimObserver(Of T)
		Implements IObserver(Of T)

		Public Sub OnNext(value As T) Implements IObserver(Of T).OnNext
			Select Case value.GetType()
				Case GetType(CimInstance)
					'
				Case GetType(CimMethodResult)
					'
				Case GetType(CimMethodStreamedResult)
					'
				Case GetType(CimSubscriptionResult)
					'
				Case Else
					'
			End Select
		End Sub

		Public Sub OnError([error] As Exception) Implements IObserver(Of T).OnError
			Dim CimError As CimException = CType([error], CimException)

		End Sub

		Public Sub OnCompleted() Implements IObserver(Of T).OnCompleted
			'
		End Sub

	End Class
	Sub New(ByRef LocalCimSession As CimSession)
		LocalSession = LocalCimSession
	End Sub


End Class
