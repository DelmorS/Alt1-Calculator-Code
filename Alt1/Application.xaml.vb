Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    Private Sub Application_DispatcherUnhandledException(sender As Object, e As System.Windows.Threading.DispatcherUnhandledExceptionEventArgs)
        MsgBox("You have encountered a bug which is trying to close Alt1.  Please take a screenshot and send it to us at wallycloud@outlook.com.  We are sorry for the inconvienence and will try to patch the bug as soon as possible.")
        e.Handled = True
    End Sub

End Class
