Imports System.Windows.Media.Animation
Imports System.Windows.Threading

Public Class Introduction

    Dim Timer As New DispatcherTimer
    Dim Timer1 As New DispatcherTimer
    Dim marg As New Thickness
    Dim marg1 As New Thickness
    Dim From1 As Integer = 0
    Dim To1 As Integer = 0
    Dim Page As Integer = 1
    Dim anim As New ThicknessAnimation
    Dim dur As New Duration(TimeSpan.FromSeconds(0.3))

    Private Sub Window_Initialized(sender As Object, e As EventArgs)
        Dim WorkingArea = System.Windows.SystemParameters.WorkArea

        Me.Left = WorkingArea.Right / 2 - Me.Width / 2
        Me.Top = WorkingArea.Bottom / 2 - Me.Height

        Timer.Interval = TimeSpan.FromMilliseconds(1500)

        AddHandler Timer.Tick, AddressOf Timer_Tick
        Timer.Start()

        Timer1.Interval = TimeSpan.FromMilliseconds(1000)

        AddHandler Timer1.Tick, AddressOf Timer1_Tick

        marg.Left = 0
        marg.Right = 0
        marg.Top = 0
        marg.Bottom = 0

        marg1.Left = 0
        marg1.Right = 0
        marg1.Top = 0
        marg1.Bottom = 0

    End Sub

    Private Sub Timer_Tick(sender As Object, e As EventArgs)


        Timer.Stop()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs)


        Timer1.Stop()

        Dim help As New Help
        help.Show()

        Me.Close()
    End Sub

    Private Sub AnimClose()
        Dim anim1 As New DoubleAnimation()
        Dim dur1 As New Duration(TimeSpan.FromSeconds(1))
        anim1.Duration = dur1
        anim1.To = 0
        anim1.From = 1
        anim1.AutoReverse = False
        anim1.RepeatBehavior = New RepeatBehavior(1)


        Me.BeginAnimation(Window.OpacityProperty, anim1)

        Timer1.Start()
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)


        If Page < 6 Then
            marg1.Left -= 650
            Page += 1
            anim.Duration = dur
            anim.From = marg
            anim.To = marg1
            anim.AutoReverse = False
            anim.RepeatBehavior = New RepeatBehavior(1)

            marg.Left -= 650

            StkMovement.BeginAnimation(StackPanel.MarginProperty, anim)

            If Page = 2 Then
                btnBack.Content = "Back"
            End If

            If Page = 6 Then
                btnNext.Content = "Open Keyboard Layout"
            End If

        Else
            AnimClose()


        End If

    End Sub

    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        If Page > 1 Then
            marg1.Left += 650
            Page -= 1
            anim.Duration = dur
            anim.From = marg
            anim.To = marg1
            anim.AutoReverse = False
            anim.RepeatBehavior = New RepeatBehavior(1)

            marg.Left += 650

            StkMovement.BeginAnimation(StackPanel.MarginProperty, anim)

            If Page = 1 Then
                btnBack.Content = "Skip this intro"
            End If

            If Page = 3 Then
                btnNext.Content = "Next"
            End If

        Else

            AnimClose()

        End If
    End Sub
End Class
