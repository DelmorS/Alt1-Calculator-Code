Public Class Updater
    Dim WorkingArea = System.Windows.SystemParameters.WorkArea

    Private Sub Window_Initialized(sender As Object, e As EventArgs)
        Me.Left = WorkingArea.Right / 2 - Me.Width / 2
        Me.Top = WorkingArea.Bottom / 2 - Me.Height / 2
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub
End Class
