Imports System.IO
Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure
Imports Microsoft.Win32

''' <summary>
''' Retrieves a registry KVP value and watches it for changes.
''' </summary>
Public Class RegistryController
	Inherits ModuleWithCimBase
	Implements IRunningModule
	Implements IDisposable

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
	Public Property KeySubPath As String
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

	Public ReadOnly Property KeyFullPath As String
		Get
			Return Path.Combine(RootRegistry.Name, KeySubPath)
		End Get
	End Property

	''' <summary>
	''' Creates a new registry value watcher
	''' </summary>
	''' <param name="Session"><see cref="CimSession"/> for the local system</param>
	''' <remarks>Will not operate on remote sessions</remarks>
	Public Sub New(ByVal Session As CimSession, ByVal ReportValueChangeAction As Action, ByVal GenericLogController As IModuleLogger, ByVal RegistryLogController As IRegistryLogger)
		MyBase.New(Session, GenericLogController)
		ReportValueChange = ReportValueChangeAction
		RegistryLogger = RegistryLogController
	End Sub

	''' <summary>
	''' Starts the value watcher
	''' </summary>
	Public Sub Start() Implements IRunningModule.Start
		GetKVPValue()
		ValueWatcher = New CimSubscriptionController(Session, AddressOf RelayValueChange, AddressOf ReportError) With {
			.[Namespace] = CimNamespaceRootDefault,
			.QueryText = String.Format(CimQueryTemplateRegistryValueChange, RootRegistry.Name, EscapeRegistryItem(KeySubPath), ValueName)
		}
		ValueWatcher.Start()
	End Sub

	Public ReadOnly Property IsRunning As Boolean Implements IRunningModule.IsRunning
		Get
			Return ValueWatcher.IsRunning
		End Get
	End Property

	''' <summary>
	''' Stops the value watcher
	''' </summary>
	Public Sub [Stop]() Implements IRunningModule.Stop
		ValueWatcher?.Cancel()
		ValueWatcher?.Dispose()
	End Sub

	Private _Value As Object = 0

	Private ReadOnly ReportValueChange As Action
	Private ReadOnly RegistryLogger As IRegistryLogger

	''' <summary>
	''' Current value of the KVP
	''' </summary>
	''' <remarks>Retrieves the value directly if the subscriber is not running.</remarks>
	''' <returns><see cref="Object"/></returns>
	Public ReadOnly Property Value As Object
		Get
			If ValueWatcher Is Nothing OrElse Not ValueWatcher.IsRunning Then
				GetKVPValue()
			End If
			Return _Value
		End Get
	End Property

	Public Overrides ReadOnly Property ModuleName As String = "Registry"
	Private ValueWatcher As CimSubscriptionController

	Private Sub RelayValueChange(ByVal SubscriptionNotification As CimSubscriptionResult)
		SubscriptionNotification.Dispose()
		GetKVPValue()
		ReportValueChange()
	End Sub

	''' <summary>
	''' Reads the KVP value from the registry
	''' </summary>
	Private Sub GetKVPValue()
		Dim TargetKey As RegistryKey = Nothing
		Try
			TargetKey = RootRegistry.OpenSubKey(KeySubPath)
		Catch ex As Exception
			RegistryLogger.LogRegistryOpenKeyError(KeySubPath, ex)
		End Try
		If TargetKey IsNot Nothing Then
			Try
				_Value = TargetKey.GetValue(ValueName)
				_ValueKind = TargetKey.GetValueKind(ValueName)
			Catch ioex As IO.IOException
				RegistryLogger.LogDebugRegistryKVPNotFound(ValueName, KeyFullPath)
			Catch ex As Exception
				RegistryLogger.LogRegistryAccessError(KeyFullPath, ex)
			Finally
				TargetKey.Close()
				TargetKey.Dispose()
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
