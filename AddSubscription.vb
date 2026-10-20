Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms

Public Class AddSubscription
    Inherits Form

    Private cmbCustomer As ComboBox
    Private cmbCategory As ComboBox
    Private cmbPlan As ComboBox
    Private cmbStatus As ComboBox
    Private txtPrice As TextBox
    Private dtpRenewal As DateTimePicker
    Private ReadOnly subId As Integer
    Private ready As Boolean = False

    Public Sub New(Optional id As Integer = 0)
        subId = id
        StyleForm(Me, If(id = 0, "AddSubscription", "Edit Subscription"), 1000, 580)
        PageHeader(Me, If(id = 0, "Add Subscription", "Edit Subscription"), "Add or update subscription settings", &H2295, Color.Black, Color.Yellow, 30, 15)

        Dim left As New DashedPanel()
        left.Bounds = New Rectangle(30, 110, 460, 370)
        Controls.Add(left)
        left.Controls.Add(Lbl("Customer", 45, 52, 11))
        cmbCustomer = Combo(195, 48, 220)
        left.Controls.Add(cmbCustomer)
        left.Controls.Add(Lbl("Category", 45, 128, 11))
        cmbCategory = Combo(195, 124, 220)
        left.Controls.Add(cmbCategory)
        left.Controls.Add(Lbl("Subscription Plan", 30, 204, 11))
        cmbPlan = Combo(195, 200, 220)
        left.Controls.Add(cmbPlan)
        left.Controls.Add(Lbl("Status", 45, 280, 11))
        cmbStatus = Combo(195, 276, 220)
        cmbStatus.DataSource = New String() {"Active", "Inactive"}
        left.Controls.Add(cmbStatus)

        Dim right As New DashedPanel()
        right.Bounds = New Rectangle(520, 110, 450, 210)
        Controls.Add(right)
        right.Controls.Add(Lbl("Monthly Price (" & Peso & ")", 30, 52, 11))
        txtPrice = Txt(200, 48, 215)
        right.Controls.Add(txtPrice)
        right.Controls.Add(Lbl("Renewal Date", 30, 122, 11))
        dtpRenewal = DatePick(200, 118, 215)
        right.Controls.Add(dtpRenewal)

        Dim btnSave As Button = Btn("Save", 30, 495, 110, 42, Color.FromArgb(0, 100, 0), Color.White, 11)
        Dim btnCancel As Button = Btn("Cancel", 160, 495, 120, 42, DarkRed, Color.White, 11)
        Dim btnBack As Button = Btn(ChrW(&H2190) & " BACK", 860, 495, 110, 38, PurpleBtn, Color.White, 10)
        Controls.Add(btnSave)
        Controls.Add(btnCancel)
        Controls.Add(btnBack)
        AddHandler btnSave.Click, AddressOf SaveClick
        AddHandler btnCancel.Click, Sub(s, e) LoadValues()
        AddHandler btnBack.Click, Sub(s, e) Close()
    End Sub

    Private Sub AddSubscription_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cmbCustomer.DisplayMember = "FullName"
        cmbCustomer.ValueMember = "Id"

        If CurrentRole = "Client" Then
            cmbCustomer.DataSource = Query("SELECT Id, FullName FROM Customers WHERE Id=@cid", "@cid", CurrentCustomerId)
            cmbCustomer.Enabled = False
        Else
            cmbCustomer.DataSource = Query("SELECT Id, FullName FROM Customers ORDER BY FullName")
        End If

        cmbCategory.DisplayMember = "Name"
        cmbCategory.ValueMember = "Id"
        cmbCategory.DataSource = Query("SELECT Id, Name FROM Categories ORDER BY Id")
        LoadValues()
        AddHandler cmbCategory.SelectedIndexChanged, AddressOf CategoryChanged
        AddHandler cmbPlan.SelectedIndexChanged, AddressOf PlanChanged
        ready = True
    End Sub

    Private Sub LoadPlans(catId As Integer)
        cmbPlan.DisplayMember = "Name"
        cmbPlan.ValueMember = "Id"
        cmbPlan.DataSource = Query("SELECT Id, Name, MonthlyPrice FROM Plans WHERE CategoryId=@c ORDER BY Name", "@c", catId)
    End Sub

    Private Sub FillPrice()
        Dim drv As DataRowView = TryCast(cmbPlan.SelectedItem, DataRowView)
        If drv IsNot Nothing Then
            txtPrice.Text = Dec(drv("MonthlyPrice")).ToString("0.00", CultureInfo.InvariantCulture)
        End If
    End Sub

    Private Sub CategoryChanged(sender As Object, e As EventArgs)
        If Not ready Then Return
        LoadPlans(IdOf(cmbCategory))
        FillPrice()
    End Sub

    Private Sub PlanChanged(sender As Object, e As EventArgs)
        If Not ready Then Return
        FillPrice()
    End Sub

    Private Sub LoadValues()
        Dim was As Boolean = ready
        ready = False
        If subId = 0 Then
            If cmbCategory.Items.Count > 0 Then cmbCategory.SelectedIndex = 0
            LoadPlans(IdOf(cmbCategory))
            FillPrice()
            If CurrentRole <> "Client" AndAlso cmbCustomer.Items.Count > 0 Then cmbCustomer.SelectedIndex = 0
            dtpRenewal.Value = Date.Today
            cmbStatus.SelectedItem = "Active"
        Else
            Dim dt As DataTable = Query("SELECT * FROM Subscriptions WHERE Id=@i", "@i", subId)
            If dt.Rows.Count = 1 Then
                Dim r As DataRow = dt.Rows(0)
                SelectById(cmbCustomer, ToInt(r("CustomerId")))
                SelectById(cmbCategory, ToInt(r("CategoryId")))
                LoadPlans(ToInt(r("CategoryId")))
                SelectById(cmbPlan, ToInt(r("PlanId")))
                txtPrice.Text = Dec(r("MonthlyPrice")).ToString("0.00", CultureInfo.InvariantCulture)
                dtpRenewal.Value = ParseDate(r("RenewalDate"))
                cmbStatus.SelectedItem = Sv(r("Status"))
            End If
        End If
        ready = was
    End Sub

    Private Sub SaveClick(sender As Object, e As EventArgs)
        Dim cust As Integer = IdOf(cmbCustomer)
        Dim cat As Integer = IdOf(cmbCategory)
        Dim plan As Integer = IdOf(cmbPlan)
        Dim status As String = CStr(cmbStatus.SelectedItem)
        If cust = 0 Then
            Warn("Please select a customer.")
            Return
        End If
        If cat = 0 OrElse plan = 0 Then
            Warn("Please select a category and plan.")
            Return
        End If
        Dim price As Decimal
        If Not Decimal.TryParse(txtPrice.Text.Replace(Peso, "").Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, price) OrElse price <= 0 Then
            Warn("Please enter a valid monthly price.")
            txtPrice.Focus()
            Return
        End If

        If subId = 0 Then
            Exec("INSERT INTO Subscriptions(CustomerId,CategoryId,PlanId,MonthlyPrice,RenewalDate,Status) VALUES(@c,@k,@p,@m,@d,@st)",
                 "@c", cust, "@k", cat, "@p", plan, "@m", CDbl(price), "@d", Iso(dtpRenewal.Value.Date), "@st", status)
            Info("Subscription saved.")
            LoadValues()
        Else
            Exec("UPDATE Subscriptions SET CustomerId=@c, CategoryId=@k, PlanId=@p, MonthlyPrice=@m, RenewalDate=@d, Status=@st WHERE Id=@i",
                 "@c", cust, "@k", cat, "@p", plan, "@m", CDbl(price), "@d", Iso(dtpRenewal.Value.Date), "@st", status, "@i", subId)
            Info("Subscription updated.")
            Close()
        End If
    End Sub
End Class