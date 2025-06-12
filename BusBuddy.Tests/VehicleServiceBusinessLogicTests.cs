using Xunit;
using Moq;
using BusBuddy.Business;
using BusBuddy.Data;
using BusBuddy.Models;
using System.Collections.Generic;

namespace BusBuddy.Tests
{
    public class VehicleServiceBusinessLogicTests
    {
        private readonly Mock<IVehicleRepository> _mockRepository;
        private readonly VehicleService _vehicleService;

        public VehicleServiceBusinessLogicTests()
        {
            _mockRepository = new Mock<IVehicleRepository>();
            _vehicleService = new VehicleService(_mockRepository.Object);
        }

        [Fact]
        public void IsValidVehicleNumber_ValidNumber_ReturnsTrue()
        {
            // Arrange
            var validNumber = "BUS001";

            // Act
            var result = _vehicleService.IsValidVehicleNumber(validNumber);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidVehicleNumber_NullNumber_ReturnsFalse()
        {
            // Act
            var result = _vehicleService.IsValidVehicleNumber(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidVehicleNumber_EmptyNumber_ReturnsFalse()
        {
            // Act
            var result = _vehicleService.IsValidVehicleNumber("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidVehicleNumber_TooShort_ReturnsFalse()
        {
            // Act
            var result = _vehicleService.IsValidVehicleNumber("AB");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CalculateVehicleAge_ValidYear_ReturnsCorrectAge()
        {
            // Arrange
            var vehicleYear = 2020;
            var expectedAge = System.DateTime.Now.Year - vehicleYear;

            // Act
            var result = _vehicleService.CalculateVehicleAge(vehicleYear);

            // Assert
            Assert.Equal(expectedAge, result);
        }

        [Fact]
        public void GetActiveVehicles_ReturnsOnlyActiveVehicles()
        {
            // Arrange
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, VehicleNumber = "BUS001", Status = "Active" },
                new Vehicle { Id = 2, VehicleNumber = "BUS002", Status = "Inactive" },
                new Vehicle { Id = 3, VehicleNumber = "BUS003", Status = "Active" },
                new Vehicle { Id = 4, VehicleNumber = "BUS004", Status = "Maintenance" }
            };

            _mockRepository.Setup(r => r.GetAllVehicles()).Returns(allVehicles);

            // Act
            var result = _vehicleService.GetActiveVehicles();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, v => Assert.Equal("Active", v.Status));
        }

        [Fact]
        public void AddVehicle_ValidVehicle_ReturnsTrue()
        {
            // Arrange
            var vehicle = new Vehicle { VehicleNumber = "BUS005", Status = "Active" };

            // Act
            var result = _vehicleService.AddVehicle(vehicle);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.AddVehicle(vehicle), Times.Once);
        }

        [Fact]
        public void AddVehicle_InvalidVehicleNumber_ReturnsFalse()
        {
            // Arrange
            var vehicle = new Vehicle { VehicleNumber = "AB", Status = "Active" };

            // Act
            var result = _vehicleService.AddVehicle(vehicle);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.AddVehicle(It.IsAny<Vehicle>()), Times.Never);
        }

        [Fact]
        public void UpdateVehicle_ValidVehicle_ReturnsRepositoryResult()
        {
            // Arrange
            var vehicle = new Vehicle { Id = 1, VehicleNumber = "BUS001", Status = "Active" };
            _mockRepository.Setup(r => r.UpdateVehicle(vehicle)).Returns(true);

            // Act
            var result = _vehicleService.UpdateVehicle(vehicle);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.UpdateVehicle(vehicle), Times.Once);
        }

        [Fact]
        public void UpdateVehicle_InvalidVehicleNumber_ReturnsFalse()
        {
            // Arrange
            var vehicle = new Vehicle { Id = 1, VehicleNumber = "", Status = "Active" };

            // Act
            var result = _vehicleService.UpdateVehicle(vehicle);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.UpdateVehicle(It.IsAny<Vehicle>()), Times.Never);
        }

        [Fact]
        public void DeleteVehicle_ValidId_ReturnsRepositoryResult()
        {
            // Arrange
            var vehicleId = 1;
            _mockRepository.Setup(r => r.DeleteVehicle(vehicleId)).Returns(true);

            // Act
            var result = _vehicleService.DeleteVehicle(vehicleId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteVehicle(vehicleId), Times.Once);
        }

        [Fact]
        public void DeleteVehicle_InvalidId_ReturnsFalse()
        {
            // Arrange
            var invalidId = 0;

            // Act
            var result = _vehicleService.DeleteVehicle(invalidId);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.DeleteVehicle(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void DeleteVehicle_NegativeId_ReturnsFalse()
        {
            // Arrange
            var invalidId = -1;

            // Act
            var result = _vehicleService.DeleteVehicle(invalidId);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.DeleteVehicle(It.IsAny<int>()), Times.Never);
        }
    }
}
