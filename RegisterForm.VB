Imports System.Drawing
Imports System.Windows.Forms

Public Class RegisterForm
    Inherits Form
    Private txtUser As TextBox
    Private txtPass As TextBox
    Private txtName As TextBox
    Private txtEmail As TextBox

    Public Sub New()
        StyleForm(Me, "Register New Account", 500, 420)
        PageHeader(Me, "SIGN UP", "Create a new client account", &H1F464, PurpleBtn, Color.White, 30, 15)

        Controls.Add(Lbl("Full Name", 40, 110, 10))
        txtName = Txt(180, 106, 260)
        Controls.Add(txtName)

        Controls.Add(Lbl("Email", 40, 160, 10))
        txtEmail = Txt(180, 156, 260)
        Controls.Add(txtEmail)

        Controls.Add(Lbl("Username", 40, 210, 10))
        txtUser = Txt(180, 206, 260)
        Controls.Add(txtUser)

        Controls.Add(Lbl("Password", 40, 260, 10))
        txtPass = Txt(180, 256, 260)
        txtPass.UseSystemPasswordChar = True
        Controls.Add(txtPass)

        Dim btnSave As Button = Btn("Register", 180, 315, 120, 38, GreenBtn, Color.White, 10)
        Dim btnCancel As Button = Btn("Cancel", 320, 315, 120, 38, Color.Black, Color.White, 10)
        Controls.Add(btnSave)
        Controls.Add(btnCancel)
        AddHandler btnSave.Click, AddressOf SaveClick
        AddHandler btnCancel.Click, Sub(s, e) Close()
    End Sub

    Private Sub SaveClick(sender As Object, e As EventArgs)
        If txtUser.Text.Trim() = "" OrElse txtPass.Text.Length < 6 OrElse txtName.Text.Trim() = "" Then
            Warn("Please fill all fields. Password must be at least 6 characters.")
            Return
        End If
        Dim exists = Convert.ToInt32(Scalar("SELECT COUNT(*) FROM Users WHERE Username=@u", "@u", txtUser.Text.Trim()))
        If exists > 0 Then
            Warn("Username already taken.")
            Return
        End If

        Exec("INSERT INTO Customers(FullName,Email) VALUES(@n,@e)", "@n", txtName.Text.Trim(), "@e", txtEmail.Text.Trim())
        Dim cid = Convert.ToInt32(Scalar("SELECT Id FROM Customers ORDER BY Id DESC LIMIT 1"))
        Exec("INSERT INTO Users(Username,PasswordHash,Role,CustomerId) VALUES(@u,@h,'Client',@c)", "@u", txtUser.Text.Trim(), "@h", Hash(txtPass.Text), "@c", cid)
        Info("Account registered successfully! You can now log in.")
        Close()
    End Sub
End Class
