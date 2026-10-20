Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms

Public Class PaymentHistoryClientForm
    Inherits Form
    Private flow As FlowLayoutPanel

    Public Sub New()
        StyleForm(Me, "My Payment History", 900, 520)
        PageHeader(Me, "PAYMENT HISTORY", "Your recorded subscription payments", &H1F4B3, PurpleBtn, Color.White, 30, 10)

        Dim right As New DashedPanel()
        right.Bounds = New Rectangle(30, 100, 830, 340)
        Controls.Add(right)

        flow = New FlowLayoutPanel()
        flow.Bounds = New Rectangle(15, 20, 800, 300)
        flow.AutoScroll = True
        flow.FlowDirection = FlowDirection.TopDown
        flow.WrapContents = False
        right.Controls.Add(flow)

        Dim btnBack As Button = Btn(ChrW(&H2190) & " BACK", 750, 455, 110, 36, PurpleBtn, Color.White, 9)
        Controls.Add(btnBack)
        AddHandler btnBack.Click, Sub(s, e) Close()

        LoadHistory()
    End Sub

    Private Sub LoadHistory()
        flow.SuspendLayout()
        flow.Controls.Clear()
        Dim dt As DataTable = Query("SELECT pm.PaymentDate, p.Name AS Plan, pm.Amount, pm.Method " &
                                    "FROM Payments pm JOIN Subscriptions s ON s.Id=pm.SubscriptionId JOIN Plans p ON p.Id=s.PlanId " &
                                    "WHERE s.CustomerId=@cid ORDER BY pm.PaymentDate DESC", "@cid", CurrentCustomerId)
        For Each r As DataRow In dt.Rows
            Dim row As New DashedPanel()
            row.Size = New Size(775, 48)
            row.Margin = New Padding(0, 0, 0, 6)
            row.Controls.Add(CellLbl(ParseDate(r("PaymentDate")).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), 15, 10, 120, 28, 10))
            row.Controls.Add(CellLbl(Sv(r("Plan")), 150, 10, 280, 28, 10))
            row.Controls.Add(CellLbl(Money(Dec(r("Amount"))), 450, 10, 140, 28, 10))
            row.Controls.Add(CellLbl(Sv(r("Method")), 610, 10, 140, 28, 10))
            flow.Controls.Add(row)
        Next
        flow.ResumeLayout()
    End Sub
End Class