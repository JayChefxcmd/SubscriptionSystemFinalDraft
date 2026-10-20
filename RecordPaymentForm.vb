Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms

Public Class RecordPaymentForm
    Inherits Form

    Private cmbSub As ComboBox
    Private cmbMethod As ComboBox
    Private txtAmount As TextBox
    Private dtpDate As DateTimePicker
    Private flow As FlowLayoutPanel
    Private ready As Boolean = False
    Private ReadOnly methods As String() = {"GCash", "Maya", "Credit Card", "Debit Card", "PayPal", "Bank Transfer", "Cash"}

    Public Sub New()
        StyleForm(Me, "Record Payment (Admin)", 1100, 610)
        PageHeader(Me, "RECORD PAYMENT", "Record subscription payment transactions", &H1F4B3, PurpleBtn, Color.White, 30, 10)

        Dim left As New DashedPanel()
        left.Bounds = New Rectangle(30, 100, 370, 440)
        Controls.Add(left)
        left.Controls.Add(Lbl("NEW PAYMENT", 35, 20, 10, True))
        left.Controls.Add(Lbl("Subscription", 35, 55, 10))
        cmbSub = Combo(33, 85, 300)
        left.Controls.Add(cmbSub)
        left.Controls.Add(Lbl("Amount (" & Peso & ")", 35, 130, 10))
        txtAmount = Txt(33, 160, 300)
        left.Controls.Add(txtAmount)
        left.Controls.Add(Lbl("Payment Date", 35, 205, 10))
        dtpDate = DatePick(33, 235, 300)
        left.Controls.Add(dtpDate)
        left.Controls.Add(Lbl("Payment Method", 35, 285, 10))
        cmbMethod = Combo(33, 315, 300)
        cmbMethod.DataSource = methods
        cmbMethod.SelectedIndex = -1
        left.Controls.Add(cmbMethod)

        Dim btnSave As Button = Btn("SAVE PAYMENT", 30, 390, 150, 34, GreenBtn, Color.White, 8.5F)
        Dim btnClear As Button = Btn("CLEAR", 190, 390, 143, 34, Color.Red, Color.White, 8.5F)
        left.Controls.Add(btnSave)
        left.Controls.Add(btnClear)
        AddHandler btnSave.Click, AddressOf SaveClick
        AddHandler btnClear.Click, Sub(s, e) ClearForm()

        Dim right As New DashedPanel()
        right.Bounds = New Rectangle(420, 100, 650, 420)
        Controls.Add(right)
        right.Controls.Add(Lbl("RECENT PAYMENTS", 25, 20, 10, True))

        flow = New FlowLayoutPanel()
        flow.Bounds = New Rectangle(15, 60, 620, 348)
        flow.AutoScroll = True
        flow.FlowDirection = FlowDirection.TopDown
        flow.WrapContents = False
        right.Controls.Add(flow)

        Dim btnBack As Button = Btn(ChrW(&H2190) & " BACK", 965, 545, 105, 36, PurpleBtn, Color.White, 9)
        Controls.Add(btnBack)
        AddHandler btnBack.Click, Sub(s, e) Close()
    End Sub

    Private Sub RecordPaymentForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cmbSub.DisplayMember = "Label"
        cmbSub.ValueMember = "Id"
        cmbSub.DataSource = Query("SELECT s.Id, p.Name || ' - ' || c.FullName || ' [Acc: ' || COALESCE(c.AccountNumber,'-') || ']' AS Label, s.MonthlyPrice " &
                                  "FROM Subscriptions s JOIN Plans p ON p.Id=s.PlanId JOIN Customers c ON c.Id=s.CustomerId " &
                                  "WHERE s.Status='Active' ORDER BY p.Name")
        cmbSub.SelectedIndex = -1
        LoadRecent()
        AddHandler cmbSub.SelectedIndexChanged, AddressOf SubChanged
        ready = True
    End Sub

    Private Sub SubChanged(sender As Object, e As EventArgs)
        If Not ready Then Return
        Dim drv As DataRowView = TryCast(cmbSub.SelectedItem, DataRowView)
        If drv IsNot Nothing Then txtAmount.Text = Dec(drv("MonthlyPrice")).ToString("0.00", CultureInfo.InvariantCulture)
    End Sub

    Private Sub ClearForm()
        cmbSub.SelectedIndex = -1
        txtAmount.Clear()
        dtpDate.Value = Date.Today
        cmbMethod.SelectedIndex = -1
    End Sub

    Private Sub LoadRecent()
        flow.SuspendLayout()
        flow.Controls.Clear()
        Dim dt As DataTable = Query("SELECT pm.PaymentDate, p.Name AS Plan, c.FullName AS Customer, pm.Amount, pm.Method " &
                                    "FROM Payments pm JOIN Subscriptions s ON s.Id=pm.SubscriptionId JOIN Plans p ON p.Id=s.PlanId JOIN Customers c ON c.Id=s.CustomerId " &
                                    "ORDER BY pm.PaymentDate DESC, pm.Id DESC LIMIT 50")
        For Each r As DataRow In dt.Rows
            Dim row As New DashedPanel()
            row.Size = New Size(598, 50)
            row.Margin = New Padding(0, 0, 0, 6)
            row.Controls.Add(CellLbl(ParseDate(r("PaymentDate")).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), 10, 11, 90, 28, 9))
            row.Controls.Add(CellLbl(Sv(r("Plan")), 105, 11, 140, 28, 9))
            row.Controls.Add(CellLbl(Sv(r("Customer")), 250, 11, 120, 28, 9))
            row.Controls.Add(CellLbl(Money(Dec(r("Amount"))), 375, 11, 90, 28, 9))
            row.Controls.Add(CellLbl(Sv(r("Method")), 470, 11, 120, 28, 9))
            flow.Controls.Add(row)
        Next
        flow.ResumeLayout()
    End Sub

    Private Sub SaveClick(sender As Object, e As EventArgs)
        Dim sid As Integer = IdOf(cmbSub)
        If sid = 0 Then
            Warn("Please select a subscription.")
            Return
        End If
        Dim amt As Decimal
        If Not Decimal.TryParse(txtAmount.Text.Replace(Peso, "").Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, amt) OrElse amt <= 0 Then
            Warn("Please enter a valid amount.")
            txtAmount.Focus()
            Return
        End If
        If cmbMethod.SelectedIndex < 0 Then
            Warn("Please choose a payment method.")
            Return
        End If

        Dim payDate As Date = dtpDate.Value.Date
        Exec("INSERT INTO Payments(SubscriptionId,Amount,PaymentDate,Method) VALUES(@s,@a,@d,@m)",
             "@s", sid, "@a", CDbl(amt), "@d", Iso(payDate), "@m", CStr(cmbMethod.SelectedItem))

        Dim cur As DataTable = Query("SELECT RenewalDate FROM Subscriptions WHERE Id=@i", "@i", sid)
        If cur.Rows.Count = 1 Then
            Dim rd As Date = ParseDate(cur.Rows(0)(0))
            If payDate >= rd.AddDays(-7) Then
                Dim nr As Date = rd
                Do
                    nr = nr.AddMonths(1)
                Loop While nr <= payDate
                Exec("UPDATE Subscriptions SET RenewalDate=@d WHERE Id=@i", "@d", Iso(nr), "@i", sid)
            End If
        End If

        Info("Payment recorded successfully.")
        ClearForm()
        LoadRecent()
    End Sub
End Class