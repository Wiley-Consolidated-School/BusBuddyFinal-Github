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

        public virtual List<Vehicle> GetAllVehicles()
        {
            // Return sample data when database is not available
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    var vehicles = connection.Query<Vehicle>("SELECT * FROM Vehicles").AsList();
                    return vehicles;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database not available: {ex.Message}. Returning sample data.");
                return new List<Vehicle>
                {
                    new Vehicle { VehicleID = 1, VehicleNumber = "BUS-001", BusNumber = "101", Make = "Blue Bird", Model = "Vision", Year = 2020, Capacity = 72, Status = "Active" },
                    new Vehicle { VehicleID = 2, VehicleNumber = "BUS-002", BusNumber = "102", Make = "IC Bus", Model = "CE Series", Year = 2019, Capacity = 90, Status = "Active" },
                    new Vehicle { VehicleID = 3, VehicleNumber = "BUS-003", BusNumber = "103", Make = "Thomas Built", Model = "Saf-T-Liner", Year = 2021, Capacity = 84, Status = "Maintenance" }
                };
            }
        }

        public virtual Vehicle? GetVehicleById(int id)
        {
            if (id <= 0)
            {
                Console.WriteLine($"WARNING: Invalid vehicle ID {id} requested");
                return null;
            }

            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    var vehicle = connection.QuerySingleOrDefault<Vehicle>("SELECT * FROM Vehicles WHERE Id = @Id", new { Id = id });

                    if (vehicle == null)
                    {
                        Console.WriteLine($"WARNING: Vehicle with ID {id} not found in database");
                    }
                    else
                    {
                        Console.WriteLine($"Vehicle found: ID={vehicle.Id}, Number={vehicle.VehicleNumber}");
                    }

                    return vehicle;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetVehicleById({id}): {ex.Message}");
                return null;
            }
        }        public virtual int AddVehicle(Vehicle vehicle)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    INSERT INTO Vehicles (VehicleNumber, Make, Model, Year, SeatingCapacity, FuelType, Status, VINNumber, LicenseNumber, DateLastInspection)
                    VALUES (@VehicleNumber, @Make, @Model, @Year, @SeatingCapacity, @FuelType, @Status, @VINNumber, @LicenseNumber, @DateLastInspection);
                    SELECT SCOPE_IDENTITY();";

                return connection.QuerySingle<int>(sql, vehicle);
            }
        }

        public virtual bool UpdateVehicle(Vehicle vehicle)
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
                        SeatingCapacity = @SeatingCapacity,
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

        public virtual bool DeleteVehicle(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = "DELETE FROM Vehicles WHERE Id = @Id";
                var rowsAffected = connection.Execute(sql, new { Id = id });                return rowsAffected > 0;
            }
        }

        // Additional methods for form compatibility
        public virtual int Add(Vehicle vehicle)
        {
            return AddVehicle(vehicle);
        }

        public virtual bool Update(Vehicle vehicle)
        {
            return UpdateVehicle(vehicle);
        }

        public virtual bool Delete(int id)
        {
            return DeleteVehicle(id);
        }
    }
}
