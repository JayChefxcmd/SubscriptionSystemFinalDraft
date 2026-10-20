Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms

Public Class ReportsForm
    Inherits Form

    Public Sub New()
        StyleForm(Me, "Reports", 720, 560)
        PageHeader(Me, "REPORTS", "Spending summary", &H1F4CA, Color.FromArgb(0, 100, 0), Color.White, 30, 10)

        Dim lv As New ListView()
        lv.Bounds = New Rectangle(30, 100, 660, 220)
        lv.View = View.Details
        lv.FullRowSelect = True
        lv.GridLines = True
        lv.BackColor = Color.FromArgb(30, 27, 45)
        lv.ForeColor = Color.White
        lv.Font = New Font("Segoe UI", 10.0F)
        lv.Columns.Add("Category", 250)
        lv.Columns.Add("Active subscriptions", 190, HorizontalAlignment.Center)
        lv.Columns.Add("Monthly cost", 190, HorizontalAlignment.Right)
        Controls.Add(lv)

        Dim dt As DataTable = Query("SELECT cat.Name AS Name, COUNT(*) AS N, SUM(s.MonthlyPrice) AS T FROM Subscriptions s " &
                                    "JOIN Categories cat ON cat.Id=s.CategoryId WHERE s.Status='Active' GROUP BY cat.Name ORDER BY T DESC")
        For Each r As DataRow In dt.Rows
            Dim it As New ListViewItem(Sv(r("Name")))
            it.SubItems.Add(Sv(r("N")))
            it.SubItems.Add(Money(Dec(r("T"))))
            lv.Items.Add(it)
        Next

        Dim monthKey As String = Date.Today.ToString("yyyy-MM", CultureInfo.InvariantCulture)
        Dim totalMonthly As Decimal = Dec(Scalar("SELECT COALESCE(SUM(MonthlyPrice),0) FROM Subscriptions WHERE Status='Active'"))
        Dim paidMonth As Decimal = Dec(Scalar("SELECT COALESCE(SUM(Amount),0) FROM Payments WHERE substr(PaymentDate,1,7)=@m", "@m", monthKey))
        Dim paidAll As Decimal = Dec(Scalar("SELECT COALESCE(SUM(Amount),0) FROM Payments"))
        Dim custs As Integer = ToInt(Scalar("SELECT COUNT(*) FROM Customers"))

        Controls.Add(Lbl("Total monthly cost (active):", 40, 345, 11))
        Controls.Add(Lbl(Money(totalMonthly), 400, 345, 11, True, Color.Gold))
        Controls.Add(Lbl("Payments recorded this month:", 40, 380, 11))
        Controls.Add(Lbl(Money(paidMonth), 400, 380, 11, True, Color.Gold))
        Controls.Add(Lbl("All-time payments recorded:", 40, 415, 11))
        Controls.Add(Lbl(Money(paidAll), 400, 415, 11, True, Color.Gold))
        Controls.Add(Lbl("Customers:", 40, 450, 11))
        Controls.Add(Lbl(custs.ToString(), 400, 450, 11, True, Color.Gold))

        Dim btnClose As Button = Btn("Close", 580, 495, 110, 38, Color.Black, Color.White, 10)
        Controls.Add(btnClose)
        AddHandler btnClose.Click, Sub(s, e) Close()
    End Sub
End Class

Public Class SettingsForm
    Inherits Form

    Private txtOld As TextBox
    Private txtNew As TextBox
    Private txtConfirm As TextBox

    Public Sub New()
        StyleForm(Me, "Settings", 560, 440)
        PageHeader(Me, "SETTINGS", "Change your password", &H2699, Color.FromArgb(75, 0, 130), Color.White, 30, 10)

        Controls.Add(Lbl("Signed in as:  " & CurrentUser, 40, 100, 10, False, Color.Silver))
        Controls.Add(Lbl("Current password", 40, 140, 10))
        txtOld = Txt(230, 136, 260)
        txtOld.UseSystemPasswordChar = True
        Controls.Add(txtOld)
        Controls.Add(Lbl("New password", 40, 190, 10))
        txtNew = Txt(230, 186, 260)
        txtNew.UseSystemPasswordChar = True
        Controls.Add(txtNew)
        Controls.Add(Lbl("Confirm new password", 40, 240, 10))
        txtConfirm = Txt(230, 236, 260)
        txtConfirm.UseSystemPasswordChar = True
        Controls.Add(txtConfirm)

        Dim btnSave As Button = Btn("Save", 230, 295, 120, 40, GreenBtn, Color.White, 10)
        Dim btnClose As Button = Btn("Close", 370, 295, 120, 40, Color.Black, Color.White, 10)
        Controls.Add(btnSave)
        Controls.Add(btnClose)
        AddHandler btnSave.Click, AddressOf SaveClick
        AddHandler btnClose.Click, Sub(s, e) Close()

        Dim dbl As Label = Lbl("Database: " & DbFile, 40, 385, 8, False, Color.Gray)
        Controls.Add(dbl)
    End Sub

    Private Sub SaveClick(sender As Object, e As EventArgs)
        If txtNew.Text.Length < 6 Then
            Warn("The new password must be at least 6 characters.")
            Return
        End If
        If txtNew.Text <> txtConfirm.Text Then
            Warn("The new passwords do not match.")
            Return
        End If
        If ChangePassword(txtOld.Text, txtNew.Text) Then
            Info("Password changed.")
            Close()
        Else
            Warn("Your current password is incorrect.")
        End If
    End Sub
End Class
