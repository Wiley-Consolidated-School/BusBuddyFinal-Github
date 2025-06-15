using Xunit;
using Moq;
using BusBuddy.Business;
using BusBuddy.Models;
using BusBuddy.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Comprehensive test class for VehicleService
    /// Tests all business logic and validation scenarios
    /// </summary>
    public class VehicleServiceTests
    {
        private readonly Mock<IVehicleRepository> _mockRepository;
        private readonly VehicleService _vehicleService;

        public VehicleServiceTests()
        {
            _mockRepository = new Mock<IVehicleRepository>();
            _vehicleService = new VehicleService(_mockRepository.Object);
        }

        #region Vehicle Number Validation Tests

        [Theory]
        [InlineData("123", true)]
        [InlineData("ABC", true)]
        [InlineData("12345", true)]
        [InlineData("BUS001", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("12", false)]
        [InlineData("AB", false)]
        public void IsValidVehicleNumber_ShouldValidateCorrectly(string vehicleNumber, bool expected)
        {
            // Act
            var result = _vehicleService.IsValidVehicleNumber(vehicleNumber);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region Vehicle Age Calculation Tests

        [Fact]
        public void CalculateVehicleAge_ShouldReturnCorrectAge()
        {
            // Arrange
            var currentYear = DateTime.Now.Year;
            var vehicleYear = currentYear - 5;

            // Act
            var age = _vehicleService.CalculateVehicleAge(vehicleYear);

            // Assert
            Assert.Equal(5, age);
        }

        [Fact]
        public void CalculateVehicleAge_ForCurrentYear_ShouldReturnZero()
        {
            // Arrange
            var currentYear = DateTime.Now.Year;

            // Act
            var age = _vehicleService.CalculateVehicleAge(currentYear);

            // Assert
            Assert.Equal(0, age);
        }

        [Fact]
        public void CalculateVehicleAge_ForFutureYear_ShouldReturnNegativeAge()
        {
            // Arrange
            var futureYear = DateTime.Now.Year + 2;

            // Act
            var age = _vehicleService.CalculateVehicleAge(futureYear);

            // Assert
            Assert.Equal(-2, age);
        }

        #endregion

        #region GetActiveVehicles Tests

        [Fact]
        public void GetActiveVehicles_ShouldReturnOnlyActiveVehicles()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle { VehicleID = 1, VehicleNumber = "BUS001", Status = "Active" },
                new Vehicle { VehicleID = 2, VehicleNumber = "BUS002", Status = "Inactive" },
                new Vehicle { VehicleID = 3, VehicleNumber = "BUS003", Status = "Active" },
                new Vehicle { VehicleID = 4, VehicleNumber = "BUS004", Status = "Maintenance" }
            };

            _mockRepository.Setup(r => r.GetAllVehicles()).Returns(vehicles);

            // Act
            var activeVehicles = _vehicleService.GetActiveVehicles();

            // Assert
            Assert.Equal(2, activeVehicles.Count);
            Assert.All(activeVehicles, v => Assert.Equal("Active", v.Status));
            Assert.Contains(activeVehicles, v => v.VehicleNumber == "BUS001");
            Assert.Contains(activeVehicles, v => v.VehicleNumber == "BUS003");
        }

        [Fact]
        public void GetActiveVehicles_WithNoActiveVehicles_ShouldReturnEmptyList()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle { VehicleID = 1, VehicleNumber = "BUS001", Status = "Inactive" },
                new Vehicle { VehicleID = 2, VehicleNumber = "BUS002", Status = "Maintenance" }
            };

            _mockRepository.Setup(r => r.GetAllVehicles()).Returns(vehicles);

            // Act
            var activeVehicles = _vehicleService.GetActiveVehicles();

            // Assert
            Assert.Empty(activeVehicles);
        }

        [Fact]
        public void GetActiveVehicles_WithEmptyRepository_ShouldReturnEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllVehicles()).Returns(new List<Vehicle>());

            // Act
            var activeVehicles = _vehicleService.GetActiveVehicles();

            // Assert
            Assert.Empty(activeVehicles);
        }

        #endregion

        #region AddVehicle Tests

        [Fact]
        public void AddVehicle_WithValidVehicleNumber_ShouldReturnTrue()
        {
            // Arrange
            var vehicle = new Vehicle { VehicleNumber = "BUS001", Status = "Active" };

            // Act
            var result = _vehicleService.AddVehicle(vehicle);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.AddVehicle(vehicle), Times.Once);
        }

        [Fact]
        public void AddVehicle_WithInvalidVehicleNumber_ShouldReturnFalse()
        {
            // Arrange
            var vehicle = new Vehicle { VehicleNumber = "12", Status = "Active" }; // Too short

            // Act
            var result = _vehicleService.AddVehicle(vehicle);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.AddVehicle(It.IsAny<Vehicle>()), Times.Never);
        }

        [Fact]
        public void AddVehicle_WithNullVehicleNumber_ShouldReturnFalse()
        {
            // Arrange
            var vehicle = new Vehicle { VehicleNumber = null, Status = "Active" };

            // Act
            var result = _vehicleService.AddVehicle(vehicle);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.AddVehicle(It.IsAny<Vehicle>()), Times.Never);
        }

        [Fact]
        public void AddVehicle_WithEmptyVehicleNumber_ShouldReturnFalse()
        {
            // Arrange
            var vehicle = new Vehicle { VehicleNumber = "", Status = "Active" };

            // Act
            var result = _vehicleService.AddVehicle(vehicle);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.AddVehicle(It.IsAny<Vehicle>()), Times.Never);
        }

        #endregion

        #region UpdateVehicle Tests

        [Fact]
        public void UpdateVehicle_WithValidVehicleNumber_ShouldReturnRepositoryResult()
        {
            // Arrange
            var vehicle = new Vehicle { VehicleID = 1, VehicleNumber = "BUS001", Status = "Active" };
            _mockRepository.Setup(r => r.UpdateVehicle(vehicle)).Returns(true);

            // Act
            var result = _vehicleService.UpdateVehicle(vehicle);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.UpdateVehicle(vehicle), Times.Once);
        }

        [Fact]
        public void UpdateVehicle_WithInvalidVehicleNumber_ShouldReturnFalse()
        {
            // Arrange
            var vehicle = new Vehicle { VehicleID = 1, VehicleNumber = "AB", Status = "Active" }; // Too short

            // Act
            var result = _vehicleService.UpdateVehicle(vehicle);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.UpdateVehicle(It.IsAny<Vehicle>()), Times.Never);
        }

        [Fact]
        public void UpdateVehicle_WhenRepositoryFails_ShouldReturnFalse()
        {
            // Arrange
            var vehicle = new Vehicle { VehicleID = 1, VehicleNumber = "BUS001", Status = "Active" };
            _mockRepository.Setup(r => r.UpdateVehicle(vehicle)).Returns(false);

            // Act
            var result = _vehicleService.UpdateVehicle(vehicle);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.UpdateVehicle(vehicle), Times.Once);
        }

        #endregion

        #region Additional Vehicle Business Logic Tests

        [Fact]
        public void VehicleService_ShouldHandleRepositoryExceptions()
        {
            // Arrange
            var vehicle = new Vehicle { VehicleNumber = "BUS001", Status = "Active" };
            _mockRepository.Setup(r => r.AddVehicle(vehicle)).Throws(new Exception("Database error"));

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _vehicleService.AddVehicle(vehicle));
            Assert.Equal("Database error", exception.Message);
        }

        [Theory]
        [InlineData("ACTIVE")]
        [InlineData("active")]
        [InlineData("Active")]
        [InlineData("INACTIVE")]
        [InlineData("inactive")]
        [InlineData("Maintenance")]
        public void GetActiveVehicles_ShouldBeCaseSensitive(string status)
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle { VehicleID = 1, VehicleNumber = "BUS001", Status = status }
            };

            _mockRepository.Setup(r => r.GetAllVehicles()).Returns(vehicles);

            // Act
            var activeVehicles = _vehicleService.GetActiveVehicles();

            // Assert
            if (status == "Active")
            {
                Assert.Single(activeVehicles);
            }
            else
            {
                Assert.Empty(activeVehicles);
            }
        }

        #endregion
    }
}
