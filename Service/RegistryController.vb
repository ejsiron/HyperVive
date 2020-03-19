Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure
Imports Microsoft.Win32

Public Module RegistryEvents
	Public Class RegistryDwordKVPChangedEventArgs
		Inherits EventArgs

		Public Property KeyName As String
		Public Property Value As UInt32
	End Class
End Module

Partial Public Class RegistryController
	Implements IDisposable

	Private Session As CimSession

	'Private ReadOnly Property KVPRootKey As RegistryKey
	'	Get
	'		If Environment.Is64BitOperatingSystem Then
	'			Return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
	'		Else
	'			Return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
	'		End If
	'	End Get
	'End Property

	Private WithEvents DebugModeKeyWatcher As CimSubscriptionController

	Public Sub New(ByRef Session As CimSession)
		Me.Session = Session
	End Sub

	Public Sub Start()
		DebugModeKeyWatcher = New CimSubscriptionController(Session) With {
			.QueryText = String.Format(CimSelectRegistyChangeEventTemplate, "HKEY_LOCAL_MACHINE", EscapeRegistryItem(RegistryKeyName))
		}
	End Sub

	Public Sub [Stop]()
		DebugModeKeyWatcher?.Cancel()
	End Sub

	Private Function GetKeyValue(ByVal ContainerKeyName As String, ByVal KVPKeyName As String) As Object
		Return Registry.LocalMachine.GetValue(ContainerKeyName + "\" + KVPKeyName, Nothing)
	End Function

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
				DebugModeKeyWatcher?.Dispose()
			End If
		End If
		disposedValue = True
	End Sub

	Public Sub Dispose() Implements IDisposable.Dispose
		Dispose(True)
	End Sub
#End Region
End Class
