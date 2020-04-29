Imports Microsoft.Management.Infrastructure

Public MustInherit Class ModuleBase
	Protected Sub New(DebugMessageAction As Action(Of String))
		LogDebugMessage = DebugMessageAction
	End Sub
	Public MustOverride Property ModuleName As String

	Public MustOverride ReadOnly Property IsRunning As Boolean

	Public MustOverride Sub Start()

	Public MustOverride Sub [Stop]()

	Protected ReadOnly LogDebugMessage As Action(Of String)
End Class

Public MustInherit Class ModuleWithCimBase
	Inherits ModuleBase

	Private ReadOnly Session As CimSession

	Protected Sub New(ByVal Session As CimSession, ByVal DebugMessageAction As Action(Of String))
		MyBase.New(DebugMessageAction)
		Me.Session = Session
	End Sub
End Class

Partial Public Class ModuleController
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

	Private Shared Service As HyperViveService
	Private Shared LocalCimSession As CimSession
	Private Shared ControllerInstance As ModuleController = Nothing

	Private Sub New(ByVal MainService As HyperViveService)
		Service = MainService
		LocalCimSession = CimSession.Create(Nothing)
	End Sub
End Class