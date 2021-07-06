Public Class MyEvaluator

    Dim ScriptControl As New MSScriptControl.ScriptControl

    Private Degrees As Boolean

    Public Sub New(ByVal Degrees)
        Me.Degrees = Degrees
        Me.ScriptControl.Language = "vbscript"
        Me.ScriptControl.AddCode("Function asin(X): asin=Atn(X / Sqr(-X * X + 1)): End Function")
        Me.ScriptControl.AddCode("Function acos(X): acos=Atn(-X / Sqr(-X * X + 1)) + 2 * Atn(1): End Function")
        Me.ScriptControl.AddCode("Function llog(X): llog=log(X)/log(10): End Function")

    End Sub

    Private Function asin(val As Decimal) As Decimal

        Return Math.Asin(val)

    End Function

    Private Function CheckFormula(str As String) As Boolean

        Dim openbrackets As Integer = 0
        Dim closingbrackets As Integer = 0

        If str.Length = 1 Then
            If IsNumeric(str(0)) Or str(0) = "π" Or str(0) = "e" Then
                Return True
            Else
                Return False
            End If
        End If

        If {"+", "-", "*", "^", "/", "⁻", "×", ")", "₀"}.Contains(str(0)) Then
            Return False
        End If

        If {"+", "-", "*", "^", "/", "×", "(", "₀"}.Contains(str(str.Length - 1)) Then
            Return False
        End If

        For i As Integer = 0 To str.Length - 1

            If str(i) = "(" Then openbrackets += 1
            If str(i) = ")" Then closingbrackets += 1

            If i > 0 Then
                If {"+", "-", "*", "^", "/", "×", "("}.Contains(str(i - 1)) And {"+", "*", "^", "/", "⁻", "×", ")"}.Contains(str(i)) Then
                    Return False
                End If

                If str(i - 1) = "₀" And {"+", "*", "^", "/", "⁻", "×", ")"}.Contains(str(i)) Then
                    Return False
                End If
            End If

            Dim nextchar As String = str.SafeIndex(i + 1)

            If (str(i) = "¹" Or str(i) = "²" Or str(i) = "π" Or str(i) = "e") And (IsNumeric(nextchar) Or nextchar = "π" Or nextchar = "e") Then
                Return False
            End If

        Next

        If openbrackets <> closingbrackets Then
            Return False
        End If

        Return True

    End Function


    Private Function ProcessString(UserInput As String, LV As ListView, LVI As Integer) As String
        Dim EvalString As String = ""
        Dim BracketNo As Integer
        Dim i As Integer = 0

        If UserInput = "" Then Return 0

        While i <= UserInput.Length - 1

            If UserInput(i) = "A" Then

                Dim s As String = UserInput.SafeString(i + 1, 2)

                If UserInput.SafeString(i + 1, 3) = "ns[" Then

                    Dim indexstr As String = ""
                    Dim index As Integer

                    For ii As Integer = i + 4 To UserInput.Length

                        If UserInput(ii) = "]" Then

                            index = LV.Items.Count - Convert.ToInt32(indexstr)

                            Dim PreviousHistory As String = LV.Items(index).Content

                            PreviousHistory = PreviousHistory.Replace(",", ".")

                            EvalString += PreviousHistory

                            indexstr = ""

                            i = ii + 1

                            Continue While
                        End If

                        indexstr += UserInput(ii)

                    Next


                ElseIf UserInput.SafeString(i + 1, 2) = "ns" Then

                    Dim PreviousHistory As String = LV.Items(LVI).Content.ToString

                    PreviousHistory = PreviousHistory.Replace(",", ".")

                    EvalString += PreviousHistory

                    i += 3
                    Continue While

                End If

            End If

            Select Case UserInput(i)

                Case "n"

                    If UserInput.SafeString(i + 1, 4) = "log(" Then
                        EvalString += "log("
                        i += 5
                        Continue While

                    End If

                Case "l"

                    If UserInput.SafeString(i + 1, 3) = "og(" Then
                        EvalString += "llog("
                        i += 4
                        Continue While
                    End If

                Case "ₓ"
                    If UserInput.SafeString(i, 3) = "ₓ₁₀" Then
                        EvalString += "E"
                        i += 3
                        Continue While
                    End If

                Case "⁻" 'Inverse

                    Dim s As String = UserInput.SafeIndex(i + 1)

                    If UserInput.SafeIndex(i + 1) = "¹" Then
                        EvalString += "^-1"
                        i += 2
                        Continue While
                    End If

                Case "π" 'Pi
                    EvalString += Math.PI.ToString
                    i += 1
                    Continue While

                Case "e" 'eulers constant
                    EvalString += "2.71828182845904523"
                    i += 1
                    Continue While

                Case "²" 'Sqaured
                    EvalString += "^2"
                    i += 1
                    Continue While

                Case "A"


                Case "×"
                    EvalString += "*"
                    i += 1
                    Continue While

                Case ","
                    EvalString += "."
                    i += 1
                    Continue While
            End Select

            If UserInput(i) = "√" Then

                If UserInput(i + 1) = "(" Then

                    BracketNo = ClosingBracket(i + 1, UserInput)
                    UserInput = UserInput.Insert(BracketNo + 1, "^(1/2)")

                End If

                i += 1
                Continue While

            End If

            If i < UserInput.Length - 2 Then

                If UserInput(i + 1) = "√" And IsNumeric(UserInput(i)) Then

                    If UserInput(i + 2) = "(" Then
                        BracketNo = ClosingBracket(i + 2, UserInput)
                        UserInput = UserInput.Insert(BracketNo + 1, "^(1/" + UserInput(i) + ")")

                    End If

                    i += 2

                    Continue While

                ElseIf UserInput(i + 1) = "v" Then
                    'avg(23 12 43 54 (23+2))

                    If UserInput.SafeString(i + 1, 3) = "vg(" Then

                        Dim AverageString As String = ""
                        'opening bracket at i+3
                        Dim SpaceNo As Integer = 0
                        BracketNo = ClosingBracket(i + 3, UserInput)
                        'Numbers start at i+4

                        'If IsNumeric(UserInput(i + 4)) Then
                        Dim AverageElementStr As String = "("

                        For ii As Integer = i + 3 To BracketNo - 1

                            If UserInput(ii) = " " Then
                                If ii + 1 = BracketNo Then Exit For

                                AverageString += ProcessString(AverageElementStr, LV, LVI)
                                AverageElementStr = ""
                                AverageString += ")+("
                                SpaceNo += 1
                                Continue For
                            End If

                            AverageElementStr += UserInput(ii)
                            'AverageString += UserInput(ii)

                        Next

                        AverageString += ProcessString(AverageElementStr, LV, LVI)

                        EvalString += AverageString & "))" & "/(" & SpaceNo + 1 & ")"
                        'EvalString += (ScriptControl.Eval(AverageString) / (SpaceNo + 1)).ToString

                        i = BracketNo + 1
                        Continue While

                    End If

                ElseIf UserInput(i) = "a" And {"s", "c", "t"}.Contains(UserInput(i + 1)) Then
                        'atan(

                        If UserInput.SafeString(i + 1, 4) = "sin(" OrElse UserInput.SafeString(i + 1, 4) = "cos(" OrElse UserInput.SafeString(i + 1, 4) = "tan(" Then
                            Dim a As String = ""

                            If Degrees = True Then a = "57.295779513082320876798154814105*"

                            Select Case UserInput(i + 1)
                                Case "s"
                                    EvalString += a + "asin("
                                    i += 5
                                    Continue While
                                Case "c"
                                    EvalString += a + "acos("
                                    i += 5
                                    Continue While
                                Case "t"
                                    EvalString += a + "Atn("
                                    i += 5
                                    Continue While
                            End Select
                        End If

                    ElseIf {"s", "c", "t"}.Contains(UserInput(i)) Then

                    If UserInput.SafeString(i, 4) = "sin(" OrElse UserInput.SafeString(i, 4) = "cos(" OrElse UserInput.SafeString(i, 4) = "tan(" Then

                        Dim d As String = ""
                        If Degrees = True Then d = "0.01745329251994329576923690768489*"

                        Select Case UserInput(i)
                            Case "s"

                                EvalString += "sin(" + d
                                i += 4
                                Continue While

                            Case "c"
                                EvalString += "cos(" + d
                                i += 4
                                Continue While

                            Case "t"
                                EvalString += "tan(" + d
                                i += 4
                                Continue While

                        End Select

                    End If
                End If

            End If

            EvalString += UserInput(i)

            If IsNumeric(UserInput(i)) And {"s", "c", "t", "a", "A", "(", "π", "e", "l", "n"}.Contains(UserInput.SafeIndex(i + 1)) Then

                EvalString += "*"

            ElseIf UserInput(i) = ")" And UserInput.SafeIndex(i + 1) = "(" Then

                EvalString += "*"

            End If


            i += 1
        End While

        Return EvalString

    End Function

    Public Shared Function ClosingBracket(index As Integer, input As String) As Integer

        'eg "56+(3+(2-3))"
        '3 -> 11

        Dim BracketNo As Integer = 0

        If index >= input.Length - 1 Then Return -1
        For i As Integer = index To input.Length - 1

            Select Case input(i)
                Case "("
                    BracketNo += 1

                Case ")"
                    BracketNo -= 1

            End Select

            If BracketNo <= 0 Then
                Return i
            End If

        Next

        'there are not enough brackets to close it
        Return -1

    End Function

    Public Shared Function OpeningBracket(index As Integer, input As String) As Integer

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

    Public Function Evaluate(UserInput As String, LV As ListView, LVI As Integer) As String

        If UserInput = "" Then Return 0

        If CheckFormula(UserInput) = False Then
            Return ""

        End If

        Try

            Dim EvaledString As String = ScriptControl.Eval(ProcessString(UserInput, LV, LVI)).ToString

            If EvaledString.Contains("E") And EvaledString.Length > 12 Then

                Dim Eindex As Integer
                Dim Ecount As Integer

                For i As Integer = 0 To EvaledString.Length - 1

                    If EvaledString(i) = "E" Then

                        Eindex = i
                        Ecount = EvaledString.Length - (Eindex + 1)
                        Exit For
                    End If

                Next

                EvaledString = EvaledString.Substring(0, Eindex - Ecount) + EvaledString.Substring(Eindex, Ecount + 1)

            End If

            Console.WriteLine(ProcessString(UserInput, LV, LVI))

            Return EvaledString
        Catch ex As Exception
            ScriptControl.Error.Clear()
            Return ""

        End Try

    End Function

    Public Function ShowString(UserInput As String) As String

    End Function

End Class