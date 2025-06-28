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

    /// <summary>
    /// Diagnostic method to verify data retrieval is working
    /// </summary>
    /// <returns>A diagnostic string with information about data retrieval</returns>
    public string DiagnoseDataRetrieval()
    {
        var result = new System.Text.StringBuilder();
        result.AppendLine($"=== Vehicle Repository Diagnostics: {DateTime.Now} ===");

        try
        {
            result.AppendLine("Testing database connection...");
            using (var connection = CreateConnection())
            {
                try
                {
                    connection.Open();
                    result.AppendLine($"✅ Database connection successful: {connection.State}");

                    // Check if we can count vehicles
                    result.AppendLine("Checking Vehicles table...");

                    // Try to count records
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM Vehicles";
                        var count = Convert.ToInt32(command.ExecuteScalar());
                        result.AppendLine($"Vehicles table has {count} records");

                        if (count > 0)
                        {
                            // Get sample records
                            command.CommandText = "SELECT TOP 3 Id, VehicleNumber FROM Vehicles";
                            using (var reader = command.ExecuteReader())
                            {
                                int recordCount = 0;
                                result.AppendLine("Sample vehicle records:");

                                while (reader.Read())
                                {
                                    recordCount++;
                                    var id = reader["Id"];
                                    var number = reader["VehicleNumber"];
                                    result.AppendLine($"  ID: {id}, Number: {number}");
                                }

                                if (recordCount == 0)
                                {
                                    result.AppendLine("⚠️ No vehicle records found despite count > 0");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.AppendLine($"❌ Error accessing database: {ex.Message}");
                    result.AppendLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }
        catch (Exception ex)
        {
            result.AppendLine($"❌ Error creating connection: {ex.Message}");
            result.AppendLine($"Stack trace: {ex.StackTrace}");
        }

        return result.ToString();
    }

        public virtual List<Vehicle> GetAllVehicles()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    var vehicles = connection.Query<Vehicle>("SELECT * FROM Vehicles").AsList();

                    // Map the database Id column to VehicleID property for consistency
                    foreach (var vehicle in vehicles)
                    {
                        if (vehicle.VehicleID == 0 && vehicle.Id > 0)
                        {
                            vehicle.VehicleID = vehicle.Id;
                        }
                    }

                    Console.WriteLine($"Retrieved {vehicles.Count} vehicles from database");
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
                        // Ensure the VehicleID property is set correctly from the Id column
                        vehicle.VehicleID = vehicle.Id;
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
            if (vehicle == null)
                throw new ArgumentNullException(nameof(vehicle));

            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    INSERT INTO Vehicles (VehicleNumber, Make, Model, Year, SeatingCapacity, FuelType, Status, VINNumber, LicenseNumber, DateLastInspection)
                    VALUES (@VehicleNumber, @Make, @Model, @Year, @SeatingCapacity, @FuelType, @Status, @VINNumber, @LicenseNumber, @DateLastInspection);
                    SELECT SCOPE_IDENTITY();";

                var newId = connection.QuerySingle<int>(sql, vehicle);
                vehicle.VehicleID = newId; // Set the ID on the vehicle object
                return newId;
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
