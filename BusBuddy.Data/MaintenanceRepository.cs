using System;
using System.Collections.Generic;
using System.Data;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{
    public class MaintenanceRepository : BaseRepository, IMaintenanceRepository
    {
        public MaintenanceRepository() : base()
        {
        }

        public MaintenanceRepository(string connectionString, string providerName) : base(connectionString, providerName)
        {
        }

        public List<Maintenance> GetAllMaintenanceRecords()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var maintenanceRecords = connection.Query<Maintenance>("SELECT * FROM Maintenance").AsList();
                return maintenanceRecords;
            }
        }

        public Maintenance GetMaintenanceById(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return connection.QuerySingleOrDefault<Maintenance>(
                    "SELECT * FROM Maintenance WHERE MaintenanceID = @MaintenanceID",
                    new { MaintenanceID = id });
            }
        }

        public List<Maintenance> GetMaintenanceByDate(DateTime date)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                // Format the date as a string in ISO format to match storage format
                string formattedDate = date.ToString("yyyy-MM-dd");
                var maintenanceRecords = connection.Query<Maintenance>(
                    "SELECT * FROM Maintenance WHERE Date = @Date",
                    new { Date = formattedDate }).AsList();
                return maintenanceRecords;
            }
        }

        public List<Maintenance> GetMaintenanceByBus(int busId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var maintenanceRecords = connection.Query<Maintenance>(
                    "SELECT * FROM Maintenance WHERE BusId = @BusId",
                    new { BusId = busId }).AsList();
                return maintenanceRecords;
            }
        }

        public List<Maintenance> GetMaintenanceByType(string maintenanceType)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var maintenanceRecords = connection.Query<Maintenance>(
                    "SELECT * FROM Maintenance WHERE MaintenanceCompleted = @MaintenanceType",
                    new { MaintenanceType = maintenanceType }).AsList();
                return maintenanceRecords;
            }
        }

        public int AddMaintenance(Maintenance maintenance)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                // Validate foreign key constraint - ensure BusId exists
                if (maintenance.BusId.HasValue)
                {
                    var vehicleExists = connection.QuerySingleOrDefault<int>(
                        "SELECT COUNT(*) FROM Vehicles WHERE Id = @BusId",
                        new { BusId = maintenance.BusId });
                    if (vehicleExists == 0)
                    {
                        throw new InvalidOperationException($"Vehicle with ID {maintenance.BusId} does not exist.");
                    }
                }
                var sql = @"
                    INSERT INTO Maintenance (
                        Date, BusId, OdometerReading,
                        MaintenanceCompleted, RepairCost, Notes
                    )
                    VALUES (
                        @Date, @BusId, @OdometerReading,
                        @MaintenanceCompleted, @RepairCost, @Notes
                    );
                    SELECT SCOPE_IDENTITY()";
                return connection.QuerySingle<int>(sql, maintenance);
            }
        }

        public bool UpdateMaintenance(Maintenance maintenance)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                // Validate foreign key constraint - ensure BusId exists
                if (maintenance.BusId.HasValue)
                {
                    var vehicleExists = connection.QuerySingleOrDefault<int>(
                        "SELECT COUNT(*) FROM Vehicles WHERE Id = @BusId",
                        new { BusId = maintenance.BusId });
                    if (vehicleExists == 0)
                    {
                        throw new InvalidOperationException($"Vehicle with ID {maintenance.BusId} does not exist.");
                    }
                }
                // Check if Vendor column exists before trying to update it
                var checkColumnSql = @"
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'Maintenance' AND COLUMN_NAME = 'Vendor'";
                var vendorColumnExists = connection.QuerySingleOrDefault<int>(checkColumnSql);
                string sql;
                if (vendorColumnExists > 0)
                {
                    sql = @"
                        UPDATE Maintenance
                        SET Date = @Date,
                            BusId = @BusId,
                            OdometerReading = @OdometerReading,
                            MaintenanceCompleted = @MaintenanceCompleted,
                            Vendor = @Vendor,
                            RepairCost = @RepairCost,
                            Notes = @Notes
                        WHERE MaintenanceID = @MaintenanceID";
                }
                else
                {
                    // Fallback: update without Vendor column if it doesn't exist
                    sql = @"
                        UPDATE Maintenance
                        SET Date = @Date,
                            BusId = @BusId,
                            OdometerReading = @OdometerReading,
                            MaintenanceCompleted = @MaintenanceCompleted,
                            RepairCost = @RepairCost,
                            Notes = @Notes
                        WHERE MaintenanceID = @MaintenanceID";
                }
                var rowsAffected = connection.Execute(sql, maintenance);
                return rowsAffected > 0;
            }
        }

        public bool DeleteMaintenance(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = "DELETE FROM Maintenance WHERE MaintenanceID = @MaintenanceID";
                var rowsAffected = connection.Execute(sql, new { MaintenanceID = id });
                return rowsAffected > 0;
            }
        }

        // Additional methods for compatibility
        public List<Maintenance> GetAllMaintenances()
        {
            return GetAllMaintenanceRecords();
        }

        public bool DeleteMaintenanceRecord(int id)
        {
            return DeleteMaintenance(id);
        }

        public int Add(Maintenance maintenance)
        {
            return AddMaintenance(maintenance);
        }
    }
}

