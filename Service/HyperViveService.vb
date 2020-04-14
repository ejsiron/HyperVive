Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure
Imports System.Security.Principal

Public Module HyperViveEvents
	''' <summary>
	''' Debug-level message event
	''' </summary>
	Public Class DebugMessageEventArgs

		Inherits EventArgs
		Public Property Message As String
		Public Property EventId As Integer
		Public Sub New(ByVal Message As String, Optional ByVal EventId As Integer = EventIdDebugGeneral)
			Me.Message = Message
			Me.EventId = EventId
		End Sub
	End Class

	''' <summary>
	''' Module-specific error message
	''' </summary>
	Public Class ModuleExceptionEventArgs
		Inherits EventArgs

		Public Property ModuleName As String
		Public Property [Error] As Exception
		Public Property EventId As Integer = EventIdModuleErrorGeneral
	End Class
End Module

Public Class HyperViveService
	Private LocalCimSession As CimSession
	Private WithEvents DebugModeSettingReader As RegistryController
	Private WithEvents AdapterInventory As VMNetAdapterInventory
	Private WithEvents WOLListener As WakeOnLanListener
	Private WithEvents VMStarter As VMStartController
	Private WithEvents CheckpointWatcher As CheckpointJobWatcher

	Private Const ServiceRegistryPathTemplate As String = "SYSTEM\CurrentControlSet\Services\{0}"
	Private Const ElevationError As String = "Must run as an elevated user"
	Private Const DebugModeReportTemplate As String = "Debug mode set to {0}"
	Private Const UnexpectedAppErrorTemplate As String = "Halting due to unexpected error ""{0}"" of type {1}"
	Private Const UnexpectedModuleErrorTemplate As String = "Unexpected error of type ""{0}"" in module ""{1}"": {2}"
	Private Const WolReceivedTemplate As String = "Received WOL frame from {0} for {1}"
	Private Const StartResultTemplate As String = "VM wake-on-LAN start operation {0}.
VM name: {1}
VM ID: {2}
VM MAC address: {3}
Request source: {4}
Result code: {5}
Result message: {6}"
	Private Const SucceededMessage As String = "succeeded"
	Private Const FailedMessage As String = "failed"
	Private Const CheckpointActionTemplate As String = "Checkpoint action ""{0}"" initiated by {1} for VM {2}
VM ID: {3}
Job instance ID: {4}"
	Private Const CheckpointActionCompletedTemplate As String = "
Result: {0} ({1})"

	Private ReadOnly Property ServiceRegistryPath As String
		Get
			Return String.Format(ServiceRegistryPathTemplate, ServiceName)
		End Get
	End Property

	Private DebugMode As Boolean = False

	Protected Overrides Sub OnStart(ByVal args() As String)
		AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf AppErrorReceived
		If New WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) Then
			LocalCimSession = CimSession.Create(Nothing)
			DebugModeSettingReader = New RegistryController(LocalCimSession) With {.RootRegistry = Microsoft.Win32.Registry.LocalMachine, .KeyPath = ServiceRegistryPath, .ValueName = "DebugMode"}
			UpdateDebugMode(Nothing, Nothing)
			DebugModeSettingReader.Start()
			AdapterInventory = New VMNetAdapterInventory(LocalCimSession)
			VMStarter = New VMStartController(LocalCimSession)
			WOLListener = New WakeOnLanListener
			WOLListener.Start()
			CheckpointWatcher = New CheckpointJobWatcher(LocalCimSession)
			CheckpointWatcher.Start()
		Else
			EventLog.WriteEntry(ElevationError, EventLogEntryType.Error)
			Kill(5)
		End If
	End Sub

	Protected Overrides Sub OnStop()
		WOLListener?.Dispose()
		DebugModeSettingReader.Stop()
		AdapterInventory?.Dispose()
		VMStarter = Nothing
		CheckpointWatcher?.Cancel()
		CheckpointWatcher?.Dispose()
		LocalCimSession?.Dispose()
		RemoveHandler AppDomain.CurrentDomain.UnhandledException, AddressOf AppErrorReceived
	End Sub

	Private Sub UpdateDebugMode(ByVal sender As Object, ByVal e As RegistryValueChangedEventArgs) Handles DebugModeSettingReader.RegistryValueChanged
		Try
			DebugMode = CBool(DebugModeSettingReader.Value)
		Catch ex As Exception
			DebugMode = False
		End Try
		WriteDebugMessage(Me, New DebugMessageEventArgs(String.Format(DebugModeReportTemplate, DebugMode), EventIdDebugModeChanged))
	End Sub

	Private Sub AppErrorReceived(ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs)
		Dim UnknownError As Exception = CType(e.ExceptionObject, Exception)
		EventLog.WriteEntry(String.Format(UnexpectedAppErrorTemplate, UnknownError.Message, UnknownError.GetType.FullName), EventLogEntryType.Error, EventIdAppError)
		Kill(CType(IIf(UnknownError.HResult = 0, -1, UnknownError.HResult), Integer))
		If TypeOf UnknownError Is IDisposable Then   ' CIM exceptions are disposable
			CType(UnknownError, IDisposable).Dispose()
		End If
	End Sub

	Private Sub ModuleErrorReceived(ByVal sender As Object, ByVal e As ModuleExceptionEventArgs) Handles DebugModeSettingReader.RegistryAccessError, WOLListener.ReceiverError, CheckpointWatcher.CheckpointWatcherErrorOccurred ' , VMStarter.StarterError (unused at this time)
		EventLog.WriteEntry(String.Format(UnexpectedModuleErrorTemplate, e.Error.GetType.FullName(), e.ModuleName, e.Error.Message), EventLogEntryType.Error, e.EventId)
		If TypeOf e Is IDisposable Then
			CType(e, IDisposable).Dispose()
		End If
	End Sub

	Private Sub MagicPacketReceived(ByVal sender As Object, ByVal e As WOLEvents.MagicPacketReceivedEventArgs) Handles WOLListener.MagicPacketReceived
		WriteDebugMessage(Me, New DebugMessageEventArgs(String.Format(WolReceivedTemplate, e.SenderIP.ToString, e.MacAddress), EventIdMagicPacketReceived))
		Dim VmIDs As List(Of String) = AdapterInventory.GetVmIDFromMac(e.MacAddress)
		VMStarter.Start(e.MacAddress, VmIDs, e.SenderIP.ToString)
	End Sub

	Private Sub WriteDebugMessage(ByVal sender As Object, ByVal e As DebugMessageEventArgs) Handles WOLListener.DebugMessageGenerated, AdapterInventory.DebugMessageGenerated, VMStarter.DebugMessageGenerated, DebugModeSettingReader.DebugMessageGenerated, CheckpointWatcher.DebugMessageGenerated
		If DebugMode Then
			EventLog.WriteEntry(e.Message, EventLogEntryType.Information, e.EventId)
		End If
	End Sub

	Private Sub WriteVMStartResults(ByVal sender As Object, ByVal e As VMStartResultEventArgs) Handles VMStarter.StartResult
		With e
			Dim MessageTemplate As String = String.Format(StartResultTemplate, IIf(.Success, SucceededMessage, FailedMessage).ToString,
				.VirtualMachineInfo.Name, .VirtualMachineInfo.ID, .VirtualMachineInfo.MacAddress,
				.VirtualMachineInfo.SourceIP, .ResultCode, .ResultText)
			EventLog.WriteEntry(MessageTemplate,
				CType(IIf(.Success, EventLogEntryType.Information, EventLogEntryType.Error), EventLogEntryType),
				CInt(IIf(.Success, EventIdVMStartSuccess, EventIdVMStartFailed)))
		End With
	End Sub

	Private Sub WriteCheckpointActionStarted(ByVal sender As Object, ByVal e As CheckpointActionEventArgs) Handles CheckpointWatcher.CheckpointJobStarted, CheckpointWatcher.CheckpointJobCompleted
		Dim Message As String = String.Format(CheckpointActionTemplate, e.JobTypeName, e.UserName, e.VMName, e.VMID, e.JobInstanceID)
		Dim EventType As EventLogEntryType = EventLogEntryType.Information
		Dim EventId As Integer = EventIdCheckpointActionStarted
		If e.IsCompleted Then
			Message += String.Format(CheckpointActionCompletedTemplate, e.CompletionStatus, e.CompletionCode)
			EventId = CInt(IIf(e.CompletionCode = VirtualizationMethodErrors.NoError, EventIdCheckpointActionSucceeded, EventIdCheckpointActionFailed))
			If e.CompletionCode <> VirtualizationMethodErrors.NoError Then
				EventType = EventLogEntryType.Error
			End If
		End If
		EventLog.WriteEntry(Message, EventType, EventId)
	End Sub

	Private Sub Kill(ByVal ExitCode As Integer)
		Me.ExitCode = ExitCode
		[Stop]()
	End Sub
End Class
