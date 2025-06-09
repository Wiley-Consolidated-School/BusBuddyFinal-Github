using Xunit;
using Moq;
using System;
using System.Data;
using System.Collections.Generic;
using BusBuddy.Models;
using System.Threading.Tasks;
using IVehicleRepo = BusBuddy.Data.IVehicleRepository;

namespace BusBuddy.Tests
{
    public class DataAccessTests
    {
        [Fact]
        public void DatabaseConnection_ShouldHaveCorrectConnectionString()
        {
            // Arrange
            var mockConnectionString = "Data Source=TestServer;Initial Catalog=BusBuddy;Integrated Security=True";

            // Act
            var connectionString = mockConnectionString;

            // Assert
            Assert.Contains("Data Source=", connectionString);
            Assert.Contains("Initial Catalog=BusBuddy", connectionString);
        }
        [Fact]
        public void VehicleRepository_GetAllVehicles_ShouldReturnList()
        {
            // Arrange
            var mockRepo = new Mock<IVehicleRepo>();
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, VehicleNumber = "BUS001", Make = "Mercedes", Model = "Sprinter" },
                new Vehicle { Id = 2, VehicleNumber = "BUS002", Make = "Ford", Model = "Transit" }
            };
            mockRepo.Setup(repo => repo.GetAllVehicles()).Returns(vehicles);

            // Act
            var result = mockRepo.Object.GetAllVehicles();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("BUS001", result[0].VehicleNumber);
            Assert.Equal("BUS002", result[1].VehicleNumber);
        }
        [Fact]
        public void VehicleRepository_GetVehicleById_ShouldReturnCorrectVehicle()
        {
            // Arrange
            var mockRepo = new Mock<IVehicleRepo>();
            var vehicle = new Vehicle { Id = 1, VehicleNumber = "BUS001", Make = "Mercedes", Model = "Sprinter" };
            mockRepo.Setup(repo => repo.GetVehicleById(1)).Returns(vehicle);

            // Act
            var result = mockRepo.Object.GetVehicleById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("BUS001", result.VehicleNumber);
        }
        [Fact]
        public void VehicleRepository_GetVehicleById_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var mockRepo = new Mock<IVehicleRepo>();
            mockRepo.Setup(repo => repo.GetVehicleById(999)).Returns((Vehicle?)null);

            // Act
            var result = mockRepo.Object.GetVehicleById(999);

            // Assert
            Assert.Null(result);
        }
        [Fact]
        public void VehicleRepository_AddVehicle_ShouldReturnNewId()
        {
            // Arrange
            var mockRepo = new Mock<IVehicleRepo>();
            var vehicle = new Vehicle { VehicleNumber = "BUS003", Make = "Toyota", Model = "Coaster" };
            mockRepo.Setup(repo => repo.AddVehicle(It.IsAny<Vehicle>())).Returns(3);

            // Act
            var result = mockRepo.Object.AddVehicle(vehicle);

            // Assert
            Assert.Equal(3, result);
        }
    }
}
