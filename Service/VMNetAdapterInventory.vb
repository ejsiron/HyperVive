Imports HyperVive.HyperViveService
Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure

Public Class VMNetAdapterInventory
	Implements IDisposable

	''' <summary>
	''' Matches a virtual adapter's Instance ID to its MAC address
	''' </summary>
	Public Structure AdapterEntry
		''' <summary>
		''' The virtual network adapter's instance ID
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property InstanceID As String
		''' <summary>
		''' The virtual adapters MAC address, unformatted
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property MAC As String
	End Structure

	Public Event InventoryError(ByVal sender As Object, ByVal e As ModuleExceptionEventArgs)
	Public Event DebugMessageGenerated(ByVal sender As Object, ByVal e As DebugMessageEventArgs)

	Public Sub New(ByVal Session As CimSession)
		Me.Session = Session
		SyntheticAdapterSettingsCreateSubscriber = New CimSubscriptionController(Session, CimNamespaceVirtualization) With {
				.QueryText = String.Format(CimQueryTemplateTimedEvent, CimClassNameInstanceCreation, 1, CimClassNameSyntheticAdapterSettingData)}
		SyntheticAdapterSettingsChangeSubscriber = New CimSubscriptionController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimQueryTemplateTimedEvent, CimClassNameInstanceModification, 1, CimClassNameSyntheticAdapterSettingData)}
		SyntheticAdapterSettingsDeleteSubscriber = New CimSubscriptionController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimQueryTemplateTimedEvent, CimClassNameInstanceDeletion, 1, CimClassNameSyntheticAdapterSettingData)}
		EmulatedAdapterSettingsCreateSubscriber = New CimSubscriptionController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimQueryTemplateTimedEvent, CimClassNameInstanceCreation, 1, CimClassNameEmulatedAdapterSettingData)}
		EmulatedAdapterSettingsChangeSubscriber = New CimSubscriptionController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimQueryTemplateTimedEvent, CimClassNameInstanceModification, 1, CimClassNameEmulatedAdapterSettingData)}
		EmulatedAdapterSettingsDeleteSubscriber = New CimSubscriptionController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimQueryTemplateTimedEvent, CimClassNameInstanceDeletion, 1, CimClassNameEmulatedAdapterSettingData)}
		Reset()
	End Sub

	''' <summary>
	''' Resets and restarts all subscribers
	''' </summary>
	Public Async Sub Reset()
		SyntheticAdapterSettingsCreateSubscriber.Cancel()
		SyntheticAdapterSettingsChangeSubscriber.Cancel()
		SyntheticAdapterSettingsDeleteSubscriber.Cancel()
		EmulatedAdapterSettingsCreateSubscriber.Cancel()
		EmulatedAdapterSettingsChangeSubscriber.Cancel()
		EmulatedAdapterSettingsDeleteSubscriber.Cancel()

		AdaptersLock = New Object
		SyncLock AdaptersLock
			If CurrentAdapters Is Nothing Then
				CurrentAdapters = New List(Of AdapterEntry)
			Else
				CurrentAdapters.Clear()
			End If
		End SyncLock

		For Each AdapterClassName As String In {CimClassNameSyntheticAdapterSettingData, CimClassNameEmulatedAdapterSettingData}
			Using AdapterEnumerator As New CimAsyncEnumerateInstancesController(Session, CimNamespaceVirtualization, AdapterClassName)
				Using FoundAdapters As CimInstanceList = Await AdapterEnumerator.StartAsync
					For Each AdapterInstance As CimInstance In FoundAdapters
						AddAdapter(GetAdapterEntryFromInstance(AdapterInstance))
					Next
				End Using
			End Using
		Next
		RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {.Message = String.Format(EnumeratedAdaptersTemplate, CurrentAdapters.Count)})

		SyntheticAdapterSettingsCreateSubscriber.Start()
		SyntheticAdapterSettingsChangeSubscriber.Start()
		SyntheticAdapterSettingsDeleteSubscriber.Start()
		EmulatedAdapterSettingsCreateSubscriber.Start()
		EmulatedAdapterSettingsChangeSubscriber.Start()
		EmulatedAdapterSettingsDeleteSubscriber.Start()
	End Sub

	''' <summary>
	''' Find the virtual machine that owns a given MAC address
	''' </summary>
	''' <param name="MacAddress">The desired MAC in unformatted <see cref="String"/> format</param>
	''' <returns>All virtual machine IDs that own an adapter with a matching MAC, in <see cref="List(Of String)" format/></returns>
	Public Function GetVmIDFromMac(ByVal MacAddress As String) As List(Of String)
		Dim MatchingMacs As New List(Of String)
		SyncLock AdaptersLock
			CurrentAdapters.Where(Function(SearchAdapter As AdapterEntry) SearchAdapter.MAC = MacAddress
				).ToList.ForEach(Sub(MatchingAdapter As AdapterEntry) MatchingMacs.Add(ExtractVmIDFromInstanceID(MatchingAdapter.InstanceID)))
		End SyncLock
		Return MatchingMacs
	End Function

	Private Const ModuleName As String = "Virtual Network Adapter Inventory"
	Private Const InvalidAdapterTemplate As String = "Invalid network adapter instance id: {0}"
	Private Const RegisteredNewAdapterTemplate As String = "Registered new virtual adapter with MAC {0}"
	Private Const UpdatedAdapterTemplate As String = "Updated an adapter with MAC {0}"
	Private Const AddedFromUpdateTemplate As String = "Added an adapter with MAC {0} from an update request"
	Private Const DeletedAdapterTemplate As String = "Deleted {0} adapter(s)"
	Private Const SyntheticCreate As String = "synthetic create"
	Private Const SyntheticChange As String = "synthetic change"
	Private Const SyntheticDelete As String = "synthetic delete"
	Private Const EmulatedCreate As String = "emulated create"
	Private Const EmulatedChange As String = "emulated change"
	Private Const EmulatedDelete As String = "emulated delete"
	Private Const Unknown As String = "Unknown"
	Private Const SubscriberErrorTemplate As String = "Error received from a virtual adapter subscriber of type ""{0}"": {1}"
	Private Const EnumeratedAdaptersTemplate As String = "Enumerated {0} network adapters"

	Private AdaptersLock As Object
	Private Property CurrentAdapters As List(Of AdapterEntry)

	''' <summary>
	''' Extracts the owning virtual machine's ID from a virtual network adapter's instance ID
	''' </summary>
	''' <param name="InstanceID"></param>
	''' <returns></returns>
	Private Function ExtractVmIDFromInstanceID(ByVal InstanceID As String) As String
		' InstanceID looks like this: Microsoft:A37EAECD-3442-4052-A124-B25562636069\9E9FB56B-418C-49A9-A191-5238EACEE8A1
		' synthetic adapters have an additional location, like \2
		' VmID is the first GUID
		Try
			Return InstanceID.Substring(InstanceID.IndexOf(":") + 1, 36) ' start 1 past the ":" char, then consume the length of a GUID plus hyphens
		Catch ex As Exception
			RaiseEvent InventoryError(Me, New ModuleExceptionEventArgs With {.ModuleName = ModuleName,
				.[Error] = New Exception(String.Format(InvalidAdapterTemplate, InstanceID))})
			Return Guid.Empty.ToString
		End Try
	End Function

	Private Session As CimSession
	Private WithEvents SyntheticAdapterSettingsCreateSubscriber As CimSubscriptionController
	Private WithEvents SyntheticAdapterSettingsChangeSubscriber As CimSubscriptionController
	Private WithEvents SyntheticAdapterSettingsDeleteSubscriber As CimSubscriptionController
	Private WithEvents EmulatedAdapterSettingsCreateSubscriber As CimSubscriptionController
	Private WithEvents EmulatedAdapterSettingsChangeSubscriber As CimSubscriptionController
	Private WithEvents EmulatedAdapterSettingsDeleteSubscriber As CimSubscriptionController

	Private Function GetAdapterEntryFromInstance(ByVal Instance As CimInstance) As AdapterEntry
		Dim NewEntry As New AdapterEntry
		If Instance IsNot Nothing Then
			NewEntry.InstanceID = Instance.GetInstancePropertyValueString(CimPropertyNameInstanceID)
			NewEntry.MAC = Instance.GetInstancePropertyValueString(CimPropertyNameAddress)
		End If
		Return NewEntry
	End Function

	Private Sub OnNewAdapter(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles SyntheticAdapterSettingsCreateSubscriber.EventReceived, EmulatedAdapterSettingsCreateSubscriber.EventReceived
		Dim NewAdapter As AdapterEntry = GetAdapterEntryFromInstance(e.SubscribedEvent.GetSourceInstance)
		If Not String.IsNullOrEmpty(NewAdapter.MAC) Then
			AddAdapter(NewAdapter)
			RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {.Message = String.Format(RegisteredNewAdapterTemplate, NewAdapter.MAC)})
		End If
		e.SubscribedEvent.Dispose()
	End Sub

	Private Sub OnChangeAdapter(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles SyntheticAdapterSettingsChangeSubscriber.EventReceived, EmulatedAdapterSettingsChangeSubscriber.EventReceived
		Dim AdapterFound As Boolean = False
		Dim ChangedAdapter As AdapterEntry = GetAdapterEntryFromInstance(e.SubscribedEvent.GetSourceInstance)
		If Not String.IsNullOrEmpty(ChangedAdapter.MAC) Then
			SyncLock AdaptersLock
				CurrentAdapters.Where(
					Function(ByVal SearchAdapter As AdapterEntry) SearchAdapter.InstanceID = ChangedAdapter.InstanceID
					).ToList.ForEach(Sub(ByVal MatchAdapter As AdapterEntry)
											  MatchAdapter.InstanceID = ChangedAdapter.InstanceID
											  MatchAdapter.MAC = ChangedAdapter.MAC
											  AdapterFound = True
										  End Sub)
			End SyncLock
			If AdapterFound Then
				RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {.Message = String.Format(UpdatedAdapterTemplate, ChangedAdapter.MAC)})
			Else
				AddAdapter(ChangedAdapter)
				RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {.Message = String.Format(AddedFromUpdateTemplate, ChangedAdapter.MAC)})
			End If
		End If
		e.SubscribedEvent.Dispose()
	End Sub

	Private Sub OnDeleteAdapter(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles SyntheticAdapterSettingsDeleteSubscriber.EventReceived, EmulatedAdapterSettingsDeleteSubscriber.EventReceived
		Dim ChangedAdapter As AdapterEntry = GetAdapterEntryFromInstance(e.SubscribedEvent.GetSourceInstance)
		If Not String.IsNullOrEmpty(ChangedAdapter.MAC) Then
			Dim RemovedAdapterCount As Integer = 0
			SyncLock AdaptersLock
				RemovedAdapterCount = CurrentAdapters.RemoveAll(Function(ByVal SearchAdapter As AdapterEntry) SearchAdapter.InstanceID = ChangedAdapter.InstanceID)
			End SyncLock
			RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {.Message = String.Format(DeletedAdapterTemplate, RemovedAdapterCount)})
		End If
		e.SubscribedEvent.Dispose()
	End Sub

	Private Sub OnSubscriberError(ByVal sender As Object, ByVal e As CimErrorEventArgs) Handles SyntheticAdapterSettingsCreateSubscriber.ErrorOccurred, SyntheticAdapterSettingsChangeSubscriber.ErrorOccurred, SyntheticAdapterSettingsDeleteSubscriber.ErrorOccurred, EmulatedAdapterSettingsCreateSubscriber.ErrorOccurred, EmulatedAdapterSettingsChangeSubscriber.ErrorOccurred, EmulatedAdapterSettingsDeleteSubscriber.ErrorOccurred
		Dim SubscriberType As String = Unknown
		If sender Is SyntheticAdapterSettingsCreateSubscriber Then
			SubscriberType = SyntheticCreate
		ElseIf sender Is SyntheticAdapterSettingsChangeSubscriber Then
			SubscriberType = SyntheticChange
		ElseIf sender Is SyntheticAdapterSettingsDeleteSubscriber Then
			SubscriberType = SyntheticDelete
		ElseIf sender Is EmulatedAdapterSettingsCreateSubscriber Then
			SubscriberType = EmulatedCreate
		ElseIf sender Is EmulatedAdapterSettingsChangeSubscriber Then
			SubscriberType = EmulatedChange
		ElseIf sender Is EmulatedAdapterSettingsDeleteSubscriber Then
			SubscriberType = EmulatedDelete
		End If
		RaiseEvent InventoryError(Me, New ModuleExceptionEventArgs With {.ModuleName = ModuleName, .[Error] = New Exception(String.Format(SubscriberErrorTemplate, SubscriberType, e.ErrorInstance.Message))})
		e.ErrorInstance.Dispose()
	End Sub

	Private Sub AddAdapter(ByVal NewAdapter As AdapterEntry)
		If Not String.IsNullOrEmpty(NewAdapter.MAC) Then
			SyncLock AdaptersLock
				CurrentAdapters.Add(CType(NewAdapter, AdapterEntry))
			End SyncLock
		End If
	End Sub

#Region "IDisposable Support"
	Private disposedValue As Boolean ' To detect redundant calls

	' IDisposable
	Protected Overridable Sub Dispose(disposing As Boolean)
		If Not disposedValue Then
			If disposing Then
				SyntheticAdapterSettingsCreateSubscriber.Dispose()
				SyntheticAdapterSettingsChangeSubscriber.Dispose()
				SyntheticAdapterSettingsDeleteSubscriber.Dispose()
				EmulatedAdapterSettingsCreateSubscriber.Dispose()
				EmulatedAdapterSettingsChangeSubscriber.Dispose()
				EmulatedAdapterSettingsDeleteSubscriber.Dispose()
			End If
		End If
		disposedValue = True
	End Sub
	Public Sub Dispose() Implements IDisposable.Dispose
		Dispose(True)
	End Sub
#End Region
End Class
