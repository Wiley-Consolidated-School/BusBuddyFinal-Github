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
                var maintenanceRecords = connection.Query<Maintenance>(
                    "SELECT * FROM Maintenance WHERE Date = @Date",
                    new { Date = date }).AsList();
                return maintenanceRecords;
            }
        }

        public List<Maintenance> GetMaintenanceByVehicle(int vehicleId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var maintenanceRecords = connection.Query<Maintenance>(
                    "SELECT * FROM Maintenance WHERE VehicleID = @VehicleID",
                    new { VehicleID = vehicleId }).AsList();
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
                var sql = @"
                    INSERT INTO Maintenance (
                        Date, VehicleID, OdometerReading,
                        MaintenanceCompleted, Vendor, RepairCost
                    )
                    VALUES (
                        @Date, @VehicleID, @OdometerReading,
                        @MaintenanceCompleted, @Vendor, @RepairCost
                    );
                    SELECT last_insert_rowid()";

                return connection.QuerySingle<int>(sql, maintenance);
            }
        }

        public bool UpdateMaintenance(Maintenance maintenance)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    UPDATE Maintenance
                    SET Date = @Date,
                        VehicleID = @VehicleID,
                        OdometerReading = @OdometerReading,
                        MaintenanceCompleted = @MaintenanceCompleted,
                        Vendor = @Vendor,
                        RepairCost = @RepairCost
                    WHERE MaintenanceID = @MaintenanceID";

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
    }
}
