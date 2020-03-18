Imports Microsoft.Management.Infrastructure
Imports System.Threading

Namespace CIMitar

	''' <summary>
	''' Utility extensions for built-in Microsoft.Management.Infrastructure objects
	''' </summary>
	Public Module CimExtensions
		''' <summary>
		''' Retrieves the <see cref="CimInstance"/> that triggered a CIM_Indication or __InstanceOperationEvent from a <see cref="CimSubscriptionResult"/> object
		''' </summary>
		''' <param name="SubscriptionEvent"></param>
		''' <returns><see cref="CimInstance"/></returns>
		<Runtime.CompilerServices.Extension>
		Public Function GetSourceInstance(ByVal SubscriptionEvent As CimSubscriptionResult) As CimInstance
			Dim InstancePropertyName As String =
				IIf(SubscriptionEvent.Instance.CimSystemProperties.ClassName.Substring(0, 3) = "CIM", "SourceInstance", "TargetInstance").ToString
			Return CType((SubscriptionEvent.Instance.CimInstanceProperties(InstancePropertyName)?.Value), CimInstance)
		End Function

		''' <summary>
		''' Create a clone of a CimInstance.
		''' </summary>
		''' <param name="Instance">The <see cref="CimInstance"/> to clone</param>
		''' <returns><see cref="CimInstance"/></returns>
		<Runtime.CompilerServices.Extension>
		Public Function Clone(ByVal Instance As CimInstance) As CimInstance
			Return New CimInstance(Instance)
		End Function

		''' <summary>
		''' Disposes all items in a <see cref="List(Of CimInstance)"/> before removing them.
		''' </summary>
		''' <param name="InstanceList">A <see cref="List(Of CimInstance)"/> to clear.</param>
		<Runtime.CompilerServices.Extension>
		Public Sub ClearWithDispose(ByRef InstanceList As List(Of CimInstance))
			For Each Instance As CimInstance In InstanceList
				Instance?.Dispose()
			Next
		End Sub

		''' <summary>
		''' Adds "Dispose" to a <see cref="List(Of CimInstance)"/>. Disposes all items, then destroys the list.
		''' </summary>
		''' <param name="InstanceList">A <see cref="List(Of CimInstance)"/> to destroy.</param>
		<Runtime.CompilerServices.Extension>
		Public Sub Dispose(ByRef InstanceList As List(Of CimInstance))
			ClearWithDispose(InstanceList)
			InstanceList = Nothing
		End Sub

		''' <summary>
		''' Clones a list of <see cref="Microsoft.Management.Infrastructure.CimInstance">CimInstance</see>
		''' </summary>
		''' <param name="Instances">A <see cref="List(Of CimInstance)"/> to clone</param>
		''' <returns><see cref="List(Of CimInstance)"/></returns>
		<Runtime.CompilerServices.Extension>
		Public Function Clone(ByVal Instances As List(Of CimInstance)) As List(Of CimInstance)
			Dim NewList As New List(Of CimInstance)(Instances.Count)
			For Each Instance As CimInstance In Instances
				NewList.Add(Instance.Clone)
			Next
			Return NewList
		End Function

		''' <summary>
		''' Given a <see cref="CimClass"/> and a method name, determines if the method is static to that CIM class or must run on an instance.
		''' </summary>
		''' <param name="[Class]">The <see cref="CimClass"/> to check.</param>
		''' <param name="MethodName">The name of the method to check.</param>
		''' <returns>True if the method is static for the class, False if it requires an instance.</returns>
		<Runtime.CompilerServices.Extension>
		Public Function IsMethodStatic(ByVal [Class] As CimClass, ByVal MethodName As String) As Boolean
			If [Class] IsNot Nothing AndAlso Not String.IsNullOrEmpty(MethodName) Then
				For Each MethodDeclaration As CimMethodDeclaration In [Class].CimClassMethods
					If MethodDeclaration.Name = MethodName AndAlso MethodDeclaration.Qualifiers("static") IsNot Nothing Then
						Return True
					End If
				Next
			End If
			Return False
		End Function

		''' <summary>
		''' Extracts the string value from a CIM instance or system property
		''' </summary>
		''' <param name="Property">The <see cref="CimProperty"/> that contains the value to extract</param>
		''' <returns><see cref="String"/></returns>
		<Runtime.CompilerServices.Extension>
		Public Function GetValueString(ByVal [Property] As CimProperty) As String
			Dim StringValue As String = [Property]?.Value?.ToString
			Return IIf(String.IsNullOrEmpty(StringValue), String.Empty, StringValue).ToString
		End Function

		''' <summary>
		''' Shortcut method to extract the string value of a CIM instance property
		''' </summary>
		''' <param name="Instance"><see cref="CimInstance"/> that owns the property</param>
		''' <param name="PropertyName">Name of the desired property</param>
		''' <returns>The value of the property if present, otherwise an empty string.</returns>
		<Runtime.CompilerServices.Extension>
		Public Function GetInstancePropertyValueString(ByVal Instance As CimInstance, ByVal PropertyName As String) As String
			Return GetValueString(Instance.CimInstanceProperties(PropertyName))
		End Function
	End Module

	Public Module CustomCimEvents
		Public MustInherit Class CimEventArgs
			Inherits EventArgs
			Public Property Session As CimSession
		End Class
		Public Class CimErrorEventArgs
			Inherits CimEventArgs
			Public Property ErrorInstance As CimException
		End Class

		Public Delegate Sub CimErrorEventHandler(ByVal sender As Object, ByVal e As CimErrorEventArgs)

		Public Class CimInstancesReceivedArgs
			Inherits CimEventArgs
			Public Property Instances As List(Of CimInstance)
		End Class

		Public Delegate Sub CimInstancesReceivedHandler(ByVal sender As Object, ByVal e As CimInstancesReceivedArgs)

		Public Class CimResultReceivedArgs
			Inherits CimEventArgs
			Public Property Result As CimMethodResult
		End Class

		Public Delegate Sub CimResultReceivedHandler(ByVal sender As Object, ByVal e As CimResultReceivedArgs)

		Public Class CimSubscribedEventReceivedArgs
			Inherits CimEventArgs

			Public Property SubscribedEvent As CimSubscriptionResult
		End Class

		Public Delegate Sub CimSubscribedEventReceivedHandler(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs)

		Public Class CimActionCompletedArgs
			Inherits CimEventArgs
		End Class

		Public Delegate Sub CimActionCompletedHandler(ByVal sender As Object, ByVal e As CimActionCompletedArgs)
	End Module

	''' <summary>
	''' Base class for all CIM activities.
	''' </summary>
	''' <typeparam name="SubscriberType">The CIM object type to watch for asynchronous events.</typeparam>
	''' <typeparam name="ReturnType">The type that will return to asynchronous callers.</typeparam>
	Public MustInherit Class CimControllerBase(Of SubscriberType, ReturnType)
		Implements IDisposable

		Public Const DefaultNamespace As String = "root/CIMV2"

		''' <summary>
		''' The CIM namespace that an activity operates in.
		''' </summary>
		''' <remarks>The CIM error handlers work well with invalid namespaces, but not with null or empty namespaces. Always ensure that the field contains something.</remarks>
		''' <returns>The current CIM namespace of the activity.</returns>
		Public Property [Namespace] As String
			Get
				Return _Namespace
			End Get
			Set(value As String)
				If String.IsNullOrEmpty(value) Then
					_Namespace = DefaultNamespace
				Else
					_Namespace = value
				End If
			End Set
		End Property

		''' <summary>
		''' The most recent CIM error encountered by this activity. Null if no error has occurred.
		''' </summary>
		''' <returns><see cref="CimException"/> or null.</returns>
		Public Property LastError As CimException
			Protected Set(value As CimException)
				_LastError = value
			End Set
			Get
				Return _LastError
			End Get
		End Property

		Public Overridable Sub Cancel()
			CimCancellationSource?.Cancel()
			Subscriber?.Dispose()
		End Sub

		''' <summary>
		''' Creates a new CIM controller in the specified CIM session and namespace.
		''' </summary>
		''' <param name="Session">An existing CIM session to the local or a remote computer.</param>
		''' <param name="[Namespace]">The CIM namespace for the activity to operate in.</param>
		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			Me.Session = Session
			Me.Namespace = [Namespace]
		End Sub

		''' <param name="[Error]"></param>
		Protected Overridable Sub ReportError(ByVal [Error] As CimException)
			_LastError?.Dispose()
			_LastError = [Error]
		End Sub
		Protected MustOverride Sub ReportResult(ByVal Result As SubscriberType)
		Protected MustOverride Sub ReportCompletion()

		Protected Const QueryLanguage As String = "WQL"
		Protected Session As CimSession
		Protected AsyncOptions As New Options.CimOperationOptions

		''' <summary>
		''' An object that will receive events and responses from the infrastructure.
		''' </summary>
		Protected Subscriber As IDisposable

		Protected MustOverride Function InvokeOperation() As IObservable(Of SubscriberType)

		''' <summary>
		''' Allows child classes to perform additional cleanup tasks following operation cancellation
		''' </summary>
		Protected Overridable Sub CancellationCallback()
			' override in child classes
		End Sub

		''' <summary>
		''' Subscribes to receive the results of an async CIM operation
		''' </summary>
		Protected Sub StartSubscriber()
			Reset()
			CimCancellationSource = New CancellationTokenSource
			CimCancellationSource.Token.Register(Sub() CancellationCallback())
			AsyncOptions.CancellationToken = CimCancellationSource.Token
			Dim Observable As IObservable(Of SubscriberType) = InvokeOperation()
			Dim Observer As New CimObserver(Of SubscriberType)(AddressOf ReportError, AddressOf ReportResult, AddressOf ReportCompletion)
			Subscriber = Observable.Subscribe(Observer)
		End Sub

		''' <summary>
		''' Resets the object for disposal or to perform another operation. Outstanding operations will silently run to completion; use Cancel() first to stop them.
		''' </summary>
		''' <param name="CompleteClean">True for a full-teardown, false to set the object up for a new operation</param>
		Protected Overridable Sub Reset(Optional ByVal CompleteClean As Boolean = False)
			CimCancellationSource?.Dispose()
			Subscriber?.Dispose()
			Subscriber = Nothing
			_LastError?.Dispose()
			_LastError = Nothing
		End Sub

		Private _Namespace As String
		Private _LastError As CimException
		Private CimCancellationSource As Threading.CancellationTokenSource

		Protected Class CimObserver(Of ObserverType)
			Implements IObserver(Of ObserverType)

			Public Delegate Sub ReportErrorDelegate(ByVal [Error] As CimException)
			Public Delegate Sub ReportResultDelegate(ByVal Result As ObserverType)
			Public Delegate Sub ReportCompletionDelegate()

			Public ReportError As ReportErrorDelegate
			Public ReportResult As ReportResultDelegate
			Public ReportCompletion As ReportCompletionDelegate

			Public Sub OnNext(value As ObserverType) Implements IObserver(Of ObserverType).OnNext
				ReportResult?(CType(value, ObserverType))
			End Sub

			Public Sub OnError([error] As Exception) Implements IObserver(Of ObserverType).OnError
				ReportError?(CType([error], CimException))
			End Sub

			Public Sub OnCompleted() Implements IObserver(Of ObserverType).OnCompleted
				If ReportCompletion IsNot Nothing Then
					ReportCompletion()
				End If
			End Sub

			Sub New(
					 ByVal ReportErrorSub As ReportErrorDelegate,
					 ByVal ReportResultSub As ReportResultDelegate,
					 ByVal ReportCompletionSub As ReportCompletionDelegate
					 )
				ReportError = ReportErrorSub
				ReportResult = ReportResultSub
				ReportCompletion = ReportCompletionSub
			End Sub

		End Class

#Region "CimControllerBase IDisposable Support"
		Private disposedValue As Boolean
		Protected Overridable Sub Dispose(disposing As Boolean)
			If Not disposedValue Then
				If disposing Then
					Reset(True)
				End If
			End If
			disposedValue = True
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			Dispose(True)
		End Sub
#End Region
	End Class

	Public MustInherit Class CimAsyncController(Of SubscriberType, ReturnType)
		Inherits CimControllerBase(Of SubscriberType, ReturnType)

		Protected CimCompletionTaskSource As TaskCompletionSource(Of ReturnType)

		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			MyBase.New(Session, [Namespace])
		End Sub

		Public MustOverride Function StartAsync() As Task(Of ReturnType)

		Protected Overrides Sub CancellationCallback()
			CimCompletionTaskSource?.TrySetCanceled()
		End Sub

		Protected Overrides Sub ReportError(ByVal [Error] As CimException)
			MyBase.ReportError([Error])
			CimCompletionTaskSource?.TrySetException([Error])
		End Sub

		Protected Overrides Sub Reset(Optional CompleteClean As Boolean = False)
			CimCompletionTaskSource?.Task?.Dispose()
			CimCompletionTaskSource = Nothing
			If Not CompleteClean Then
				CimCompletionTaskSource = New TaskCompletionSource(Of ReturnType)
			End If
			MyBase.Reset(CompleteClean)
		End Sub
	End Class

	Public MustInherit Class CimAsyncInstanceController
		Inherits CimAsyncController(Of CimInstance, List(Of CimInstance))

		Private Instances As New List(Of CimInstance)

		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			MyBase.New(Session, [Namespace])
		End Sub

		Protected Overrides Sub ReportResult(Result As CimInstance)
			Instances.Add(Result)
		End Sub

		Protected Overrides Sub ReportCompletion()
			CimCompletionTaskSource?.TrySetResult(Instances)
		End Sub

		Protected Overrides Sub Reset(Optional CompleteClean As Boolean = False)
			Instances?.ClearWithDispose
			MyBase.Reset(CompleteClean)
		End Sub
	End Class

	Public Class CimAsyncEnumerateInstancesController
		Inherits CimAsyncInstanceController

		Public Property ClassName As String

		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace, Optional ByVal ClassName As String = "")
			MyBase.New(Session, [Namespace])
			Me.ClassName = ClassName
		End Sub

		Public Overrides Function StartAsync() As Task(Of List(Of CimInstance))
			StartSubscriber()
			Return CimCompletionTaskSource.Task
		End Function

		Protected Overrides Function InvokeOperation() As IObservable(Of CimInstance)
			Return Session.EnumerateInstancesAsync([Namespace], ClassName, AsyncOptions)
		End Function
	End Class

	Public Class CimAsyncQueryInstancesController
		Inherits CimAsyncInstanceController

		Public Property QueryText As String

		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace, Optional ByVal QueryText As String = "")
			MyBase.New(Session, [Namespace])
			Me.QueryText = QueryText
		End Sub

		Public Overrides Function StartAsync() As Task(Of List(Of CimInstance))
			StartSubscriber()
			Return CimCompletionTaskSource.Task
		End Function

		Protected Overrides Function InvokeOperation() As IObservable(Of CimInstance)
			Return Session.QueryInstancesAsync([Namespace], QueryLanguage, QueryText, AsyncOptions)
		End Function
	End Class

	Public MustInherit Class CimAsyncInvokeMethodControllerBase
		Inherits CimAsyncController(Of CimMethodResultBase, CimMethodResult)

		Private Result As CimMethodResultBase

		Public Property MethodName As String
		Public Property InputParameters As New CimMethodParametersCollection

		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			MyBase.New(Session, [Namespace])
		End Sub

		Public Shared Function InvokeMethodControllerFactory(ByVal Session As CimSession, ByVal Instance As CimInstance, ByVal MethodName As String) As CimAsyncInvokeMethodControllerBase
			If Instance Is Nothing OrElse String.IsNullOrEmpty(MethodName) Then
				Return Nothing
			End If
			If Instance.CimClass.IsMethodStatic(MethodName) Then
				Return New CimAsyncInvokeClassMethodController(Session, Instance.CimSystemProperties.Namespace) With {.ClassName = Instance.CimSystemProperties.ClassName, .MethodName = MethodName}
			Else
				Return New CimAsyncInvokeInstanceMethodController(Session, Instance.CimSystemProperties.Namespace) With {.Instance = Instance, .MethodName = MethodName}
			End If
		End Function

		Protected Overrides Sub ReportResult(Result As CimMethodResultBase)
			Me.Result = Result
		End Sub

		Protected Overrides Sub ReportCompletion()
			CimCompletionTaskSource.TrySetResult(TryCast(Result, CimMethodResult))
		End Sub

		Protected Overrides Sub Reset(Optional CompleteClean As Boolean = False)
			If Result IsNot Nothing AndAlso TypeOf Result Is IDisposable Then
				CType(Result, IDisposable).Dispose()
			End If
			MyBase.Reset(CompleteClean)
		End Sub
	End Class

	Public Class CimAsyncInvokeInstanceMethodController
		Inherits CimAsyncInvokeMethodControllerBase

		Public Property Instance As CimInstance

		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			MyBase.New(Session, [Namespace])
		End Sub

		Public Overrides Function StartAsync() As Task(Of CimMethodResult)
			StartSubscriber()
			Return CimCompletionTaskSource.Task
		End Function

		Protected Overrides Function InvokeOperation() As IObservable(Of CimMethodResultBase)
			Return Session.InvokeMethodAsync([Namespace], Instance, MethodName, InputParameters, AsyncOptions)
		End Function
	End Class

	Public Class CimAsyncInvokeClassMethodController
		Inherits CimAsyncInvokeMethodControllerBase

		Public Property ClassName As String

		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			MyBase.New(Session, [Namespace])
		End Sub

		Public Overrides Function StartAsync() As Task(Of CimMethodResult)
			StartSubscriber()
			Return CimCompletionTaskSource.Task
		End Function

		Protected Overrides Function InvokeOperation() As IObservable(Of CimMethodResultBase)
			Return Session.InvokeMethodAsync([Namespace], ClassName, MethodName, InputParameters)
		End Function
	End Class

	Public Class CimSubscriptionController
		Inherits CimControllerBase(Of CimSubscriptionResult, CimSubscriptionResult)

		Public Event ErrorOccurred As CimErrorEventHandler
		Public Event EventReceived As CimSubscribedEventReceivedHandler

		Public Property QueryText As String
		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			MyBase.New(Session, [Namespace])
		End Sub

		Public Sub Start()
			StartSubscriber()
		End Sub

		Protected Overrides Function InvokeOperation() As IObservable(Of CimSubscriptionResult)
			Return Session.SubscribeAsync([Namespace], QueryLanguage, QueryText)
		End Function

		Protected Overrides Sub ReportError([Error] As CimException)
			MyBase.ReportError([Error])
			RaiseEvent ErrorOccurred(Me, New CimErrorEventArgs With {.Session = Session, .ErrorInstance = [Error]})
		End Sub

		Protected Overrides Sub ReportResult(Result As CimSubscriptionResult)
			RaiseEvent EventReceived(Me, New CimSubscribedEventReceivedArgs With {.Session = Session, .SubscribedEvent = Result})
		End Sub

		Protected Overrides Sub ReportCompletion()
			' subscriptions do not report completion
		End Sub
	End Class

End Namespace