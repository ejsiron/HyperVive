Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure

Public Class JobSubscriber
	Implements IDisposable

	Public Sub New(ByVal Session As CimSession)
		Me.Session = Session
		Subscriber = New CimSubscriptionController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimQueryTemplateTimedEvent, CimClassNameInstanceCreation, 1, CimClassNameVirtualizationJob)}
	End Sub

	Public Sub Start()
		Subscriber.Start()
	End Sub

	Public Sub [Stop]()
		Subscriber.Cancel()
	End Sub

	Private Session As CimSession
	Private Const ModuleName As String = "Job Subcriber"
	Private Subscriber As CimSubscriptionController

#Region "IDisposable Support"
	Private disposedValue As Boolean ' To detect redundant calls

	' IDisposable
	Protected Overridable Sub Dispose(disposing As Boolean)
		If Not disposedValue Then
			If disposing Then
				Subscriber.Dispose()
			End If
		End If
		disposedValue = True
	End Sub

	Public Sub Dispose() Implements IDisposable.Dispose
		Dispose(True)
	End Sub
#End Region
End Class
