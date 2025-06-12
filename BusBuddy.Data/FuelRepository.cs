using System;
using System.Collections.Generic;
using System.Data;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{
    public class FuelRepository : BaseRepository, IFuelRepository
    {
        public FuelRepository() : base()
        {
        }

        public List<Fuel> GetAllFuelRecords()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var fuelRecords = connection.Query<Fuel>("SELECT * FROM Fuel").AsList();
                return fuelRecords;
            }
        }

        public Fuel GetFuelRecordById(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return connection.QuerySingleOrDefault<Fuel>(
                    "SELECT * FROM Fuel WHERE FuelID = @FuelID",
                    new { FuelID = id });
            }
        }

        public List<Fuel> GetFuelRecordsByDate(DateTime date)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var fuelRecords = connection.Query<Fuel>(
                    "SELECT * FROM Fuel WHERE FuelDate = @FuelDate",
                    new { FuelDate = date }).AsList();
                return fuelRecords;
            }
        }

        public List<Fuel> GetFuelRecordsByVehicle(int vehicleId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var fuelRecords = connection.Query<Fuel>(
                    "SELECT * FROM Fuel WHERE VehicleFueledID = @VehicleID",
                    new { VehicleID = vehicleId }).AsList();
                return fuelRecords;
            }
        }

        public int AddFuelRecord(Fuel fuelRecord)
        {
            using (var connection = CreateConnection())
            {
                connection.Open(); var sql = @"
                    INSERT INTO Fuel (
                        FuelDate, FuelLocation, VehicleFueledID,
                        VehicleOdometerReading, FuelType, FuelAmount,
                        FuelCost, Notes
                    )
                    VALUES (
                        @FuelDate, @FuelLocation, @VehicleFueledID,
                        @VehicleOdometerReading, @FuelType, @FuelAmount,
                        @FuelCost, @Notes
                    );
                    SELECT last_insert_rowid()";

                return connection.QuerySingle<int>(sql, fuelRecord);
            }
        }

        public bool UpdateFuelRecord(Fuel fuelRecord)
        {
            using (var connection = CreateConnection())
            {
                connection.Open(); var sql = @"
                    UPDATE Fuel
                    SET FuelDate = @FuelDate,
                        FuelLocation = @FuelLocation,
                        VehicleFueledID = @VehicleFueledID,
                        VehicleOdometerReading = @VehicleOdometerReading,
                        FuelType = @FuelType,
                        FuelAmount = @FuelAmount,
                        FuelCost = @FuelCost,
                        Notes = @Notes
                    WHERE FuelID = @FuelID";

                var rowsAffected = connection.Execute(sql, fuelRecord);
                return rowsAffected > 0;
            }
        }

        public bool DeleteFuelRecord(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = "DELETE FROM Fuel WHERE FuelID = @FuelID";
                var rowsAffected = connection.Execute(sql, new { FuelID = id });
                return rowsAffected > 0;
            }
        }
    }
}
