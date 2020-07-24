Imports Microsoft.Management.Infrastructure

Namespace CIMitar
	Public Class CimPropertyCore
		Public ReadOnly CimType As CimType
		Public ReadOnly Value As Object
		Public ReadOnly ManagedType As Type

		Sub New(ByVal InProperty As CimProperty)
			CimType = InProperty.CimType
			Value = InProperty.Value
			ManagedType = InProperty.GetManagedType
		End Sub

		Private Shared Function ValueEquals(ByVal XVal As Object, ByVal YVal As Object, ByVal ValueType As Type) As Boolean
			If TypeOf XVal Is Array Then

				For Each Member As Object In XVal

				Next
			End If
		End Function

		Public Shared Operator =(ByVal x As CimPropertyCore, ByVal y As CimPropertyCore) As Boolean
			If x Is y Then
				Return True
			End If
			If x.ManagedType = y.ManagedType Then
				Dim NetType As Type = x.ManagedType
				If TypeOf x.Value Is Array Then
					Return x.Value = y.Value
				Else

				End If
			End If
			Return False
		End Operator


		Public Shared Operator <>(ByVal x As CimPropertyCore, ByVal y As CimPropertyCore) As Boolean
			Return Not x = y
		End Operator
	End Class
	Public Module CIMTypeEnhancements

		Public Function GetCimPropertyManagedType(ByVal CimValueType As CimType) As Type
			Dim ValueType As Type
			Select Case CimValueType
				Case CimType.Boolean
					ValueType = GetType(Boolean)
				Case CimType.BooleanArray
					ValueType = GetType(Boolean())
				Case CimType.Char16
					ValueType = GetType(Char)
				Case CimType.Char16Array
					ValueType = GetType(Char())
				Case CimType.DateTime
					ValueType = GetType(Date)
				Case CimType.DateTimeArray
					ValueType = GetType(Date())
				Case CimType.Real32
					ValueType = GetType(Single)
				Case CimType.Real32Array
					ValueType = GetType(Single())
				Case CimType.Real64
					ValueType = GetType(Double)
				Case CimType.Real64Array
					ValueType = GetType(Double())
				Case CimType.ReferenceArray
					ValueType = GetType(Object())
				Case CimType.SInt8, CimType.SInt16
					ValueType = GetType(Short)
				Case CimType.SInt8Array, CimType.SInt16Array
					ValueType = GetType(Short())
				Case CimType.SInt32
					ValueType = GetType(Integer)
				Case CimType.SInt32Array
					ValueType = GetType(Integer())
				Case CimType.SInt64
					ValueType = GetType(Long)
				Case CimType.SInt64Array
					ValueType = GetType(Long())
				Case CimType.String
					ValueType = GetType(String)
				Case CimType.StringArray
					ValueType = GetType(String())
				Case CimType.UInt8, CimType.UInt16
					ValueType = GetType(UShort)
				Case CimType.UInt8Array, CimType.UInt16Array
					ValueType = GetType(UShort())
				Case CimType.UInt32
					ValueType = GetType(UInteger)
				Case CimType.UInt32Array
					ValueType = GetType(UInteger())
				Case CimType.UInt64
					ValueType = GetType(ULong)
				Case CimType.UInt64Array
					ValueType = GetType(ULong())
				Case Else
					ValueType = GetType(Object)
			End Select
			Return ValueType
		End Function

#Region "CIM Default Values"
		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As Boolean) As Boolean
			Return False
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As Char) As Char
			Return Char.MinValue
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As Date) As Date
			Return Date.MinValue
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As Single) As Single
			Return 0!
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As Double) As Double
			Return 0#
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As Short) As Short
			Return 0S
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As Integer) As Integer
			Return 0
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As Long) As Long
			Return 0L
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As String) As String
			Return String.Empty
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As UShort) As UShort
			Return 0US
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As UInteger) As UInteger
			Return 0UI
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As ULong) As ULong
			Return 0UL
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(ByRef Value As Object) As Object
			Return New Object
		End Function

		<CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload differentiator")>
		Public Function GetCimDefaultValue(Of T)(ByRef Value As T()) As T()
			Return Array.Empty(Of T)
		End Function
#End Region 'CIM Default Values'

#Region "CIM Value Converters"

#End Region 'CIM Value Converters'
	End Module
End Namespace