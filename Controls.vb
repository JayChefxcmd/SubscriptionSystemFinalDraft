Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

''' <summary>Panel with the thin dashed purple outline used across the design.</summary>
Public Class DashedPanel
    Inherits Panel

    Public Sub New()
        DoubleBuffered = True
        ResizeRedraw = True
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Using p As New Pen(Theme.BorderCol)
            p.DashStyle = DashStyle.Dash
            e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1)
        End Using
    End Sub
End Class

Public Class DonutItem
    Public Name As String
    Public Value As Decimal
    Public ItemColor As Color
End Class

''' <summary>Donut chart used by the dashboard "Monthly Overview".</summary>
Public Class DonutPanel
    Inherits Panel

    Public Items As New List(Of DonutItem)
    Public Total As Decimal = 0

    Public Sub New()
        DoubleBuffered = True
        BackColor = Color.FromArgb(28, 25, 40)
    End Sub

    Public Sub SetData(list As List(Of DonutItem), sum As Decimal)
        Items = list
        Total = sum
        Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim th As Integer = 18
        Dim d As Integer = Width - 16
        Dim rc As New Rectangle(8 + th \ 2, 30 + th \ 2, d - th, d - th)

        Using pen As New Pen(Color.FromArgb(60, 55, 80), th)
            g.DrawEllipse(pen, rc)
        End Using

        If Total > 0 Then
            Dim startAngle As Single = -90
            For Each it As DonutItem In Items
                Dim sweep As Single = CSng(it.Value / Total * 360D)
                If sweep > 0 Then
                    Using pen As New Pen(it.ItemColor, th)
                        g.DrawArc(pen, rc, startAngle, sweep)
                    End Using
                End If
                startAngle += sweep
            Next
        End If

        Using f As New Font("Segoe UI", 8.0F, FontStyle.Bold)
            Using sf As New StringFormat()
                sf.Alignment = StringAlignment.Center
                sf.LineAlignment = StringAlignment.Center
                g.DrawString(Theme.Money(Total) & vbLf & "Total", f, Brushes.White, New RectangleF(rc.X, rc.Y, rc.Width, rc.Height), sf)
            End Using
        End Using
    End Sub
End Class
