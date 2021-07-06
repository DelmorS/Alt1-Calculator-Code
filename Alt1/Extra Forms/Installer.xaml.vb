Imports System.Windows.Media.Animation
Imports System.Windows.Threading

Public Class Installer
    Dim sb As Storyboard
    Dim time As New DispatcherTimer
    Dim time2 As New DispatcherTimer
    Dim time3 As New DispatcherTimer
    Dim time4 As New DispatcherTimer
    Dim time5 As New DispatcherTimer
    Dim time6 As New DispatcherTimer


    Private Sub Window_Initialized(sender As Object, e As EventArgs)
        Dim WorkingArea = System.Windows.SystemParameters.WorkArea

        Me.Left = WorkingArea.Right / 2 - Me.Width / 2
        Me.Top = WorkingArea.Bottom / 2 - Me.Height

        time.Interval = TimeSpan.FromMilliseconds(7000)
        AddHandler time.Tick, AddressOf time_Tick
        time.Start()

        time2.Interval = TimeSpan.FromMilliseconds(1500)
        AddHandler time2.Tick, AddressOf time_Tick2
        time2.Start()

        time3.Interval = TimeSpan.FromMilliseconds(5000)
        AddHandler time3.Tick, AddressOf time_Tick3
        time3.Start()

        time4.Interval = TimeSpan.FromMilliseconds(6000)
        AddHandler time4.Tick, AddressOf time_Tick4
        time4.Start()

        time5.Interval = TimeSpan.FromMilliseconds(30)
        AddHandler time5.Tick, AddressOf time_Tick5
        'time5.Start()

        time6.Interval = TimeSpan.FromMilliseconds(1500)
        AddHandler time6.Tick, AddressOf time_Tick6

        Label1.Opacity = 0

    End Sub

    Private Sub time_Tick5(sender As Object, e As EventArgs)

        If pbar.Value < 100 Then
            If pbar.Value < 30 Then
                pbar.Value += 0.75

            ElseIf pbar.Value > 60 Then
                pbar.Value += 1
            Else
                pbar.Value += 0.35
            End If

        Else
            time5.IsEnabled = False
        End If

    End Sub

    Private Sub time_Tick4(sender As Object, e As EventArgs)

        Label1.Content = "Okay, you're ready to go!"
        Dim anim As New DoubleAnimation()
        Dim dur As New Duration(TimeSpan.FromSeconds(1))
        anim.Duration = dur
        anim.To = 1
        anim.From = 0
        anim.AutoReverse = False
        anim.RepeatBehavior = New RepeatBehavior(1)


        Label1.BeginAnimation(Label.OpacityProperty, anim)
        time4.IsEnabled = False
    End Sub

    Private Sub time_Tick3(sender As Object, e As EventArgs)


        Dim anim As New DoubleAnimation()
        Dim dur As New Duration(TimeSpan.FromSeconds(1))
        anim.Duration = dur
        anim.To = 0
        anim.From = 1
        anim.AutoReverse = False
        anim.RepeatBehavior = New RepeatBehavior(1)


        Label1.BeginAnimation(Label.OpacityProperty, anim)
        time3.IsEnabled = False
    End Sub

    Private Sub time_Tick2(sender As Object, e As EventArgs)

        'Label1.Content = "Okay, you're ready to go!"
        Dim anim As New DoubleAnimation()
        Dim dur As New Duration(TimeSpan.FromSeconds(1))
        anim.Duration = dur
        anim.To = 1
        anim.From = 0
        anim.AutoReverse = False
        anim.RepeatBehavior = New RepeatBehavior(1)


        Label1.BeginAnimation(Label.OpacityProperty, anim)

        time5.Start()
        time2.IsEnabled = False

    End Sub

    Private Sub time_Tick(sender As Object, e As EventArgs)
        Dim anim1 As New DoubleAnimation()
        Dim dur1 As New Duration(TimeSpan.FromSeconds(1))
        anim1.Duration = dur1
        anim1.To = 0
        anim1.From = 1
        anim1.AutoReverse = False
        anim1.RepeatBehavior = New RepeatBehavior(1)


        Me.BeginAnimation(Window.OpacityProperty, anim1)

        time6.Start()
        time.IsEnabled = False
    End Sub

    Private Sub time_Tick6(sender As Object, e As EventArgs)

        Dim intro As New Introduction
        intro.Show()
        time6.IsEnabled = False
        Me.Close()

    End Sub

End Class
