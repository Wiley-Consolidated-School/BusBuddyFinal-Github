using System;
using System.Collections.Generic;
using System.Data;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{
    public class VehicleRepository : BaseRepository, IVehicleRepository
    {
        public VehicleRepository() : base()
        {
        }

        public VehicleRepository(string connectionString, string providerName) : base(connectionString, providerName)
        {
        }

        public List<Vehicle> GetAllVehicles()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var vehicles = connection.Query<Vehicle>("SELECT * FROM Vehicles").AsList();
                return vehicles;
            }
        }

        public Vehicle? GetVehicleById(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return connection.QuerySingleOrDefault<Vehicle>("SELECT * FROM Vehicles WHERE Id = @Id", new { Id = id });
            }
        }

        public int AddVehicle(Vehicle vehicle)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    INSERT INTO Vehicles (VehicleNumber, Make, Model, Year, SeatingCapacity, FuelType, Status, VINNumber, LicenseNumber, DateLastInspection)
                    VALUES (@VehicleNumber, @Make, @Model, @Year, @Capacity, @FuelType, @Status, @VINNumber, @LicenseNumber, @DateLastInspection);
                    SELECT SCOPE_IDENTITY();";

                return connection.QuerySingle<int>(sql, vehicle);
            }
        }

        public bool UpdateVehicle(Vehicle vehicle)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    UPDATE Vehicles
                    SET VehicleNumber = @VehicleNumber,
                        Make = @Make,
                        Model = @Model,
                        Year = @Year,
                        Capacity = @Capacity,
                        FuelType = @FuelType,
                        Status = @Status,
                        VINNumber = @VINNumber,
                        LicenseNumber = @LicenseNumber,
                        DateLastInspection = @DateLastInspection
                    WHERE Id = @Id";

                var rowsAffected = connection.Execute(sql, vehicle);
                return rowsAffected > 0;
            }
        }

        public bool DeleteVehicle(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = "DELETE FROM Vehicles WHERE Id = @Id";
                var rowsAffected = connection.Execute(sql, new { Id = id });                return rowsAffected > 0;
            }
        }

        // Additional methods for form compatibility
        public int Add(Vehicle vehicle)
        {
            return AddVehicle(vehicle);
        }

        public bool Update(Vehicle vehicle)
        {
            return UpdateVehicle(vehicle);
        }

        public bool Delete(int id)
        {
            return DeleteVehicle(id);
        }
    }
}
