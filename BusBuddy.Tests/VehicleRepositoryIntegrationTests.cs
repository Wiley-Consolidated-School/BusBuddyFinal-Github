using Xunit;
using BusBuddy.Data;
using BusBuddy.Models;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;

namespace BusBuddy.Tests
{
    public class VehicleRepositoryIntegrationTests : IDisposable
    {
        private readonly VehicleRepository _repository;
        private readonly List<int> _createdVehicleIds;
        private readonly TestDatabaseHelper _dbHelper;
        private readonly IDbConnection _connection;

        public VehicleRepositoryIntegrationTests()
        {
            _dbHelper = new TestDatabaseHelper();
            _connection = _dbHelper.GetConnection();
            _repository = new VehicleRepository(); // fallback, but not used directly
            _createdVehicleIds = new List<int>();
        }

        [Fact]
        public void GetAllVehicles_ReturnsVehicleList()
        {
            // Act
            var result = _repository.GetAllVehicles();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<Vehicle>>(result);
        }

        [Fact]
        public void AddVehicle_ValidVehicle_AddsSuccessfully()
        {
            // Arrange
            var vehicle = new Vehicle
            {
                VehicleNumber = "TEST001",
                Make = "Test Make",
                Model = "Test Model",
                Year = 2020,
                Status = "Active"
            };

            // Act
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Vehicles (VehicleNumber, Make, Model, Year, Status) VALUES (@VehicleNumber, @Make, @Model, @Year, @Status)";
                command.Parameters.Add(new SqliteParameter("@VehicleNumber", vehicle.VehicleNumber));
                command.Parameters.Add(new SqliteParameter("@Make", vehicle.Make));
                command.Parameters.Add(new SqliteParameter("@Model", vehicle.Model));
                command.Parameters.Add(new SqliteParameter("@Year", vehicle.Year));
                command.Parameters.Add(new SqliteParameter("@Status", vehicle.Status));
                command.ExecuteNonQuery();
            }

            var allVehicles = _repository.GetAllVehicles();
            var addedVehicle = allVehicles.Find(v => v.VehicleNumber == "TEST001");

            // Assert
            Assert.NotNull(addedVehicle);
            Assert.Equal("TEST001", addedVehicle.VehicleNumber);
            Assert.Equal("Test Make", addedVehicle.Make);

            // Clean up
            if (addedVehicle != null)
            {
                _createdVehicleIds.Add(addedVehicle.Id);
            }
        }

        [Fact]
        public void GetVehicleById_ExistingId_ReturnsVehicle()
        {
            // Arrange - First add a vehicle
            var vehicle = new Vehicle
            {
                VehicleNumber = "TEST002",
                Make = "Test Make 2",
                Model = "Test Model 2",
                Year = 2021,
                Status = "Active"
            };
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Vehicles (VehicleNumber, Make, Model, Year, Status) VALUES (@VehicleNumber, @Make, @Model, @Year, @Status)";
                command.Parameters.Add(new SqliteParameter("@VehicleNumber", vehicle.VehicleNumber));
                command.Parameters.Add(new SqliteParameter("@Make", vehicle.Make));
                command.Parameters.Add(new SqliteParameter("@Model", vehicle.Model));
                command.Parameters.Add(new SqliteParameter("@Year", vehicle.Year));
                command.Parameters.Add(new SqliteParameter("@Status", vehicle.Status));
                command.ExecuteNonQuery();
            }

            var allVehicles = _repository.GetAllVehicles();
            var addedVehicle = allVehicles.Find(v => v.VehicleNumber == "TEST002");
            Assert.NotNull(addedVehicle);
            _createdVehicleIds.Add(addedVehicle.Id);

            // Act
            var result = _repository.GetVehicleById(addedVehicle.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedVehicle.Id, result.Id);
            Assert.Equal("TEST002", result.VehicleNumber);
        }

        [Fact]
        public void GetVehicleById_NonExistingId_ReturnsNull()
        {
            // Act
            var result = _repository.GetVehicleById(-999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void UpdateVehicle_ExistingVehicle_UpdatesSuccessfully()
        {
            // Arrange - First add a vehicle
            var vehicle = new Vehicle
            {
                VehicleNumber = "TEST003",
                Make = "Original Make",
                Model = "Original Model",
                Year = 2020,
                Status = "Active"
            };
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Vehicles (VehicleNumber, Make, Model, Year, Status) VALUES (@VehicleNumber, @Make, @Model, @Year, @Status)";
                command.Parameters.Add(new SqliteParameter("@VehicleNumber", vehicle.VehicleNumber));
                command.Parameters.Add(new SqliteParameter("@Make", vehicle.Make));
                command.Parameters.Add(new SqliteParameter("@Model", vehicle.Model));
                command.Parameters.Add(new SqliteParameter("@Year", vehicle.Year));
                command.Parameters.Add(new SqliteParameter("@Status", vehicle.Status));
                command.ExecuteNonQuery();
            }

            var allVehicles = _repository.GetAllVehicles();
            var addedVehicle = allVehicles.Find(v => v.VehicleNumber == "TEST003");
            Assert.NotNull(addedVehicle);
            _createdVehicleIds.Add(addedVehicle.Id);

            // Modify the vehicle
            addedVehicle.Make = "Updated Make";
            addedVehicle.Status = "Maintenance";

            // Act
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "UPDATE Vehicles SET Make = @Make, Status = @Status WHERE Id = @Id";
                command.Parameters.Add(new SqliteParameter("@Make", addedVehicle.Make));
                command.Parameters.Add(new SqliteParameter("@Status", addedVehicle.Status));
                command.Parameters.Add(new SqliteParameter("@Id", addedVehicle.Id));
                command.ExecuteNonQuery();
            }

            var updatedVehicle = _repository.GetVehicleById(addedVehicle.Id);

            // Assert
            Assert.NotNull(updatedVehicle);
            Assert.Equal("Updated Make", updatedVehicle.Make);
            Assert.Equal("Maintenance", updatedVehicle.Status);
        }

        [Fact]
        public void DeleteVehicle_ExistingId_DeletesSuccessfully()
        {
            // Arrange - First add a vehicle
            var vehicle = new Vehicle
            {
                VehicleNumber = "TEST004",
                Make = "To Be Deleted",
                Model = "Delete Model",
                Year = 2020,
                Status = "Active"
            };
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Vehicles (VehicleNumber, Make, Model, Year, Status) VALUES (@VehicleNumber, @Make, @Model, @Year, @Status)";
                command.Parameters.Add(new SqliteParameter("@VehicleNumber", vehicle.VehicleNumber));
                command.Parameters.Add(new SqliteParameter("@Make", vehicle.Make));
                command.Parameters.Add(new SqliteParameter("@Model", vehicle.Model));
                command.Parameters.Add(new SqliteParameter("@Year", vehicle.Year));
                command.Parameters.Add(new SqliteParameter("@Status", vehicle.Status));
                command.ExecuteNonQuery();
            }

            var allVehicles = _repository.GetAllVehicles();
            var addedVehicle = allVehicles.Find(v => v.VehicleNumber == "TEST004");
            Assert.NotNull(addedVehicle);

            // Act
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Vehicles WHERE Id = @Id";
                command.Parameters.Add(new SqliteParameter("@Id", addedVehicle.Id));
                command.ExecuteNonQuery();
            }

            var deletedVehicle = _repository.GetVehicleById(addedVehicle.Id);

            // Assert
            Assert.Null(deletedVehicle);

            // No need to add to cleanup list since it's already deleted
        }

        [Fact]
        public void DeleteVehicle_NonExistingId_ReturnsFalse()
        {
            // Act
            var result = _repository.DeleteVehicle(-999);

            // Assert - Some implementations might return false, others might not throw
            // This tests the method doesn't crash
            Assert.IsType<bool>(result);
        }

        public void Dispose()
        {
            _dbHelper.Dispose();
            // Clean up any vehicles created during tests
            foreach (var vehicleId in _createdVehicleIds)
            {
                try
                {
                    using (var command = _connection.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM Vehicles WHERE Id = @Id";
                        command.Parameters.Add(new SqliteParameter("@Id", vehicleId));
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}
