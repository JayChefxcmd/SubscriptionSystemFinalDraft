Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms

Public Class CalendarForm
    Inherits Form

    Private cal As MonthCalendar
    Private flow As FlowLayoutPanel

    Public Sub New()
        StyleForm(Me, "Calendar", 960, 590)
        PageHeader(Me, "Calendar", "Track your upcoming subscription renewals", &H1F4C5, Color.FromArgb(120, 80, 220), Color.White, 30, 10)

        Dim left As New DashedPanel()
        left.Bounds = New Rectangle(30, 100, 300, 260)
        Controls.Add(left)
        cal = New MonthCalendar()
        cal.Location = New Point(36, 40)
        cal.MaxSelectionCount = 1
        left.Controls.Add(cal)

        Dim legend As New DashedPanel()
        legend.Bounds = New Rectangle(30, 372, 300, 110)
        Controls.Add(legend)
        AddLegend(legend, &H26A0, Color.FromArgb(139, 0, 0), "Overdue", 12, 10)
        AddLegend(legend, &H23F0, Color.FromArgb(255, 140, 0), "Due Today", 160, 10)
        AddLegend(legend, &H23F1, Color.Yellow, "Due Soon", 12, 58)
        AddLegend(legend, &H231B, Color.FromArgb(0, 100, 0), "Upcoming", 160, 58)

        Dim right As New DashedPanel()
        right.Bounds = New Rectangle(350, 100, 580, 382)
        Controls.Add(right)
        right.Controls.Add(Lbl("RENEWALS LIST", 25, 20, 14, True))

        flow = New FlowLayoutPanel()
        flow.Bounds = New Rectangle(14, 65, 552, 306)
        flow.AutoScroll = True
        flow.FlowDirection = FlowDirection.TopDown
        flow.WrapContents = False
        right.Controls.Add(flow)

        Dim btnClose As Button = Btn("Close", 820, 500, 110, 38, Color.Black, Color.White, 10)
        Controls.Add(btnClose)
        AddHandler btnClose.Click, Sub(s, e) Close()
    End Sub

    Private Sub AddLegend(host As Control, cp As Integer, back As Color, text As String, x As Integer, y As Integer)
        Dim tile As Label = GlyphTile(cp, x, y, 38, 38, back, If(back = Color.Yellow, Color.Black, Color.White), 12)
        tile.Font = New Font("Segoe UI Symbol", 12.0F)
        tile.BorderStyle = BorderStyle.FixedSingle
        host.Controls.Add(tile)
        host.Controls.Add(Lbl(text, x + 46, y + 8, 10))
    End Sub

    Private Sub CalendarForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim dt As DataTable = Query(SubSql & "WHERE s.Status='Active' ORDER BY s.RenewalDate, s.Id")

        Dim bold As New List(Of Date)
        flow.SuspendLayout()
        For Each r As DataRow In dt.Rows
            Dim d As Date = ParseDate(r("RenewalDate"))
            If Not bold.Contains(d) Then bold.Add(d)

            Dim st = StateOf(DaysLeft(r("RenewalDate")))
            Dim row As New DashedPanel()
            row.Size = New Size(530, 50)
            row.Margin = New Padding(0, 0, 0, 8)
            row.Controls.Add(CellLbl(d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), 10, 11, 100, 28, 10))
            row.Controls.Add(CellLbl(Sv(r("Plan")), 120, 11, 170, 28, 10))
            row.Controls.Add(CellLbl(Money(Dec(r("MonthlyPrice"))), 295, 11, 100, 28, 10))
            Dim pill As Label = CellLbl(st.Text, 405, 9, 115, 30, 8.5F, True)
            pill.BackColor = st.Back
            pill.ForeColor = st.Fore
            pill.BorderStyle = BorderStyle.FixedSingle
            row.Controls.Add(pill)
            flow.Controls.Add(row)
        Next
        flow.ResumeLayout()

        cal.BoldedDates = bold.ToArray()
        cal.UpdateBoldedDates()
    End Sub
End Class
