Imports System.Windows.Forms

Public Module Program

    <STAThread>
    Public Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Try
            Db.Init()
        Catch ex As Exception
            MessageBox.Show("Could not open the database:" & vbCrLf & ex.Message, "The Ledger", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        Do
            Using lf As New LoginForm()
                If lf.ShowDialog() <> DialogResult.OK Then Exit Do
            End Using

            Dim loggedOut As Boolean
            Using mf As New MainForm()
                mf.ShowDialog()
                loggedOut = mf.LoggedOut
            End Using
            If Not loggedOut Then Exit Do
        Loop
    End Sub

End Module
