using System;
using System.IO;
using System.Text.RegularExpressions;

public class DebugScript
{
    public static void Main()
    {
        string script = @"-- BusBuddy Enhanced Database Schema SQL Script for SQL Server

-- Vehicles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Vehicles')
CREATE TABLE Vehicles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VehicleNumber NVARCHAR(MAX) NOT NULL,
    BusNumber NVARCHAR(MAX),
    Make NVARCHAR(MAX),
    Model NVARCHAR(MAX),
    Year INT,
    SeatingCapacity INT,
    VINNumber NVARCHAR(MAX),
    LicenseNumber NVARCHAR(MAX),
    DateLastInspection NVARCHAR(MAX),
    FuelType NVARCHAR(MAX),
    Status NVARCHAR(MAX),
    Notes NVARCHAR(MAX)
);";

        Console.WriteLine("Original script:");
        Console.WriteLine(script);
        Console.WriteLine("\n" + new string('=', 50) + "\n");

        // Convert SQL Server syntax to SQLite
        script = script.Replace("INT IDENTITY(1,1) PRIMARY KEY", "INTEGER PRIMARY KEY AUTOINCREMENT");
        script = script.Replace("NVARCHAR(MAX)", "TEXT");
        script = script.Replace("DECIMAL(18,2)", "REAL");

        // Convert SQL Server table existence checks to SQLite syntax
        script = Regex.Replace(
            script,
            @"IF NOT EXISTS \(SELECT \* FROM sys\.tables WHERE name = N'(\w+)'\)\s*CREATE TABLE \1",
            "CREATE TABLE IF NOT EXISTS $1",
            RegexOptions.IgnoreCase | RegexOptions.Multiline
        );

        Console.WriteLine("After conversion:");
        Console.WriteLine(script);
        Console.WriteLine("\n" + new string('=', 50) + "\n");

        var commands = script.Split(';');
        for (int i = 0; i < commands.Length; i++)
        {
            var trimmedCommand = commands[i].Trim();
            if (!string.IsNullOrWhiteSpace(trimmedCommand) && !trimmedCommand.StartsWith("--"))
            {
                Console.WriteLine($"Command {i + 1}:");
                Console.WriteLine(trimmedCommand);
                Console.WriteLine();
            }
        }
    }
}
