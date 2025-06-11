-- BusBuddy SQL Server Setup Script
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BusBuddy')
BEGIN
    CREATE DATABASE BusBuddy;
END
GO
USE BusBuddy;
GO
:r ../BusBuddy.Data/DatabaseScript.SqlServer.sql
