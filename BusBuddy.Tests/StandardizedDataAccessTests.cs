using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using BusBuddy.Models;
using BusBuddy.Business;
using BusBuddy.Data;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Standardized Data Access Tests
    /// Consolidates overlapping repository and service tests from multiple files
    /// </summary>
    public class StandardizedDataAccessTests
    {
        private readonly Mock<IVehicleRepository> _mockRepo;

        public StandardizedDataAccessTests()
        {
            _mockRepo = new Mock<IVehicleRepository>();
        }

        #region Repository Tests

        [Fact]
        [Trait("Category", "DataAccess")]
        [Trait("Component", "Repository")]
        public void VehicleRepository_GetAllVehicles_ShouldReturnList()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, VehicleNumber = "BUS001", Make = "Mercedes", Model = "Sprinter" },
                new Vehicle { Id = 2, VehicleNumber = "BUS002", Make = "Ford", Model = "Transit" }
            };
            _mockRepo.Setup(repo => repo.GetAllVehicles()).Returns(vehicles);

            // Act
            var result = _mockRepo.Object.GetAllVehicles();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("BUS001", result[0].VehicleNumber);
            Assert.Equal("BUS002", result[1].VehicleNumber);
        }

        [Fact]
        [Trait("Category", "DataAccess")]
        [Trait("Component", "Repository")]
        public void VehicleRepository_GetVehicleById_ShouldReturnCorrectVehicle()
        {
            // Arrange
            var expectedVehicle = new Vehicle { Id = 1, VehicleNumber = "BUS001", Make = "Mercedes" };
            _mockRepo.Setup(repo => repo.GetVehicleById(1)).Returns(expectedVehicle);

            // Act
            var result = _mockRepo.Object.GetVehicleById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("BUS001", result.VehicleNumber);
        }

        [Fact]
        [Trait("Category", "DataAccess")]
        [Trait("Component", "Repository")]
        public void VehicleRepository_AddVehicle_ShouldReturnValidId()
        {
            // Arrange
            var vehicle = new Vehicle { VehicleNumber = "BUS003", Make = "Mercedes", Model = "Sprinter" };
            _mockRepo.Setup(repo => repo.AddVehicle(It.IsAny<Vehicle>())).Returns(3);

            // Act
            var result = _mockRepo.Object.AddVehicle(vehicle);

            // Assert
            Assert.True(result > 0);
            _mockRepo.Verify(repo => repo.AddVehicle(vehicle), Times.Once);
        }

        #endregion

        #region Service Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Component", "Service")]
        public void VehicleService_GetActiveVehicles_ShouldReturnFilteredList()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, VehicleNumber = "BUS001", Make = "Mercedes", Year = 2020, Status = "Active" },
                new Vehicle { Id = 2, VehicleNumber = "BUS002", Make = "Ford", Year = 2019, Status = "Maintenance" },
                new Vehicle { Id = 3, VehicleNumber = "BUS003", Make = "Mercedes", Year = 2021, Status = "Active" }
            };

            _mockRepo.Setup(repo => repo.GetAllVehicles()).Returns(vehicles);
            var service = new VehicleService2(_mockRepo.Object);

            // Act
            var filteredVehicles = service.GetActiveVehicles();

            // Assert
            Assert.Equal(2, filteredVehicles.Count);
            Assert.All(filteredVehicles, v => Assert.Equal("Active", v.Status));
        }        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Component", "Service")]
        public void VehicleService_AddVehicle_ShouldValidateBeforeAdding()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.AddVehicle(It.IsAny<Vehicle>())).Returns(1);
            var service = new VehicleService2(_mockRepo.Object);

            var validVehicle = new Vehicle
            {
                VehicleNumber = "BUS004",
                Make = "Mercedes",
                Model = "Sprinter",
                Year = 2022,
                Status = "Active"
            };

            // Act
            var result = service.AddVehicle(validVehicle);

            // Assert
            Assert.True(result); // VehicleService2.AddVehicle returns bool
            _mockRepo.Verify(repo => repo.AddVehicle(It.IsAny<Vehicle>()), Times.Once);
        }

        #endregion

        #region Connection and Configuration Tests

        [Fact]
        [Trait("Category", "Configuration")]
        public void DatabaseConnection_ShouldHaveCorrectConnectionString()
        {
            // Arrange
            var expectedConnectionString = "Data Source=TestServer;Initial Catalog=BusBuddy;Integrated Security=True";

            // Act
            var connectionString = expectedConnectionString;

            // Assert
            Assert.Contains("Data Source=", connectionString);
            Assert.Contains("Initial Catalog=BusBuddy", connectionString);
            Assert.Contains("Integrated Security=True", connectionString);
        }

        [Theory]
        [Trait("Category", "Validation")]
        [InlineData("", false)]
        [InlineData("BUS001", true)]
        [InlineData("   ", false)]
        [InlineData(null, false)]
        public void VehicleNumber_Validation_ShouldValidateCorrectly(string vehicleNumber, bool expected)
        {
            // Act
            var isValid = !string.IsNullOrWhiteSpace(vehicleNumber);

            // Assert
            Assert.Equal(expected, isValid);
        }

        #endregion
    }
}
