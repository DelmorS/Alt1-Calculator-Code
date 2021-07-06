'NOT CURRENTLY FINISHED OR IN USE

Public Class Power

    Private PowerPanel As New StackPanel
    Private pBox As New TextBox
    Private pPanel As New StackPanel
    Private pBorder As New Border


    Public Sub New(pwr As String)
        PowerPanel.MinWidth = 6
        PowerPanel.VerticalAlignment = VerticalAlignment.Center
        PowerPanel.Orientation = Orientation.Vertical
        PowerPanel.Name = "Power" + (Rnd()).ToString.Substring(2, 5) 'not sure if this is okay for speed, hopefully find better way

        pPanel.VerticalAlignment = VerticalAlignment.Center
        pPanel.Orientation = Orientation.Vertical

        pBorder.Style = Application.Current.MainWindow.Resources("bdrPower")

        pBox.Style = Application.Current.MainWindow.FindResource("txtPower")
        pBox.Text = pwr

        pPanel.Children.Add(pBox)

        PowerPanel.Children.Add(pPanel)
        PowerPanel.Children.Add(pBorder)

    End Sub

    Public Sub Add(txt As TextBox)
        'Adds a new fraction to a textbox ANYWHERE but textbox must have his own StackPanel

        Dim stk As StackPanel = VisualTreeHelper.GetParent(txt)
        Dim i As Integer = stk.Children.IndexOf(txt) + 1

        Dim SplitStr() As String = txt.Text.IndexSplit(txt.CaretIndex)

        txt.Text = SplitStr(0)

        Dim Box As New TextBox

        Box.Style = txt.Style
        Box.MinWidth = 0

        If SplitStr(1).Length > 0 Then
            If IsNumeric(SplitStr(1)(0)) = True Or SplitStr(1)(0) = "(" Then
                SplitStr(1) = SplitStr(1).Insert(0, "*")
            End If
        End If


        Box.Text = SplitStr(1)

        stk.Children.Insert(i, Box)

        stk.Children.Insert(i, PowerPanel)

        pBox.Focus()

    End Sub

End Class
