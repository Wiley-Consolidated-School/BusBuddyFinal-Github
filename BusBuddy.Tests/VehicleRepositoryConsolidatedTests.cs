using Xunit;
using BusBuddy.Data;
using BusBuddy.Models;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using System;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Consolidated Vehicle Repository Tests
    /// Combines unit tests (raw SQL operations) and integration tests (repository class)
    /// </summary>
    public class VehicleRepositoryConsolidatedTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly VehicleRepository _repository;
        private readonly TestDatabaseHelper _dbHelper;
        private readonly List<int> _createdVehicleIds;

        public VehicleRepositoryConsolidatedTests()
        {
            // Setup in-memory SQLite for unit tests
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            // Create test table for raw SQL tests
            _connection.Execute(@"
                CREATE TABLE Vehicles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    VehicleNumber TEXT NOT NULL,
                    Make TEXT NOT NULL,
                    Model TEXT NOT NULL,
                    Year INTEGER NOT NULL,
                    Mileage INTEGER NOT NULL,
                    Status TEXT NOT NULL
                )");

            // Setup for integration tests
            _dbHelper = new TestDatabaseHelper();
            _repository = new VehicleRepository();
            _createdVehicleIds = new List<int>();
        }

        #region Unit Tests - Raw SQL Operations

        [Fact]
        [Trait("Category", "Unit")]
        public void CreateVehicle_ShouldReturnId_WhenValidData()
        {
            // Arrange
            var vehicleData = new
            {
                VehicleNumber = "BUS001",
                Make = "Ford",
                Model = "Transit",
                Year = 2020,
                Mileage = 50000,
                Status = "Active"
            };

            // Act
            var id = _connection.QuerySingle<int>(@"
                INSERT INTO Vehicles (VehicleNumber, Make, Model, Year, Mileage, Status)
                VALUES (@VehicleNumber, @Make, @Model, @Year, @Mileage, @Status);
                SELECT last_insert_rowid();", vehicleData);

            // Assert
            Assert.True(id > 0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UpdateVehicle_ShouldUpdateRecord_WhenValidData()
        {
            // Arrange
            _connection.Execute(@"
                INSERT INTO Vehicles (VehicleNumber, Make, Model, Year, Mileage, Status)
                VALUES ('BUS001', 'Ford', 'Transit', 2020, 50000, 'Active')");

            // Act
            _connection.Execute(@"
                UPDATE Vehicles SET Mileage = 60000 WHERE VehicleNumber = 'BUS001'");

            // Assert
            var mileage = _connection.QuerySingle<int>("SELECT Mileage FROM Vehicles WHERE VehicleNumber = 'BUS001'");
            Assert.Equal(60000, mileage);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DeleteVehicle_ShouldRemoveRecord_WhenValidId()
        {
            // Arrange
            _connection.Execute(@"
                INSERT INTO Vehicles (VehicleNumber, Make, Model, Year, Mileage, Status)
                VALUES ('BUS001', 'Ford', 'Transit', 2020, 50000, 'Active')");

            // Act
            _connection.Execute("DELETE FROM Vehicles WHERE VehicleNumber = 'BUS001'");

            // Assert
            var count = _connection.QuerySingle<int>("SELECT COUNT(*) FROM Vehicles WHERE VehicleNumber = 'BUS001'");
            Assert.Equal(0, count);
        }

        #endregion

        #region Integration Tests - Repository Class

        [Fact]
        [Trait("Category", "Integration")]
        public void GetAllVehicles_ReturnsVehicleList()
        {
            // Act
            var result = _repository.GetAllVehicles();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<Vehicle>>(result);
        }

        [Fact]
        [Trait("Category", "Integration")]
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
            using (var connection = _dbHelper.GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO Vehicles (VehicleNumber, Make, Model, Year, Status)
                    VALUES (@VehicleNumber, @Make, @Model, @Year, @Status)";

                var param1 = command.CreateParameter();
                param1.ParameterName = "@VehicleNumber";
                param1.Value = vehicle.VehicleNumber;
                command.Parameters.Add(param1);

                var param2 = command.CreateParameter();
                param2.ParameterName = "@Make";
                param2.Value = vehicle.Make;
                command.Parameters.Add(param2);

                var param3 = command.CreateParameter();
                param3.ParameterName = "@Model";
                param3.Value = vehicle.Model;
                command.Parameters.Add(param3);

                var param4 = command.CreateParameter();
                param4.ParameterName = "@Year";
                param4.Value = vehicle.Year;
                command.Parameters.Add(param4);

                var param5 = command.CreateParameter();
                param5.ParameterName = "@Status";
                param5.Value = vehicle.Status;
                command.Parameters.Add(param5);

                var result = command.ExecuteNonQuery();

                // Assert
                Assert.Equal(1, result);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Repository_Constructor_InitializesSuccessfully()
        {
            // Act & Assert
            var repo = new VehicleRepository();
            Assert.NotNull(repo);
        }

        #endregion

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
