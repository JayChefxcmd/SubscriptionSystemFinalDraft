Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms

Public Class MainForm
    Inherits Form

    Public LoggedOut As Boolean = False

    Private lblMonthly As Label
    Private lblActive As Label
    Private lblDueSoon As Label
    Private lblOverdue As Label
    Private lblUser As Label
    Private donut As DonutPanel
    Private pnlLegend As Panel
    Private pnlNotif As Panel
    Private pnlUpcoming As Panel
    Private flowSubs As FlowLayoutPanel
    Private selectedId As Integer = 0
    Private ReadOnly rowPanels As New Dictionary(Of Integer, Panel)
    Private ReadOnly Palette As Color() = {
        Color.FromArgb(0, 128, 0), Color.FromArgb(255, 69, 0), Color.FromArgb(139, 0, 0),
        Color.FromArgb(128, 0, 128), Color.FromArgb(255, 193, 7), Color.FromArgb(0, 150, 150)}

    Public Sub New()
        StyleForm(Me, "The Ledger - Subscription Tracker", 1200, 670)
        BuildUi()
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RefreshAll()
    End Sub

    Private Sub BuildUi()
        Controls.Add(Lbl("THE LEDGER", 240, 8, 17, True))
        Controls.Add(Lbl("Subscription Tracker", 242, 42, 10, False, Color.Silver))
        lblUser = Lbl(CurrentUser & " (" & CurrentRole & ")", 1000, 24, 9, False, Color.Silver)
        Controls.Add(lblUser)
        lblUser.Left = 1180 - lblUser.Width

        ' ----- dynamic sidebar -----
        Dim side As New DashedPanel()
        side.Bounds = New Rectangle(10, 70, 215, 570)
        Controls.Add(side)

        Dim names As List(Of String)
        If CurrentRole = "Administrator" Then
            names = New List(Of String)({"DASHBOARD", "CUSTOMERS", "SUBSCRIPTIONS", "PAYMENTS", "CALENDAR", "REPORTS", "SETTINGS", "LOGOUT"})
        Else
            names = New List(Of String)({"DASHBOARD", "SUBSCRIPTIONS", "PAYMENTS", "CALENDAR", "REPORTS", "SETTINGS", "LOGOUT"})
        End If

        For i As Integer = 0 To names.Count - 1
            Dim b As Button = Btn(names(i), 4, 4 + i * 70, 207, 66, BgDark, Color.White, 9)
            b.Tag = names(i)
            AddHandler b.Click, AddressOf NavClick
            side.Controls.Add(b)
        Next

        lblMonthly = StatCard(240, "MONTHLY SPENDINGS", &H1F45B, Money(0))
        lblActive = StatCard(475, "ACTIVE SUBSCRIPTIONS", &H1F501, "0")
        lblDueSoon = StatCard(710, "DUE SOON", &H1F4C5, "0")
        lblOverdue = StatCard(945, "OVERDUE", &H26A0, "0")

        Dim ov As New DashedPanel()
        ov.Bounds = New Rectangle(240, 150, 280, 255)
        Controls.Add(ov)
        ov.Controls.Add(Lbl("MONTHLY OVERVIEW", 10, 8, 10, True))
        donut = New DonutPanel()
        donut.Bounds = New Rectangle(10, 40, 130, 200)
        ov.Controls.Add(donut)
        pnlLegend = New Panel()
        pnlLegend.Bounds = New Rectangle(152, 44, 124, 200)
        ov.Controls.Add(pnlLegend)

        Dim nf As New DashedPanel()
        nf.Bounds = New Rectangle(525, 150, 330, 255)
        Controls.Add(nf)
        nf.Controls.Add(Lbl("NOTIFICATIONS", 10, 8, 10, True))
        Dim btnAll As Button = Btn("View All", 238, 6, 82, 26, NavyBtn, Color.White, 8.5F)
        nf.Controls.Add(btnAll)
        AddHandler btnAll.Click, Sub(s, e) ShowDlg(New CalendarForm())
        pnlNotif = New Panel()
        pnlNotif.Bounds = New Rectangle(6, 40, 318, 210)
        nf.Controls.Add(pnlNotif)

        Dim up As New DashedPanel()
        up.Bounds = New Rectangle(860, 150, 310, 255)
        Controls.Add(up)
        up.Controls.Add(Lbl("UPCOMING RENEWALS", 10, 8, 10, True))
        Dim btnCal As Button = Btn("View Calendar", 190, 6, 112, 26, NavyBtn, Color.White, 8.5F)
        up.Controls.Add(btnCal)
        AddHandler btnCal.Click, Sub(s, e) ShowDlg(New CalendarForm())
        pnlUpcoming = New Panel()
        pnlUpcoming.Bounds = New Rectangle(6, 40, 298, 210)
        up.Controls.Add(pnlUpcoming)

        Controls.Add(Lbl("ACTIVE SUBSCRIPTIONS", 240, 414, 10, True))
        Dim act As New DashedPanel()
        act.Bounds = New Rectangle(240, 440, 930, 190)
        Controls.Add(act)
        HeaderCell(act, "Subscriptions", 8, 78)
        HeaderCell(act, "Category", 100, 90)
        HeaderCell(act, "Price (" & Peso & ")", 195, 90)
        HeaderCell(act, "Renewal Date", 295, 110)
        HeaderCell(act, "Status", 415, 90)
        HeaderCell(act, "Payment Method", 515, 110)
        HeaderCell(act, "Actions", 650, 190)
        flowSubs = New FlowLayoutPanel()
        flowSubs.Bounds = New Rectangle(4, 30, 922, 150)
        flowSubs.AutoScroll = True
        flowSubs.FlowDirection = FlowDirection.TopDown
        flowSubs.WrapContents = False
        act.Controls.Add(flowSubs)
    End Sub

    Private Function StatCard(x As Integer, caption As String, cp As Integer, initial As String) As Label
        Dim p As New DashedPanel()
        p.Bounds = New Rectangle(x, 70, 225, 66)
        p.Controls.Add(GlyphTile(cp, 8, 12, 40, 40, Color.Transparent, Color.White, 18))
        p.Controls.Add(Lbl(caption, 56, 8, 8.5F, True, Color.Silver))
        Dim v As Label = Lbl(initial, 56, 30, 12, True, Color.Black)
        v.BackColor = Color.Gold
        p.Controls.Add(v)
        Controls.Add(p)
        Return v
    End Function

    Private Sub HeaderCell(host As Control, text As String, x As Integer, w As Integer)
        host.Controls.Add(CellLbl(text, x, 4, w, 22, 8.5F, True))
    End Sub

    Private Sub NavClick(sender As Object, e As EventArgs)
        Select Case CStr(DirectCast(sender, Control).Tag)
            Case "DASHBOARD"
                RefreshAll()
            Case "CUSTOMERS"
                If CurrentRole = "Administrator" Then ShowDlg(New Customer_Management())
            Case "SUBSCRIPTIONS"
                ShowDlg(New AddSubscription(0))
            Case "PAYMENTS"
                If CurrentRole = "Administrator" Then
                    ShowDlg(New RecordPaymentForm())
                Else
                    ShowDlg(New PaymentHistoryClientForm())
                End If
            Case "CALENDAR"
                ShowDlg(New CalendarForm())
            Case "REPORTS"
                ShowDlg(New ReportsForm())
            Case "SETTINGS"
                ShowDlg(New SettingsForm())
            Case "LOGOUT"
                If Confirm("Log out of The Ledger?") Then
                    LoggedOut = True
                    Close()
                End If
        End Select
    End Sub

    Private Sub ShowDlg(f As Form)
        Using f
            f.ShowDialog(Me)
        End Using
        RefreshAll()
    End Sub

    Private Sub RefreshAll()
        Dim subs As DataTable = AllSubscriptions()
        Dim total As Decimal = 0
        Dim active As Integer = 0
        Dim soon As Integer = 0
        Dim over As Integer = 0
        Dim items As New List(Of DonutItem)

        For Each r As DataRow In subs.Rows
            If CStr(r("Status")) <> "Active" Then Continue For
            Dim price As Decimal = Dec(r("MonthlyPrice"))
            Dim d As Integer = DaysLeft(r("RenewalDate"))
            total += price
            active += 1
            If d < 0 Then
                over += 1
            ElseIf d <= 3 Then
                soon += 1
            End If
            Dim it As New DonutItem()
            it.Name = Sv(r("Plan"))
            it.Value = price
            it.ItemColor = Palette((items.Count) Mod Palette.Length)
            items.Add(it)
        Next

        lblMonthly.Text = Money(total)
        lblActive.Text = active.ToString()
        lblDueSoon.Text = soon.ToString()
        lblOverdue.Text = over.ToString()

        donut.SetData(items, total)
        BuildLegend(items)
        BuildNotifications(subs)
        BuildUpcoming(subs)
        BuildActiveList(subs)
    End Sub

    Private Sub BuildLegend(items As List(Of DonutItem))
        pnlLegend.Controls.Clear()
        For i As Integer = 0 To Math.Min(3, items.Count - 1)
            Dim sq As New Label()
            sq.AutoSize = False
            sq.Bounds = New Rectangle(0, i * 48 + 4, 20, 20)
            sq.BackColor = items(i).ItemColor
            sq.BorderStyle = BorderStyle.FixedSingle
            pnlLegend.Controls.Add(sq)
            pnlLegend.Controls.Add(Lbl(items(i).Name, 28, i * 48, 8.5F, True))
            pnlLegend.Controls.Add(Lbl(Money(items(i).Value), 28, i * 48 + 20, 8))
        Next
    End Sub

    Private Sub BuildNotifications(subs As DataTable)
        pnlNotif.Controls.Clear()
        Dim n As Integer = 0
        For Each r As DataRow In subs.Rows
            If CStr(r("Status")) <> "Active" Then Continue For
            Dim d As Integer = DaysLeft(r("RenewalDate"))
            If d > 7 Then Continue For
            If n >= 3 Then Exit For

            Dim title As String
            Dim col As Color
            Dim msg As String
            Dim whenTxt As String
            Dim glyph As Integer = &H2611
            If d < 0 Then
                title = "OVERDUE"
                col = Color.White
                glyph = &H26A0
                msg = "Renewal was due " & (-d) & If(d = -1, " day", " days") & " ago"
                whenTxt = (-d) & "d ago"
            ElseIf d = 0 Then
                title = "DUE TODAY"
                col = Color.FromArgb(255, 140, 0)
                msg = "Renewal is today"
                whenTxt = "today"
            ElseIf d = 1 Then
                title = "DUE TOMORROW"
                col = Color.FromArgb(255, 69, 0)
                msg = "Renewal is tomorrow"
                whenTxt = "1d left"
            Else
                title = "DUE SOON"
                col = Color.Yellow
                msg = "Renewal in " & d & " days"
                whenTxt = d & "d left"
            End If

            Dim row As New DashedPanel()
            row.Bounds = New Rectangle(0, n * 70, 316, 66)
            Dim ic As Label = GlyphTile(glyph, 8, 14, 38, 36, Color.Transparent, Color.White, 16)
            ic.Font = New Font("Segoe UI Symbol", 16.0F)
            row.Controls.Add(ic)
            row.Controls.Add(Lbl(title, 56, 4, 9, True, col))
            row.Controls.Add(Lbl(Sv(r("Plan")), 56, 24, 7.5F))
            row.Controls.Add(Lbl(msg, 56, 42, 7.5F, False, Color.Silver))
            Dim tl As Label = Lbl(whenTxt, 250, 14, 8, False)
            tl.Left = 306 - tl.Width
            row.Controls.Add(tl)
            pnlNotif.Controls.Add(row)
            n += 1
        Next
        If n = 0 Then pnlNotif.Controls.Add(Lbl("No renewals due in the next 7 days.", 10, 10, 9, False, Color.Silver))
    End Sub

    Private Sub BuildUpcoming(subs As DataTable)
        pnlUpcoming.Controls.Clear()
        Dim n As Integer = 0
        For Each r As DataRow In subs.Rows
            If CStr(r("Status")) <> "Active" Then Continue For
            Dim d As Integer = DaysLeft(r("RenewalDate"))
            If d < 0 Then Continue For
            If n >= 3 Then Exit For

            Dim dt As Date = ParseDate(r("RenewalDate"))
            Dim row As New DashedPanel()
            row.Bounds = New Rectangle(0, n * 70, 296, 66)

            Dim tile As New Label()
            tile.AutoSize = False
            tile.Bounds = New Rectangle(8, 6, 52, 54)
            tile.BackColor = Color.Gray
            tile.ForeColor = Color.White
            tile.BorderStyle = BorderStyle.FixedSingle
            tile.TextAlign = ContentAlignment.MiddleCenter
            tile.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
            tile.Text = dt.ToString("MMM", CultureInfo.InvariantCulture).ToUpper() & vbLf & dt.Day
            row.Controls.Add(tile)

            Dim nm As Label = Lbl(Sv(r("Plan")), 68, 8, 8.5F, True, Color.Black)
            nm.BackColor = Color.White
            row.Controls.Add(nm)
            row.Controls.Add(Lbl(Sv(r("Category")), 68, 38, 8, False, Color.Silver))
            Dim pr As Label = Lbl(Money(Dec(r("MonthlyPrice"))), 200, 8, 8.5F, True)
            pr.Left = 286 - pr.Width
            row.Controls.Add(pr)
            Dim tx As Label = Lbl(If(d = 0, "Today", If(d = 1, "Tomorrow", "in " & d & " days")), 220, 38, 8.5F, False)
            tx.Left = 286 - tx.Width
            row.Controls.Add(tx)
            pnlUpcoming.Controls.Add(row)
            n += 1
        Next
        If n = 0 Then pnlUpcoming.Controls.Add(Lbl("No upcoming renewals.", 10, 10, 9, False, Color.Silver))
    End Sub

    Private Sub BuildActiveList(subs As DataTable)
        flowSubs.SuspendLayout()
        flowSubs.Controls.Clear()
        rowPanels.Clear()

        For Each r As DataRow In subs.Rows
            Dim id As Integer = ToInt(r("Id"))
            Dim isActive As Boolean = (CStr(r("Status")) = "Active")
            Dim plan As String = Sv(r("Plan"))

            Dim p As New DashedPanel()
            p.Size = New Size(898, 52)
            p.Margin = New Padding(0, 0, 0, 4)
            p.Tag = id
            AddHandler p.Click, AddressOf RowClick

            Dim logo As New Label()
            logo.AutoSize = False
            logo.Bounds = New Rectangle(8, 6, 78, 40)
            logo.BackColor = Color.Black
            logo.ForeColor = Palette(id Mod Palette.Length)
            logo.Font = New Font("Segoe UI", 8.5F, FontStyle.Bold)
            logo.TextAlign = ContentAlignment.MiddleCenter
            logo.Text = plan.Split(" "c)(0).ToUpper()
            logo.Tag = id
            AddHandler logo.Click, AddressOf RowClick
            p.Controls.Add(logo)

            AddCell(p, Sv(r("Category")), 100, 90, id)
            AddCell(p, Money(Dec(r("MonthlyPrice"))), 195, 90, id)
            AddCell(p, ParseDate(r("RenewalDate")).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), 295, 110, id)

            Dim pill As Label = CellLbl(CStr(r("Status")), 420, 12, 84, 28, 8.5F, True)
            pill.BackColor = If(isActive, Color.FromArgb(0, 128, 0), Color.FromArgb(139, 0, 0))
            pill.BorderStyle = BorderStyle.FixedSingle
            pill.Tag = id
            AddHandler pill.Click, AddressOf RowClick
            p.Controls.Add(pill)

            AddCell(p, Sv(r("Method")), 515, 110, id)

            Dim bv As Button = Btn("View", 660, 10, 58, 32, NavyBtn, Color.White, 8.5F)
            Dim be As Button = Btn("Edit", 725, 10, 58, 32, NavyBtn, Color.White, 8.5F)
            bv.Tag = id
            be.Tag = id
            AddHandler bv.Click, AddressOf ViewClick
            AddHandler be.Click, AddressOf EditClick
            p.Controls.Add(bv)
            p.Controls.Add(be)

            If CurrentRole = "Administrator" Then
                Dim bx As Button = Btn("X", 790, 10, 58, 32, DarkRed, Color.White, 8.5F)
                bx.Tag = id
                AddHandler bx.Click, AddressOf DeleteClick
                p.Controls.Add(bx)
            End If

            flowSubs.Controls.Add(p)
            rowPanels(id) = p
        Next
        flowSubs.ResumeLayout()

        If Not rowPanels.ContainsKey(selectedId) Then selectedId = 0
        HighlightSelected()
    End Sub

    Private Sub AddCell(host As Control, text As String, x As Integer, w As Integer, id As Integer)
        Dim l As Label = CellLbl(text, x, 12, w, 28, 9)
        l.Tag = id
        AddHandler l.Click, AddressOf RowClick
        host.Controls.Add(l)
    End Sub

    Private Sub RowClick(sender As Object, e As EventArgs)
        selectedId = CInt(DirectCast(sender, Control).Tag)
        HighlightSelected()
    End Sub

    Private Sub HighlightSelected()
        For Each kv As KeyValuePair(Of Integer, Panel) In rowPanels
            kv.Value.BackColor = If(kv.Key = selectedId, Color.FromArgb(48, 40, 90), BgDark)
        Next
    End Sub

    Private Sub ViewClick(sender As Object, e As EventArgs)
        Dim id As Integer = CInt(DirectCast(sender, Control).Tag)
        Dim dt As DataTable = Query(SubSql & "WHERE s.Id=@i", "@i", id)
        If dt.Rows.Count = 0 Then Return
        Dim r As DataRow = dt.Rows(0)
        Dim d As Integer = DaysLeft(r("RenewalDate"))
        MessageBox.Show(
            "Plan:  " & Sv(r("Plan")) & vbCrLf &
            "Customer:  " & Sv(r("Customer")) & vbCrLf &
            "Category:  " & Sv(r("Category")) & vbCrLf &
            "Monthly price:  " & Money(Dec(r("MonthlyPrice"))) & vbCrLf &
            "Renewal date:  " & ParseDate(r("RenewalDate")).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) & "  (" & StateOf(d).Text & ")" & vbCrLf &
            "Status:  " & Sv(r("Status")) & vbCrLf &
            "Last payment method:  " & Sv(r("Method")),
            "Subscription Details", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub EditClick(sender As Object, e As EventArgs)
        ShowDlg(New AddSubscription(CInt(DirectCast(sender, Control).Tag)))
    End Sub

    Private Sub DeleteClick(sender As Object, e As EventArgs)
        Dim id As Integer = CInt(DirectCast(sender, Control).Tag)
        Dim nm As String = Sv(Scalar("SELECT p.Name FROM Subscriptions s JOIN Plans p ON p.Id=s.PlanId WHERE s.Id=@i", "@i", id))
        If Confirm("Delete the '" & nm & "' subscription and its payment records?") Then
            Exec("DELETE FROM Subscriptions WHERE Id=@i", "@i", id)
            RefreshAll()
        End If
    End Sub
End Class
