Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports Microsoft.Data.Sqlite
Imports MySqlConnector

Public Module Db

    Private connString As String = ""
    Public DbFile As String = ""
    Public CurrentUser As String = ""
    Public CurrentRole As String = ""
    Public CurrentCustomerId As Integer = 0
    Public UseMySql As Boolean = False ' Toggle True to connect to XAMPP MySQL

    Public Const SubSql As String =
        "SELECT s.Id, s.CustomerId, c.FullName AS Customer, p.Name AS Plan, cat.Name AS Category, " &
        "s.CategoryId, s.PlanId, s.MonthlyPrice, s.RenewalDate, s.Status, " &
        "COALESCE((SELECT pm.Method FROM Payments pm WHERE pm.SubscriptionId=s.Id " &
        "ORDER BY pm.PaymentDate DESC, pm.Id DESC LIMIT 1),'-') AS Method " &
        "FROM Subscriptions s " &
        "JOIN Customers c ON c.Id=s.CustomerId " &
        "JOIN Plans p ON p.Id=s.PlanId " &
        "JOIN Categories cat ON cat.Id=s.CategoryId "

    Public Sub Init()
        If UseMySql Then
            connString = "Server=localhost;Database=the_ledger;Uid=root;Pwd=;"
        Else
            Dim folder As String = Path.Combine(AppContext.BaseDirectory, "Data")
            Try
                Directory.CreateDirectory(folder)
                Dim probe As String = Path.Combine(folder, ".write-test")
                File.WriteAllText(probe, "x")
                File.Delete(probe)
            Catch
                folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TheLedger")
                Directory.CreateDirectory(folder)
            End Try
            DbFile = Path.Combine(folder, "ledger.db")
            connString = "Data Source=" & DbFile & ";Foreign Keys=True"
        End If
        CreateSchema()
        SeedIfEmpty()
    End Sub

    Private Sub CreateSchema()
        If UseMySql Then
            Exec("CREATE TABLE IF NOT EXISTS Users(Id INT AUTO_INCREMENT PRIMARY KEY, Username VARCHAR(255) NOT NULL UNIQUE, PasswordHash VARCHAR(255) NOT NULL, Role VARCHAR(50) NOT NULL DEFAULT 'Client', CustomerId INT DEFAULT 0);" &
                 "CREATE TABLE IF NOT EXISTS Customers(Id INT AUTO_INCREMENT PRIMARY KEY, FullName VARCHAR(255) NOT NULL, Email VARCHAR(255), Phone VARCHAR(50), Address TEXT, AccountNumber VARCHAR(100), CardInfo VARCHAR(100));" &
                 "CREATE TABLE IF NOT EXISTS Categories(Id INT AUTO_INCREMENT PRIMARY KEY, Name VARCHAR(255) NOT NULL UNIQUE);" &
                 "CREATE TABLE IF NOT EXISTS Plans(Id INT AUTO_INCREMENT PRIMARY KEY, Name VARCHAR(255) NOT NULL, CategoryId INT NOT NULL, MonthlyPrice DOUBLE NOT NULL);" &
                 "CREATE TABLE IF NOT EXISTS Subscriptions(Id INT AUTO_INCREMENT PRIMARY KEY, CustomerId INT NOT NULL, CategoryId INT NOT NULL, PlanId INT NOT NULL, MonthlyPrice DOUBLE NOT NULL, RenewalDate VARCHAR(50) NOT NULL, Status VARCHAR(50) NOT NULL DEFAULT 'Active');" &
                 "CREATE TABLE IF NOT EXISTS Payments(Id INT AUTO_INCREMENT PRIMARY KEY, SubscriptionId INT NOT NULL, Amount DOUBLE NOT NULL, PaymentDate VARCHAR(50) NOT NULL, Method VARCHAR(100) NOT NULL);")
        Else
            Exec("CREATE TABLE IF NOT EXISTS Users(Id INTEGER PRIMARY KEY AUTOINCREMENT, Username TEXT NOT NULL UNIQUE, PasswordHash TEXT NOT NULL, Role TEXT NOT NULL DEFAULT 'Client', CustomerId INTEGER DEFAULT 0);" &
                 "CREATE TABLE IF NOT EXISTS Customers(Id INTEGER PRIMARY KEY AUTOINCREMENT, FullName TEXT NOT NULL, Email TEXT, Phone TEXT, Address TEXT, AccountNumber TEXT, CardInfo TEXT);" &
                 "CREATE TABLE IF NOT EXISTS Categories(Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL UNIQUE);" &
                 "CREATE TABLE IF NOT EXISTS Plans(Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, CategoryId INTEGER NOT NULL REFERENCES Categories(Id), MonthlyPrice REAL NOT NULL);" &
                 "CREATE TABLE IF NOT EXISTS Subscriptions(Id INTEGER PRIMARY KEY AUTOINCREMENT, CustomerId INTEGER NOT NULL REFERENCES Customers(Id) ON DELETE CASCADE, CategoryId INTEGER NOT NULL REFERENCES Categories(Id), PlanId INTEGER NOT NULL REFERENCES Plans(Id), MonthlyPrice REAL NOT NULL, RenewalDate TEXT NOT NULL, Status TEXT NOT NULL DEFAULT 'Active');" &
                 "CREATE TABLE IF NOT EXISTS Payments(Id INTEGER PRIMARY KEY AUTOINCREMENT, SubscriptionId INTEGER NOT NULL REFERENCES Subscriptions(Id) ON DELETE CASCADE, Amount REAL NOT NULL, PaymentDate TEXT NOT NULL, Method TEXT NOT NULL);")
        End If
    End Sub

    Private Sub SeedIfEmpty()
        If Convert.ToInt32(Scalar("SELECT COUNT(*) FROM Users")) > 0 Then Return

        Exec("INSERT INTO Users(Username,PasswordHash,Role,CustomerId) VALUES(@u,@h,'Administrator',0)", "@u", "admin", "@h", Hash("admin123"))

        For Each c As String In {"Music", "Video", "Cinema", "Social", "Gaming", "Productivity", "Other"}
            Exec("INSERT INTO Categories(Name) VALUES(@n)", "@n", c)
        Next

        SeedPlan("Spotify Premium", "Music", 169)
        SeedPlan("Netflix Standard", "Cinema", 449)
        SeedPlan("YouTube Premium", "Video", 189)

        SeedCustomer("Jackson Wang", "jacksonwang@gmail.com", "0912-345-6789", "Quezon City", "ACC-998822", "Visa ending 4321")
        SeedCustomer("Jane Dy", "janedy@gmail.com", "0911-335-6779", "Makati City", "ACC-112233", "Mastercard ending 8876")

        Exec("INSERT INTO Users(Username,PasswordHash,Role,CustomerId) VALUES(@u,@h,'Client',1)", "@u", "jackson", "@h", Hash("client123"))
        Exec("INSERT INTO Users(Username,PasswordHash,Role,CustomerId) VALUES(@u,@h,'Client',2)", "@u", "jane", "@h", Hash("client123"))

        SeedSub("Jackson Wang", "Netflix Standard", -2, "Active")
        SeedSub("Jane Dy", "Spotify Premium", 1, "Active")
    End Sub

    Private Sub SeedPlan(name As String, cat As String, price As Double)
        Exec("INSERT INTO Plans(Name,CategoryId,MonthlyPrice) VALUES(@n,(SELECT Id FROM Categories WHERE Name=@c),@p)", "@n", name, "@c", cat, "@p", price)
    End Sub

    Private Sub SeedCustomer(name As String, email As String, phone As String, addr As String, acc As String, card As String)
        Exec("INSERT INTO Customers(FullName,Email,Phone,Address,AccountNumber,CardInfo) VALUES(@n,@e,@p,@a,@ac,@cr)", "@n", name, "@e", email, "@p", phone, "@a", addr, "@ac", acc, "@cr", card)
    End Sub

    Private Sub SeedSub(cust As String, plan As String, offsetDays As Integer, status As String)
        Exec("INSERT INTO Subscriptions(CustomerId,CategoryId,PlanId,MonthlyPrice,RenewalDate,Status) " &
             "SELECT c.Id, p.CategoryId, p.Id, p.MonthlyPrice, @d, @s FROM Customers c, Plans p WHERE c.FullName=@c AND p.Name=@p",
             "@d", Iso(Date.Today.AddDays(offsetDays)), "@s", status, "@c", cust, "@p", plan)
    End Sub

    Public Function GetConn() As IDbConnection
        If UseMySql Then
            Return New MySqlConnection(connString)
        Else
            Dim c As New SqliteConnection(connString)
            c.Open()
            Return c
        End If
    End Function

    Private Sub AddParams(cmd As IDbCommand, args() As Object)
        If args Is Nothing Then Return
        For i As Integer = 0 To args.Length - 2 Step 2
            Dim p As IDbDataParameter = cmd.CreateParameter()
            p.ParameterName = CStr(args(i))
            p.Value = If(args(i + 1) Is Nothing, DBNull.Value, args(i + 1))
            cmd.Parameters.Add(p)
        Next
    End Sub

    Public Sub Exec(sql As String, ParamArray args() As Object)
        Using c As IDbConnection = GetConn()
            If c.State <> ConnectionState.Open Then c.Open()
            Using cmd As IDbCommand = c.CreateCommand()
                cmd.CommandText = sql
                AddParams(cmd, args)
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Function Scalar(sql As String, ParamArray args() As Object) As Object
        Using c As IDbConnection = GetConn()
            If c.State <> ConnectionState.Open Then c.Open()
            Using cmd As IDbCommand = c.CreateCommand()
                cmd.CommandText = sql
                AddParams(cmd, args)
                Return cmd.ExecuteScalar()
            End Using
        End Using
    End Function

    Public Function Query(sql As String, ParamArray args() As Object) As DataTable
        Dim dt As New DataTable()
        Using c As IDbConnection = GetConn()
            If c.State <> ConnectionState.Open Then c.Open()
            Using cmd As IDbCommand = c.CreateCommand()
                cmd.CommandText = sql
                AddParams(cmd, args)
                Using r As IDataReader = cmd.ExecuteReader()
                    For i As Integer = 0 To r.FieldCount - 1
                        dt.Columns.Add(r.GetName(i), GetType(Object))
                    Next
                    While r.Read()
                        Dim row As DataRow = dt.NewRow()
                        For i As Integer = 0 To r.FieldCount - 1
                            row(i) = If(r.IsDBNull(i), DBNull.Value, r.GetValue(i))
                        Next
                        dt.Rows.Add(row)
                    End While
                End Using
            End Using
        End Using
        Return dt
    End Function

    Public Function Iso(d As Date) As String
        Return d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    End Function

    Public Function ParseDate(o As Object) As Date
        Return Date.ParseExact(CStr(o), "yyyy-MM-dd", CultureInfo.InvariantCulture)
    End Function

    Public Function DaysLeft(o As Object) As Integer
        Return CInt((ParseDate(o).Date - Date.Today).TotalDays)
    End Function

    Public Function Dec(o As Object) As Decimal
        Return Convert.ToDecimal(o, CultureInfo.InvariantCulture)
    End Function

    Public Function ToInt(o As Object) As Integer
        Return Convert.ToInt32(o)
    End Function

    Public Function Sv(o As Object) As String
        If o Is Nothing OrElse IsDBNull(o) Then Return ""
        Return CStr(o)
    End Function

    Public Function Hash(s As String) As String
        Return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)))
    End Function

    Public Function CheckLogin(user As String, pass As String) As Boolean
        Dim dt As DataTable = Query("SELECT Username, Role, CustomerId FROM Users WHERE Username=@u AND PasswordHash=@h", "@u", user.Trim(), "@h", Hash(pass))
        If dt.Rows.Count = 1 Then
            CurrentUser = CStr(dt.Rows(0)("Username"))
            CurrentRole = CStr(dt.Rows(0)("Role"))
            CurrentCustomerId = ToInt(dt.Rows(0)("CustomerId"))
            Return True
        End If
        Return False
    End Function

    Public Function ChangePassword(oldPass As String, newPass As String) As Boolean
        If Not CheckLogin(CurrentUser, oldPass) Then Return False
        Exec("UPDATE Users SET PasswordHash=@h WHERE Username=@u", "@h", Hash(newPass), "@u", CurrentUser)
        Return True
    End Function

    Public Function AllSubscriptions() As DataTable
        If CurrentRole = "Client" Then
            Return Query(SubSql & "WHERE s.CustomerId=@cid ORDER BY s.RenewalDate, s.Id", "@cid", CurrentCustomerId)
        End If
        Return Query(SubSql & "ORDER BY s.RenewalDate, s.Id")
    End Function

End Module
