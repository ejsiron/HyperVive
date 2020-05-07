Public Class HyperViveService
	Private ModuleController As ModuleController

	Private Const ServiceRegistryPathTemplate As String = "SYSTEM\CurrentControlSet\Services\{0}"
	Private Const ElevationError As String = "Must run as an elevated user"

	Private ReadOnly Property ServiceRegistryPath As String
		Get
			Return String.Format(ServiceRegistryPathTemplate, ServiceName)
		End Get
	End Property

	Private DebugMode As Boolean = False

	Protected Overrides Sub OnStart(ByVal args() As String)

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
		LogController.CloseAll()
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
		VMStarter.StartVM(e.MacAddress, VmIDs, e.SenderIP.ToString)
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

	Public Sub Kill(ByVal ExitCode As Integer)
		Me.ExitCode = ExitCode
		[Stop]()
	End Sub
End Class
