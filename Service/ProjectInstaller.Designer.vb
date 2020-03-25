<System.ComponentModel.RunInstaller(True)> Partial Class ProjectInstaller
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
		Me.HyperViveServiceProcessInstaller = New System.ServiceProcess.ServiceProcessInstaller()
		Me.HyperViveServiceInstaller = New System.ServiceProcess.ServiceInstaller()
		'
		'HyperViveServiceProcessInstaller
		'
		Me.HyperViveServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem
		Me.HyperViveServiceProcessInstaller.Password = Nothing
		Me.HyperViveServiceProcessInstaller.Username = Nothing
		'
		'HyperViveServiceInstaller
		'
		Me.HyperViveServiceInstaller.Description = "Performs utility functions for Hyper-V virtual machines, such as wake-on-LAN oper" &
	"ations"
		Me.HyperViveServiceInstaller.DisplayName = "HyperVive"
		Me.HyperViveServiceInstaller.ServiceName = "HyperVive"
		Me.HyperViveServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic
		'
		'ProjectInstaller
		'
		Me.Installers.AddRange(New System.Configuration.Install.Installer() {Me.HyperViveServiceProcessInstaller, Me.HyperViveServiceInstaller})

	End Sub

	Friend WithEvents HyperViveServiceProcessInstaller As ServiceProcess.ServiceProcessInstaller
	Friend WithEvents HyperViveServiceInstaller As ServiceProcess.ServiceInstaller
End Class
