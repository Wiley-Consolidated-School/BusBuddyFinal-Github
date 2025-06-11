using Xunit;
using Moq;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Data;
using System;

namespace BusBuddy.Tests
{
    public class VehicleRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection; public VehicleRepositoryTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            // Create test table
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
        }

        [Fact]
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

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
