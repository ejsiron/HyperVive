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
	Public Class JobSubscriber
		Implements IComparable

		Public ReadOnly ID As New Guid
		Public ReadOnly Session As CimSession
		Public ReadOnly [Namespace] As String
		Public ReadOnly ClassName As String
		Public ReadOnly JobType As VirtualizationJobTypes
		Public ReadOnly Callback As Action(Of CimSubscriptionResult)
		Public ReadOnly PropertyMatches As Dictionary(Of String, Object)

		Public Overrides Function Equals(obj As Object) As Boolean
			Dim other As JobSubscriber = TryCast(obj, JobSubscriber)
			If other IsNot Nothing AndAlso other.ID = ID Then
				Return True
			End If
			Return False
		End Function

		Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
			Throw New NotImplementedException()
		End Function

		Public Shared Operator =(ByVal x As JobSubscriber, ByVal y As JobSubscriber) As Boolean
			Return x.ID = y.ID
		End Operator

		Public Shared Operator <>(ByVal x As JobSubscriber, ByVal y As JobSubscriber) As Boolean
			Return x.ID <> y.ID
		End Operator
	End Class

	Public Class CIMJobDispatcher

			Private Sub JobCreated(Result As CimSubscriptionResult)

			End Sub

			Private Sub JobWatcherErrored(ByVal [Error] As CimException)

			End Sub

			Private ReadOnly JobCreationWatcher As InstanceCreationController

			Private disposedValue As Boolean

			Protected Overridable Sub Dispose(disposing As Boolean)
				If Not disposedValue Then
					If disposing Then
						JobCreationWatcher.Dispose()
					End If

					' TODO: free unmanaged resources (unmanaged objects) and override finalizer
					' TODO: set large fields to null
					disposedValue = True
				End If
			End Sub

			Public Sub Dispose() Implements IDisposable.Dispose
				' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
				Dispose(disposing:=True)
			End Sub
		End Class
	End Namespace

