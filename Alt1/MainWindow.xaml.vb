Imports Squirrel
Imports System.Runtime.InteropServices
Imports System.Windows.Interop
Imports System.Windows.Threading
Imports System.Diagnostics
Imports System.Windows
Imports System.Text
Imports System.Reflection
Imports System.IO


Class MainWindow
    'hello
    Public Const MOD_ALT As Integer = &H1 'Alt key
    Dim SC As New MSScriptControl.ScriptControl
    Dim Count1 As Integer = -1
    Dim SlashCount As Integer = 0
    Dim Eval As New MyEvaluator(My.Settings.Degrees)
    Dim nf As New System.Windows.Forms.NotifyIcon
    Dim ctx As System.Windows.Forms.ContextMenuStrip
    Dim LVILeftLast As ListViewItem = Nothing
    Dim LVIRightLast As ListViewItem = Nothing
    Dim InitialTextBox1 As TextBox
    Dim FormTimer As New Timers.Timer
    Dim SelectedStackPanel As StackPanel = Nothing
    Dim careti As Integer
    Dim SuppressTextChanged As Boolean = False 'Used to ignore the textchanged event that occurs when inserting a fraction
    Dim PauseEval As Boolean = False
    Dim PrevStr As String = ""
    Dim SetSelection As Boolean = False
    Dim LastClickedListViewItem As ListViewItem
    Dim StackPanelHistory As New List(Of UIElementCollection)
    Dim LastFocusedTxt As TextBox = InitialTextBox

    '''This allows for the checking of the char from a keypress
    Public Enum MapType As UInteger
        MAPVK_VK_TO_VSC = &H0
        MAPVK_VSC_TO_VK = &H1
        MAPVK_VK_TO_CHAR = &H2
        MAPVK_VSC_TO_VK_EX = &H3
    End Enum

    <DllImport("user32.dll")>
    Public Shared Function ToUnicode(wVirtKey As UInteger, wScanCode As UInteger, lpKeyState As Byte(), <Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex:=4)> pwszBuff As StringBuilder, cchBuff As Integer, wFlags As UInteger) As Integer
    End Function

    <DllImport("user32.dll")>
    Public Shared Function GetKeyboardState(lpKeyState As Byte()) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function MapVirtualKey(uCode As UInteger, uMapType As MapType) As UInteger
    End Function

    Public Shared Function GetCharFromKey(key As Key) As Char
        Dim ch As Char = " "c

        Dim virtualKey As Integer = KeyInterop.VirtualKeyFromKey(key)
        Dim keyboardState As Byte() = New Byte(255) {}
        GetKeyboardState(keyboardState)

        Dim scanCode As UInteger = MapVirtualKey(CUInt(virtualKey), MapType.MAPVK_VK_TO_VSC)
        Dim stringBuilder As New StringBuilder(2)

        Dim result As Integer = ToUnicode(CUInt(virtualKey), scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0)
        Select Case result
            Case -1
                Exit Select
            Case 0
                Exit Select
            Case 1
                If True Then
                    ch = stringBuilder(0)
                    Exit Select
                End If
            Case Else
                If True Then
                    ch = stringBuilder(0)
                    Exit Select
                End If
        End Select
        Return ch
    End Function

    '=======================================================
    'Service provided by Telerik (www.telerik.com)
    'Conversion powered by NRefactory.
    'Twitter: @telerik
    'Facebook: facebook.com/telerik
    '=======================================================


    ''''''''''''

    <DllImport("User32.dll")>
    Private Shared Function RegisterHotKey(<[In]> hWnd As IntPtr, <[In]> id As Integer, <[In]> fsModifiers As UInteger, <[In]> vk As UInteger) As Boolean
    End Function

    <DllImport("User32.dll")>
    Private Shared Function UnregisterHotKey(<[In]> hWnd As IntPtr, <[In]> id As Integer) As Boolean
    End Function

    Private _source As HwndSource
    Private Const HOTKEY_ID As Integer = 9000

    Protected Overrides Sub OnSourceInitialized(e As EventArgs)
        MyBase.OnSourceInitialized(e)
        Dim helper = New WindowInteropHelper(Me)
        _source = HwndSource.FromHwnd(helper.Handle)
        _source.AddHook(AddressOf HwndHook)
        RegisterHotKey()
    End Sub

    Protected Overrides Sub OnClosed(e As EventArgs)
        _source.RemoveHook(AddressOf HwndHook)
        _source = Nothing
        UnregisterHotKey()
        MyBase.OnClosed(e)
    End Sub

    Private Sub RegisterHotKey()
        Dim helper = New WindowInteropHelper(Me)
        Const VK_F10 As UInteger = &H31
        Const MOD_CTRL As UInteger = &H1
        ' handle error
        If Not RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL, VK_F10) Then
        End If
    End Sub

    Private Sub UnregisterHotKey()
        Dim helper = New WindowInteropHelper(Me)
        UnregisterHotKey(helper.Handle, HOTKEY_ID)
    End Sub

    Private Function HwndHook(hwnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr, ByRef handled As Boolean) As IntPtr
        Const WM_HOTKEY As Integer = &H312
        Select Case msg
            Case WM_HOTKEY
                Select Case wParam.ToInt32()
                    Case HOTKEY_ID
                        OnHotKeyPressed()
                        handled = True
                        Exit Select
                End Select
                Exit Select
        End Select
        Return IntPtr.Zero
    End Function

    Private Sub OnHotKeyPressed()

        If Me.WindowState = Windows.WindowState.Minimized Then
            ToggleVisibility()

        ElseIf Me.WindowState = Windows.WindowState.Normal Then

            If Me.IsActive = True Then
                ToggleVisibility()
            Else
                Me.Activate()
            End If

        End If
    End Sub

    Private Function ReduceElement(txt As TextBox) As String
        Dim CaretLoc As Integer = txt.CaretIndex
        Dim str As String = txt.Text
        Dim NewString As String = txt.Text
        Dim i As Integer = CaretLoc - 2

        If str(i) = ")" Then
            i = str.OpeningBracket(i)
            NewString = str.Substring(i)
            txt.Text = str.Remove(i)
        Else
            While IsNumeric(str(i)) = True Or str(i) = "." Or i = 0
                i -= 1
            End While
            NewString = str.Substring(i + 1)
            txt.Text = str.Remove(i + 1)
        End If

        Return NewString.Remove(NewString.Length - 1)

    End Function

    Private Sub SendEvalRequest(Optional Backwards As Boolean = False)

        Dim Xindex As Integer

        If Count1 = -1 Then
            Xindex = LVLeft.Items.Count - 1
        Else
            Xindex = Count1 - 1
        End If

        Dim str As String = CollatePanels(InputStk.Children)

        If PrevStr = str Then
            PauseEval = True
        End If

        'Only do this is it is not a backspace case, as we always want to evaluate in Backspace cases
        If str.Length > 1 And str.Length >= PrevStr.Length Then
            If str(str.Length - 2) = "(" And str.Last = ")" Then
                PauseEval = True
            End If
        End If

        PrevStr = str

        If str.Length > 0 Then
            If {"+", "-", "/", "*", "×", "^"}.Contains(str.Last) Then
                str = str.Substring(0, str.Length - 1)
            End If
        End If



        If PauseEval = False Then LBOutput.Content = Eval.Evaluate(str, LVLeft, Xindex)

        PauseEval = False
    End Sub

    Private Sub TextBox_TextChanged(sender As TextBox, e As TextChangedEventArgs)

        Dim box As TextBox = TryCast(InputStk.Children.Item(0), TextBox)

        ''''''''''Add ANS Automatically-------------
        If InputStk.Children.Count = 1 Then

            If {"+", "-", "*", "^", "/", "⁻¹", "×"}.Contains(box.Text) And LVLeft.Items.Count > 0 Then

                LBOutput.Content = LVLeft.Items(LVLeft.Items.Count - 1).Content.ToString

                'WORKAROUND FOR FUNNY ANS× SELCTION THING
                If box.Text = "*" Then
                    box.Text = "Ans×"
                    box.Select(box.Text.Length, 0)
                    Exit Sub
                End If

                'Had to move Ans inverse to preview keyup since it wouldn't select properly here.

                box.Text = box.Text.Insert(0, "Ans")

                box.Select(box.Text.Length, 0)

            End If
        End If

        If Count1 > -1 Then

            Dim str As String = CollatePanels(InputStk.Children)

            If str <> "" Then
                LVRight.Items(Count1).Content.Text = str
            End If

        End If

        SendEvalRequest()

        If Count1 > -1 Then

            For i As Integer = 0 To LVLeft.Items.Count - 1


                If i = Count1 Then
                    LVLeft.Items(i).Content = LBOutput.Content
                    Continue For
                End If

                LVLeft.Items(i).Content = Eval.Evaluate(LVRight.Items(i).Content.Text, LVLeft, i - 1)
            Next

        End If
    End Sub

    Private Function PrelimEvaluate(str As String) As Decimal


        Dim i As Integer
        Dim Xindex As Integer

        While True

            For i = 0 To str.Length - 1
                If str(i) = "x" Then
                    Exit For
                End If

                If i = str.Length - 1 Then
                    i = -1
                    Exit While
                End If
            Next

            If i > -1 Then
                Dim split() As String

                If IsNumeric(str(i + 1)) Then
                    split = str.IndexSplit(i + 2)

                    Xindex = split(0).Last.ToString

                    split(0) = split(0).Remove(split(0).Length - 2)

                    str = split(0) + LVLeft.Items(Xindex).Content.ToString + split(1)
                Else

                    split = str.IndexSplit(i + 1)

                    split(0) = split(0).Remove(split(0).Length - 1)

                    If Count1 > -1 Then
                        Xindex = LVLeft.SelectedIndex - 1
                    Else

                        Xindex = LVLeft.Items.Count - 1
                    End If
                    str = split(0) + LVLeft.Items(Xindex).Content.ToString + split(1)

                End If

            End If

        End While
        SC.Language = "vbscript"
        Try
            Return SC.Eval(str)
        Catch ex As Exception
            SC.Error.Clear()
            Return vbNull
        End Try
    End Function

    Private Function CollatePanels(collection As UIElementCollection) As String

        Dim EvalStr As String = ""
        Dim box As New TextBox
        Dim panel As New StackPanel
        Dim bdr As New Border

        For Each element As UIElement In collection
            panel = TryCast(element, StackPanel)

            If panel IsNot Nothing Then
                Dim str As String = CollatePanels(panel.Children)
                If panel.Name.Length > 0 Then
                    If panel.Name(0) = "P" And str.Length > 0 Then
                        EvalStr += "^(" + CollatePanels(panel.Children) + ")"
                        Continue For
                    End If
                End If

                If str.Length > 0 Then
                    EvalStr += "(" + CollatePanels(panel.Children) + ")"
                    Continue For
                End If

            End If

            bdr = TryCast(element, Border)

            If bdr IsNot Nothing Then
                Dim stk As StackPanel = TryCast(bdr.Parent, StackPanel)

                If stk.Name(0) = "F" Then
                    EvalStr += "/"
                End If
            End If

            box = TryCast(element, TextBox)

            If box IsNot Nothing Then
                EvalStr += box.Text
                Continue For
            End If

        Next

        Return EvalStr

    End Function

    Private Sub UpdateHistory()

        Dim str As String = CollatePanels(InputStk.Children)

        Dim Xindex As Integer

        Dim count1old = Count1

        If Count1 = -1 Then
            Xindex = LVLeft.Items.Count - 1
        Else
            Xindex = Count1 - 1
        End If

        Dim evalstr As String = Eval.Evaluate(str, LVLeft, Xindex)

        If str = "" Or evalstr = "" Then Exit Sub
        'Exit if the formula can not be evaluated

        Dim LVILeft As New ListViewItem
        Dim LVIRight As New ListViewItem
        Dim HistoryEdit As Boolean = False

        If Count1 > -1 Then 'Might be causing double evals when unnessecary

            LVLeft.SelectedItem = Nothing
            LVRight.SelectedItem = Nothing

            HistoryEdit = True

            Count1 = -1
        Else

            Dim TBRight As New TextBlock

            LVILeft.Content = evalstr
            LVILeft.Style = Application.Current.MainWindow.Resources("LVILeft")

            LVIRight.Content = TBRight
            LVIRight.Style = Application.Current.MainWindow.Resources("LVIRight")

            TBRight.Text = str
            TBRight.TextTrimming = TextTrimming.CharacterEllipsis
            LVLeft.Items.Insert(LVLeft.Items.Count, LVILeft)

            LVRight.Items.Insert(LVRight.Items.Count, LVIRight)
            'End If
        End If

        InitialTextBox = New TextBox
        InitialTextBox.Style = Application.Current.MainWindow.Resources("Input_n")
        InitialTextBox.Name = "InitialTextBox1"
        InitialTextBox.Text = ""

        AddStackHistory(count1old)

        InputStk.Children.Add(InitialTextBox)
        InitialTextBox.Focus()

        LBOutput.Content = ""

        LVLeft.ScrollIntoView(LVILeft)
        LVRight.ScrollIntoView(LVIRight)

        AddHandler LVILeft.MouseUp, AddressOf ListViewItems_Click
        AddHandler LVIRight.MouseUp, AddressOf ListViewItems_Click

        LVILeftLast = LVILeft
        LVIRightLast = LVIRight

        PrevStr = ""

        If HistoryEdit = False Then
            ''''''''''''''''''''''''''''''Increment Ans[] indexes'''''''''''''''''''''''''''''''
            Dim newstring As String

            For i As Integer = 0 To LVLeft.Items.Count - 1

                '''''''''''''''''''''''''''''''''''''''''''''''The visual entires''''''''''''''''''''''''''
                newstring = ReplaceAnsIndexes(LVRight.Items(i).Content.Text)

                If newstring <> LVRight.Items(i).Content.Text Then
                    LVRight.Items(i).content.text = newstring
                End If



                '''''''''''''''''''''''''''''''''''''''''''''The stackpanel entries'''''''''''''''''''''''''''''''''''''''''''''''''
                Dim txtlist As List(Of TextBox) = FindAllTextChildren(StackPanelHistory(i))

                For Each txtbox As TextBox In txtlist

                    newstring = ReplaceAnsIndexes(txtbox.Text)

                    If newstring <> txtbox.Text Then
                        txtbox.Text = newstring
                    End If

                Next

            Next

            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        End If


    End Sub

    Private Function ReplaceAnsIndexes(lvtext As String) As String

        Dim insideindex As Boolean = False

        Dim indexstring As String = ""
        Dim newstring As String = ""
        insideindex = False

        If lvtext.Contains("[") Then

            For ii As Integer = 0 To lvtext.Length - 1

                If lvtext(ii) = "]" Then

                    insideindex = False

                    Dim index As Integer = Convert.ToInt16(indexstring) + 1

                    newstring += index.ToString

                    indexstring = ""

                End If

                If insideindex = True Then

                    indexstring += lvtext(ii)
                    Continue For

                End If

                If lvtext(ii) = "[" Then

                    insideindex = True

                End If

                newstring += lvtext(ii)

            Next

            Return newstring

        End If

        Return lvtext

    End Function


    Private Function FindAllTextChildren(stk As UIElementCollection) As List(Of TextBox)

        Dim txtlist As New List(Of TextBox)

        For i As Integer = 0 To stk.Count - 1

            If TypeOf stk(i) Is Panel Then

                Dim stk2 As Panel = stk(i)

                Dim stklist As List(Of TextBox) = FindAllTextChildren(stk2.Children)

                txtlist.AddRange(stklist)

            ElseIf TypeOf stk(i) Is TextBox Then

                txtlist.Add(stk(i))

            End If

        Next

        Return txtlist

    End Function

    Private Sub TxtBackspace(txtbox As TextBox, amount As Integer)

        Dim txtstring As String = txtbox.Text
        Dim caretloc As Integer = txtbox.CaretIndex

        txtstring = txtstring.Remove(caretloc - amount, amount)

        txtbox.Text = txtstring
        txtbox.CaretIndex = careti - amount

    End Sub

    Private Sub TxtInsert(txtbox As TextBox, InsertString As String, Optional NewCaretPos As Integer = 0)

        Dim CaretLoc As Integer = txtbox.SelectionStart
        Dim SelectionLength As Integer = txtbox.SelectionLength
        Dim TxtString As String = txtbox.Text

        TxtString = TxtString.Remove(CaretLoc, SelectionLength)
        TxtString = TxtString.Insert(CaretLoc, InsertString)
        txtbox.Text = TxtString

        txtbox.Select(CaretLoc + InsertString.Length + NewCaretPos, 0)

    End Sub

    Dim CaretLoc As Integer
    Dim BracketLoc As Integer

    Private Function CountChar(str As String, ch As String) As Integer
        Dim count As Integer = 0
        For i As Integer = 0 To str.Length - 1
            If str(i) = ch Then count += 1
        Next
        Return count
    End Function

    Private Sub BracketInsert(txtbox As TextBox, LeftBracket As Boolean)
        Dim box As TextBox = TryCast(InputStk.Children.Item(0), TextBox)
        CaretLoc = txtbox.SelectionStart

        Select Case LeftBracket

            Case True
                TxtInsert(txtbox, "(")

            Case False
                If txtbox.Text.Length > CaretLoc Then
                    If txtbox.Text(CaretLoc) = ")" Then
                        If CountChar(txtbox.Text, "(") <= CountChar(txtbox.Text, ")") Then
                            box.Select(CaretLoc + 1, 0)
                            Return
                        End If
                    End If
                End If
                TxtInsert(txtbox, ")")
        End Select

    End Sub

    Private Sub TextBox_PreviewKeyUp(sender As TextBox, e As KeyEventArgs)
        If e.Key = Key.LeftAlt Or e.Key = Key.RightAlt Then
            e.Handled = True
        End If

        'Used for setting the selection after inserting an extra bracket.
        'Was very annoying and you couldn't set it in text changed.
        If SetSelection = True Then
            sender.Select(sender.CaretIndex + 1, 0)
            SetSelection = False
        End If

        'Would not select properly so moved it to here
        If sender.Text = "Ans⁻¹" And e.Key = Key.I Then
            sender.Select(sender.Text.Length, 0)
        End If

    End Sub

    Private Sub AddStackHistory(index As Integer)

        Dim uielist As New StackPanel
        Dim uie As UIElement
        Dim oldcount As Integer = InputStk.Children.Count - 1

        For i As Integer = 0 To oldcount

            uie = InputStk.Children(0)
            InputStk.Children.Remove(uie)
            uielist.Children.Add(uie)

        Next

        If index = -1 Then
            StackPanelHistory.Add(uielist.Children)
        Else
            StackPanelHistory(index) = uielist.Children
        End If

    End Sub

    Private Sub TakeStkHistory(index As Integer)

        InputStk.Children.Clear()
        Dim uie As UIElement
        Dim oldcount As Integer = StackPanelHistory(index).Count - 1

        For i As Integer = 0 To oldcount

            uie = StackPanelHistory(index)(0)
            StackPanelHistory(index).Remove(uie)
            InputStk.Children.Add(uie)

        Next

        Dim lsttxt As TextBox = InputStk.Children(InputStk.Children.Count - 1)

        lsttxt.Select(lsttxt.Text.Length, 0)
        lsttxt.Focus()

    End Sub

    Private Sub TextBox_PreviewKeyDown(sender As TextBox, e As KeyEventArgs)

        Dim keychar As Char = GetCharFromKey(e.Key)

        careti = sender.CaretIndex 'used for legitimate formula masking

        Dim box As TextBox = sender

        If Keyboard.IsKeyDown(Key.LeftCtrl) Or Keyboard.IsKeyDown(Key.RightCtrl) Then
            Exit Sub
        End If

        '''''Make it so that the fraction is unhighlighted if any button is pressed other than backspace
        If IsNothing(SelectedStackPanel) = False And e.Key <> Key.Back Then
            SelectedStackPanel.Background = Nothing
            SelectedStackPanel = Nothing
        End If

        If keychar = ")" Then

            If InputStk.Children.Count <= 1 Then
                Dim i As Integer = careti - 1

                Dim Openbrackets As Integer = 0
                Dim Closebrackets As Integer = 1

                While i >= 0
                    If box.Text(i) = ")" Then
                        Closebrackets += 1
                    ElseIf box.Text(i) = "(" Then
                        Openbrackets += 1
                    End If

                    i -= 1
                End While

                If Openbrackets < Closebrackets Then
                    box.Text = box.Text.Insert(0, "(")
                    box.Select(careti + 1, 0)
                End If

            End If

        End If

        If e.Key = Key.Escape Then
            ToggleVisibility()
        End If

        If e.Key = Key.Enter Then
            UpdateHistory()
        End If

        If e.Key = Key.Up Then
            If CollatePanels(InputStk.Children) = "" Or Count1 > 0 Then



                If LVLeft.Items.Count > 0 Then

                    If Count1 > -1 Then
                        AddStackHistory(Count1)
                    End If

                    If Count1 = -1 Then
                        Count1 = LVLeft.Items.Count
                    End If

                    Count1 -= 1

                    TakeStkHistory(Count1)


                    LVLeft.SelectedIndex = Count1
                    LVRight.SelectedIndex = Count1
                    LBOutput.Content = LVLeft.SelectedItem.content

                End If
                Exit Sub
            End If
            Exit Sub
        End If

        If e.Key = Key.Down Then

            If Count1 < LVLeft.Items.Count - 1 And Count1 <> -1 Then

                AddStackHistory(Count1)

                Count1 += 1

                TakeStkHistory(Count1)

                LVLeft.SelectedIndex = Count1
                LVRight.SelectedIndex = Count1
                LBOutput.Content = LVLeft.SelectedItem.content

            Else

                If Count1 <> -1 Then
                    LVLeft.SelectedItem = Nothing
                    LVRight.SelectedItem = Nothing

                    AddStackHistory(Count1)

                    Count1 = -1

                    InitialTextBox = New TextBox
                    InitialTextBox.Style = Application.Current.MainWindow.Resources("Input_n")
                    InitialTextBox.Name = "InitialTextBox1"
                    InitialTextBox.Text = ""
                    InputStk.Children.Add(InitialTextBox)
                    InitialTextBox.Select(0, 0)
                    InitialTextBox.Focus()
                    LBOutput.Content = ""
                End If


            End If

            Exit Sub
        End If

        Select Case e.Key

            Case Key.Home
                If InputStk.Children.Count > 1 Then

                    Dim txt As TextBox = TryCast(InputStk.Children(0), TextBox)
                    txt.Select(0, 0)
                    txt.Focus()
                    e.Handled = True
                End If

            Case Key.End

                If InputStk.Children.Count > 1 Then
                    Dim txt As TextBox = TryCast(InputStk.Children(InputStk.Children.Count - 1), TextBox)
                    txt.Select(txt.Text.Length, 0)
                    txt.Focus()
                    e.Handled = True
                End If

            Case Key.E
                TxtInsert(box, "e", 0)
                e.Handled = True

            Case Key.X
                TxtInsert(box, "ₓ₁₀", 0)
                e.Handled = True

            Case Key.L

                If box.Text.SafeIndex(careti) = "(" Then
                    TxtInsert(box, "log", 0)
                Else
                    TxtInsert(box, "log()", -1)
                End If
                e.Handled = True

            Case Key.S
                If box.Text.SafeIndex(careti) = "(" Then
                    TxtInsert(box, "sin", 0)
                Else
                    TxtInsert(box, "sin()", -1)
                End If
                e.Handled = True

            Case Key.C
                If box.Text.SafeIndex(careti) = "(" Then
                    TxtInsert(box, "cos", 0)
                Else
                    TxtInsert(box, "cos()", -1)
                End If
                e.Handled = True

            Case Key.T
                If box.Text.SafeIndex(careti) = "(" Then
                    TxtInsert(box, "tan", 0)
                Else
                    TxtInsert(box, "tan()", -1)
                End If
                e.Handled = True

            Case Key.R
                If box.Text.SafeIndex(careti) = "(" Then
                    TxtInsert(box, "√", 0)
                Else
                    TxtInsert(box, "√()", -1)
                End If
                e.Handled = True

            Case Key.N
                If box.Text.Length > 0 Then
                    Dim selectionst As Integer = box.SelectionStart

                    If box.Text(selectionst - 1) = "a" Then
                        box.Text = box.Text.Remove(box.SelectionStart - 1, 1)
                        box.Select(selectionst - 1, 0)
                        TxtInsert(box, "Ans", 0)
                        e.Handled = True
                    End If
                End If

            Case Key.V
                If box.Text.Length > 0 Then
                    If box.Text(box.SelectionStart - 1) = "a" Then
                        TxtInsert(box, "vg()", -1)
                        e.Handled = True
                    End If
                End If

            Case Key.I
                TxtInsert(box, "⁻¹")
                e.Handled = True

            Case Key.Q
                TxtInsert(box, "²")
                e.Handled = True

            Case Key.P
                TxtInsert(box, "π")
                e.Handled = True

            Case Key.OemOpenBrackets
                BracketInsert(box, True)
                e.Handled = True

            Case Key.OemCloseBrackets

                '''''''''Copied from above'''''''''''
                If InputStk.Children.Count <= 1 Then
                    Dim i As Integer = careti - 1

                    Dim Openbrackets As Integer = 0
                    Dim Closebrackets As Integer = 1

                    While i >= 0
                        If box.Text(i) = ")" Then
                            Closebrackets += 1
                        ElseIf box.Text(i) = "(" Then
                            Openbrackets += 1
                        End If

                        i -= 1
                    End While

                    If Openbrackets < Closebrackets Then
                        box.Text = box.Text.Insert(0, "(")
                        box.Select(careti + 1, 0)
                    End If

                End If

                BracketInsert(box, False)
                e.Handled = True

            Case Key.Multiply
                CaretLoc = box.SelectionStart
                If CaretLoc >= 1 Then
                    If box.Text(CaretLoc - 1) = "×" Then
                        box.Text = box.Text.Remove(CaretLoc - 1, 1)
                        box.CaretIndex = CaretLoc - 1
                        TxtInsert(box, "^")
                        e.Handled = True
                        Exit Sub
                    End If
                End If
                ''''''''

                'WORK AROUND FOR FUNNY ANS× SELCTION THING
                If box.Text = "" And InputStk.Children.Count = 1 Then Exit Sub
                TxtInsert(box, "×", 0)
                e.Handled = True

                Exit Sub

        End Select

        If keychar = "*" Then
            'copied from above for the shift one

            CaretLoc = box.SelectionStart
            If CaretLoc >= 1 Then
                If box.Text(CaretLoc - 1) = "×" Then
                    box.Text = box.Text.Remove(CaretLoc - 1, 1)
                    box.CaretIndex = CaretLoc - 1
                    TxtInsert(box, "^")
                    e.Handled = True
                End If
            End If

            'WORK AROUND FOR FUNNY ANS× SELCTION THING
            If box.Text = "" And InputStk.Children.Count = 1 Then Exit Sub

            TxtInsert(box, "×", 0)
            e.Handled = True


        End If

        '''''''''''''''''''''''''''''''''''''''''''''''''''RICH MATH IMPLEMENTATION''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        Select Case e.Key

            Case Key.Back

                If sender.CaretIndex = 0 Then
                    Dim stk As Panel
                    Dim stkprevious As Panel
                    Dim txt As TextBox
                    Dim s As Integer

                    stk = VisualTreeHelper.GetParent(sender)
                    'stackprevious = VisualTreeHelper.GetParent(stk)

                    s = stk.Children.IndexOf(sender)

                    If s > 0 Then

                        Dim uiprevious As UIElement = stk.Children(s - 1)

                        If TypeOf stk.Children(s - 1) Is StackPanel Then
                            stkprevious = uiprevious

                            Dim txt1 As TextBox = TryCast(stk.Children(s), TextBox)

                            If txt1.SelectionLength = 0 Then
                                If IsNothing(stkprevious.Background) Then
                                    stkprevious.Background = Brushes.LightBlue
                                    SelectedStackPanel = stkprevious
                                Else

                                    RemoveStackPanel(sender, stk, stkprevious)
                                    SelectedStackPanel = Nothing
                                End If
                                e.Handled = True

                            End If

                        ElseIf TypeOf uiprevious Is TextBox Then
                            txt = uiprevious

                            txt.Focus()
                            txt.Select(txt.Text.Length, 0)
                            e.Handled = True
                        End If

                    ElseIf s = 0 Then
                        If stk.Equals(InputStk) = False Then

                            Dim stk2 As StackPanel = VisualTreeHelper.GetParent(stk)

                            If CollatePanels(stk2.Children).ContainsOnly({"(", ")", "/"}) Then
                                'Check if the panel is pretty much empty

                                If IsNothing(stk2.Background) Then
                                    stk2.Background = Brushes.LightBlue
                                    SelectedStackPanel = stk2
                                ElseIf stk2.Background.Equals(Brushes.LightBlue) Then
                                    Dim stk3 As Panel = VisualTreeHelper.GetParent(stk2)

                                    RemoveStackPanel(sender, stk3, stk2)
                                    SelectedStackPanel = Nothing
                                End If

                            Else

                                MoveLeft(e)

                            End If

                            e.Handled = True
                        End If

                    End If

                ElseIf sender.SelectionLength = 0 Then

                    Dim txt As String = sender.Text
                    Dim delete As Integer = sender.CaretIndex - 1
                    Dim check As String

                    Select Case txt(delete)

                        Case "n"
                            check = txt.SafeString(delete - 3, 3)
                            If check = "ata" Or check = "asi" Then
                                TxtBackspace(sender, 4)
                                e.Handled = True
                            Else
                                check = txt.SafeString(delete - 2, 2)
                                If check = "ta" Or check = "si" Then
                                    TxtBackspace(sender, 3)
                                    e.Handled = True
                                End If

                            End If

                        Case "s"
                            check = txt.SafeString(delete - 3, 3)
                            If check = "aco" Then
                                TxtBackspace(sender, 4)
                                e.Handled = True

                            Else
                                check = txt.SafeString(delete - 2, 2)
                                If check = "co" Or check = "An" Then
                                    TxtBackspace(sender, 3)
                                    e.Handled = True
                                End If


                            End If

                        Case "g"
                            check = txt.SafeString(delete - 3, 3)
                            If check = "nlo" Then
                                TxtBackspace(sender, 4)
                                e.Handled = True

                            Else
                                check = txt.SafeString(delete - 2, 2)
                                If check = "lo" Or check = "av" Then
                                    TxtBackspace(sender, 3)
                                    e.Handled = True
                                End If


                            End If

                        Case "¹"
                            check = txt.SafeIndex(delete - 1)
                            If check = "⁻" Then
                                TxtBackspace(sender, 2)
                                e.Handled = True
                            End If

                        Case "₀"
                            check = txt.SafeString(delete - 2, 2)
                            If check = "ₓ₁" Then
                                TxtBackspace(sender, 3)
                                e.Handled = True
                            End If

                        Case "]"
                            Dim startindex As Integer = delete
                            Dim ii As Integer = delete
                            While ii >= 0

                                If txt(ii) = "[" Then
                                    startindex = ii
                                    Exit While
                                End If

                                ii -= 1
                            End While

                            If startindex < delete Then
                                TxtBackspace(sender, delete - startindex + 1)
                                e.Handled = True
                            End If

                    End Select

                End If

                '''''''New back handling
                SendEvalRequest()

                'For updating top with backspace
                If Count1 > -1 Then

                    Dim str As String = CollatePanels(InputStk.Children)

                    If str.Length = 1 Or str.Length = 0 Then
                        LVRight.Items(Count1).Content.Text = ""
                        LVLeft.Items(Count1).content = "0"
                    End If

                End If
                '''''''
            Case Key.Tab
                If InputStk.Children.IndexOf(sender) = InputStk.Children.Count - 1 Then
                    e.Handled = True
                End If

            Case Key.Right

                If IsNothing(SelectedStackPanel) Then
                    If sender.CaretIndex = sender.Text.Length Then
                        MoveRight(e)
                    End If

                Else
                    'SelectedStackPanel.Background = Nothing
                    'SelectedStackPanel = Nothing
                End If

            Case Key.Left

                If IsNothing(SelectedStackPanel) Then
                    If sender.CaretIndex = 0 Then MoveLeft(e)
                Else
                    'SelectedStackPanel.Background = Nothing
                    'SelectedStackPanel = Nothing
                End If

            'Case Key.O
            '    '''''''''Preliminary key
            '    Dim pwr As New Power("")

            '    PauseEval = True
            '    pwr.Add(sender)
            '    e.Handled = True

            Case Key.Divide

                If sender.CaretIndex > 0 Then
                    If sender.Text(sender.CaretIndex - 1) = "/" Then
                        e.Handled = True


                        Dim fPanel As New Fraction("", "")

                        PauseEval = True
                        fPanel.Add(sender)

                        Exit Sub
                    End If
                End If

        End Select

        'copied from above for the shift divide symbols
        If keychar = "/" Then

            If sender.CaretIndex > 0 Then
                If sender.Text(sender.CaretIndex - 1) = "/" Then
                    e.Handled = True


                    Dim fPanel As New Fraction("", "")
                    'Dim StkHeight As Integer = InputStk.ActualHeight

                    PauseEval = True
                    fPanel.Add(sender)

                    'Me.Height += InputStk.ActualHeight - StkHeight
                    'Should work but somehow the 

                    Exit Sub
                End If
            End If

        End If

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    End Sub


    Private Sub RemoveStackPanel(sender As TextBox, Parent As Panel, Child As StackPanel)
        'Removes fraction stackpanel and deals with collating the textboxes left over

        'Remove the previous element

        Dim s As Integer
        s = Parent.Children.IndexOf(Child)
        'Will get index of next sender which takes its place

        Parent.Children.Remove(Child)

        'Collate two textboxes if they are touching
        'sender is destroyed if executed within panel

        If s > 0 Then
            Dim uiprevious As UIElement
            Dim uicurrent As UIElement
            Dim txt2 As TextBox
            Dim txt As TextBox
            uiprevious = Parent.Children(s - 1)
            txt = uiprevious
            uicurrent = Parent.Children(s)
            txt2 = uicurrent

            ''''Run textbox checkers!
            Dim NewCaretLocation As Integer = txt.Text.Length
            txt.Text += txt2.Text

            Parent.Children.Remove(txt2)
            txt.Focus()
            txt.Select(NewCaretLocation, 0)

        End If

    End Sub

    Private Sub MoveRight(e As KeyEventArgs)
        e.Handled = True

        Dim focusDirection As FocusNavigationDirection = FocusNavigationDirection.Next

        Dim request As New TraversalRequest(focusDirection)

        Dim elementWithFocus As UIElement = TryCast(Keyboard.FocusedElement, UIElement)

        If elementWithFocus IsNot Nothing Then
            elementWithFocus.MoveFocus(request)
            Dim txt As TextBox = TryCast(Keyboard.FocusedElement, TextBox)

            If txt IsNot Nothing Then
                txt.CaretIndex() = 0
            Else
                focusDirection = FocusNavigationDirection.Previous
                request = New TraversalRequest(focusDirection)
                elementWithFocus = TryCast(Keyboard.FocusedElement, UIElement)
                elementWithFocus.MoveFocus(request)
            End If

        End If
    End Sub

    Private Sub MoveLeft(e As KeyEventArgs)
        e.Handled = True

        Dim focusDirection As FocusNavigationDirection = FocusNavigationDirection.Previous

        Dim request As New TraversalRequest(focusDirection)

        Dim elementWithFocus As UIElement = TryCast(Keyboard.FocusedElement, UIElement)

        If elementWithFocus IsNot Nothing Then
            elementWithFocus.MoveFocus(request)
            Dim txt As TextBox = TryCast(Keyboard.FocusedElement, TextBox)

            If txt IsNot Nothing Then
                txt.CaretIndex() = txt.Text.Length
            Else
                focusDirection = FocusNavigationDirection.Next
                request = New TraversalRequest(focusDirection)
                elementWithFocus = TryCast(Keyboard.FocusedElement, UIElement)
                elementWithFocus.MoveFocus(request)
            End If

        End If
    End Sub

    Private Sub ContextMenuHandler_Click(ByVal sender As Object, ByVal e As System.EventArgs)

        Select Case sender.Text
            Case "Exit"
                Application.Current.Shutdown()
            Case "Help"
                Dim help As New Help
                help.Show()

            Case "About"
                Dim aboutform As New About
                aboutform.Show()

            Case "Reset Location"
                Dim WorkingArea = System.Windows.SystemParameters.WorkArea
                My.Settings.WindowPosX = WorkingArea.Right - Me.Width - 10
                My.Settings.WindowPosY = WorkingArea.Bottom - Me.Height - 10

                Me.Left = My.Settings.WindowPosX
                Me.Top = My.Settings.WindowPosY

                My.Settings.Save()

            Case "Radians"
                ctx.Items(0).Text = "✔ Radians"
                ctx.Items(1).Text = "Degrees"
                My.Settings.Degrees = False
                Eval = New MyEvaluator(False)
                UpdateHistory()

            Case "Degrees"
                ctx.Items(0).Text = "Radians"
                ctx.Items(1).Text = "✔ Degrees"
                My.Settings.Degrees = True
                Eval = New MyEvaluator(True)
                UpdateHistory()

        End Select


    End Sub

    Private Sub ToggleVisibility(Optional action As String = "")


        If Me.WindowState = Windows.WindowState.Minimized Or action = "show" Then
            Me.Show()
            Me.WindowState = Windows.WindowState.Normal
            Me.Focus()
            If LastFocusedTxt Is Nothing Then
                LastFocusedTxt = InitialTextBox
            End If
            LastFocusedTxt.Focus()
            Exit Sub
        End If

        If Me.WindowState = Windows.WindowState.Normal Or action = "hide" Then

            Me.WindowState = Windows.WindowState.Minimized
            Me.Hide()
        End If


    End Sub


    Private Sub nf_Click(sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        If e.Button = Forms.MouseButtons.Left Then
            ToggleVisibility()

        ElseIf e.Button = Forms.MouseButtons.Right Then
            Dim pt As System.Drawing.Point
            pt.X = 0
            pt.Y = 0
            ctx.Show(Forms.Control.MousePosition)

        End If

    End Sub

    Private Sub Window_Loaded(sender As Object, e As EventArgs)
        nf.Visible = True
        Me.Hide()

    End Sub

    Private Sub AddTrayIcon()
        nf = New System.Windows.Forms.NotifyIcon()
        nf.Icon = My.Resources.Resources.u1818

        nf.BalloonTipText = "Alt1 - Calculator"



        AddHandler nf.Click, AddressOf nf_Click

        ctx = New Forms.ContextMenuStrip


        Dim ctxi As New Forms.ToolStripMenuItem

        ctxi.Text = "Exit"
        ctxi.Tag = "1"

        AddHandler ctxi.Click, AddressOf ContextMenuHandler_Click

        Dim ctxi1 As New Forms.ToolStripMenuItem

        ctxi1.Text = "Help"
        ctxi1.Tag = "2"

        AddHandler ctxi1.Click, AddressOf ContextMenuHandler_Click

        Dim ctxi2 As New Forms.ToolStripMenuItem

        ctxi2.Text = "About"
        ctxi2.Tag = "3"

        AddHandler ctxi2.Click, AddressOf ContextMenuHandler_Click

        Dim ctxi3 As New Forms.ToolStripMenuItem

        ctxi3.Text = "Reset Location"
        ctxi3.Tag = "4"

        AddHandler ctxi3.Click, AddressOf ContextMenuHandler_Click

        Dim ctxi4 As New Forms.ToolStripMenuItem

        ctxi4.Text = "✔ Degrees"
        ctxi4.Tag = "5"

        AddHandler ctxi4.Click, AddressOf ContextMenuHandler_Click

        Dim ctxi5 As New Forms.ToolStripMenuItem

        ctxi5.Text = "Radians"
        ctxi5.Tag = "6"

        AddHandler ctxi5.Click, AddressOf ContextMenuHandler_Click

        ctx.Items.Add(ctxi5)
        ctx.Items.Add(ctxi4)
        ctx.Items.Add(ctxi3)
        ctx.Items.Add(ctxi2)
        ctx.Items.Add(ctxi1)
        ctx.Items.Add(ctxi)

        nf.ContextMenuStrip() = ctx

    End Sub

    Public Sub Current_DispatcherUnhandledException(sender As Object, e As System.Windows.Threading.DispatcherUnhandledExceptionEventArgs)
        e.Handled = True
        MessageBox.Show("We are sorry but you have encountered an error" + Environment.NewLine + Environment.NewLine + "Please send a screenshot And some details to support@alt1.co" + Environment.NewLine + Environment.NewLine + "Thanks!")
    End Sub

    Private Sub ShowBalloon()
        Dim assemblyversion As Version = Assembly.GetEntryAssembly().GetName().Version

        Dim s As String = assemblyversion.ToString()

        s = s.Remove(s.Length - 2)

        nf.BalloonTipTitle = "Alt1 v" + s
        nf.BalloonTipText = "You've just downloaded the latest updates. They will be applied when you restart your computer or the application."
        nf.ShowBalloonTip(10000)
    End Sub

    Private Sub Window_Initialized(sender As Object, e As EventArgs)

        FormTimer.Interval = 50

        AddHandler Application.Current.DispatcherUnhandledException, AddressOf Current_DispatcherUnhandledException

        Dim WorkingArea = System.Windows.SystemParameters.WorkArea

        If My.Settings.WindowPosX = -1 Or My.Settings.WindowPosY = -1 Then
            My.Settings.WindowPosX = WorkingArea.Right - Me.Width - 10
            My.Settings.WindowPosY = WorkingArea.Bottom - Me.Height - 10
        End If

        Me.Left = My.Settings.WindowPosX
        Me.Top = My.Settings.WindowPosY

        Me.WindowState = Windows.WindowState.Minimized

        AddTrayIcon()

        If My.Settings.Degrees = False Then
            ctx.Items(0).Text = "✔ Radians"
            ctx.Items(1).Text = "Degrees"
        End If

        Task.Run(Async Function()
                     Using mgr As New UpdateManager("http://www.alt1-calculator.sourceforge.net/Alt1Downloads/")
                         Await mgr.UpdateApp

                     End Using
                 End Function)


#If CONFIG = "Release" Then
        Using mgr = New UpdateManager("http://www.alt1-calculator.sourceforge.net/Alt1Downloads/")
            ' Note, in most of these scenarios, the app exits after this method
            ' completes!
            SquirrelAwareApp.HandleEvents(
                onInitialInstall:=Sub()
                                      'mgr.CreateShortcutForThisExe()

                                      mgr.CreateShortcutsForExecutable("Alt1.exe", ShortcutLocation.StartMenu, False)
                                      mgr.CreateShortcutsForExecutable("Alt1.exe", ShortcutLocation.Startup, False)
                                  End Sub,
                onAppUpdate:=Sub()
                                 mgr.CreateShortcutForThisExe()

                                 mgr.CreateShortcutsForExecutable("Alt1.exe", ShortcutLocation.StartMenu, True)
                                 mgr.CreateShortcutsForExecutable("Alt1.exe", ShortcutLocation.Startup, True)

                                 'MsgBox("You've just downloaded the latest updates.  They will be applied when you restart your computer or the application.")
                                 'Dim updater = New Updater
                                 ShowBalloon()
                                 'Updater.Show()
                             End Sub,
                onAppUninstall:=Sub() mgr.RemoveShortcutForThisExe(),
                onFirstRun:=Sub()
                                Dim installer = New Installer
                                installer.Show()
                            End Sub
                )
        End Using
#End If


    End Sub


    Private Sub hotkeypressed(sender As Object)

        ToggleVisibility()

    End Sub


    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)

        ToggleVisibility()

    End Sub

    Private Sub Window_SizeChanged(sender As Object, e As SizeChangedEventArgs)

        Dim border As Border
        Dim scrollv As ScrollViewer

        Try
            border = VisualTreeHelper.GetChild(LVRight, 0)
        Catch ex As Exception

        End Try

        Try
            scrollv = VisualTreeHelper.GetChild(border, 0)
        Catch ex As Exception

        End Try


        If IsNothing(LVILeftLast) = False Then
            LVLeft.ScrollIntoView(LVILeftLast)
        End If

        If IsNothing(LVIRightLast) = False Then
            scrollv.ScrollToEnd()
        End If

    End Sub

    Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)

        InitialTextBox.Select(InitialTextBox.Text.Length, 0)
        InitialTextBox.Focus()
    End Sub

    Private Sub InputStk_MouseDown(sender As Object, e As MouseButtonEventArgs)
        InitialTextBox.Focus()
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        nf.Visible = False
        My.Settings.Save()
    End Sub

    Public Const WM_NCLBUTTONDOWN As Integer = &HA1
    Public Const HT_CAPTION As Integer = &H2
    Dim hWnd As IntPtr = FindWindow("MainWindow", vbNull)

    <DllImportAttribute("user32.dll")>
    Public Shared Function SendMessage(hWnd As IntPtr, Msg As Integer, wParam As Integer, lParam As Integer) As Integer
    End Function
    <DllImportAttribute("user32.dll")>
    Public Shared Function ReleaseCapture() As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function FindWindow(lpClassName As String, lpWindowName As String) As IntPtr
    End Function

    Private Sub Label_MouseDown(sender As Object, e As MouseButtonEventArgs)
        If e.LeftButton = MouseButtonState.Pressed Then
            Me.DragMove()
            My.Settings.WindowPosX = Me.Left
            My.Settings.WindowPosY = Me.Top
            My.Settings.Save()
        End If
    End Sub

    Private Sub Label_MouseUp(sender As Object, e As MouseButtonEventArgs)

    End Sub

    Private Sub PrintVisualTree(depth As Integer, obj As DependencyObject)
        ' Print the object with preceding spaces that represent its depth

        Console.WriteLine(New String(" "c, depth) & Convert.ToString(obj))

        ' Recursive call for each visual child
        For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(obj) - 1
            PrintVisualTree(depth + 1, VisualTreeHelper.GetChild(obj, i))
        Next
    End Sub

    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)

        LVLeft.Items.Clear()
        LVRight.Items.Clear()
        InitialTextBox.Clear()
        InitialTextBox.Focus()
        Count1 = -1
        StackPanelHistory.Clear()

    End Sub

    Private Sub Window_PreviewKeyDown(sender As Object, e As KeyEventArgs)

        If Keyboard.Modifiers = ModifierKeys.Alt Then
            e.Handled = True
        End If

    End Sub

    Private Sub LVLeft_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs)
        e.Handled = True
    End Sub

    Private Sub MenuItem_Click(sender As Object, e As RoutedEventArgs)

        If LastClickedListViewItem IsNot Nothing Then

            Dim txt As TextBlock

            If TypeOf LastClickedListViewItem.Content Is TextBlock Then

                Clipboard.SetText(LastClickedListViewItem.Content.text)
            Else
                Clipboard.SetText(LastClickedListViewItem.Content)
            End If

        End If

    End Sub

    Private Sub ListViewItems_Click(sender As Object, e As MouseButtonEventArgs)
        LastClickedListViewItem = sender

        If e.ChangedButton = MouseButton.Left Then

            Dim lv As ListView = LastClickedListViewItem.Parent

            Dim fakeindex As Integer = lv.Items.Count - lv.Items.IndexOf(LastClickedListViewItem)

            TxtInsert(LastFocusedTxt, "Ans[" + fakeindex.ToString + "]")

            LastFocusedTxt.Focus()

        End If

    End Sub

    Private Sub InitialTextBox_GotFocus(sender As Object, e As RoutedEventArgs)
        LastFocusedTxt = sender
        Console.WriteLine(sender.text)
    End Sub
End Class
