Imports System.IO
Imports HyperVive.CIMitar
Imports HyperVive.CIMitar.Virtualization
Imports Microsoft.Management.Infrastructure

Public Class VMNetAdapterInventory
	Inherits ModuleWithCimBase
	Implements IRunningModule
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
		''' The virtual adapter's MAC address, unformatted
		''' </summary>
		''' <returns><see cref="String"/></returns>
		Public Property MAC As String

		''' <summary>
		''' True when the adapter is emulated, false when it is synthetic
		''' </summary>
		''' <returns><see cref="Boolean"/></returns>
		Public Property IsEmulated As Boolean
	End Structure

	Public Sub New(ByVal Session As CimSession, ByVal ModuleLogger As IModuleLogger, ByVal VirtualNetAdapterLogger As IVirtualNetAdapterLogger)
		MyBase.New(Session, ModuleLogger)
		Me.VirtualNetAdapterLogger = VirtualNetAdapterLogger
	End Sub

	''' <summary>
	''' Starts all subscribers. Resets any existing subscribers as necessary
	''' </summary>
	Public Async Sub Start() Implements IRunningModule.Start
		[Stop]()

		SyncLock AdaptersLock
			If CurrentAdapters Is Nothing Then
				CurrentAdapters = New List(Of AdapterEntry)
			Else
				CurrentAdapters.Clear()
			End If
		End SyncLock

		For Each AdapterClassName As String In {CimClassNameSyntheticAdapterSettingData, CimClassNameEmulatedAdapterSettingData}
			Using AdapterEnumerator As New CimAsyncEnumerateInstancesController(Session, NamespaceVirtualization, AdapterClassName)
				Using FoundAdapters As CimInstanceCollection = Await AdapterEnumerator.StartAsync
					For Each AdapterInstance As CimInstance In FoundAdapters
						AddAdapter(GetAdapterEntryFromInstance(AdapterInstance))
					Next
				End Using
			End Using
		Next
		VirtualNetAdapterLogger.LogDebugVirtualAdapterEnumeratedCount(CurrentAdapters.Count)

		SyntheticAdapterSettingsCreateSubscriber = New InstanceCreationController(Session, NamespaceVirtualization, CimClassNameSyntheticAdapterSettingData, AddressOf OnNewAdapter, AddressOf ReportError)
		SyntheticAdapterSettingsChangeSubscriber = New InstanceModificationController(Session, NamespaceVirtualization, CimClassNameSyntheticAdapterSettingData, AddressOf OnChangeAdapter, AddressOf ReportError)
		SyntheticAdapterSettingsDeleteSubscriber = New InstanceDeletionController(Session, NamespaceVirtualization, CimClassNameSyntheticAdapterSettingData, AddressOf OnDeleteAdapter, AddressOf ReportError)
		EmulatedAdapterSettingsCreateSubscriber = New InstanceCreationController(Session, NamespaceVirtualization, CimClassNameEmulatedAdapterSettingData, AddressOf OnNewAdapter, AddressOf ReportError)
		EmulatedAdapterSettingsChangeSubscriber = New InstanceModificationController(Session, NamespaceVirtualization, CimClassNameEmulatedAdapterSettingData, AddressOf OnChangeAdapter, AddressOf ReportError)
		EmulatedAdapterSettingsDeleteSubscriber = New InstanceDeletionController(Session, NamespaceVirtualization, CimClassNameEmulatedAdapterSettingData, AddressOf OnDeleteAdapter, AddressOf ReportError)

		SyntheticAdapterSettingsCreateSubscriber.Start()
		SyntheticAdapterSettingsChangeSubscriber.Start()
		SyntheticAdapterSettingsDeleteSubscriber.Start()
		EmulatedAdapterSettingsCreateSubscriber.Start()
		EmulatedAdapterSettingsChangeSubscriber.Start()
		EmulatedAdapterSettingsDeleteSubscriber.Start()
		_IsRunning = True
	End Sub

	Public ReadOnly Property IsRunning As Boolean Implements IRunningModule.IsRunning
		Get
			Return _IsRunning
		End Get
	End Property

	Public Sub [Stop]() Implements IRunningModule.Stop
		SyntheticAdapterSettingsCreateSubscriber?.Dispose()
		SyntheticAdapterSettingsChangeSubscriber?.Dispose()
		SyntheticAdapterSettingsDeleteSubscriber?.Dispose()
		EmulatedAdapterSettingsCreateSubscriber?.Dispose()
		EmulatedAdapterSettingsChangeSubscriber?.Dispose()
		EmulatedAdapterSettingsDeleteSubscriber?.Dispose()
		_IsRunning = False
	End Sub

	''' <summary>
	''' Find the virtual machine that owns a given MAC address
	''' </summary>
	''' <param name="MacAddress">The desired MAC in unformatted <see cref="String"/> format</param>
	''' <returns>All virtual machine IDs that own an adapter with a matching MAC, in <see cref="List(Of String)"/> format</returns>
	Public Function GetVmIDFromMac(ByVal MacAddress As String) As List(Of String)
		Dim MatchingMacs As New List(Of String)
		SyncLock AdaptersLock
			CurrentAdapters.Where(Function(SearchAdapter As AdapterEntry) SearchAdapter.MAC = MacAddress
				).ToList.ForEach(Sub(MatchingAdapter As AdapterEntry) MatchingMacs.Add(ExtractVmIDFromInstanceID(MatchingAdapter.InstanceID)))
		End SyncLock
		Return MatchingMacs
	End Function

	Public Overrides ReadOnly Property ModuleName As String = "Virtual Network Adapter Inventory"

	Private VirtualNetAdapterLogger As IVirtualNetAdapterLogger
	Private AdaptersLock As New Object
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
			VirtualNetAdapterLogger.LogInvalidVirtualAdapter(InstanceID)
			Return Guid.Empty.ToString
		End Try
	End Function

	Private _IsRunning As Boolean = False
	Private SyntheticAdapterSettingsCreateSubscriber As InstanceCreationController
	Private SyntheticAdapterSettingsChangeSubscriber As InstanceModificationController
	Private SyntheticAdapterSettingsDeleteSubscriber As InstanceDeletionController
	Private EmulatedAdapterSettingsCreateSubscriber As InstanceCreationController
	Private EmulatedAdapterSettingsChangeSubscriber As InstanceModificationController
	Private EmulatedAdapterSettingsDeleteSubscriber As InstanceDeletionController

	Private Function GetAdapterEntryFromInstance(ByVal Instance As CimInstance) As AdapterEntry
		Dim NewEntry As New AdapterEntry
		If Instance IsNot Nothing Then
			NewEntry.InstanceID = Instance.InstancePropertyString(PropertyNameInstanceID)
			NewEntry.MAC = Instance.InstancePropertyString(PropertyNameAddress)
			NewEntry.IsEmulated = Instance.CimSystemProperties.ClassName = CimClassNameEmulatedAdapterSettingData
		End If
		Return NewEntry
	End Function

	Private Sub OnNewAdapter(ByVal Result As CimSubscriptionResult)
		Dim NewAdapter As AdapterEntry = GetAdapterEntryFromInstance(Result.GetSourceInstance)
		If Not String.IsNullOrEmpty(NewAdapter.MAC) Then
			AddAdapter(NewAdapter)
			VirtualNetAdapterLogger.LogDebugVirtualAdapterEvent(NewAdapter.MAC, IVirtualNetAdapterLogger.VirtualAdapterAction.Added, NewAdapter.IsEmulated)
		End If
		Result.Dispose()
	End Sub

	Private Sub OnChangeAdapter(ByVal Result As CimSubscriptionResult)
		Dim AdapterFound As Boolean = False
		Dim ChangedAdapter As AdapterEntry = GetAdapterEntryFromInstance(Result.GetSourceInstance)
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
				VirtualNetAdapterLogger.LogDebugVirtualAdapterEvent(ChangedAdapter.MAC, IVirtualNetAdapterLogger.VirtualAdapterAction.Changed, ChangedAdapter.IsEmulated)
			Else
				AddAdapter(ChangedAdapter)
				VirtualNetAdapterLogger.LogDebugVirtualAdapterEvent(ChangedAdapter.MAC, IVirtualNetAdapterLogger.VirtualAdapterAction.AddedFromUpdate, ChangedAdapter.IsEmulated)
			End If
		End If
		Result.Dispose()
	End Sub

	Private Sub OnDeleteAdapter(ByVal Result As CimSubscriptionResult)
		Dim DeletedAdapter As AdapterEntry = GetAdapterEntryFromInstance(Result.GetSourceInstance)
		If Not String.IsNullOrEmpty(DeletedAdapter.MAC) Then
			Dim RemovedAdapterCount As Integer = 0
			SyncLock AdaptersLock
				RemovedAdapterCount = CurrentAdapters.RemoveAll(Function(ByVal SearchAdapter As AdapterEntry) SearchAdapter.InstanceID = DeletedAdapter.InstanceID)
			End SyncLock
			VirtualNetAdapterLogger.LogDebugVirtualAdapterEvent(DeletedAdapter.MAC, IVirtualNetAdapterLogger.VirtualAdapterAction.Deleted, DeletedAdapter.IsEmulated)
		End If
		Result.Dispose()
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
				[Stop]()
			End If
		End If
		disposedValue = True
	End Sub
	Public Sub Dispose() Implements IDisposable.Dispose
		Dispose(True)
	End Sub
#End Region
End Class
