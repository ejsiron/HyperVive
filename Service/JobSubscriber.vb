Imports HyperVive.CIMitar
Imports Microsoft.Management.Infrastructure

' TODO: genericize, especially constructors, move to CIMitar

Public Class JobSubscriber
	Implements IDisposable

	Public Event DebugMessageGenerated(ByVal sender As Object, ByVal e As DebugMessageEventArgs)
	Public Event CimException(ByVal sender As Object, ByVal e As CimErrorEventArgs)
	Public Shared Event JobWatcherException(ByVal sender As Object, ByVal e As ModuleExceptionEventArgs)
	Public Event SubscribedJobCreated(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs)

	Public ReadOnly LastCimError As CimException = Subscriber?.LastError

	Public ReadOnly Property CimNamespace As String
		Get
			Return Subscriber?.Namespace
		End Get
	End Property
	Public ReadOnly Property CimClassName As String
		Get
			Return Subscriber?.WatchedClassName
		End Get
	End Property

	Public Shared Function GetCreationSubscriber(ByVal Session As CimSession, ByVal [Namespace] As String, ByVal ClassName As String) As JobSubscriber
		Return GetSubscriber(Session, IndicationType.Creation, [Namespace], ClassName)
	End Function

	Public Shared Function GetModificationSubscriber(ByVal Session As CimSession, ByVal [Namespace] As String, ByVal ClassName As String) As JobSubscriber
		Return GetSubscriber(Session, IndicationType.Modification, [Namespace], ClassName)
	End Function

	Public Shared Function GetDeletionSubscriber(ByVal Session As CimSession, ByVal [Namespace] As String, ByVal ClassName As String) As JobSubscriber
		Return GetSubscriber(Session, IndicationType.Deletion, [Namespace], ClassName)
	End Function

	''' <summary>
	''' Waits asynchronously for an Msvm_ConcreteJob to complete its task.
	''' </summary>
	''' <param name="Caller">The source class that awaits completion. Used as the sender in exception events.</param>
	''' <param name="Session">The <see cref="CimSession"/> that owns the job.</param>
	''' <param name="JobInstanceID">The InstanceID of the target Msvm_ConcreteJob, in string form.</param>
	''' <returns>A <see cref="Task(Of KeyValuePair(Of UShort, String))"/> that contains the job's return code and status text.</returns>
	Public Shared Async Function WatchVirtualizationJobCompletionAsync(ByVal Caller As Object, ByVal Session As CimSession, ByVal JobInstanceID As String) As Task(Of KeyValuePair(Of UShort, String))
		Dim Job As CimInstance
		Dim JobList As CimInstanceList
		Dim JobState As JobStates = JobStates.New
		Dim ErrorCode As UShort
		Dim JobStatus As String = JobNotFoundMessage
		Using JobChecker As New CimAsyncQueryInstancesController(Session, CimNamespaceVirtualization) With {
			.QueryText = String.Format(CimQueryTemplateMsvmConcreteJob, JobInstanceID)
			}
			JobList = Await JobChecker.StartAsync
			If JobList.Count > 0 Then
				Job = JobList.First
				While JobState = JobStates.Running OrElse JobState = JobStates.New OrElse JobState = JobStates.Starting
					Job.Refresh(Session)
					JobState = CType(Job.CimInstanceProperties(CimPropertyNameJobState).Value, JobStates)
				End While
				ErrorCode = Job.GetInstancePropertyUInt16Value(CimPropertyNameErrorCode)
				JobStatus = Job.GetInstancePropertyStringValue(CimPropertyNameJobStatus)
			Else
				RaiseEvent JobWatcherException(Caller, New ModuleExceptionEventArgs With {.ModuleName = ModuleName, .[Error] =
					New Exception(String.Format(JobNotFoundErrorTemplate, JobInstanceID))})
				JobState = JobStates.Exception
			End If
			Return New KeyValuePair(Of UShort, String)(ErrorCode, JobStatus)
		End Using
	End Function

	Public Overrides Function Equals(obj As Object) As Boolean
		Dim other As JobSubscriber = TryCast(obj, JobSubscriber)
		If other Is Nothing Then
			Return False
		Else
			Return Me = other
		End If
	End Function

	Public Shared Operator =(ByVal x As JobSubscriber, y As JobSubscriber) As Boolean
		Return x.Session.InstanceId = y.Session.InstanceId AndAlso
			x.CimNamespace = y.CimNamespace AndAlso
			x.CimClassName = y.CimClassName
	End Operator

	Public Shared Operator <>(ByVal x As JobSubscriber, y As JobSubscriber) As Boolean
		Return Not x = y
	End Operator

	Private Shared Function GetSubscriber(ByVal Session As CimSession, ByVal WatchType As IndicationType, ByVal [Namespace] As String, ByVal WatchedClassName As String) As JobSubscriber
		Dim DebugAction As String = NewSubscriberText
		Dim NewSubscriber As JobSubscriber = Subscribers.Where(
			Function(ByVal CheckSubscriberEntry As KeyValuePair(Of JobSubscriber, Long))
				With CheckSubscriberEntry.Key.Subscriber
					Return Session.InstanceId = CheckSubscriberEntry.Key.Session.InstanceId AndAlso
					[Namespace] = .Namespace AndAlso
					WatchType = .WatchType AndAlso
					WatchedClassName = .WatchedClassName
				End With
			End Function
			).FirstOrDefault.Key
		If NewSubscriber Is Nothing Then
			NewSubscriber = New JobSubscriber(Session, WatchType, [Namespace], WatchedClassName)
			Subscribers.Add(NewSubscriber, 1L)
			DebugAction = ReusedSubscriberText
		Else
			Subscribers(NewSubscriber) += 1L
		End If
		NewSubscriber.SendDebugMessage(String.Format(DebugSubscriberTemplate, DebugAction, NewSubscriber.Subscriber.QueryText))
		Return NewSubscriber
	End Function

	Private Session As CimSession
	Private WithEvents Subscriber As InstanceIndicationController
	Private Shared Subscribers As New Dictionary(Of JobSubscriber, Long)(0)
	Private Const ModuleName As String = "Job Subscriber"
	Private Const NewSubscriberText As String = "Created new"
	Private Const ReusedSubscriberText As String = "Reused existing"
	Private Const DecrementedSubscriberText As String = "Decremented refcount for"
	Private Const DeletedSubscriberText As String = "Deleted"
	Private Const DebugSubscriberTemplate As String = "{0} subscriber with query: {1}"
	Private Const JobNotFoundMessage As String = "Job not found"
	Private Const JobNotFoundErrorTemplate As String = "Attemped to process a job with instance ID {0} but the job was not found."

	Private Sub New(ByVal Session As CimSession, ByVal WatchType As IndicationType, ByVal [Namespace] As String, ByVal WatchedClassName As String)
		Subscriber = InstanceIndicationController.IndicationControllerFactory(Session, WatchType, [Namespace], WatchedClassName)
		Subscriber.Start()
	End Sub

	Private Sub PassSubscribedEvent(ByVal sender As Object, ByVal e As CimSubscribedEventReceivedArgs) Handles Subscriber.EventReceived
		RaiseEvent SubscribedJobCreated(Me, e)
	End Sub

	Private Sub PassCimError(ByVal sender As Object, ByVal e As CimErrorEventArgs) Handles Subscriber.ErrorOccurred
		RaiseEvent CimException(Me, e)
	End Sub

	Private Sub SendDebugMessage(ByVal Message As String)
		RaiseEvent DebugMessageGenerated(Me, New DebugMessageEventArgs With {.Message = Message})
	End Sub

#Region "IDisposable Support"
	Private disposedValue As Boolean

	Protected Overridable Sub Dispose(disposing As Boolean)
		If Not disposedValue Then
			If disposing Then
				Dim ShouldDispose As Boolean = True
				Dim DebugAction As String = DeletedSubscriberText
				If Subscribers.ContainsKey(Me) Then
					Subscribers(Me) -= 1L
					If Subscribers(Me) > 0 Then
						ShouldDispose = False
						DebugAction = DecrementedSubscriberText
					Else
						Subscribers.Remove(Me)
					End If
					SendDebugMessage(String.Format(DebugSubscriberTemplate, DebugAction, Subscriber.QueryText))
					If ShouldDispose Then Subscriber?.Dispose()
				End If
			End If
		End If
		disposedValue = True
	End Sub

	Public Sub Dispose() Implements IDisposable.Dispose
		Dispose(True)
	End Sub
#End Region
End Class
