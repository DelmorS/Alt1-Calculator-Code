Imports System.Reflection
Imports System.IO

Public Class About
    Private Sub Window_Initialized(sender As Object, e As EventArgs)

        Dim assemblyversion As Version = Assembly.GetEntryAssembly().GetName().Version

        Dim s As String = assemblyversion.ToString()

        s = s.Remove(s.Length - 2)

        lblversion.Content = "Alt1 v" + s

    End Sub
End Class
