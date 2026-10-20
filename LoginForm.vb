Imports System.Drawing
Imports System.Windows.Forms

Public Class LoginForm
    Inherits Form

    Private txtUser As TextBox
    Private txtPass As TextBox
    Private btnLogin As Button
    Private btnRegister As Button
    Private btnExit As Button

    Public Sub New()
        StyleForm(Me, "Login", 630, 580)

        Controls.Add(GlyphTile(&H1F512, 292, 45, 46, 46, Color.Yellow, Color.Black, 20))
        Controls.Add(CellLbl("THE LEDGER", 0, 105, 630, 50, 24, True))
        Controls.Add(CellLbl("Subscription Tracker", 0, 152, 630, 26, 11, False))

        Controls.Add(Lbl("Username", 105, 218, 11, True))
        txtUser = Txt(213, 215, 246)
        Controls.Add(txtUser)

        Controls.Add(Lbl("Password", 108, 273, 11, True))
        txtPass = Txt(213, 270, 246)
        txtPass.UseSystemPasswordChar = True
        Controls.Add(txtPass)

        btnLogin = Btn("LOGIN", 213, 340, 246, 42, NavyBtn, Color.White, 12)
        btnRegister = Btn("REGISTER", 213, 395, 246, 42, PurpleBtn, Color.White, 11)
        btnExit = Btn("EXIT", 213, 450, 246, 40, DarkRed, Color.White, 11)
        Controls.Add(btnLogin)
        Controls.Add(btnRegister)
        Controls.Add(btnExit)

        AcceptButton = btnLogin
        AddHandler btnLogin.Click, AddressOf LoginClick
        AddHandler btnRegister.Click, AddressOf RegisterClick
        AddHandler btnExit.Click, Sub(s, e) Me.DialogResult = DialogResult.Cancel
    End Sub

    Private Sub LoginClick(sender As Object, e As EventArgs)
        If txtUser.Text.Trim() = "" OrElse txtPass.Text = "" Then
            Warn("Please enter your username and password.")
            Return
        End If
        If CheckLogin(txtUser.Text, txtPass.Text) Then
            DialogResult = DialogResult.OK
        Else
            Warn("Invalid username or password.")
            txtPass.Clear()
            txtPass.Focus()
        End If
    End Sub

    Private Sub RegisterClick(sender As Object, e As EventArgs)
        Using rf As New RegisterForm()
            rf.ShowDialog(Me)
        End Using
    End Sub
End Class
