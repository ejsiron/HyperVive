﻿<System.ComponentModel.RunInstaller(True)> Partial Class ProjectInstaller
	Inherits System.Configuration.Install.Installer

	'Installer overrides dispose to clean up the component list.
	<System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		Try
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
		Finally
			MyBase.Dispose(disposing)
		End Try
	End Sub

	'Required by the Component Designer
	Private components As System.ComponentModel.IContainer

	'NOTE: The following procedure is required by the Component Designer
	'It can be modified using the Component Designer.  
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()>
	Private Sub InitializeComponent()
		Me.WOLServiceProcessInstaller = New System.ServiceProcess.ServiceProcessInstaller()
		Me.WOLServiceInstaller = New System.ServiceProcess.ServiceInstaller()
		'
		'WOLServiceProcessInstaller
		'
		Me.WOLServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem
		Me.WOLServiceProcessInstaller.Password = Nothing
		Me.WOLServiceProcessInstaller.Username = Nothing
		'
		'WOLServiceInstaller
		'
		Me.WOLServiceInstaller.DisplayName = "Hyper-Vive"
		Me.WOLServiceInstaller.ServiceName = "HyperVive"
		'
		'ProjectInstaller
		'
		Me.Installers.AddRange(New System.Configuration.Install.Installer() {Me.WOLServiceProcessInstaller, Me.WOLServiceInstaller})

	End Sub

	Friend WithEvents WOLServiceProcessInstaller As ServiceProcess.ServiceProcessInstaller
	Friend WithEvents WOLServiceInstaller As ServiceProcess.ServiceInstaller
End Class
