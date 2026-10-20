-- The Ledger : Subscription Tracker  (SQLite)
-- The app creates this automatically on first run (Data\ledger.db next to the .exe).
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Users(
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Username TEXT NOT NULL UNIQUE,
  PasswordHash TEXT NOT NULL,              -- SHA-256 hex
  Role TEXT NOT NULL DEFAULT 'Administrator');

CREATE TABLE IF NOT EXISTS Customers(
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  FullName TEXT NOT NULL,
  Email TEXT, Phone TEXT, Address TEXT);

CREATE TABLE IF NOT EXISTS Categories(
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL UNIQUE);

CREATE TABLE IF NOT EXISTS Plans(
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL,
  CategoryId INTEGER NOT NULL REFERENCES Categories(Id),
  MonthlyPrice REAL NOT NULL);

CREATE TABLE IF NOT EXISTS Subscriptions(
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  CustomerId INTEGER NOT NULL REFERENCES Customers(Id) ON DELETE CASCADE,
  CategoryId INTEGER NOT NULL REFERENCES Categories(Id),
  PlanId INTEGER NOT NULL REFERENCES Plans(Id),
  MonthlyPrice REAL NOT NULL,
  RenewalDate TEXT NOT NULL,               -- yyyy-MM-dd
  Status TEXT NOT NULL DEFAULT 'Active');  -- Active / Inactive

CREATE TABLE IF NOT EXISTS Payments(
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  SubscriptionId INTEGER NOT NULL REFERENCES Subscriptions(Id) ON DELETE CASCADE,
  Amount REAL NOT NULL,
  PaymentDate TEXT NOT NULL,               -- yyyy-MM-dd
  Method TEXT NOT NULL);
