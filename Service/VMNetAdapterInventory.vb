﻿Imports WOLService.CIMitar
Imports Microsoft.Management.Infrastructure

Public Class VMNetAdapterInventory
	Implements IDisposable

	Public Structure AdapterEntry
		Public Property InstanceID As String
		Public Property MAC As String
	End Structure

	Public Property CurrentAdapters As List(Of AdapterEntry)

	Private ServiceLog As EventLog
	Private TargetSession As CimSession
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
			ServiceLog.WriteEntry(String.Format("Registered new virtual adapter with MAC {0}", NewAdapter.MAC), EventLogEntryType.Information)
		End If
		e.SubscribedEvent.Dispose()
	End Sub

	Private Sub OnChangeAdapter(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles SyntheticAdapterSettingsChangeSubscriber.EventReceived, EmulatedAdapterSettingsChangeSubscriber.EventReceived
		Dim AdapterFound As Boolean = False
		Dim ChangedAdapter As AdapterEntry = GetAdapterEntryFromInstance(e.SubscribedEvent.GetSourceInstance)
		If Not String.IsNullOrEmpty(ChangedAdapter.MAC) Then
			SyncLock CurrentAdapters
				CurrentAdapters.Where(
					Function(ByVal SearchAdapter As AdapterEntry) SearchAdapter.InstanceID = ChangedAdapter.InstanceID
					).ToList.ForEach(Sub(ByVal MatchAdapter As AdapterEntry)
											  MatchAdapter.InstanceID = ChangedAdapter.InstanceID
											  MatchAdapter.MAC = ChangedAdapter.MAC
											  AdapterFound = True
										  End Sub)
			End SyncLock
			If AdapterFound Then
				ServiceLog.WriteEntry(String.Format("Updated an adapter with MAC {0}", ChangedAdapter.MAC))
			Else
				AddAdapter(ChangedAdapter)
				ServiceLog.WriteEntry(String.Format("Added an adapter with MAC {0} from an update request", ChangedAdapter.MAC))
			End If
		End If
		e.SubscribedEvent.Dispose()
	End Sub

	Public Sub OnDeleteAdapter(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles SyntheticAdapterSettingsDeleteSubscriber.EventReceived, EmulatedAdapterSettingsDeleteSubscriber.EventReceived
		Dim ChangedAdapter As AdapterEntry = GetAdapterEntryFromInstance(e.SubscribedEvent.GetSourceInstance)
		If Not String.IsNullOrEmpty(ChangedAdapter.MAC) Then
			Dim RemovedAdapterCount As Integer = 0
			SyncLock CurrentAdapters
				RemovedAdapterCount = CurrentAdapters.RemoveAll(Function(ByVal SearchAdapter As AdapterEntry) SearchAdapter.InstanceID = ChangedAdapter.InstanceID)
			End SyncLock
			ServiceLog.WriteEntry(String.Format("Deleted {0} adapter(s)", RemovedAdapterCount))
		End If
		e.SubscribedEvent.Dispose()
	End Sub

	Public Sub OnSubscriberError(ByVal sender As Object, ByVal e As CimErrorEventArgs) Handles SyntheticAdapterSettingsCreateSubscriber.ErrorOccurred, SyntheticAdapterSettingsChangeSubscriber.ErrorOccurred, SyntheticAdapterSettingsDeleteSubscriber.ErrorOccurred, EmulatedAdapterSettingsCreateSubscriber.ErrorOccurred, EmulatedAdapterSettingsChangeSubscriber.ErrorOccurred, EmulatedAdapterSettingsDeleteSubscriber.ErrorOccurred
		ServiceLog.WriteEntry(String.Format("Error received from a subscriber: {0}", e.ErrorInstance.Message), EventLogEntryType.Error)
		e.ErrorInstance.Dispose()
	End Sub

	Private Sub AddAdapter(ByVal NewAdapter As AdapterEntry)
		If Not String.IsNullOrEmpty(NewAdapter.MAC) Then
			SyncLock CurrentAdapters
				CurrentAdapters.Add(CType(NewAdapter, AdapterEntry))
			End SyncLock
		End If
	End Sub

	Public Sub New(ByRef TargetSession As CimSession, ByVal Log As EventLog)
		Try
			CurrentAdapters = New List(Of AdapterEntry)
			ServiceLog = Log
			Me.TargetSession = TargetSession
			SyntheticAdapterSettingsCreateSubscriber = New CimSubscriptionController(TargetSession, CimNamespaceVirtualization) With {
				.QueryText = String.Format(CimSelectEventTemplate, CimInstanceCreationClassName, 1, CimClassNameSyntheticAdapterSettingData)}
			SyntheticAdapterSettingsChangeSubscriber = New CimSubscriptionController(TargetSession, CimNamespaceVirtualization) With {
				.QueryText = String.Format(CimSelectEventTemplate, CimInstanceModificationClassName, 1, CimClassNameSyntheticAdapterSettingData)}
			SyntheticAdapterSettingsDeleteSubscriber = New CimSubscriptionController(TargetSession, CimNamespaceVirtualization) With {
				.QueryText = String.Format(CimSelectEventTemplate, CimInstanceDeletionClassName, 1, CimClassNameSyntheticAdapterSettingData)}
			EmulatedAdapterSettingsCreateSubscriber = New CimSubscriptionController(TargetSession, CimNamespaceVirtualization) With {
				.QueryText = String.Format(CimSelectEventTemplate, CimInstanceCreationClassName, 1, CimClassNameEmulatedAdapterSettingData)}
			EmulatedAdapterSettingsChangeSubscriber = New CimSubscriptionController(TargetSession, CimNamespaceVirtualization) With {
				.QueryText = String.Format(CimSelectEventTemplate, CimInstanceModificationClassName, 1, CimClassNameEmulatedAdapterSettingData)}
			EmulatedAdapterSettingsDeleteSubscriber = New CimSubscriptionController(TargetSession, CimNamespaceVirtualization) With {
				.QueryText = String.Format(CimSelectEventTemplate, CimInstanceDeletionClassName, 1, CimClassNameEmulatedAdapterSettingData)}
			Reset()
		Catch ex As Exception
			ServiceLog.WriteEntry(String.Format("New failed with {0}", ex.Message), EventLogEntryType.Error)
		End Try
	End Sub

	Public Async Sub Reset()
		SyncLock CurrentAdapters
			If CurrentAdapters Is Nothing Then
				CurrentAdapters = New List(Of AdapterEntry)
			Else
				CurrentAdapters.Clear()
			End If
		End SyncLock

		SyntheticAdapterSettingsCreateSubscriber.Cancel()
		SyntheticAdapterSettingsChangeSubscriber.Cancel()
		SyntheticAdapterSettingsDeleteSubscriber.Cancel()
		EmulatedAdapterSettingsCreateSubscriber.Cancel()
		EmulatedAdapterSettingsChangeSubscriber.Cancel()
		EmulatedAdapterSettingsDeleteSubscriber.Cancel()

		For Each AdapterClassName As String In {CimClassNameSyntheticAdapterSettingData, CimClassNameEmulatedAdapterSettingData}
			Using AdapterEnumerator As New CimAsyncEnumerateInstancesController(TargetSession, CimNamespaceVirtualization, AdapterClassName)
				Try
					Dim FoundAdapters As List(Of CimInstance) = Await AdapterEnumerator.StartAsync
					For Each AdapterInstance As CimInstance In FoundAdapters
						AddAdapter(GetAdapterEntryFromInstance(AdapterInstance))
					Next
				Catch ex As Exception
					ServiceLog.WriteEntry(String.Format("DEBUG: AdapterEnumerator threw: {0}", ex.Message), EventLogEntryType.Error)
				End Try
			End Using
		Next
		ServiceLog.WriteEntry(String.Format("Enumerated {0} network adapters", CurrentAdapters.Count))

		SyntheticAdapterSettingsCreateSubscriber.Start()
		SyntheticAdapterSettingsChangeSubscriber.Start()
		SyntheticAdapterSettingsDeleteSubscriber.Start()
		EmulatedAdapterSettingsCreateSubscriber.Start()
		EmulatedAdapterSettingsChangeSubscriber.Start()
		EmulatedAdapterSettingsDeleteSubscriber.Start()
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
