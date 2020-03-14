﻿Imports WOLService.CIMitar
Imports Microsoft.Management.Infrastructure

Public Class VMNetAdapterInventory
	Public Structure AdapterEntry
		Public Property DeviceID As String
		Public Property MAC As String
		Public Property VMID As String
	End Structure

	Public Property CurrentAdapters As List(Of AdapterEntry)

	Private TargetSession As CimSession
	Private WithEvents SyntheticAdapterCreateSubscriber As CimSubscriptionController
	Private WithEvents SyntheticAdapterChangeSubscriber As CimSubscriptionController
	Private WithEvents SyntheticAdapterDeleteSubscriber As CimSubscriptionController
	Private WithEvents EmulatedAdapterCreateSubscriber As CimSubscriptionController
	Private WithEvents EmulatedAdapterChangeSubscriber As CimSubscriptionController
	Private WithEvents EmulatedAdapterDeleteSubscriber As CimSubscriptionController

	Private Sub OnNewAdapter(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles SyntheticAdapterCreateSubscriber.EventReceived, EmulatedAdapterCreateSubscriber.EventReceived
		AddAdapter(e.SubscribedEvent.GetSourceInstance)
		e.SubscribedEvent.Dispose()
	End Sub

	Private Sub OnChangeAdapter(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles SyntheticAdapterChangeSubscriber.EventReceived, EmulatedAdapterChangeSubscriber.EventReceived
		Dim AdapterInstance As CimInstance = e.SubscribedEvent.GetSourceInstance
		Dim AdapterFound As Boolean = False
		SyncLock CurrentAdapters
			CurrentAdapters.Where(
				Function(ByVal SearchAdapter As AdapterEntry) SearchAdapter.DeviceID = AdapterInstance.GetInstancePropertyValueString(CimPropertyNameDeviceID)
				).ToList.ForEach(Sub(ByVal MatchAdapter As AdapterEntry)
										  MatchAdapter.MAC = AdapterInstance.GetInstancePropertyValueString(CimPropertyNamePermanentAddress)
										  MatchAdapter.VMID = AdapterInstance.GetInstancePropertyValueString(CimPropertyNameSystemName)
										  AdapterFound = True
									  End Sub)
		End SyncLock
		If Not AdapterFound Then
			AddAdapter(AdapterInstance)
		End If
		e.SubscribedEvent.Dispose()
	End Sub

	Public Sub OnDeleteAdapter(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles SyntheticAdapterDeleteSubscriber.EventReceived, EmulatedAdapterDeleteSubscriber.EventReceived
		Dim AdapterInstance As CimInstance = e.SubscribedEvent.GetSourceInstance
		SyncLock CurrentAdapters
			CurrentAdapters.RemoveAll(Function(ByVal SearchAdapter As AdapterEntry) SearchAdapter.DeviceID = AdapterInstance.GetInstancePropertyValueString("DeviceID"))
		End SyncLock
	End Sub

	Private Sub AddAdapter(ByVal AdapterInstance As CimInstance)
		SyncLock CurrentAdapters
			CurrentAdapters.Add(New AdapterEntry With {
			.DeviceID = AdapterInstance.GetInstancePropertyValueString(CimPropertyNameDeviceID),
			.MAC = AdapterInstance.GetInstancePropertyValueString(CimPropertyNamePermanentAddress),
			.VMID = AdapterInstance.GetInstancePropertyValueString(CimPropertyNameSystemName)
			})
		End SyncLock
	End Sub

	Public Sub New(ByRef TargetSession As CimSession)
		Me.TargetSession = TargetSession
		SyntheticAdapterCreateSubscriber = New CimSubscriptionController(TargetSession, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimSelectEventTemplate, CimInstanceCreationClassName, 1, CimClassNameSyntheticAdapter)}
		SyntheticAdapterChangeSubscriber = New CimSubscriptionController(TargetSession, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimSelectEventTemplate, CimInstanceModificationClassName, 1, CimClassNameSyntheticAdapter)}
		SyntheticAdapterDeleteSubscriber = New CimSubscriptionController(TargetSession, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimSelectEventTemplate, CimInstanceDeletionClassName, 1, CimClassNameSyntheticAdapter)}
		EmulatedAdapterCreateSubscriber = New CimSubscriptionController(TargetSession, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimSelectEventTemplate, CimInstanceCreationClassName, 1, CimClassNameEmulatedAdapter)}
		EmulatedAdapterChangeSubscriber = New CimSubscriptionController(TargetSession, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimSelectEventTemplate, CimInstanceModificationClassName, 1, CimClassNameEmulatedAdapter)}
		EmulatedAdapterDeleteSubscriber = New CimSubscriptionController(TargetSession, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimSelectEventTemplate, CimInstanceDeletionClassName, 1, CimClassNameEmulatedAdapter)}
		Reset()
	End Sub

	Public Async Sub Reset()
		SyncLock CurrentAdapters
			If CurrentAdapters Is Nothing Then
				CurrentAdapters = New List(Of AdapterEntry)
			Else
				CurrentAdapters.Clear()
			End If
		End SyncLock

		SyntheticAdapterCreateSubscriber.Cancel()
		SyntheticAdapterChangeSubscriber.Cancel()
		SyntheticAdapterDeleteSubscriber.Cancel()
		EmulatedAdapterCreateSubscriber.Cancel()
		EmulatedAdapterChangeSubscriber.Cancel()
		EmulatedAdapterDeleteSubscriber.Cancel()

		For Each AdapterClassName As String In {CimClassNameSyntheticAdapter, CimClassNameEmulatedAdapter}
			Using AdapterEnumerator As New CimAsyncEnumerateInstancesController(TargetSession, CimNamespaceVirtualization, AdapterClassName)
				Try
					Dim FoundAdapters As List(Of CimInstance) = Await AdapterEnumerator.StartAsync
					For Each AdapterInstance As CimInstance In FoundAdapters
						AddAdapter(AdapterInstance)
					Next
				Catch ex As Exception

				End Try
			End Using
		Next

		SyntheticAdapterCreateSubscriber.Start()
		SyntheticAdapterChangeSubscriber.Start()
		SyntheticAdapterDeleteSubscriber.Start()
		EmulatedAdapterCreateSubscriber.Start()
		EmulatedAdapterChangeSubscriber.Start()
		EmulatedAdapterDeleteSubscriber.Start()
	End Sub
End Class
