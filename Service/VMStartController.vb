Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure

Public Class VMInfo
	Public Property MacAddress As String
	Public Property ID As String
	Public Property Name As String
End Class

Public Module VMEvents
	Public Class VMStartedEventArgs
		Inherits EventArgs
		Public Property VirtualMachineInfo As VMInfo
	End Class

	Public Class VMStartFailureEventArgs
		Inherits VMStartedEventArgs
		Public Property ErrorCode As UInt16
		Public Property Reason As String
	End Class
End Module

Public Class VMStartController
	Private Session As CimSession

	Public Event VMStarted(ByVal sender As Object, ByVal e As VMStartedEventArgs)
	Public Event VMStartFailure(ByVal sender As Object, ByVal e As VMStartFailureEventArgs)
	Public Event VMStartDebugMessage(ByVal sender As Object, ByVal e As DebugMessageEventArgs)
	Public Event VMStartError(ByVal sender As Object, ByVal e As ModuleExceptionEventArgs)

	Public Sub New(ByVal Session As CimSession)
		Me.Session = Session
	End Sub

	Public Async Sub Start(ByVal MacAddress As String, ByVal VmIDs As List(Of String))
		Using VMLister As New CimAsyncQueryInstancesController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimQueryTemplateVirtualMachine, ConvertVMListToQueryFilter(VmIDs)),
			.KeysOnly = True
		}
			Using TargetVMs As CimInstanceList = Await VMLister.StartAsync
				Parallel.ForEach(TargetVMs,
					Async Sub(ByVal VM As CimInstance)
						Dim CurrentState As VirtualMachineStates = CType(VM.CimInstanceProperties(CimPropertyNameEnabledState).Value, VirtualMachineStates)
						Dim Info As New VMInfo With {
							.ID = VM.GetInstancePropertyValueString(CimPropertyNameName),
							.Name = VM.GetInstancePropertyValueString(CimPropertyNameElementName),
							.MacAddress = MacAddress
						}
						Dim ResultCode As UShort = VirtualizationMethodErrors.InvalidState
						Dim JobID As String = String.Empty
						If IsVmStartable(CurrentState) Then
							Using VMStartController As New CimAsyncInvokeInstanceMethodController(Session, CimNamespaceVirtualization) With {.Instance = VM}
								VMStartController.MethodName = CimMethodNameRequestStateChange
								VMStartController.InputParameters.Add(CimMethodParameter.Create(CimParameterNameRequestedState, VirtualMachineStates.Running, CimType.UInt16, 0))
								Using StartResult As CimMethodResult = Await VMStartController.StartAsync
									ResultCode = CUShort(StartResult.ReturnValue.Value)
									If ResultCode = VirtualizationMethodErrors.JobStarted Then
										Dim JobReference As CimInstance = CType(StartResult.OutParameters(CimParameterNameJob).Value, CimInstance)
										JobID = JobReference.GetInstancePropertyValueString(CimPropertyNameInstanceID)
										RaiseEvent VMStartDebugMessage(Me, New DebugMessageEventArgs With {.Message = String.Format(JobCreatedMessageTemplate, JobID, Info.Name, Info.ID)})
										Await WatchVMStartJob(JobID, Info)
									Else
										ProcessVMStartResult(ResultCode, Info)
									End If
								End Using
							End Using
						End If
					End Sub)
			End Using
		End Using
	End Sub

	Private Const ModuleName As String = "VM Starter"
	Private Const DefaultFailureReason As String = "Code not recognized"
	Private Const JobNotFoundErrorTemplate As String = "Received a WOL packet for MAC {0} which mapped to VM {1} with ID {2}. An attempt was made to start it on a job with instance ID {3}, but the job was not found"
	Private Const JobCreatedMessageTemplate As String = "Created start job with instance ID {0} for VM {1} with GUID {0}"

	Private Shared Function ConvertVMListToQueryFilter(ByVal VmIDs As List(Of String)) As String
		Dim ListEntries As New List(Of String)(VmIDs.Count)
		For Each VmID As String In VmIDs
			ListEntries.Add(String.Format(" Name='{0}'", VmID))
		Next
		Return "WHERE" & String.Join(" AND", ListEntries)
	End Function

	Private Shared Function IsVmStartable(ByVal CurrentState As VirtualMachineStates) As Boolean
		Return CurrentState = VirtualMachineStates.Off OrElse CurrentState = VirtualMachineStates.Saved
	End Function

	Private Sub ProcessVMStartResult(ByVal ResultCode As UShort, ByVal Info As VMInfo)
		If ResultCode = VirtualizationMethodErrors.NoError Then
			RaiseEvent VMStarted(Me, New VMStartedEventArgs With {.VirtualMachineInfo = Info})
		Else  ' take care not to call with a 4096
			Dim FailureReason As String
			If [Enum].IsDefined(GetType(VirtualizationMethodErrors), ResultCode) Then
				FailureReason = CType(ResultCode, VirtualizationMethodErrors).ToString
			Else
				FailureReason = DefaultFailureReason
			End If
			RaiseEvent VMStartFailure(Me, New VMStartFailureEventArgs With {.VirtualMachineInfo = Info, .ErrorCode = ResultCode, .Reason = FailureReason})
		End If
	End Sub

	Private Async Function WatchVMStartJob(ByVal JobInstanceID As String, ByVal Info As VMInfo) As Task
		Dim Job As CimInstance
		Dim JobList As CimInstanceList
		Dim JobState As JobStates = JobStates.New
		Using JobChecker As New CimAsyncQueryInstancesController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimQueryTemplateMsvmConcreteJob, JobInstanceID)
			}
			JobList = Await JobChecker.StartAsync
		End Using
		If JobList.Count > 0 Then
			Job = JobList.First
			While JobState = JobStates.Running OrElse JobState = JobStates.New OrElse JobState = JobStates.Starting
				Job.Refresh(Session)
				JobState = CType(Job.CimInstanceProperties(CimPropertyNameJobState).Value, JobStates)
			End While
		Else
			RaiseEvent VMStartError(Me, New ModuleExceptionEventArgs With {.ModuleName = ModuleName, .[Error] =
				New Exception(String.Format(JobNotFoundErrorTemplate, Info.MacAddress, Info.Name, Info.ID, JobInstanceID))})
			Return
		End If
		ProcessVMStartResult(CUShort(Job.CimInstanceProperties(CimPropertyNameErrorCode).Value), Info)
	End Function
End Class
