using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using BusBuddy.Business;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Comprehensive tests for the actual VehicleService business logic class
    /// Tests real business methods to improve code coverage significantly
    /// </summary>
    public class VehicleServiceBusinessTests
    {
        private readonly Mock<IVehicleRepository> _mockRepository;
        private readonly VehicleService _vehicleService;

        public VehicleServiceBusinessTests()
        {
            _mockRepository = new Mock<IVehicleRepository>();
            _vehicleService = new VehicleService(_mockRepository.Object);
        }

        [Theory]
        [InlineData("BUS001", true)]
        [InlineData("BUS", true)]
        [InlineData("AB", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("A", false)]
        [InlineData("SCHOOLBUS001", true)]
        public void IsValidVehicleNumber_WithVariousInputs_ShouldReturnExpectedResult(string input, bool expected)
        {
            // Act
            var result = _vehicleService.IsValidVehicleNumber(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(2020, 5)] // Current year is 2025
        [InlineData(2025, 0)]
        [InlineData(2010, 15)]
        [InlineData(1990, 35)]
        public void CalculateVehicleAge_WithVariousYears_ShouldReturnCorrectAge(int vehicleYear, int expectedAge)
        {
            // Act
            var result = _vehicleService.CalculateVehicleAge(vehicleYear);

            // Assert
            Assert.Equal(expectedAge, result);
        }

        [Fact]
        public void GetActiveVehicles_WithMixedStatusVehicles_ShouldReturnOnlyActiveOnes()
        {
            // Arrange
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, VehicleNumber = "BUS001", Status = "Active" },
                new Vehicle { Id = 2, VehicleNumber = "BUS002", Status = "Inactive" },
                new Vehicle { Id = 3, VehicleNumber = "BUS003", Status = "Active" },
                new Vehicle { Id = 4, VehicleNumber = "BUS004", Status = "Maintenance" },
                new Vehicle { Id = 5, VehicleNumber = "BUS005", Status = "Active" }
            };

            _mockRepository.Setup(r => r.GetAllVehicles()).Returns(allVehicles);

            // Act
            var result = _vehicleService.GetActiveVehicles();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.All(result, v => Assert.Equal("Active", v.Status));
            Assert.Contains(result, v => v.VehicleNumber == "BUS001");
            Assert.Contains(result, v => v.VehicleNumber == "BUS003");
            Assert.Contains(result, v => v.VehicleNumber == "BUS005");
        }

        [Fact]
        public void GetActiveVehicles_WithNoActiveVehicles_ShouldReturnEmptyList()
        {
            // Arrange
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, VehicleNumber = "BUS001", Status = "Inactive" },
                new Vehicle { Id = 2, VehicleNumber = "BUS002", Status = "Maintenance" }
            };

            _mockRepository.Setup(r => r.GetAllVehicles()).Returns(allVehicles);

            // Act
            var result = _vehicleService.GetActiveVehicles();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetActiveVehicles_WithEmptyRepository_ShouldReturnEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllVehicles()).Returns(new List<Vehicle>());

            // Act
            var result = _vehicleService.GetActiveVehicles();

            // Assert
            Assert.Empty(result);
            _mockRepository.Verify(r => r.GetAllVehicles(), Times.Once);
        }

        [Fact]
        public void AddVehicle_WithValidVehicle_ShouldReturnTrueAndCallRepository()
        {
            // Arrange
            var vehicle = new Vehicle
            {
                VehicleNumber = "BUS001",
                Make = "Ford",
                Model = "Transit",
                Year = 2023
            };

            // Act
            var result = _vehicleService.AddVehicle(vehicle);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.AddVehicle(vehicle), Times.Once);
        }

        [Theory]
        [InlineData("AB")] // Too short
        [InlineData("")] // Empty
        [InlineData(null)] // Null
        public void AddVehicle_WithInvalidVehicleNumber_ShouldReturnFalse(string invalidVehicleNumber)
        {
            // Arrange
            var vehicle = new Vehicle
            {
                VehicleNumber = invalidVehicleNumber,
                Make = "Ford",
                Model = "Transit"
            };

            // Act
            var result = _vehicleService.AddVehicle(vehicle);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.AddVehicle(It.IsAny<Vehicle>()), Times.Never);
        }

        [Fact]
        public void UpdateVehicle_WithValidVehicle_ShouldReturnRepositoryResult()
        {
            // Arrange
            var vehicle = new Vehicle
            {
                Id = 1,
                VehicleNumber = "BUS001",
                Make = "Ford",
                Model = "Transit",
                Year = 2023
            };

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
            var vehicle = new Vehicle
            {
                Id = 1,
                VehicleNumber = "AB", // Invalid - too short
                Make = "Ford",
                Model = "Transit"
            };

            // Act
            var result = _vehicleService.UpdateVehicle(vehicle);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.UpdateVehicle(It.IsAny<Vehicle>()), Times.Never);
        }

        [Fact]
        public void UpdateVehicle_WhenRepositoryReturnsFalse_ShouldReturnFalse()
        {
            // Arrange
            var vehicle = new Vehicle
            {
                Id = 1,
                VehicleNumber = "BUS001",
                Make = "Ford",
                Model = "Transit"
            };

            _mockRepository.Setup(r => r.UpdateVehicle(vehicle)).Returns(false);

            // Act
            var result = _vehicleService.UpdateVehicle(vehicle);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.UpdateVehicle(vehicle), Times.Once);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(100, true)]
        [InlineData(999, true)]
        public void DeleteVehicle_WithValidId_ShouldReturnRepositoryResult(int validId, bool repositoryResult)
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteVehicle(validId)).Returns(repositoryResult);

            // Act
            var result = _vehicleService.DeleteVehicle(validId);

            // Assert
            Assert.Equal(repositoryResult, result);
            _mockRepository.Verify(r => r.DeleteVehicle(validId), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void DeleteVehicle_WithInvalidId_ShouldReturnFalse(int invalidId)
        {
            // Act
            var result = _vehicleService.DeleteVehicle(invalidId);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.DeleteVehicle(It.IsAny<int>()), Times.Never);
        }
    }
}
