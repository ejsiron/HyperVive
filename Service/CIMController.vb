Imports Microsoft.Management.Infrastructure
Imports Microsoft.Management.Infrastructure.Generic
Imports System.Threading

Public Module CustomCimEventArgs
	Public Class CimErrorEventArgs
		Inherits EventArgs
		Public Property ErrorInstance As CimException
	End Class

	Public Delegate Sub CimErrorEventHandler(ByVal sender As Object, ByVal e As CimErrorEventArgs)

	Public Class CimInstancesReceivedArgs
		Inherits EventArgs
		Public Property Instances As List(Of CimInstance)
	End Class

	Public Delegate Sub CimInstancesReceivedHandler(ByVal sender As Object, ByVal e As CimInstancesReceivedArgs)

	Public Class CimResultReceivedArgs
		Inherits EventArgs
		Public Property Result As CimMethodResult
	End Class

	Public Delegate Sub CimResultReceivedHandler(ByVal sender As Object, ByVal e As CimResultReceivedArgs)

	Public Class CimStreamedResultsReceivedArgs
		Inherits EventArgs
		Public Property Results As List(Of CimMethodStreamedResult)
	End Class

	Public Delegate Sub CimStreamedResultsReceivedHandler(ByVal sender As Object, ByVal e As CimStreamedResultsReceivedArgs)

	Public Class CimEmptyCompletionArgs
		Inherits EventArgs
	End Class

	Public Delegate Sub CimEmptyCompletionHandler(ByVal sender As Object, ByVal e As CimEmptyCompletionArgs)
End Module

Public Class CIMController
	Implements IDisposable

	Public Const DefaultNamespace As String = "root/CIMV2"
	Public Property [Namespace] As String = DefaultNamespace

	Private TargetSession As CimSession

	Private Class CimObserver(Of T)
		Implements IObserver(Of T)

		Public Delegate Sub ReportErrorDelegate(ByRef [Error] As CimException)
		Public Delegate Sub ReportInstancesDelegate(ByRef Instances As List(Of CimInstance))
		Public Delegate Sub ReportMethodResultDelegate(ByRef Result As CimMethodResult)
		Public Delegate Sub ReportMethodResultsDelegate(ByRef Results As List(Of CimMethodStreamedResult))
		Public Delegate Sub ReportEmptyCompletionDelegate()

		Public ReportError As ReportErrorDelegate
		Public ReportInstances As ReportInstancesDelegate
		Public ReportMethodResult As ReportMethodResultDelegate
		Public ReportMethodResults As ReportMethodResultsDelegate
		Public ReportEmptyCompletion As ReportEmptyCompletionDelegate

		Private ReceivedInstances As List(Of CimInstance)

		Private Shared Sub AddInstance(ByRef Observer As CimObserver(Of T), ByRef ReceivedInstance As CimInstance)
			If Observer.ReceivedInstances Is Nothing Then
				Observer.ReceivedInstances = New List(Of CimInstance)
			End If
			Observer.ReceivedInstances.Add(ReceivedInstance)
		End Sub

		Private ReceivedResults As List(Of CimMethodStreamedResult)

		Private Shared Sub AddResult(ByRef Observer As CimObserver(Of T), ByRef ReceivedResult As CimMethodStreamedResult)
			If Observer.ReceivedResults Is Nothing Then
				Observer.ReceivedResults = New List(Of CimMethodStreamedResult)
			End If
			Observer.ReceivedResults.Add(ReceivedResult)
		End Sub

		Public Sub OnNext(value As T) Implements IObserver(Of T).OnNext
			Select Case value.GetType()
				Case GetType(CimInstance)
					Dim ReceivedInstance As CimInstance = TryCast(value, CimInstance)
					If ReceivedInstance IsNot Nothing Then
						AddInstance(Me, ReceivedInstance)
					End If
				Case GetType(CimMethodResult)
					If ReportMethodResult IsNot Nothing Then
						Dim ReceivedResult As CimMethodResult = TryCast(value, CimMethodResult)
						If ReceivedResult IsNot Nothing Then
							ReportMethodResult(ReceivedResult)
						End If
					End If
				Case GetType(CimMethodStreamedResult)
					Dim ReceivedResult As CimMethodStreamedResult = TryCast(value, CimMethodStreamedResult)
					If ReceivedResult IsNot Nothing Then
						AddResult(Me, ReceivedResult)
					End If
				Case GetType(CimSubscriptionResult)
					Dim ReceivedResult As CimSubscriptionResult = TryCast(value, CimSubscriptionResult)
					If ReceivedResult IsNot Nothing Then
						AddInstance(Me, ReceivedResult.Instance)
					End If
					OnCompleted()  ' subscription returns one instances and does not trigger oncompleted
				Case Else
					' should never receive any other types, TODO: raise error?
			End Select
		End Sub

		Public Sub OnError([error] As Exception) Implements IObserver(Of T).OnError
			If ReportError IsNot Nothing Then
				ReportError(CType([error], CimException))
			End If
		End Sub

		Public Sub OnCompleted() Implements IObserver(Of T).OnCompleted
			If ReceivedInstances IsNot Nothing AndAlso ReportInstances IsNot Nothing Then
				ReportInstances(ReceivedInstances)
			ElseIf ReceivedResults IsNot Nothing AndAlso ReportMethodResults IsNot Nothing Then
				ReportMethodResults(ReceivedResults)
			ElseIf ReportEmptyCompletion IsNot Nothing Then
				ReportEmptyCompletion()
			End If
		End Sub

		Sub New(
				 Optional ByRef ReportErrorFunction As ReportErrorDelegate = Nothing,
				 Optional ByRef ReportInstancesFunction As ReportInstancesDelegate = Nothing,
				 Optional ByRef ReportMethodResultFunction As ReportMethodResultDelegate = Nothing,
				 Optional ByRef ReportMethodResultsFunction As ReportMethodResultsDelegate = Nothing
				 )
			ReportError = ReportErrorFunction
			ReportInstances = ReportInstancesFunction
			ReportMethodResult = ReportMethodResultFunction
			ReportMethodResults = ReportMethodResultsFunction
		End Sub

	End Class

	Public Event CimErrorOccurred As CimErrorEventHandler
	Public Event CimInstancesReceived As CimInstancesReceivedHandler
	Public Event CimResultReceived As CimResultReceivedHandler
	Public Event CimStreamedResultsReceived As CimStreamedResultsReceivedHandler
	Public Event CimEmptyCompletion As CimEmptyCompletionHandler

	Private LastCimError As CimException = Nothing
	Private Instances As List(Of CimInstance) = Nothing
	Private Result As CimMethodResult = Nothing
	Private Results As List(Of CimMethodStreamedResult) = Nothing

	Sub New(ByRef TargetCimSession As CimSession, Optional ByVal CimNamespace As String = DefaultNamespace)
		TargetSession = TargetCimSession
		[Namespace] = CimNamespace
	End Sub

	Private Subscriber As IDisposable

	Private Sub ClearLastOperation()
		If Subscriber IsNot Nothing Then
			Subscriber.Dispose()
		End If
		If LastCimError IsNot Nothing Then
			LastCimError.Dispose()
			LastCimError = Nothing
		End If
		If Instances IsNot Nothing Then
			For Each Instance As CimInstance In Instances
				Instance.Dispose()
			Next
			Instances.Clear()
			Instances = Nothing
		End If
		If Result IsNot Nothing Then
			Result.Dispose()
			Result = Nothing
		End If
		If Results IsNot Nothing Then
			Results.Clear()
			Results = Nothing
		End If
	End Sub

	Private Sub ReportError(ByRef [Error] As CimException)
		RaiseEvent CimErrorOccurred(Me, New CimErrorEventArgs With {.ErrorInstance = [Error]})
	End Sub

	Private Sub ReportInstances(ByRef Instances As List(Of CimInstance))
		RaiseEvent CimInstancesReceived(Me, New CimInstancesReceivedArgs With {.Instances = Instances})
	End Sub

	Private Sub ReportResult(ByRef Result As CimMethodResult)
		RaiseEvent CimResultReceived(Me, New CimResultReceivedArgs With {.Result = Result})
	End Sub

	Private Sub ReportResults(ByRef Results As List(Of CimMethodStreamedResult))
		RaiseEvent CimStreamedResultsReceived(Me, New CimStreamedResultsReceivedArgs With {.Results = Results})
	End Sub

	Public Sub GetAllInstancesAsync(ByVal ClassName As String)
		ClearLastOperation()
		Dim InstanceObserver As New CimObserver(Of CimInstance)(ReportErrorFunction:=AddressOf ReportError, ReportInstancesFunction:=AddressOf ReportInstances)
		Dim InstanceObservable As CimAsyncMultipleResults(Of CimInstance) = TargetSession.EnumerateInstancesAsync([Namespace], ClassName)
		Subscriber = InstanceObservable.Subscribe(InstanceObserver)
	End Sub

	Public Sub InvokeMethodAsync(ByRef Instance As CimInstance, ByVal MethodName As String, Optional ByRef MethodParameters As CimMethodParametersCollection = Nothing)
		Dim IsStatic As Boolean = False
		ClearLastOperation()
		If MethodParameters Is Nothing Then
			MethodParameters = New CimMethodParametersCollection
		End If
		For Each MethodDeclaration As CimMethodDeclaration In Instance.CimClass.CimClassMethods
			If MethodDeclaration.Name = MethodName Then
				IsStatic = MethodDeclaration.Qualifiers("static") IsNot Nothing
			End If
		Next
		Dim MethodObserver As New CimObserver(Of CimMethodResultBase)(ReportErrorFunction:=AddressOf ReportError, ReportMethodResultFunction:=AddressOf ReportResult, ReportMethodResultsFunction:=AddressOf ReportResults)
		Dim MethodObservable As CimAsyncResult(Of CimMethodResult)
		If IsStatic Then
			MethodObservable = TargetSession.InvokeMethodAsync([Namespace], Instance.CimClass.CimSystemProperties.ClassName, MethodName, MethodParameters)
		Else
			MethodObservable = TargetSession.InvokeMethodAsync([Namespace], Instance, MethodName, MethodParameters)
		End If
		Subscriber = MethodObservable.Subscribe(MethodObserver)
	End Sub

	Public Sub InvokeMethodAsync(ByRef Instance As CimInstance, ByVal MethodName As String, ByRef MethodParameters As Dictionary(Of String, Object))
		Dim Parameters As New CimMethodParametersCollection
		For Each InputParameter As KeyValuePair(Of String, Object) In MethodParameters
			Parameters.Add(CimMethodParameter.Create(InputParameter.Key, InputParameter.Value, 0))
		Next
		InvokeMethodAsync(Instance, MethodName, Parameters)
	End Sub

#Region "IDisposable Support"
	Private disposedValue As Boolean

	Protected Overridable Sub Dispose(disposing As Boolean)
		If Not disposedValue Then
			If disposing Then
				ClearLastOperation()
			End If
		End If
		disposedValue = True
	End Sub
	Public Sub Dispose() Implements IDisposable.Dispose
		Dispose(True)
	End Sub
#End Region

End Class
