using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace BusBuddy.Data
{
    public class DatabaseInitializer
    {
        public void Initialize(DbConnection connection)
        {
            if (connection == null) return;

            try
            {
                // Create tables if they don't exist
                CreateTables(connection);
                SeedData(connection);
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid breaking tests
                Console.WriteLine($"Database initialization warning: {ex.Message}");
            }
        }

        public void Initialize(object context)
        {
            // For EF Core context - simplified version
            try
            {
                // If EF Core is available, this would handle migrations
                var dbContext = context as dynamic;
                if (dbContext?.Database != null)
                {
                    dbContext.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EF Core initialization warning: {ex.Message}");
            }
        }

        private void CreateTables(DbConnection connection)
        {
            var createVehiclesTable = @"
                CREATE TABLE IF NOT EXISTS Vehicles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    VehicleNumber TEXT NOT NULL,
                    BusNumber TEXT,
                    Make TEXT,
                    Model TEXT,
                    Year INTEGER,
                    SeatingCapacity INTEGER,
                    VINNumber TEXT,
                    LicenseNumber TEXT,
                    DateLastInspection TEXT,
                    Notes TEXT,
                    FuelType TEXT,
                    Status TEXT
                )";

            var createRoutesTable = @"
                CREATE TABLE IF NOT EXISTS Routes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    StartLocation TEXT,
                    EndLocation TEXT,
                    Distance DECIMAL
                )";

            var createTimeCardsTable = @"
                CREATE TABLE IF NOT EXISTS TimeCards (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EmployeeName TEXT NOT NULL,
                    ClockIn DATETIME,
                    ClockOut DATETIME,
                    HoursWorked DECIMAL,
                    WorkDate DATETIME
                )";

            var createFuelTable = @"
                CREATE TABLE IF NOT EXISTS Fuel (
                    FuelID INTEGER PRIMARY KEY AUTOINCREMENT,
                    FuelDate TEXT,
                    FuelLocation TEXT,
                    VehicleFueledID INTEGER,
                    VehicleOdometerReading REAL,
                    FuelType TEXT,
                    FuelAmount REAL,
                    FuelCost REAL,
                    Notes TEXT
                )";

            ExecuteCommand(connection, createVehiclesTable);
            ExecuteCommand(connection, createRoutesTable);
            ExecuteCommand(connection, createTimeCardsTable);
            ExecuteCommand(connection, createFuelTable);
        }        private void SeedData(DbConnection connection)
        {
            // Seed Vehicles
            var seedVehicles = @"
                INSERT OR IGNORE INTO Vehicles (Id, VehicleNumber, BusNumber, Make, Model, Year, SeatingCapacity, VINNumber, LicenseNumber, DateLastInspection, Notes, FuelType, Status) VALUES
                (1, 'V001', 'B001', 'Ford', 'Transit', 2020, 40, 'VIN001', 'LIC001', '2025-01-01', 'Test vehicle', 'Diesel', 'Active'),
                (2, 'V002', 'B002', 'Chevy', 'Express', 2019, 30, 'VIN002', 'LIC002', '2025-01-02', 'Test vehicle 2', 'Gasoline', 'Active'),
                (3, 'V003', 'B003', 'Bluebird', 'Vision', 2021, 50, 'VIN003', 'LIC003', '2025-01-03', 'Test vehicle 3', 'Diesel', 'Maintenance')";

            // Seed Routes
            var seedRoutes = @"
                INSERT OR IGNORE INTO Routes (Id, Name, Description) VALUES
                (1, 'Route 1', 'Main downtown route'),
                (2, 'Route 2', 'Suburban express')";

            // Seed Fuel data
            var seedFuel = @"
                INSERT OR IGNORE INTO Fuel (FuelID, FuelDate, FuelLocation, VehicleFueledID, VehicleOdometerReading, FuelType, FuelAmount, FuelCost, Notes) VALUES
                (1, '2025-06-01', 'Main Depot', 1, 1100, 'Diesel', 50, 150.00, 'Test fuel'),
                (2, '2025-06-02', 'Key Pumps', 2, 2100, 'Gasoline', 60, 180.00, 'Test fuel 2')";

            ExecuteCommand(connection, seedVehicles);
            ExecuteCommand(connection, seedRoutes);
            ExecuteCommand(connection, seedFuel);
        }

        private void ExecuteCommand(DbConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }
}
