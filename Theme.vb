Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports System.Data

''' <summary>Colours, fonts and small control factories shared by every form.</summary>
Public Module Theme

    Public ReadOnly BgDark As Color = Color.FromArgb(20, 17, 30)
    Public ReadOnly BorderCol As Color = Color.FromArgb(72, 61, 139)
    Public ReadOnly PurpleBtn As Color = Color.FromArgb(75, 0, 130)
    Public ReadOnly NavyBtn As Color = Color.FromArgb(0, 0, 100)
    Public ReadOnly DarkRed As Color = Color.FromArgb(139, 0, 0)
    Public ReadOnly GreenBtn As Color = Color.FromArgb(0, 128, 0)
    Public ReadOnly Peso As String = ChrW(&H20B1)

    Public Function Money(v As Decimal) As String
        Return Peso & v.ToString("N2", CultureInfo.InvariantCulture)
    End Function

    ' Renewal state used by dashboard + calendar
    Public Function StateOf(days As Integer) As (Text As String, Back As Color, Fore As Color)
        If days < 0 Then Return ("Overdue", Color.FromArgb(139, 0, 0), Color.White)
        If days = 0 Then Return ("Due Today", Color.FromArgb(255, 140, 0), Color.White)
        If days = 1 Then Return ("Due Tomorrow", Color.FromArgb(255, 140, 0), Color.White)
        If days <= 3 Then Return ("Due in " & days & " days", Color.Yellow, Color.Black)
        Return ("in " & days & " days", Color.FromArgb(0, 100, 0), Color.White)
    End Function

    Public Sub StyleForm(f As Form, title As String, w As Integer, h As Integer)
        f.Text = title
        f.AutoScaleMode = AutoScaleMode.None
        f.ClientSize = New Size(w, h)
        f.BackColor = BgDark
        f.ForeColor = Color.White
        f.Font = New Font("Segoe UI", 9.0F)
        f.FormBorderStyle = FormBorderStyle.FixedDialog
        f.MaximizeBox = False
        f.MinimizeBox = False
        f.StartPosition = FormStartPosition.CenterScreen
    End Sub

    Public Function Lbl(text As String, x As Integer, y As Integer, Optional size As Single = 9.5F, Optional bold As Boolean = False, Optional fore As Color = Nothing) As Label
        Dim l As New Label()
        l.Text = text
        l.AutoSize = True
        l.Location = New Point(x, y)
        l.ForeColor = If(fore.IsEmpty, Color.White, fore)
        l.BackColor = Color.Transparent
        l.Font = New Font("Segoe UI", size, If(bold, FontStyle.Bold, FontStyle.Regular))
        Return l
    End Function

    Public Function CellLbl(text As String, x As Integer, y As Integer, w As Integer, h As Integer, Optional size As Single = 9.0F, Optional bold As Boolean = False, Optional align As ContentAlignment = ContentAlignment.MiddleCenter) As Label
        Dim l As New Label()
        l.Text = text
        l.AutoSize = False
        l.Bounds = New Rectangle(x, y, w, h)
        l.ForeColor = Color.White
        l.BackColor = Color.Transparent
        l.TextAlign = align
        l.Font = New Font("Segoe UI", size, If(bold, FontStyle.Bold, FontStyle.Regular))
        Return l
    End Function

    Public Function GlyphTile(cp As Integer, x As Integer, y As Integer, w As Integer, h As Integer, back As Color, fore As Color, size As Single) As Label
        Dim l As New Label()
        l.AutoSize = False
        l.Bounds = New Rectangle(x, y, w, h)
        l.Text = Char.ConvertFromUtf32(cp)
        l.Font = New Font("Segoe UI Emoji", size)
        l.ForeColor = fore
        l.BackColor = back
        l.TextAlign = ContentAlignment.MiddleCenter
        Return l
    End Function

    Public Function Btn(text As String, x As Integer, y As Integer, w As Integer, h As Integer, back As Color, Optional fore As Color = Nothing, Optional size As Single = 9.0F) As Button
        Dim b As New Button()
        b.Text = text
        b.SetBounds(x, y, w, h)
        b.FlatStyle = FlatStyle.Flat
        b.UseVisualStyleBackColor = False
        b.BackColor = back
        b.ForeColor = If(fore.IsEmpty, Color.White, fore)
        b.FlatAppearance.BorderColor = Color.White
        b.FlatAppearance.BorderSize = 1
        b.Font = New Font("Segoe UI", size, FontStyle.Bold)
        b.Cursor = Cursors.Hand
        Return b
    End Function

    Public Function Txt(x As Integer, y As Integer, w As Integer) As TextBox
        Dim t As New TextBox()
        t.BorderStyle = BorderStyle.FixedSingle
        t.BackColor = Color.White
        t.ForeColor = Color.Black
        t.Font = New Font("Segoe UI", 10.0F)
        t.SetBounds(x, y, w, 26)
        Return t
    End Function

    Public Function Combo(x As Integer, y As Integer, w As Integer) As ComboBox
        Dim c As New ComboBox()
        c.DropDownStyle = ComboBoxStyle.DropDownList
        c.FlatStyle = FlatStyle.Flat
        c.BackColor = Color.White
        c.ForeColor = Color.Black
        c.Font = New Font("Segoe UI", 10.0F)
        c.SetBounds(x, y, w, 28)
        Return c
    End Function

    Public Function DatePick(x As Integer, y As Integer, w As Integer) As DateTimePicker
        Dim d As New DateTimePicker()
        d.Format = DateTimePickerFormat.Long
        d.Font = New Font("Segoe UI", 9.5F)
        d.SetBounds(x, y, w, 28)
        Return d
    End Function

    Public Sub PageHeader(host As Control, title As String, subtitle As String, cp As Integer, back As Color, fore As Color, x As Integer, y As Integer)
        host.Controls.Add(GlyphTile(cp, x, y, 100, 64, back, fore, 28))
        host.Controls.Add(Lbl(title, x + 120, y + 2, 20, True))
        host.Controls.Add(Lbl(subtitle, x + 124, y + 42, 11, False))
    End Sub

    ''' <summary>Id of the selected row of a combo that is bound to a DataTable with an Id column.</summary>
    Public Function IdOf(c As ComboBox) As Integer
        Dim d As DataRowView = TryCast(c.SelectedItem, DataRowView)
        If d Is Nothing Then Return 0
        Return Convert.ToInt32(d("Id"))
    End Function

    ''' <summary>Selects the row whose Id equals <paramref name="id"/> in a DataTable-bound combo.</summary>
    Public Sub SelectById(c As ComboBox, id As Integer)
        For i As Integer = 0 To c.Items.Count - 1
            Dim d As DataRowView = TryCast(c.Items(i), DataRowView)
            If d IsNot Nothing AndAlso Convert.ToInt32(d("Id")) = id Then
                c.SelectedIndex = i
                Return
            End If
        Next
    End Sub

    Public Sub Info(msg As String)
        MessageBox.Show(msg, "The Ledger", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Public Sub Warn(msg As String)
        MessageBox.Show(msg, "The Ledger", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Public Function Confirm(msg As String) As Boolean
        Return MessageBox.Show(msg, "The Ledger", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes
    End Function

End Module
