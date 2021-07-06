Public Class Fraction

    Private Numerator As New StackPanel
    Private Denominator As New StackPanel
    Private fBorder As New Border
    Private FractionPanel As New StackPanel
    Private fBoxNum As New TextBox
    Private fBoxDenom As New TextBox


    Private Function OpeningBracket(txt As TextBox, index As Integer)

        Dim currenttxt As TextBox = txt

        Dim stk As Panel = VisualTreeHelper.GetParent(txt)

        Dim stkindex As Integer = stk.Children.IndexOf(currenttxt)

        Dim i As Integer = index
        Dim BracketNo As Integer = 0

        Dim LastChecked As Boolean = False

        While stkindex >= 0

            If stkindex = 0 Then LastChecked = True

            While i >= 0

                Select Case currenttxt.Text(i)
                    Case ")"
                        BracketNo += 1

                    Case "("
                        BracketNo -= 1

                End Select

                If BracketNo <= 0 Then
                    Return {stkindex, i}
                End If

                i -= 1
            End While

            While True

                If stkindex <= 0 Then Exit While

                stkindex -= 1

                If TypeOf stk.Children(stkindex) Is TextBox Then
                    currenttxt = stk.Children(stkindex)
                    i = currenttxt.Text.Length - 1
                    Exit While
                End If

            End While

            If BracketNo > 0 And stkindex = 0 And LastChecked = True Then Exit While

        End While

        'Not enough opening brackets anywhere
        Return {-1, -1}

    End Function

    Public Sub New(Num As String, Denom As String)
        FractionPanel.MinWidth = 20
        FractionPanel.VerticalAlignment = VerticalAlignment.Center
        FractionPanel.Name = "Fraction" + (Rnd()).ToString.Substring(2, 5) 'not sure if this is okay for speed, hopefully find better way

        Numerator.Style = Application.Current.MainWindow.Resources("Stack1")
        Denominator.Style = Application.Current.MainWindow.Resources("Stack1")

        fBorder.Height = 0.75
        fBorder.BorderBrush = New SolidColorBrush(Colors.Black)
        fBorder.BorderThickness = New Thickness(1, 1, 1, 1)

        fBoxNum.Style = Application.Current.MainWindow.FindResource("Input_s")
        fBoxNum.Text = Num

        Numerator.Children.Add(fBoxNum)

        fBoxDenom.Style = Application.Current.MainWindow.FindResource("Input_s")
        fBoxDenom.Text = Denom

        Denominator.Children.Add(fBoxDenom)

        FractionPanel.Children.Add(Numerator)
        FractionPanel.Children.Add(fBorder)
        FractionPanel.Children.Add(Denominator)

    End Sub

    Private Function FindNum(str As String, txt As TextBox)
        'Finds the required numerator and text for remaining textbox

        'Take care of the remaining "/"

        Dim stk As Panel = VisualTreeHelper.GetParent(txt)

        str = str.Remove(str.Length - 1, 1)

        Dim i As Integer = str.Length - 1
        Dim truestart As Boolean = False

        While True

            If {"+", "-", "/", "(", " "}.Contains(str(i)) Then 'lose multiplies?

                If str(i) = "-" And (str.SafeIndex(i - 1) = "₀" Or str.SafeIndex(i - 1) = "^") Then

                Else
                    truestart = False
                    Exit While
                End If

            ElseIf str(i) = ")" Then

                Dim output() As Integer
                output = OpeningBracket(txt, i)

                If output(0) = -1 Then Return False 'Return false if no opening bracket exists

                Dim openingtxtboxindex As Integer = output(0)
                Dim openingindex As Integer = output(1)

                'openingtxt As TextBox = output(0)
                'Dim openingindex As Integer = output(1)

                'maybe need to make sure there actually is a bracket

                If stk.Children(openingtxtboxindex).Equals(txt) Then
                    'closing bracket is included in same textbox, easy normal way
                    i = openingindex - 1

                    If openingindex > 0 Then
                        i = openingindex - 1
                        Continue While
                    ElseIf openingindex <= 0 Then
                        i = openingindex
                        truestart = True
                        Exit While
                    End If

                Else
                    'We've got the closing bracket in another textbox

                    Dim specialcase As Boolean = False
                    Dim newstk As New List(Of Object)
                    Dim newstartingindex As Integer = openingindex - 1
                    Dim newtxt As TextBox = stk.Children(openingtxtboxindex)
                    Dim newstr As String = newtxt.Text

                    If newstartingindex > 0 Then
                        'Check if we are already at the start to prevent error

                        While {"+", "-", "/", "(", ")"}.Contains(newstr(newstartingindex)) = False

                            newstartingindex -= 1

                            If newstartingindex = 0 Then

                                If {"+", "-", "/", "(", ")"}.Contains(newstr(newstartingindex)) Then
                                    newstartingindex -= 0
                                Else
                                    newstartingindex -= 1
                                End If

                                Exit While
                            End If

                        End While

                    Else

                        'we are at the start ie = 0 already so the opening bracket is the start of the numerator
                        specialcase = True

                    End If

                    If newstartingindex < 0 Then
                        newstartingindex = 0
                    End If

                    Dim add As Integer = 1

                    If {"+", "-", "/", "("}.Contains(newstr(newstartingindex)) And specialcase = False Then
                        add = 1
                    Else
                        add = 0
                    End If

                    Dim splitstrings() As String = newstr.IndexSplit(newstartingindex + add)

                    newtxt.Text = splitstrings(0)

                    Dim txt1 As New TextBox
                    txt1.Style = fBoxNum.Style
                    txt1.MinWidth = 0
                    txt1.Text = splitstrings(1)
                    newstk.Add(txt1)

                    Dim endtextbox As Integer = stk.Children.IndexOf(txt)

                    Dim ii As Integer = openingtxtboxindex + 1

                    While ii <= endtextbox - 1

                        Dim newobject As Object = stk.Children(ii)

                        If TypeOf newobject Is TextBox Then

                            newobject.style = Application.Current.MainWindow.FindResource("Input_s")

                        End If

                        stk.Children.RemoveAt(ii)
                        endtextbox -= 1

                        newstk.Add(newobject)

                    End While

                    txt1 = New TextBox
                    txt1.Style = fBoxNum.Style
                    txt1.MinWidth = 0
                    txt1.Text = str
                    newstk.Add(txt1)
                    txt.Text = txt.Text.IndexSplit(str.Length - 1)(1)

                    Return newstk

                End If

            End If

            If i > 0 Then
                i -= 1
            End If

            'Just copied from above to fix problems
            If {"+", "-", "/", "(", " "}.Contains(str(i)) Then 'lose multiplies?

                If str(i) = "-" And (str.SafeIndex(i - 1) = "₀" Or str.SafeIndex(i - 1) = "^") Then

                Else
                    truestart = False
                    Exit While
                End If

            End If

            If i = 0 Then

                If str(i) = "×" Then
                    truestart = False
                    Exit While
                End If

                truestart = True
                'i -= 1
                Exit While
            End If

        End While

        If truestart = True Then
            Return str.IndexSplit(i)
        Else
            Return str.IndexSplit(i + 1)
        End If

    End Function

    Public Sub Add(txt As TextBox)
        'Adds a new fraction to a textbox ANYWHERE but textbox must have his own StackPanel

        Dim stk As Panel = VisualTreeHelper.GetParent(txt)
        Dim i As Integer = stk.Children.IndexOf(txt) + 1
        Dim Box As New TextBox

        If stk.Children.IndexOf(txt) = 0 And txt.Text.Length = 1 Then
            'Starting one at the start of stackpanel
            txt.Text = ""
            Box.Style = txt.Style
            stk.Children.Insert(i, Box)
            stk.Children.Insert(i, FractionPanel)
            Box.MinWidth = 0

            fBoxNum.Focus()
            Exit Sub

        ElseIf stk.Children.IndexOf(txt) > 0 And txt.CaretIndex = 1 Then
            'We're at the start of a textbox but have something before
            'Caret index 1 because we haven't deleted the extra / yet

            Dim emptybox As New TextBox
            Dim stkcopy As New StackPanel

            stkcopy = stk.Children(i - 2)

            stk.Children.RemoveAt(i - 2)

            emptybox.Text = ""
            emptybox.Style = Application.Current.MainWindow.FindResource("Input_s")
            'newnumerator.Children.Add(emptybox)
            Numerator.Children.Add(stkcopy)
            Numerator.Children.Add(emptybox)

            txt.Text = txt.Text.Substring(1, txt.Text.Length - 1)

            stk.Children.Insert(i - 2, FractionPanel)

            fBoxDenom.Focus()

            Exit Sub
        End If

        Dim SplitStr() As String = txt.Text.IndexSplit(txt.CaretIndex)

        Dim OurOutput As Object = FindNum(SplitStr(0), txt)


        'Need also condition of finding no closing bracket?  Or just do nothing?
        If TypeOf OurOutput Is String() Then
            'Output from FundNum is a string
            'As such the closing bracket lies in the same textbox

            Dim ReSplitStr() As String = OurOutput

            txt.Text = ReSplitStr(0)


            Box.Style = txt.Style
            Box.MinWidth = 0

            If SplitStr(1).Length > 0 Then
                If IsNumeric(SplitStr(1)(0)) = True Or SplitStr(1)(0) = "(" Then
                    SplitStr(1) = SplitStr(1).Insert(0, "*")
                End If
            End If

            Box.Text = SplitStr(1)

            stk.Children.Insert(i, Box)

            If ReSplitStr(1) = "Ans" Then
                fBoxNum.Text = ""
            Else
                fBoxNum.Text = ReSplitStr(1)
            End If

            stk.Children.Insert(i, FractionPanel)

            If fBoxNum.Text = "" Then
                fBoxNum.Focus()
            Else
                fBoxDenom.Focus()
            End If

        ElseIf TypeOf OurOutput Is Boolean Then
            Exit Sub

        Else
            'As such our closing bracket lies in another textbox.

            Dim ListOfItems As List(Of Object) = OurOutput

            Numerator.Children.RemoveAt(0)

            For Each item As Object In ListOfItems
                'ListOfItems.Remove(item)
                Numerator.Children.Add(item)

            Next
            txt.Text = SplitStr(1)

            i = stk.Children.IndexOf(txt)
            stk.Children.Insert(i, FractionPanel)
            fBoxDenom.Focus()
        End If



    End Sub

End Class
