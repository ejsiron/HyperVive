﻿Imports Microsoft.Management.Infrastructure

Public Interface IServiceModule
	Property ModuleName As String
	Property IsRunning As Boolean
	Sub Start()
	Sub [Stop]()
End Interface

Public Class ModuleController
	Public Shared Function Start(ByVal MainService As HyperViveService) As ModuleController
		If ControllerInstance Is Nothing Then
			ControllerInstance = New ModuleController(MainService)
		End If
		Return ControllerInstance
	End Function

	Public Shared Function IsRunning() As Boolean
		Return ControllerInstance IsNot Nothing
	End Function

	Public Sub [Stop]()
		If ControllerInstance IsNot Nothing Then
			ControllerInstance = Nothing
		End If
	End Sub

	Private ReadOnly Service As HyperViveService
	Private ReadOnly LocalCimSession As CimSession
	Private ReadOnly EventTemplate As New EventInstance(0L, 0)
	Private Shared ControllerInstance As ModuleController = Nothing
	Private Sub New(ByVal MainService As HyperViveService)
		Service = MainService
		LocalCimSession = CimSession.Create(Nothing)
	End Sub

	Private Sub WriteEventLogEntry(ByVal EventId As Long, ByVal EventCategory As Integer, Parameters As Object(), Optional EventType As EventLogEntryType = EventLogEntryType.Information)
		EventTemplate.InstanceId = EventId
		EventTemplate.CategoryId = EventCategory
		Service.EventLog.WriteEvent(EventTemplate, Parameters)
	End Sub
End Class