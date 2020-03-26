﻿Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure
Imports Microsoft.Win32

Public Module RegistryEvents
	Public Class RegistryValueChangedEventArgs
		Public Property Hive As String
		Public Property KeyPath As String
		Public Property ValueName As String
		Public Property Value As Object
	End Class
End Module

''' <summary>
''' Retrieves a registry KVP value and watches it for changes.
''' </summary>
Public Class RegistryController
	Implements IDisposable

	Public Event RegistryValueChanged(ByVal sender As Object, ByVal e As RegistryValueChangedEventArgs)
	Public Event RegistryAccessError(ByVal sender As Object, ByVal e As ModuleExceptionEventArgs)
	Public Event DebugMessageGenerated(ByVal sender As Object, ByVal e As DebugMessageEventArgs)

	''' <summary>
	''' The root registry key that contains the desired subkey
	''' <para>Choose from:</para>
	''' <para>- <see cref="Registry.ClassesRoot"/></para>
	''' <para>- <see cref="Registry.CurrentConfig"/></para>
	''' <para>- <see cref="Registry.CurrentUser"/></para>
	''' <para>- <see cref="Registry.DynData"/></para>
	''' <para>- <see cref="Registry.LocalMachine"/></para>
	''' <para>- <see cref="Registry.PerformanceData"/></para>
	''' <para>- <see cref="Registry.Users"/></para>
	''' </summary>
	''' <returns><see cref="RegistryKey"/></returns>
	Public Property RootRegistry As RegistryKey

	''' <summary>
	''' Path to the desired key
	''' </summary>
	''' <example>"SYSTEM\CurrentControlSet\Services\HyperVive"</example>
	''' <returns><see cref="String"/></returns>
	Public Property KeyPath As String
	''' <summary>
	''' Name of the KVP that owns the target value
	''' </summary>
	''' <returns><see cref="String"/></returns>
	Public Property ValueName As String
	''' <summary>
	''' The type of the value
	''' </summary>
	''' <returns><see cref="RegistryValueKind"/></returns>
	Public ReadOnly Property ValueKind As RegistryValueKind = RegistryValueKind.String

	Private _Value As Object = 0

	''' <summary>
	''' Current value of the KVP
	''' </summary>
	''' <remarks>Retrieves the value directly if the subscriber is not running.</remarks>
	''' <returns><see cref="Object"/></returns>
	Public ReadOnly Property Value As Object
		Get
			If ValueWatcher Is Nothing OrElse Not ValueWatcher.HasStarted Then
				UpdateKeyValue()
			End If
			Return _Value
		End Get
	End Property

	Private Session As CimSession
	Private Const ModuleName As String = "Registry"
	Private WithEvents ValueWatcher As CimSubscriptionController
	Private Const MissingRegistryKVPTemplate As String = "Registry KVP ""{0}"" not found at ""{1}"", retaining current value ""{2}"""

	Private Sub ProcessUpdatedValue(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles ValueWatcher.EventReceived
		e.SubscribedEvent.Dispose()
		UpdateKeyValue()
		RaiseEvent RegistryValueChanged(Me, New RegistryValueChangedEventArgs With {.Hive = RootRegistry.Name, .KeyPath = KeyPath, .ValueName = ValueName, .Value = Value})
	End Sub

	Private Sub WatcherErrorReceived(ByVal sender As Object, ByVal e As CimErrorEventArgs) Handles ValueWatcher.ErrorOccurred
		e.ErrorInstance.Dispose()
	End Sub

	''' <summary>
	''' Creates a new registry value watcher
	''' </summary>
	''' <param name="Session"><see cref="CimSession"/> for the local system</param>
	''' <remarks>Will not operate on remote sessions</remarks>
	Public Sub New(ByRef Session As CimSession)
		Me.Session = Session
	End Sub

	''' <summary>
	''' Starts the value watcher
	''' </summary>
	Public Sub Start()
		UpdateKeyValue()
		ValueWatcher = New CimSubscriptionController(Session) With {
			.[Namespace] = CimNamespaceRootDefault,
			.QueryText = String.Format(CimQueryTemplateRegistryValueChange, RootRegistry.Name, EscapeRegistryItem(KeyPath), ValueName)
		}
		ValueWatcher.Start()
	End Sub

	''' <summary>
	''' Stops the value watcher
	''' </summary>
	Public Sub [Stop]()
		ValueWatcher?.Cancel()
		ValueWatcher?.Dispose()
	End Sub

	''' <summary>
	''' Reads the key value from the registry
	''' </summary>
	Private Sub UpdateKeyValue()
		Dim TargetKey As RegistryKey = RootRegistry.OpenSubKey(KeyPath)
		If TargetKey IsNot Nothing Then
			Try
				_Value = TargetKey.GetValue(ValueName)
				_ValueKind = TargetKey.GetValueKind(ValueName)
			Catch ioex As IO.IOException
				RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {
					.Message = String.Format(MissingRegistryKVPTemplate, ValueName, KeyPath, _Value)})
			Catch ex As Exception
				RaiseEvent RegistryAccessError(Me, New ModuleExceptionEventArgs With {.ModuleName = ModuleName, .[Error] = ex})
			Finally
				TargetKey?.Close()
				TargetKey?.Dispose()
			End Try
		Else
			RaiseEvent RegistryAccessError(Me, New ModuleExceptionEventArgs With {.ModuleName = ModuleName, .[Error] = New Exception(String.Format("Could not open registry at {0}", KeyPath))})
		End If
	End Sub

	''' <summary>
	''' Registry paths in WMI need to have every backslash escaped. This regex replaces any instance of slashes, regardless of count, with two slashes
	''' </summary>
	''' <param name="InputString">Unescaped registry path</param>
	''' <returns>Escaped registry path</returns>
	Private Function EscapeRegistryItem(ByVal InputString As String) As String
		Dim RegistryRegEx As System.Text.RegularExpressions.Regex

		RegistryRegEx = New System.Text.RegularExpressions.Regex("\\+")
		Return RegistryRegEx.Replace(InputString, "\\")
	End Function

#Region "IDisposable Support"
	Private disposedValue As Boolean ' To detect redundant calls

	Protected Overridable Sub Dispose(disposing As Boolean)
		If Not disposedValue Then
			If disposing Then
				ValueWatcher?.Dispose()
			End If
		End If
		disposedValue = True
	End Sub

	Public Sub Dispose() Implements IDisposable.Dispose
		Dispose(True)
	End Sub
#End Region
End Class
