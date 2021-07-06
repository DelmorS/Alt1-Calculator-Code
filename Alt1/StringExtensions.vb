Imports System.Runtime.CompilerServices

Module StringExtensions

    ''''''''''''''Extensions'''''''''''

    <Extension()>
    Public Function IndexSplit(ByVal str As String, index As Integer) As Array
        'Splits currently at the equivelent caret position

        Dim a(2) As String

        a(0) = str.Substring(0, index)
        a(1) = str.Substring(index, str.Length - index)

        Return a
    End Function

    <Extension()>
    Public Function OpeningBracket(ByVal input As String, index As Integer) As Integer

        'eg "56+(3+(2-3))"
        '11 -> 3

        Dim BracketNo As Integer = 0
        Dim i As Integer = index
        If index <= 0 Then Return -1
        While i >= 0

            Select Case input(i)
                Case ")"
                    BracketNo += 1

                Case "("
                    BracketNo -= 1

            End Select

            If BracketNo <= 0 Then
                Return i
            End If

            i -= 1
        End While

        'there are not enough brackets to open it
        Return -1
    End Function

    <Extension()>
    Public Function ContainsOnly(ByVal str As String, AllowableChars() As String) As Boolean
        'Checks if str contains only the values litsted in the array of chars "AllowableChar()"
        'eg. "(30+5)/()".ContainsOnly({"(",")"}) = false

        Dim LetterIsIn As Boolean

        For Each ch As Char In str

            LetterIsIn = False
            For Each check As Char In AllowableChars

                If ch = check Then LetterIsIn = True

            Next

            If LetterIsIn = False Then Return False

        Next

        Return True

    End Function

    <Extension()>
    Public Function SafeIndex(ByVal str As String, index As Integer) As String
        'Gets the char at the index of a string but first checks if it is valid or not

        If index <= str.Length - 1 And index >= 0 Then
            Return str(index)
        Else
            Return "error"
        End If

    End Function

    <Extension()>
    Public Function SafeString(ByVal str As String, index As Integer, length As Integer) As String

        If index + length - 1 <= str.Length - 1 And index >= 0 Then

            Return str.Substring(index, length)
        Else
            Return "error"
        End If

    End Function

End Module
