'NOT CURRENTLY FINISHED OR IN USE

Public Class Root

    Private Numerator As New StackPanel
    Private Denominator As New StackPanel
    Private fBorder As New Border
    Private FractionPanel As New StackPanel
    Private fBoxNum As New TextBox
    Private fBoxDenom As New TextBox

    Private RootPanel As New StackPanel
    Private radPanel As New StackPanel
    'Private radBox As New TextBox
    Private comPanel As New StackPanel
    Private comBorder As New Border
    Private valPanel As New StackPanel
    Private valBox As New TextBox


    Public Sub New(Val As String)
        RootPanel.VerticalAlignment = VerticalAlignment.Center
        RootPanel.Orientation = Orientation.Horizontal

        'radBox.Text = "√"
        'Dim sc As New ScaleTransform
        'sc.ScaleY = 1.4
        'radBox.LayoutTransform = sc
        'radBox.Style = Application.Current.MainWindow.FindResource("radBox")

        radPanel.MinWidth = 20
        Dim ln As New Line
        ln.Stroke = Brushes.Black
        ln.X1 = radPanel.Width
        ln.X2 = 5
        ln.Y1 = radPanel.Height
        ln.Y2 = 0
        ln.StrokeThickness = 2
        radPanel.Children.Add(ln)

        comPanel.Orientation = Orientation.Vertical

        comBorder.Style = Application.Current.MainWindow.FindResource("comBorder")

        valPanel.Orientation = Orientation.Horizontal
        valBox.Style = Application.Current.MainWindow.FindResource("valBox")
        valBox.Text = Val

        valPanel.Children.Add(valBox)

        comPanel.Children.Add(comBorder)
        comPanel.Children.Add(valPanel)

        'radPanel.Stretch = Stretch.Fill
        'radPanel.Child = radBox

        'radBox.VerticalAlignment = VerticalAlignment.Stretch
        'radBox.VerticalContentAlignment = VerticalAlignment.Stretch



        RootPanel.Children.Add(radPanel)
        RootPanel.Children.Add(comPanel)

    End Sub

    Private Function FindNum(str As String) As Array
        'Finds the required numerator and text for remaining textbox

        'Take care of the remaining "/"
        str = str.Remove(str.Length - 1, 1)

        Dim i As Integer = str.Length - 1

        If str.Last <> ")" Then

            While True

                If IsNumeric(str(i)) = False And str(i) <> "." Then
                    i += 1
                    Exit While
                ElseIf i = 0 Then
                    Exit While
                End If
                i -= 1

            End While

        Else
            i = str.OpeningBracket(i)
        End If

        Return str.IndexSplit(i)

    End Function

    Public Sub Add(txt As TextBox)
        'Adds a new fraction to a textbox ANYWHERE but textbox must have his own StackPanel

        Dim stk As StackPanel = VisualTreeHelper.GetParent(txt)
        Dim i As Integer = stk.Children.IndexOf(txt) + 1

        Dim SplitStr() As String = txt.Text.IndexSplit(txt.CaretIndex)
        'Dim ReSplitStr() As String = FindNum(SplitStr(0))

        txt.Text = SplitStr(0)

        Dim Box As New TextBox

        Box.Style = txt.Style

        If SplitStr(1).Length > 0 Then
            If IsNumeric(SplitStr(1)(0)) = True Or SplitStr(1)(0) = "(" Then
                SplitStr(1) = SplitStr(1).Insert(0, "*")
            End If
        End If


        Box.Text = SplitStr(1)

        stk.Children.Insert(i, Box)

        'fBoxNum.Text = ReSplitStr(1)

        stk.Children.Insert(i, RootPanel)

        valBox.Focus()

    End Sub

End Class
