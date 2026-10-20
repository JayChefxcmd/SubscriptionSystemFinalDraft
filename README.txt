THE LEDGER - Subscription Tracker   (VB.NET WinForms + SQLite)
================================================================

REQUIREMENTS
  Windows 10/11, Visual Studio 2022 or newer (".NET desktop development" workload)
  or just the .NET 8+ SDK. Internet needed ONCE so NuGet can restore the
  Microsoft.Data.Sqlite package.

RUN FROM VISUAL STUDIO
  1. Open SubscriptionSystem.sln  (or SubscriptionSystem.vbproj)
  2. Press F5.

BUILD A STANDALONE .EXE
  Double-click build-exe.bat
  -> publish\SubscriptionSystem.exe  (single file, self-contained, no .NET install needed)
  Copy that one file anywhere and run it.

LOGIN (first run)
  username: admin      password: admin123       (change it under SETTINGS)

DATABASE
  SQLite file created automatically on first launch:
      <folder of the exe>\Data\ledger.db
  (falls back to %LocalAppData%\TheLedger\ if that folder is read-only)
  Schema reference: Database\schema.sql
  Tables: Users, Customers, Categories, Plans, Subscriptions, Payments
  Sample customers/subscriptions are inserted on first run only.
  To start fresh, delete the Data\ledger.db file.

SCREENS (all from your screenshots)
  Login -> Dashboard (MainForm) -> Customers, Subscriptions (Add/Edit),
  Payments (Payment History), Calendar, Reports, Settings, Logout

SOURCE FILES
  Program.vb            entry point / login loop
  Db.vb                 database layer (schema, seed data, queries)
  Theme.vb, Controls.vb colours, control helpers, dashed panel, donut chart
  LoginForm.vb, MainForm.vb, Customer_Management.vb, AddSubscription.vb,
  Payment_History.vb, CalendarForm.vb, ReportsSettings.vb
