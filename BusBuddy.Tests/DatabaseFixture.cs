using Xunit;
using System;
using System.Collections.Generic;
using BusBuddy.Models;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using System.IO;

namespace BusBuddy.Tests
{
    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
    public class DatabaseFixture : IDisposable
    {
        public SqliteConnection Connection { get; private set; }
        public List<Vehicle> TestVehicles { get; private set; } = new List<Vehicle>();
        public List<Driver> TestDrivers { get; private set; } = new List<Driver>();

        public DatabaseFixture()
        {
            // Create and open a connection. This creates the SQLite in-memory database
            Connection = new SqliteConnection("Data Source=:memory:");
            Connection.Open();

            // Create the schema by reading the DatabaseScript.sql file
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BusBuddy.Data", "DatabaseScript.sql");
            if (!File.Exists(scriptPath))
            {
                // If the file doesn't exist in the test directory, try to find it relative to the current directory
                scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "BusBuddy.Data", "DatabaseScript.sql");
                scriptPath = Path.GetFullPath(scriptPath);
            }

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Could not find DatabaseScript.sql at {scriptPath}");
            }

            string script = File.ReadAllText(scriptPath);

            foreach (var command in script.Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(command))
                {
                    using (var cmd = Connection.CreateCommand())
                    {
                        cmd.CommandText = command;
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Seed test vehicles
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Vehicles (VehicleNumber, BusNumber, Make, Model, Year, SeatingCapacity, FuelType, Status)
                    VALUES ('BUS001', 'B001', 'Mercedes', 'Sprinter', 2020, 25, 'Diesel', 'Active');

                    INSERT INTO Vehicles (VehicleNumber, BusNumber, Make, Model, Year, SeatingCapacity, FuelType, Status)
                    VALUES ('BUS002', 'B002', 'Ford', 'Transit', 2019, 18, 'Gasoline', 'Maintenance');

                    INSERT INTO Vehicles (VehicleNumber, BusNumber, Make, Model, Year, SeatingCapacity, FuelType, Status)
                    VALUES ('VAN001', 'V001', 'Toyota', 'HiAce', 2021, 12, 'Diesel', 'Active');
                ";
                cmd.ExecuteNonQuery();
            }

            // Seed test drivers
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Drivers (DriverName, DriverPhone, DriverEmail, Address, DriversLicenseType, TrainingComplete)
                    VALUES ('John Smith', '555-1234', 'john@example.com', '123 Main St', 'CDL-B', 1);

                    INSERT INTO Drivers (DriverName, DriverPhone, DriverEmail, Address, DriversLicenseType, TrainingComplete)
                    VALUES ('Mary Johnson', '555-5678', 'mary@example.com', '456 Oak Ave', 'CDL-A', 1);
                ";
                cmd.ExecuteNonQuery();
            }

            // Create test data objects for use in tests
            TestVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, VehicleNumber = "BUS001", BusNumber = "B001", Make = "Mercedes", Model = "Sprinter", Year = 2020, SeatingCapacity = 25, FuelType = "Diesel", Status = "Active" },
                new Vehicle { Id = 2, VehicleNumber = "BUS002", BusNumber = "B002", Make = "Ford", Model = "Transit", Year = 2019, SeatingCapacity = 18, FuelType = "Gasoline", Status = "Maintenance" },
                new Vehicle { Id = 3, VehicleNumber = "VAN001", BusNumber = "V001", Make = "Toyota", Model = "HiAce", Year = 2021, SeatingCapacity = 12, FuelType = "Diesel", Status = "Active" }
            }; TestDrivers = new List<Driver>
            {
                new Driver { DriverID = 1, DriverName = "John Smith", DriverPhone = "555-1234", DriverEmail = "john@example.com", Address = "123 Main St", DriversLicenseType = "CDL-B", TrainingComplete = 1 },
                new Driver { DriverID = 2, DriverName = "Mary Johnson", DriverPhone = "555-5678", DriverEmail = "mary@example.com", Address = "456 Oak Ave", DriversLicenseType = "CDL-A", TrainingComplete = 1 }
            };
        }

        public void Dispose()
        {
            Connection.Dispose();
        }

        // Utility method to verify if a table exists
        public bool TableExists(string tableName)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@name";
                cmd.Parameters.Add(new SqliteParameter("@name", tableName));
                var result = cmd.ExecuteScalar();
                return result != null;
            }
        }

        // Utility method to verify if an index exists
        public bool IndexExists(string indexName)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='index' AND name=@name";
                cmd.Parameters.Add(new SqliteParameter("@name", indexName));
                var result = cmd.ExecuteScalar();
                return result != null;
            }
        }
    }
}
