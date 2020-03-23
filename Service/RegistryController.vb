Imports HyperVive.CIMitar
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

Partial Public Class RegistryController
	Implements IDisposable

	Public Event RegistryValueChanged(ByVal sender As Object, ByVal e As RegistryValueChangedEventArgs)
	Public Event RegistryAccessError(ByVal sender As Object, ByVal e As ModuleExceptionEventArgs)

	Public Property RootRegistry As RegistryKey
	Public Property KeyPath As String
	Public Property ValueName As String
	Public ReadOnly Property ValueKind As RegistryValueKind = RegistryValueKind.String

	Private _Value As Object = 0
	Public ReadOnly Property Value As Object
		Get
			If Not ValueWatcher.IsRunning Then
				UpdateKeyValue()
			End If
			Return _Value
		End Get
	End Property

	Private Session As CimSession
	Private Const ModuleName As String = "Registry"
	Private WithEvents ValueWatcher As CimSubscriptionController

	Private Sub ProcessUpdatedValue(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles ValueWatcher.EventReceived
		e.SubscribedEvent.Dispose()
		UpdateKeyValue()
		RaiseEvent RegistryValueChanged(Me, New RegistryValueChangedEventArgs With {.Hive = RootRegistry.Name, .KeyPath = KeyPath, .ValueName = ValueName, .Value = Value})
	End Sub

	Private Sub WatcherErrorReceived(ByVal sender As Object, ByVal e As CimErrorEventArgs) Handles ValueWatcher.ErrorOccurred
		e.ErrorInstance.Dispose()
	End Sub

	Public Sub New(ByRef Session As CimSession)
		Me.Session = Session
	End Sub

	Public Sub Start()
		UpdateKeyValue()
		ValueWatcher = New CimSubscriptionController(Session) With {
			.[Namespace] = "root/DEFAULT",
			.QueryText = String.Format(CimQueryTemplateRegistryValueChange, RootRegistry.Name, EscapeRegistryItem(KeyPath), ValueName)
		}
	End Sub

	Public Sub [Stop]()
		ValueWatcher?.Cancel()
	End Sub

	Private Sub UpdateKeyValue()
		Dim TargetKey As RegistryKey = RootRegistry.OpenSubKey(KeyPath)
		If TargetKey IsNot Nothing Then
			Try
				_Value = TargetKey.GetValue(ValueName)
				_ValueKind = TargetKey.GetValueKind(ValueName)
			Catch ex As Exception
				RaiseEvent RegistryAccessError(Me, New ModuleExceptionEventArgs With {.ModuleName = ModuleName, .[Error] = ex})
			Finally
				TargetKey?.Close()
				TargetKey?.Dispose()
			End Try
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
