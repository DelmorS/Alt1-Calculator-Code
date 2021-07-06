Public Class Help


    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs)
        Dim intro As New Introduction
        intro.Show()
        Me.Close()
    End Sub


End Class
