' This file is part of the CIMitar project, a module to ease interaction with the CIM module
' CIMitar, Copyright 2020 Eric Siron
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
' The above copyright notice and this permission notice shall be included in all copies or substantial portions
' of the Software.
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
' THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
' TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
' SOFTWARE.

Imports System.Threading
Imports Microsoft.Management.Infrastructure

Namespace CIMitar
	Public Module Strings
		Public Const DefaultNamespace As String = "root/CIMV2"
		Public Const CimClassNameInstanceCreation As String = "CIM_InstCreation"
		Public Const WmiClassNameInstanceCreation As String = "__InstanceCreationEvent"
		Public Const CimClassNameInstanceModification As String = "CIM_InstModification"
		Public Const WmiClassNameInstanceModication As String = "__InstanceModificationEvent"
		Public Const CimClassNameInstanceDeletion As String = "CIM_InstDeletion"
		Public Const WmiClassNameInstanceDeletion As String = "__InstanceDeletionEvent"
		Public Const CimSubscriptionInstanceSelector As String = "SourceInstance"
		Public Const WmiSubscriptionInstanceSelector As String = "TargetInstance"
		Public Const QueryTemplateTimedEvent As String = "SELECT * FROM {0} WITHIN {1} WHERE {2} ISA '{3}'"
		Public Const PropertyNameName As String = "Name"
	End Module

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
				IIf(SubscriptionEvent.Instance.CimSystemProperties.ClassName.Substring(0, 3) = "CIM", CimSubscriptionInstanceSelector, WmiSubscriptionInstanceSelector).ToString
			Return CType((SubscriptionEvent.Instance.CimInstanceProperties(InstancePropertyName)?.Value), CimInstance)
		End Function

		''' <summary>
		''' Synchronously create a clone of a CimInstance.
		''' </summary>
		''' <remarks>Use to separate an instance from the object that generated. The cloned instance will
		''' have its own lifetime. You are responsible for disposing clones.</remarks>
		''' <param name="Instance">The <see cref="CimInstance"/> to clone</param>
		''' <returns><see cref="CimInstance"/></returns>
		<Runtime.CompilerServices.Extension>
		Public Function Clone(ByVal Instance As CimInstance) As CimInstance
			Return New CimInstance(Instance)
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
		''' Refreshes a <see cref="CimInstance"/>
		''' </summary>
		''' <param name="Instance">The <see cref="CimInstance"/> to refresh</param>
		''' <param name="OwningSession">The <see cref="CimSession"/> that owns the instance</param>
		<Runtime.CompilerServices.Extension>
		Public Sub Refresh(ByRef Instance As CimInstance, ByVal OwningSession As CimSession)
			Dim ReplacementInstance As CimInstance = OwningSession.GetInstance(Instance.CimSystemProperties.Namespace, Instance)
			Instance.Dispose()
			Instance = ReplacementInstance
		End Sub

		''' <summary>
		''' Extracts the string value from a CIM instance or system property
		''' </summary>
		''' <param name="Property">The <see cref="CimProperty"/> that contains the value to extract</param>
		''' <returns><see cref="String"/></returns>
		<Runtime.CompilerServices.Extension>
		Public Function [String](ByVal [Property] As CimProperty) As String
			Dim StringValue As String = [Property]?.Value?.ToString
			Return IIf(String.IsNullOrEmpty(StringValue), String.Empty, StringValue).ToString
		End Function

		''' <summary>
		''' Shortcut method to extract the string value of a CIM instance property
		''' </summary>
		''' <param name="Instance"><see cref="CimInstance"/> that owns the property</param>
		''' <param name="Name">Name of the desired property</param>
		''' <returns>The value of the property if present, otherwise an empty string.</returns>
		<Runtime.CompilerServices.Extension>
		Public Function InstancePropertyString(ByVal Instance As CimInstance, ByVal Name As String) As String
			Return [String](ExtractInstanceProperty(Instance, Name))
		End Function

		Private Function ExtractValue(Of T)(ByRef [Property] As CimProperty, ByVal ExpectedType As CimType, ByRef DefaultValue As T) As T
			If [Property] IsNot Nothing AndAlso [Property].Value IsNot Nothing AndAlso [Property].CimType = ExpectedType Then
				Return CType([Property].Value, T)
			Else
				' TODO: this will crash the app if someone tries to use an incompatible type
				' for now, consider that the price they pay for trying to retrieve a value without knowing its type
				Dim TargetType As Type = GetCimPropertyManagedType([Property].CimType)
				Return CType(GetCimDefaultValue(CTypeDynamic([Property].Value, TargetType)), T)
			End If
		End Function

		Private Function ExtractArray(Of T)(ByVal [Property] As CimProperty, ByVal ExpectedType As CimType) As List(Of T)
			Return ExtractValue([Property], ExpectedType, New T(0) {}).ToList
		End Function

		''' <summary>
		''' Extracts the 16-bit unsigned integer value from a CIM instance or system property
		''' </summary>
		''' <param name="[Property]">The <see cref="CimProperty"/> that contains the value to extract</param>
		''' <returns>The <see cref="UShort"/> value if conversion possible, 0 otherwise.</returns>
		<Runtime.CompilerServices.Extension>
		Public Function UInt16(ByVal [Property] As CimProperty) As UShort
			Return ExtractValue([Property], CimType.UInt16, 0US)
		End Function

		<Runtime.CompilerServices.Extension>
		Public Function UInt16Array(ByVal [Property] As CimProperty) As List(Of UShort)
			Return ExtractArray(Of UShort)([Property], CimType.UInt16Array)
		End Function

		Private Function ExtractInstanceProperty(ByRef Instance As CimInstance, ByVal Name As String) As CimProperty
			Return Instance.CimInstanceProperties.Where(Function(SearchProperty As CimProperty) SearchProperty.Name = Name).FirstOrDefault
		End Function

		''' <summary>
		''' Shortcut method to extract the 16-bit unsigned integer value of a CIM instance property
		''' </summary>
		''' <param name="Instance"><see cref="CimInstance"/> that owns the property</param>
		''' <param name="Name">Name of the desired property</param>
		''' <returns></returns>
		<Runtime.CompilerServices.Extension>
		Public Function InstancePropertyUInt16(ByVal Instance As CimInstance, ByVal Name As String) As UShort
			Return UInt16(ExtractInstanceProperty(Instance, Name))
		End Function

		<Runtime.CompilerServices.Extension>
		Public Function InstancePropertyUInt16Array(ByVal Instance As CimInstance, ByVal Name As String) As List(Of UShort)
			Return UInt16Array(ExtractInstanceProperty(Instance, Name))
		End Function

		<Runtime.CompilerServices.Extension>
		Public Function ValueEquals(ByVal x As CimProperty, ByVal y As CimProperty) As Boolean
			If x.GetManagedType = y.GetManagedType Then
				Select Case x.CimType

				End Select
			End If
			Return False
		End Function

		<Runtime.CompilerServices.Extension>
		Public Function GetManagedType(ByRef [Property] As CimProperty) As Type
			Return GetCimPropertyManagedType([Property].CimType)
		End Function


	End Module

	''' <summary>
	''' Utility class to Dispose-enable a <see cref="List(Of CimInstance)"/>
	''' </summary>
	Public Class CimInstanceCollection
		Inherits List(Of CimInstance)
		Implements IDisposable

		''' <summary>
		''' Disposes all instances, then clears the list as normal
		''' </summary>
		Public Overloads Sub Clear()
			For Each Instance As CimInstance In Me
				Instance?.Dispose()
			Next
			MyBase.Clear()
		End Sub

		''' <summary>
		''' Creates clones of all <see cref="CimInstance"/> items in a <see cref="CimInstanceCollection"/> into a new <see cref="CimInstanceCollection"/>
		''' </summary>
		''' <returns><see cref="CimInstanceCollection"/></returns>
		Public Function Clone() As CimInstanceCollection
			Dim ClonedList As New CimInstanceCollection
			For Each Instance As CimInstance In Me
				If Instance IsNot Nothing Then
					ClonedList.Add(Instance.Clone)
				End If
			Next
			Return ClonedList
		End Function

#Region "CimInstanceList IDisposable Support"

		Private disposedValue As Boolean

		Protected Overridable Sub Dispose(disposing As Boolean)
			If Not disposedValue Then
				If disposing Then
					Clear()
				End If
			End If
			disposedValue = True
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			Dispose(True)
		End Sub

#End Region

	End Class

	''' <summary>
	''' Base class for all CIM activities.
	''' </summary>
	''' <typeparam name="SubscriberType">The CIM object type to watch for asynchronous events.</typeparam>
	''' <typeparam name="ReturnType">The type that will return to asynchronous callers.</typeparam>
	Public MustInherit Class CimOperatorBase(Of SubscriberType, ReturnType)
		Implements IDisposable

		''' <summary>
		''' The CIM namespace that an activity operates in.
		''' </summary>
		''' <remarks>The CIM error handlers work well with invalid namespaces, but not with null or empty namespaces. Always ensure that the field contains something.</remarks>
		''' <returns>The current CIM namespace of the activity.</returns>
		Public Overridable Property [Namespace] As String
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
		''' Adjusts a CIM operation to only impact key properties
		''' </summary>
		''' <returns></returns>
		Public Property KeysOnly As Boolean = False

		''' <summary>
		''' Indicates if an operation is running
		''' </summary>
		''' <returns><see cref="Boolean"/></returns>
		Public ReadOnly Property IsRunning As Boolean
			Get
				Return CimCancellationSource IsNot Nothing
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
		Protected Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			Me.Session = Session
			Me.Namespace = [Namespace]
		End Sub

		''' <summary>
		''' Relays errors reported by the operation subscriber to the owning operation object
		''' </summary>
		''' <param name="[Error]"><see cref="CimException"/></param>
		Protected MustOverride Sub ReportError(ByVal [Error] As CimException)

		''' <summary>
		''' Relays results from the operation subscriber to the owning operation object
		''' </summary>
		''' <param name="Result">An object of the owning operation's object type</param>
		Protected MustOverride Sub ReportResult(ByVal Result As SubscriberType)

		''' <summary>
		''' Relays operation completion from the subscriber to the owning object
		''' </summary>
		Protected MustOverride Sub ReportCompletion()

		Protected Const QueryLanguage As String = "WQL"
		Protected Session As CimSession
		Protected AsyncOptions As New Options.CimOperationOptions

		''' <summary>
		''' An object that will receive operation events and responses from the infrastructure.
		''' </summary>
		Protected Subscriber As IDisposable

		Protected MustOverride Function InvokeOperation() As IObservable(Of SubscriberType)

		''' <summary>
		''' Allows child classes to perform additional cleanup tasks following operation cancellation
		''' </summary>
		Protected Overridable Sub CancellationCallback()
			' override in child classes that use it
		End Sub

		''' <summary>
		''' Subscribes to receive the results of an async CIM operation
		''' </summary>
		Protected Sub StartSubscriber()
			Reset()
			CimCancellationSource = New CancellationTokenSource
			CimCancellationSource.Token.Register(Sub() CancellationCallback())
			AsyncOptions.CancellationToken = CimCancellationSource.Token
			AsyncOptions.KeysOnly = KeysOnly
			Dim Observable As IObservable(Of SubscriberType) = InvokeOperation()
			Dim Observer As New CimObserver(Of SubscriberType)(AddressOf RelayObserverError, AddressOf ReportResult, AddressOf RelayObserverCompletion)
			Subscriber = Observable.Subscribe(Observer)
		End Sub

		''' <summary>
		''' Resets the object for disposal or to perform another operation. Outstanding operations will silently run to completion; use Cancel() first to stop them.
		''' </summary>
		''' <param name="CompleteClean">Used by Dispose to completely tear down the object.</param>
		''' <remarks>The object is not guaranteed to be usable if CompleteClean is set outside of Dispose()</remarks>
		Protected Overridable Sub Reset(Optional ByVal CompleteClean As Boolean = False)
			CimCancellationSource?.Cancel()
			CimCancellationSource?.Dispose()
			CimCancellationSource = Nothing
			Subscriber?.Dispose()
			Subscriber = Nothing
			If CompleteClean Then
				AsyncOptions.Dispose()
			End If
		End Sub

		Private _Namespace As String
		Private CimCancellationSource As CancellationTokenSource

		Private Sub RelayObserverError(ByVal [Error] As CimException)
			Subscriber?.Dispose()
			Subscriber = Nothing
			ReportError([Error])
		End Sub

		Private Sub RelayObserverCompletion()
			CimCancellationSource?.Dispose()
			CimCancellationSource = Nothing
			ReportCompletion()
		End Sub

		Protected Class CimObserver(Of ObserverType)
			Implements IObserver(Of ObserverType)

			Private ReadOnly ReportError As Action(Of CimException)
			Private ReadOnly ReportResult As Action(Of ObserverType)
			Private ReadOnly ReportCompletion As Action

			Public Sub OnNext(value As ObserverType) Implements IObserver(Of ObserverType).OnNext
				ReportResult(value)
			End Sub

			Public Sub OnError([error] As Exception) Implements IObserver(Of ObserverType).OnError
				ReportError(CType([error], CimException))
			End Sub

			Public Sub OnCompleted() Implements IObserver(Of ObserverType).OnCompleted
				If ReportCompletion IsNot Nothing Then
					ReportCompletion()
				End If
			End Sub

			Sub New(
					 ByVal ReportErrorCallback As Action(Of CimException),
					 ByVal ReportResultCallback As Action(Of ObserverType),
					 ByVal ReportCompletionCallback As Action
					 )
				ReportError = ReportErrorCallback
				ReportResult = ReportResultCallback
				ReportCompletion = ReportCompletionCallback
			End Sub

		End Class

#Region "CimOperatorBase IDisposable Support"

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

	''' <summary>
	''' Base object to encapsulate CIM operation
	''' </summary>
	''' <typeparam name="SubscriberType">The CIM type the system operation works on</typeparam>
	''' <typeparam name="ReturnType">The type output by the operation</typeparam>
	Public MustInherit Class CimAsyncOperator(Of SubscriberType, ReturnType)
		Inherits CimOperatorBase(Of SubscriberType, ReturnType)

		Protected CimCompletionTaskSource As TaskCompletionSource(Of ReturnType)

		''' <summary>
		''' Creates a new operation in the indicated <see cref="CimSession"/> and CIM namespace
		''' </summary>
		''' <param name="Session">The active <see cref="CimSession"/> that will conduct the operation</param>
		''' <param name="[Namespace]">The CIM namespace for the operation. Defaults to "root/CIMv2"</param>
		Protected Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			MyBase.New(Session, [Namespace])
		End Sub

		Public Overridable Function StartAsync() As Task(Of ReturnType)
			StartSubscriber()
			Return CimCompletionTaskSource.Task
		End Function

		Protected Overrides Sub CancellationCallback()
			CimCompletionTaskSource?.TrySetCanceled()
		End Sub

		Protected Overrides Sub ReportError(ByVal [Error] As CimException)
			CimCompletionTaskSource?.TrySetException([Error])
		End Sub

		Protected Overrides Sub Reset(Optional CompleteClean As Boolean = False)
			If Not CimCompletionTaskSource?.Task.IsCompleted Then
				CancellationCallback()
			End If
			CimCompletionTaskSource?.Task?.Dispose()
			CimCompletionTaskSource = Nothing
			If Not CompleteClean Then
				CimCompletionTaskSource = New TaskCompletionSource(Of ReturnType)
			End If
			MyBase.Reset(CompleteClean)
		End Sub

	End Class

	''' <summary>
	''' Asynchronously refreshes a <see cref="CimInstance"/>
	''' </summary>
	Public Class CimAsyncRefreshInstanceOperator
		Inherits CimAsyncOperator(Of CimInstance, CimInstance)

		''' <summary>
		''' The instance to refresh
		''' </summary>
		''' <returns><see cref="CimInstance"/></returns>
		Public Property Instance As CimInstance

		Public Overrides Property [Namespace] As String
			Get
				Return IIf(Instance Is Nothing, DefaultNamespace, Instance.CimSystemProperties.Namespace).ToString
			End Get
			Set(value As String)
				'
			End Set
		End Property

		''' <summary>
		''' Creates a new asynchronous <see cref="CimInstance"/> refresher
		''' </summary>
		''' <param name="Session">An existing CIM session to the local or a remote computer.</param>
		''' <param name="Instance">The <see cref="CimInstance"/> to refresh.</param>
		Sub New(ByVal Session As CimSession, Optional ByVal Instance As CimInstance = Nothing)
			MyBase.New(Session)
			Me.Instance = Instance
		End Sub

		Protected Overrides Sub ReportResult(Result As CimInstance)
			Instance?.Dispose()
			Instance = Result
		End Sub

		Protected Overrides Sub ReportCompletion()
			CimCompletionTaskSource?.TrySetResult(Instance)
		End Sub

		Protected Overrides Function InvokeOperation() As IObservable(Of CimInstance)
			Return Session.GetInstanceAsync(Instance.CimSystemProperties.Namespace, Instance)
		End Function

	End Class

	''' <summary>
	''' A base class for CIM operations that return <see cref="CimInstanceCollection"/>
	''' </summary>
	Public MustInherit Class CimAsyncInstancesController
		Inherits CimAsyncOperator(Of CimInstance, CimInstanceCollection)

		Private ReadOnly Instances As New CimInstanceCollection

		Protected Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			MyBase.New(Session, [Namespace])
		End Sub

		''' <summary>
		''' Refreshes all instances in <see cref="Instances"/>
		''' </summary>
		Public Async Function RefreshAsync() As Task(Of CimInstanceCollection)
			If Instances IsNot Nothing AndAlso Instances.Count > 0 Then
				Dim RefreshController As New CimAsyncRefreshInstanceOperator(Session)
				For i As Integer = 0 To Instances.Count - 1
					RefreshController.Instance = Instances(i)
					Instances(i) = Await RefreshController.StartAsync
				Next
			End If
			Return Instances
		End Function

		Protected Overrides Sub ReportResult(Result As CimInstance)
			Instances.Add(Result)
		End Sub

		Protected Overrides Sub ReportCompletion()
			CimCompletionTaskSource?.TrySetResult(Instances)
		End Sub

		Protected Overrides Sub Reset(Optional CompleteClean As Boolean = False)
			Instances?.Clear()
			MyBase.Reset(CompleteClean)
		End Sub

	End Class

	''' <summary>
	''' Retrieves all instances of a <see cref="CimClass"/>
	''' </summary>
	Public Class CimAsyncEnumerateInstancesController
		Inherits CimAsyncInstancesController

		''' <summary>
		''' Name of the <see cref="CimClass"/> to enumerate
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property ClassName As String

		''' <summary>
		''' Creates a new <see cref="CimInstanceCollection"/> of all instances of the specified CIM class
		''' </summary>
		''' <param name="Session">An existing CIM session to the local or a remote computer.</param>
		''' <param name="[Namespace]">The CIM namespace for the activity to operate in.</param>
		''' <param name="ClassName">The name of the <see cref="CimClass"/> to enumerate.</param>
		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace, Optional ByVal ClassName As String = "")
			MyBase.New(Session, [Namespace])
			Me.ClassName = ClassName
		End Sub

		Protected Overrides Function InvokeOperation() As IObservable(Of CimInstance)
			Return Session.EnumerateInstancesAsync([Namespace], ClassName, AsyncOptions)
		End Function

	End Class

	''' <summary>
	''' Executes a WQL query and returns a <see cref="CimInstanceCollection"/> with the results
	''' </summary>
	Public Class CimAsyncQueryInstancesController
		Inherits CimAsyncInstancesController

		''' <summary>
		''' Complete text of the WQL query to execute
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property QueryText As String

		''' <summary>
		''' Creates a new CIM query object
		''' </summary>
		''' <param name="Session">An existing CIM session to the local or a remote computer.</param>
		''' <param name="[Namespace]">The CIM namespace for the activity to operate in.</param>
		''' <param name="QueryText">Complete text of the query to execute</param>
		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace, Optional ByVal QueryText As String = "")
			MyBase.New(Session, [Namespace])
			Me.QueryText = QueryText
		End Sub

		Protected Overrides Function InvokeOperation() As IObservable(Of CimInstance)
			Return Session.QueryInstancesAsync([Namespace], QueryLanguage, QueryText, AsyncOptions)
		End Function

	End Class

	Public Class AsyncAssociatedInstancesController
		Inherits CimAsyncInstancesController

		Public Property SourceInstance As CimInstance
		Public Property ResultClass As String = String.Empty
		Public Property AssociationClassName As String = String.Empty

		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			MyBase.New(Session, [Namespace])
		End Sub

		Protected Overrides Function InvokeOperation() As IObservable(Of CimInstance)
			Return Session.EnumerateAssociatedInstancesAsync([Namespace], SourceInstance, AssociationClassName, ResultClass, Nothing, Nothing, AsyncOptions)
		End Function

	End Class

	''' <summary>
	''' Base class for CIM method invocation objects
	''' </summary>
	Public MustInherit Class CimAsyncInvokeMethodControllerBase
		Inherits CimAsyncOperator(Of CimMethodResultBase, CimMethodResult)

		Private Result As CimMethodResultBase

		''' <summary>
		''' Name of the CIM method to execute
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property MethodName As String

		''' <summary>
		''' The input parameters to supply to the method
		''' </summary>
		''' <returns><see cref="CimMethodParametersCollection"/></returns>
		''' <remarks>Do not include output parameters.</remarks>
		Public Property InputParameters As New CimMethodParametersCollection

		''' <summary>
		''' Creates a new CIM method executor
		''' </summary>
		''' <param name="Session">An existing CIM session to the local or a remote computer.</param>
		''' <param name="[Namespace]">The CIM namespace for the activity to operate in.</param>
		Protected Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			MyBase.New(Session, [Namespace])
		End Sub

		''' <summary>
		''' Creates a <see cref="CimAsyncInvokeClassMethodController"/> or <see cref="CimAsyncInvokeInstanceMethodController"/> by detecting if the specified method is an instance method or a class method.
		''' </summary>
		''' <param name="Session">An existing CIM session to the local or a remote computer.</param>
		''' <param name="Instance">A <see cref="CimInstance"/> that contains the desired method</param>
		''' <param name="MethodName">Name of the method to execute</param>
		''' <returns><see cref="CimAsyncInvokeClassMethodController"/> or <see cref="CimAsyncInvokeInstanceMethodController"/></returns>
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

	''' <summary>
	''' Executes a CIM instance method
	''' </summary>
	Public Class CimAsyncInvokeInstanceMethodController
		Inherits CimAsyncInvokeMethodControllerBase

		''' <summary>
		''' The <see cref="CimInstance"/> that contains the method to execute
		''' </summary>
		''' <returns><see cref="CimInstance"/></returns>
		Public Property Instance As CimInstance

		''' <summary>
		''' Creates a new CIM instance method operator
		''' </summary>
		''' <param name="Session">An existing CIM session to the local or a remote computer.</param>
		''' <param name="[Namespace]">The CIM namespace for the activity to operate in.</param>
		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			MyBase.New(Session, [Namespace])
		End Sub

		Protected Overrides Function InvokeOperation() As IObservable(Of CimMethodResultBase)
			Return Session.InvokeMethodAsync([Namespace], Instance, MethodName, InputParameters, AsyncOptions)
		End Function

	End Class

	''' <summary>
	''' Executes a CIM class method
	''' </summary>
	Public Class CimAsyncInvokeClassMethodController
		Inherits CimAsyncInvokeMethodControllerBase

		''' <summary>
		''' The name of the <see cref="CimClass"/> that contains the method to execute
		''' </summary>
		''' <returns></returns>
		Public Property ClassName As String

		''' <summary>
		''' Creates a new CIM class method operator
		''' </summary>
		''' <param name="Session">An existing CIM session to the local or a remote computer.</param>
		''' <param name="[Namespace]">The CIM namespace for the activity to operate in.</param>
		Public Sub New(ByVal Session As CimSession, Optional ByVal [Namespace] As String = DefaultNamespace)
			MyBase.New(Session, [Namespace])
		End Sub

		Protected Overrides Function InvokeOperation() As IObservable(Of CimMethodResultBase)
			Return Session.InvokeMethodAsync([Namespace], ClassName, MethodName, InputParameters, AsyncOptions)
		End Function

	End Class

	''' <summary>
	''' Subscribes to CIM events
	''' </summary>
	Public Class CimSubscriptionController
		Inherits CimOperatorBase(Of CimSubscriptionResult, CimSubscriptionResult)

		Private ReadOnly ResultCallback As Action(Of CimSubscriptionResult)
		Private ReadOnly ErrorCallback As Action(Of CimException)

		''' <summary>
		''' Complete text of the query that initiates the subscriber
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Overridable Property QueryText As String

		''' <summary>
		''' Creates a new CIM subscriber
		''' </summary>
		''' <param name="Session">An existing CIM session to the local or a remote computer.</param>
		''' <param name="[Namespace]">The CIM namespace for the activity to operate in.</param>
		''' <param name="ResultCallbackAction">Callback function for results</param>
		''' <param name="ErrorCallbackAction">Callback function for CIM errors</param>
		Public Sub New(ByVal Session As CimSession, ByVal [Namespace] As String, ByVal ResultCallbackAction As Action(Of CimSubscriptionResult), ByVal ErrorCallbackAction As Action(Of CimException))
			MyBase.New(Session, [Namespace])
			ResultCallback = ResultCallbackAction
			ErrorCallback = ErrorCallbackAction
		End Sub

		''' <summary>
		''' Creates a new CIM subscriber in the default namespace (root/CIMV2)
		''' </summary>
		''' <param name="Session">An existing CIM session to the local or a remote computer.</param>
		''' <param name="ResultCallbackAction">Callback function for results</param>
		''' <param name="ErrorCallbackAction">Callback function for CIM errors</param>
		Public Sub New(ByVal Session As CimSession, ByVal ResultCallbackAction As Action(Of CimSubscriptionResult), ByVal ErrorCallbackAction As Action(Of CimException))
			MyBase.New(Session, DefaultNamespace)
			ResultCallback = ResultCallbackAction
			ErrorCallback = ErrorCallbackAction
		End Sub

		Public Sub Start()
			StartSubscriber()
		End Sub

		Protected Overrides Function InvokeOperation() As IObservable(Of CimSubscriptionResult)
			Return Session.SubscribeAsync([Namespace], QueryLanguage, QueryText, AsyncOptions)
		End Function

		Protected Overrides Sub ReportError([Error] As CimException)
			ErrorCallback([Error])
			StartSubscriber()
		End Sub

		Protected Overrides Sub ReportResult(Result As CimSubscriptionResult)
			ResultCallback(Result)
		End Sub

		Protected Overrides Sub ReportCompletion()
			' subscriptions do not report completion
		End Sub

	End Class

	''' <summary>
	''' Selector for WMI or CIM objects when both possibilities exist. Ex: __InstanceCreationEvent vs. CIM_InstCreation
	''' </summary>
	Public Enum Hierarchy

		''' <summary>
		''' Use for objects such as CIM_InstCreation, CIM_InstModification, and CIM_InstDeletion
		''' </summary>
		CIM

		''' <summary>
		''' Use for objects such as __InstanceCreationEvent, __InstanceModificationEvent, and __InstanceDeletionEvent
		''' </summary>
		WMI

	End Enum

	''' <summary>
	''' Specifies the CIM or WMI indication class category.
	''' </summary>
	Public Enum IndicationType
		Creation
		Modification
		Deletion
	End Enum

	''' <summary>
	''' Specialized CIM subscriber for CIM or WMI indication objects.
	''' </summary>
	Public MustInherit Class InstanceIndicationController
		Inherits CimSubscriptionController

		Public Property WatchHierarchy As Hierarchy = Hierarchy.WMI
		Public Property WatchedClassName As String = ""
		Public Property QueryInterval As UInteger = 1
		Public Property WatchType As IndicationType

		Protected Sub New(ByVal Session As CimSession, ByVal WatchType As IndicationType, ByVal [Namespace] As String, ByVal WatchedClassName As String, ByVal WatchCategory As Hierarchy, ByVal ResultCallbackAction As Action(Of CimSubscriptionResult), ByVal ErrorCallbackAction As Action(Of CimException))
			MyBase.New(Session, [Namespace], ResultCallbackAction, ErrorCallbackAction)
			Me.WatchedClassName = WatchedClassName
			Me.WatchHierarchy = WatchCategory
			Me.WatchType = WatchType
		End Sub

		''' <summary>
		''' Creates a specialized indication watcher for creation, deletion, or modification events of a particular class based on input.
		''' </summary>
		''' <param name="Session">The <see cref="CimSession"/> in which to create the watcher</param>
		''' <param name="WatchType">The type of instance to watch for</param>
		''' <param name="[Namespace]">The CIM namespace that contains the target class name</param>
		''' <param name="WatchedClassName">The CIM class name the indication object will watch</param>
		''' <param name="WatchHierarchy">Use indications from the WMI or CIM hierarchy</param>
		''' <returns></returns>
		Public Shared Function IndicationControllerFactory(ByVal Session As CimSession, ByVal WatchType As IndicationType, ByVal [Namespace] As String, ByVal WatchedClassName As String, ByVal ResultCallbackAction As Action(Of CimSubscriptionResult), ByVal ErrorCallbackAction As Action(Of CimException), Optional ByVal WatchHierarchy As Hierarchy = Hierarchy.WMI) As InstanceIndicationController
			Select Case WatchType
				Case IndicationType.Creation
					Return New InstanceCreationController(Session, [Namespace], WatchedClassName, ResultCallbackAction, ErrorCallbackAction, WatchHierarchy)
				Case IndicationType.Deletion
					Return New InstanceDeletionController(Session, [Namespace], WatchedClassName, ResultCallbackAction, ErrorCallbackAction, WatchHierarchy)
				Case Else ' modification
					Return New InstanceModificationController(Session, [Namespace], WatchedClassName, ResultCallbackAction, ErrorCallbackAction, WatchHierarchy)
			End Select
		End Function

		Protected Overrides Function InvokeOperation() As IObservable(Of CimSubscriptionResult)
			If QueryInterval < 2 Then QueryInterval = 2
			Dim IndicationClass As String
			Select Case WatchType
				Case IndicationType.Creation
					IndicationClass = IIf(WatchHierarchy = Hierarchy.CIM, CimClassNameInstanceCreation, WmiClassNameInstanceCreation).ToString
				Case IndicationType.Deletion
					IndicationClass = IIf(WatchHierarchy = Hierarchy.CIM, CimClassNameInstanceDeletion, WmiClassNameInstanceDeletion).ToString
				Case Else   ' modification
					IndicationClass = IIf(WatchHierarchy = Hierarchy.CIM, CimClassNameInstanceModification, WmiClassNameInstanceModication).ToString
			End Select
			Dim Selector As String = IIf(WatchHierarchy = Hierarchy.CIM, CimSubscriptionInstanceSelector, WmiSubscriptionInstanceSelector).ToString
			QueryText = String.Format(QueryTemplateTimedEvent, IndicationClass, QueryInterval, Selector, WatchedClassName)
			Return MyBase.InvokeOperation()
		End Function

	End Class

	''' <summary>
	''' Specialized CIM subscriber that watches for CIM or WMI object creation events.
	''' </summary>
	Public Class InstanceCreationController
		Inherits InstanceIndicationController

		''' <summary>
		''' Creates a new CIM or WMI object creation subscriber.
		''' </summary>
		''' <param name="Session">The <see cref="CimSession"/> in which to create the watcher</param>
		''' <param name="[Namespace]">The CIM namespace that contains the target class name</param>
		''' <param name="WatchedClassName">The CIM class name the indication object will watch</param>
		''' <param name="WatchHierarchy">Use indications from the WMI or CIM hierarchy</param>
		''' <remarks>The CIM hierarchy sometimes generates unresolvable "InvalidProperty" errors</remarks>
		Public Sub New(ByVal Session As CimSession, ByVal [Namespace] As String, ByVal WatchedClassName As String, ByVal ResultCallbackAction As Action(Of CimSubscriptionResult), ByVal ErrorCallbackAction As Action(Of CimException), Optional ByVal WatchHierarchy As Hierarchy = Hierarchy.WMI)
			MyBase.New(Session, IndicationType.Creation, [Namespace], WatchedClassName, WatchHierarchy, ResultCallbackAction, ErrorCallbackAction)
		End Sub

	End Class

	''' <summary>
	''' Specialized CIM subscriber that watches for CIM or WMI object modification events.
	''' </summary>
	Public Class InstanceModificationController
		Inherits InstanceIndicationController

		''' <summary>
		''' Creates a new CIM or WMI object modification subscriber.
		''' </summary>
		''' <param name="Session">The <see cref="CimSession"/> in which to create the watcher</param>
		''' <param name="[Namespace]">The CIM namespace that contains the target class name</param>
		''' <param name="WatchedClassName">The CIM class name the indication object will watch</param>
		''' <param name="WatchHierarchy">Use indications from the WMI or CIM hierarchy</param>
		''' <remarks>The CIM hierarchy sometimes generates unresolvable "InvalidProperty" errors</remarks>
		Public Sub New(ByVal Session As CimSession, ByVal [Namespace] As String, ByVal WatchedClassName As String, ByVal ResultCallbackAction As Action(Of CimSubscriptionResult), ByVal ErrorCallbackAction As Action(Of CimException), Optional ByVal WatchHierarchy As Hierarchy = Hierarchy.WMI)
			MyBase.New(Session, IndicationType.Modification, [Namespace], WatchedClassName, WatchHierarchy, ResultCallbackAction, ErrorCallbackAction)
		End Sub

	End Class

	''' <summary>
	''' Specialized CIM subscriber that watches for CIM or WMI object deletion events.
	''' </summary>
	Public Class InstanceDeletionController
		Inherits InstanceIndicationController

		''' <summary>
		''' Creates a new CIM or WMI object deletion subscriber.
		''' </summary>
		''' <param name="Session">The <see cref="CimSession"/> in which to create the watcher</param>
		''' <param name="[Namespace]">The CIM namespace that contains the target class name</param>
		''' <param name="WatchedClassName">The CIM class name the indication object will watch</param>
		''' <param name="WatchHierarchy">Use indications from the WMI or CIM hierarchy</param>
		''' <remarks>The CIM hierarchy sometimes generates unresolvable "InvalidProperty" errors</remarks>
		Public Sub New(ByVal Session As CimSession, ByVal [Namespace] As String, ByVal WatchedClassName As String, ByVal ResultCallbackAction As Action(Of CimSubscriptionResult), ByVal ErrorCallbackAction As Action(Of CimException), Optional ByVal WatchHierarchy As Hierarchy = Hierarchy.WMI)
			MyBase.New(Session, IndicationType.Deletion, [Namespace], WatchedClassName, WatchHierarchy, ResultCallbackAction, ErrorCallbackAction)
		End Sub

	End Class

End Namespace