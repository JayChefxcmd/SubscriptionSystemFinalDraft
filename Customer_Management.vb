Imports System.Data
Imports System.Drawing
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

Public Class Customer_Management
    Inherits Form

    Private txtName As TextBox
    Private txtEmail As TextBox
    Private txtPhone As TextBox
    Private txtAddress As TextBox
    Private txtAccount As TextBox
    Private txtCard As TextBox
    Private flow As FlowLayoutPanel
    Private editId As Integer = 0

    Public Sub New()
        StyleForm(Me, "Customer_Management", 1180, 620)
        PageHeader(Me, "CUSTOMER MANAGEMENT", "Manage Customer Information & Payment Details", &H1F465, Color.White, Color.FromArgb(110, 0, 110), 40, 15)

        Dim left As New DashedPanel()
        left.Bounds = New Rectangle(30, 110, 480, 430)
        Controls.Add(left)
        left.Controls.Add(Lbl("CUSTOMER INFORMATION", 30, 15, 11, True))

        left.Controls.Add(Lbl("Full Name", 30, 52, 10))
        txtName = Txt(180, 48, 275)
        left.Controls.Add(txtName)

        left.Controls.Add(Lbl("Email", 30, 97, 10))
        txtEmail = Txt(180, 93, 275)
        left.Controls.Add(txtEmail)

        left.Controls.Add(Lbl("Phone", 30, 142, 10))
        txtPhone = Txt(180, 138, 275)
        left.Controls.Add(txtPhone)

        left.Controls.Add(Lbl("Address", 30, 187, 10))
        txtAddress = Txt(180, 183, 275)
        left.Controls.Add(txtAddress)

        left.Controls.Add(Lbl("Account No.", 30, 232, 10))
        txtAccount = Txt(180, 228, 275)
        left.Controls.Add(txtAccount)

        left.Controls.Add(Lbl("Card/Bank Info", 30, 277, 10))
        txtCard = Txt(180, 273, 275)
        left.Controls.Add(txtCard)

        Dim btnSave As Button = Btn("Save", 90, 335, 135, 40, GreenBtn, Color.Black, 11)
        Dim btnClear As Button = Btn("Clear", 245, 335, 135, 40, Color.FromArgb(200, 0, 0), Color.Black, 11)
        left.Controls.Add(btnSave)
        left.Controls.Add(btnClear)
        AddHandler btnSave.Click, AddressOf SaveClick
        AddHandler btnClear.Click, Sub(s, e) ClearForm()

        Dim right As New DashedPanel()
        right.Bounds = New Rectangle(530, 110, 620, 430)
        Controls.Add(right)
        right.Controls.Add(Lbl("CUSTOMERS", 20, 20, 11, True))

        flow = New FlowLayoutPanel()
        flow.Bounds = New Rectangle(12, 60, 596, 356)
        flow.AutoScroll = True
        flow.FlowDirection = FlowDirection.TopDown
        flow.WrapContents = False
        right.Controls.Add(flow)

        Dim btnBack As Button = Btn(ChrW(&H2190) & " BACK", 1020, 555, 130, 38, PurpleBtn, Color.White, 10)
        Controls.Add(btnBack)
        AddHandler btnBack.Click, Sub(s, e) Close()
    End Sub

    Private Sub Customer_Management_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadList()
    End Sub

    Private Sub ClearForm()
        editId = 0
        txtName.Clear()
        txtEmail.Clear()
        txtPhone.Clear()
        txtAddress.Clear()
        txtAccount.Clear()
        txtCard.Clear()
    End Sub

    Private Sub LoadList()
        flow.SuspendLayout()
        flow.Controls.Clear()
        Dim dt As DataTable = Query("SELECT * FROM Customers ORDER BY FullName")
        For Each r As DataRow In dt.Rows
            Dim id As Integer = ToInt(r("Id"))
            Dim row As New DashedPanel()
            row.Size = New Size(570, 76)
            row.Margin = New Padding(0, 0, 0, 8)

            row.Controls.Add(Lbl(Sv(r("FullName")), 14, 8, 11))
            Dim em As Label = Lbl(Sv(r("Email")), 14, 34, 8.0F, False, Color.Silver)
            row.Controls.Add(em)
            row.Controls.Add(Lbl("Acc: " & Sv(r("AccountNumber")), 14, 52, 8.0F))
            row.Controls.Add(Lbl(Sv(r("Phone")), 260, 8, 10))

            Dim be As Button = Btn("EDIT", 410, 20, 70, 34, PurpleBtn, Color.White, 9)
            Dim bd As Button = Btn("DELETE", 490, 20, 70, 34, Color.FromArgb(128, 0, 0), Color.White, 8.5F)
            be.Tag = id
            bd.Tag = id
            AddHandler be.Click, AddressOf EditClick
            AddHandler bd.Click, AddressOf DeleteClick
            row.Controls.Add(be)
            row.Controls.Add(bd)
            flow.Controls.Add(row)
        Next
        flow.ResumeLayout()
    End Sub

    Private Sub EditClick(sender As Object, e As EventArgs)
        Dim id As Integer = CInt(DirectCast(sender, Button).Tag)
        Dim dt As DataTable = Query("SELECT * FROM Customers WHERE Id=@i", "@i", id)
        If dt.Rows.Count = 0 Then Return
        Dim r As DataRow = dt.Rows(0)
        editId = id
        txtName.Text = Sv(r("FullName"))
        txtEmail.Text = Sv(r("Email"))
        txtPhone.Text = Sv(r("Phone"))
        txtAddress.Text = Sv(r("Address"))
        txtAccount.Text = Sv(r("AccountNumber"))
        txtCard.Text = Sv(r("CardInfo"))
        txtName.Focus()
    End Sub

    Private Sub DeleteClick(sender As Object, e As EventArgs)
        Dim id As Integer = CInt(DirectCast(sender, Button).Tag)
        Dim nm As String = Sv(Scalar("SELECT FullName FROM Customers WHERE Id=@i", "@i", id))
        If Confirm("Delete customer '" & nm & "'?") Then
            Exec("DELETE FROM Customers WHERE Id=@i", "@i", id)
            If editId = id Then ClearForm()
            LoadList()
        End If
    End Sub

    Private Sub SaveClick(sender As Object, e As EventArgs)
        Dim nm As String = txtName.Text.Trim()
        Dim em As String = txtEmail.Text.Trim()
        Dim ph As String = txtPhone.Text.Trim()
        Dim ad As String = txtAddress.Text.Trim()
        Dim ac As String = txtAccount.Text.Trim()
        Dim cr As String = txtCard.Text.Trim()

        If nm = "" Then
            Warn("Please enter the customer's full name.")
            txtName.Focus()
            Return
        End If

        If editId = 0 Then
            Exec("INSERT INTO Customers(FullName,Email,Phone,Address,AccountNumber,CardInfo) VALUES(@n,@e,@p,@a,@ac,@cr)", "@n", nm, "@e", em, "@p", ph, "@a", ad, "@ac", ac, "@cr", cr)
        Else
            Exec("UPDATE Customers SET FullName=@n, Email=@e, Phone=@p, Address=@a, AccountNumber=@ac, CardInfo=@cr WHERE Id=@i", "@n", nm, "@e", em, "@p", ph, "@a", ad, "@ac", ac, "@cr", cr, "@i", editId)
        End If
        Info("Customer saved.")
        ClearForm()
        LoadList()
    End Sub
End Class
