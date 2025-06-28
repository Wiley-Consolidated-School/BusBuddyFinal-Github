using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BusBuddy.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BusBuddy.Data
{
    public class BusRepository : BaseRepository, IBusRepository
    {
        public BusRepository() : base() { }

        public IEnumerable<Bus> GetAllBuses()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT BusId, BusNumber, Year, Make, Model, SeatingCapacity,
                       VIN, LicenseNumber, LastInspectionDate
                FROM Vehicles
                ORDER BY BusNumber";
            connection.Open();
            return connection.Query<Bus>(sql);
        }

        public Bus GetBusById(int busId)
        {
            using var connection = CreateConnection();
            connection.Open();
            const string sql = @"
                SELECT BusId, BusNumber, Year, Make, Model, SeatingCapacity,
                       VIN, LicenseNumber, LastInspectionDate
                FROM Vehicles
                WHERE BusId = @BusId";
            return connection.QuerySingleOrDefault<Bus>(sql, new { BusId = busId });
        }

        public void AddBus(Bus bus)
        {
            using var connection = CreateConnection();
            connection.Open();
            const string sql = @"
                INSERT INTO Vehicles (BusNumber, Year, Make, Model, SeatingCapacity, VIN, LicenseNumber, LastInspectionDate)
                VALUES (@BusNumber, @Year, @Make, @Model, @SeatingCapacity, @VIN, @LicenseNumber, @LastInspectionDate)";
            connection.Execute(sql, bus);
        }

        public void UpdateBus(Bus bus)
        {
            using var connection = CreateConnection();
            connection.Open();
            const string sql = @"
                UPDATE Vehicles
                SET BusNumber = @BusNumber,
                    Year = @Year,
                    Make = @Make,
                    Model = @Model,
                    SeatingCapacity = @SeatingCapacity,
                    VIN = @VIN,
                    LicenseNumber = @LicenseNumber,
                    LastInspectionDate = @LastInspectionDate
                WHERE BusId = @BusId";
            connection.Execute(sql, bus);
        }

        public void DeleteBus(int busId)
        {
            using var connection = CreateConnection();
            connection.Open();
            const string sql = "DELETE FROM Vehicles WHERE BusId = @BusId";
            connection.Execute(sql, new { BusId = busId });
        }

        // Diagnostic method for database troubleshooting
        public void DiagnoseDataRetrieval()
        {
            try
            {
                using var connection = CreateConnection();
                Console.WriteLine($"Testing BusRepository data retrieval...");

                // Test basic connection
                connection.Open();
                Console.WriteLine($"✅ Database connection successful: {connection.State}");

                // Test table existence and row count
                var tableCount = connection.QuerySingle<int>("SELECT COUNT(*) FROM Vehicles");
                Console.WriteLine($"Vehicles table has {tableCount} records");

                // Test actual data retrieval
                var buses = GetAllBuses().ToList();
                Console.WriteLine($"✅ Successfully retrieved {buses.Count} buses from database");

                foreach (var bus in buses.Take(3))
                {
                    Console.WriteLine($"  - Bus {bus.BusNumber}: {bus.Year} {bus.Make} {bus.Model}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in BusRepository: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}

