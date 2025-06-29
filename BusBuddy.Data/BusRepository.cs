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
                SELECT BusId, BusNumber, Year, Make, Model, Capacity,
                       VIN, LicenseNumber, LastInspectionDate, Status
                FROM Buses
                ORDER BY BusNumber";
            connection.Open();
            return connection.Query<Bus>(sql);
        }

        public Bus GetBusById(int busId)
        {
            using var connection = CreateConnection();
            connection.Open();
            const string sql = @"
                SELECT BusId, BusNumber, Year, Make, Model, Capacity,
                       VIN, LicenseNumber, LastInspectionDate, Status
                FROM Buses
                WHERE BusId = @BusId";
            return connection.QuerySingleOrDefault<Bus>(sql, new { BusId = busId });
        }

        public void AddBus(Bus bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            using var connection = CreateConnection();
            connection.Open();
            const string sql = @"
                INSERT INTO Buses (BusNumber, Year, Make, Model, Capacity, VIN, LicenseNumber, LastInspectionDate, Status)
                VALUES (@BusNumber, @Year, @Make, @Model, @Capacity, @VIN, @LicenseNumber, @LastInspectionDate, @Status)";
            connection.Execute(sql, bus);
        }

        public void UpdateBus(Bus bus)
        {
            using var connection = CreateConnection();
            connection.Open();
            const string sql = @"
                UPDATE Buses
                SET BusNumber = @BusNumber,
                    Year = @Year,
                    Make = @Make,
                    Model = @Model,
                    Capacity = @Capacity,
                    VIN = @VIN,
                    LicenseNumber = @LicenseNumber,
                    LastInspectionDate = @LastInspectionDate,
                    Status = @Status
                WHERE BusId = @BusId";
            connection.Execute(sql, bus);
        }

        public void DeleteBus(int busId)
        {
            using var connection = CreateConnection();
            connection.Open();
            const string sql = "DELETE FROM Buses WHERE BusId = @BusId";
            connection.Execute(sql, new { BusId = busId });
        }

        // Diagnostic method for database troubleshooting
        public void DiagnoseDataRetrieval()
        {
            try
            {
                using var connection = CreateConnection();
                Console.WriteLine($"Testing BusRepository data retrieval (Buses table)...");

                // Test basic connection
                connection.Open();
                Console.WriteLine($"✅ Database connection successful: {connection.State}");

                // Test table existence and row count
                var tableCount = connection.QuerySingle<int>("SELECT COUNT(*) FROM Buses");
                Console.WriteLine($"Buses table has {tableCount} records");

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

