' This file is part of the CIMitar project, a module to ease interaction with the CIM module
' CIMitar, Copyright 2020 Eric Siron
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
' The above copyright notice and this permission notice shall be included in all copies or substantial portions
' of the Software.
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
' THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
' TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
' SOFTWARE.

Imports HyperVive.CIMitar.Virtualization
Imports Microsoft.Management.Infrastructure

Namespace CIMitar

	Public Class JobSubscriberData
		Public ReadOnly [Namespace] As String
		Public ReadOnly ClassName As String
		Public ReadOnly Callback As String
		Public ReadOnly PropertyMatches As Dictionary(Of String, KeyValuePair(Of CimType, Object))

		Public Overrides Function Equals(obj As Object) As Boolean
			Dim other As JobSubscriberData = TryCast(obj, JobSubscriberData)
			If other IsNot Nothing AndAlso
					other.Namespace = [Namespace] AndAlso
					other.ClassName = ClassName AndAlso
					other.Callback = Callback AndAlso
					other.PropertyMatches.Count = PropertyMatches.Count Then

				For Each ValueSet As KeyValuePair(Of String, KeyValuePair(Of CimType, Object)) In PropertyMatches
					If Not (other.PropertyMatches.Keys.Contains(ValueSet.Key) AndAlso
						ValueSet.Value.Key = other.PropertyMatches(ValueSet.Key).Key) Then
						' todo: continue here, compare the value
					End If
					Return True
				Next
			End If
			Return False
		End Function
	End Class

	Public Class JobSubscriber

		Public ReadOnly ID As New Guid
		Public ReadOnly Session As CimSession
		Public ReadOnly [Namespace] As String
		Public ReadOnly ClassName As String
		Public ReadOnly JobType As VirtualizationJobTypes
		Public ReadOnly Callback As Action(Of CimSubscriptionResult)
		Public ReadOnly PropertyMatches As Dictionary(Of String, KeyValuePair(Of CimType, Object))

		Public Overrides Function Equals(obj As Object) As Boolean
			Dim other As JobSubscriber = TryCast(obj, JobSubscriber)
			If other IsNot Nothing AndAlso other.ID = ID Then
				Return True
			End If
			Return False
		End Function

		Public Shared Operator =(ByVal x As JobSubscriber, ByVal y As JobSubscriber) As Boolean
			Return x.ID = y.ID
		End Operator

		Public Shared Operator <>(ByVal x As JobSubscriber, ByVal y As JobSubscriber) As Boolean
			Return x.ID <> y.ID
		End Operator

		Public Shared Operator =(ByVal x As JobSubscriber, ByVal y As Tuple(Of CimSession, String, String, Dictionary(Of String, KeyValuePair(Of CimType, Object)))) As Boolean

		End Operator

		Public Overrides Function GetHashCode() As Integer
			Return ID.GetHashCode()
		End Function
	End Class

	Public Class CIMJobDispatcher

		Private Sub JobCreated(Result As CimSubscriptionResult)

		End Sub

		Private Sub JobWatcherErrored(ByVal [Error] As CimException)

		End Sub

		Private ReadOnly JobCreationWatcher As InstanceCreationController

	End Class
End Namespace

