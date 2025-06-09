using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;

namespace BusBuddy.Tests
{
    public class IntegrationTests
    {
        private readonly Mock<BusBuddy.Data.IVehicleRepository> _mockRepo;
        
        public IntegrationTests()
        {
            _mockRepo = new Mock<BusBuddy.Data.IVehicleRepository>();
        }
        
        [Fact]
        public void VehicleService_GetAllVehicles_ShouldReturnFilteredList()
        {
            // Arrange
            var vehicles = new List<BusBuddy.Models.Vehicle>
            {
                new BusBuddy.Models.Vehicle { Id = 1, VehicleNumber = "BUS001", Make = "Mercedes", Year = 2020, Status = "Active" },
                new BusBuddy.Models.Vehicle { Id = 2, VehicleNumber = "BUS002", Make = "Ford", Year = 2019, Status = "Maintenance" },
                new BusBuddy.Models.Vehicle { Id = 3, VehicleNumber = "BUS003", Make = "Mercedes", Year = 2021, Status = "Active" }
            };
            
            _mockRepo.Setup(repo => repo.GetAllVehicles()).Returns(vehicles);
            var service = new BusBuddy.Business.VehicleService2(_mockRepo.Object);
            
            // Act
            var filteredVehicles = service.GetActiveVehicles();
            
            // Assert
            Assert.Equal(2, filteredVehicles.Count);
            Assert.Equal("BUS001", filteredVehicles[0].VehicleNumber);
            Assert.Equal("BUS003", filteredVehicles[1].VehicleNumber);
        }
        
        [Fact]
        public void VehicleService_AddVehicle_ShouldValidateBeforeAdding()
        {
            // Arrange            _mockRepo.Setup(repo => repo.AddVehicle(It.IsAny<BusBuddy.Models.Vehicle>())).Returns(1);
            var service = new BusBuddy.Business.VehicleService2(_mockRepo.Object);
            
            // Act & Assert - Invalid vehicle number
            var invalidVehicle = new BusBuddy.Models.Vehicle { VehicleNumber = "B", Make = "Mercedes", Year = 2022 };
            Assert.False(service.AddVehicle(invalidVehicle));
              // Act & Assert - Valid vehicle
            var validVehicle = new BusBuddy.Models.Vehicle { VehicleNumber = "BUS004", Make = "Mercedes", Year = 2022 };
            Assert.True(service.AddVehicle(validVehicle));
            _mockRepo.Verify(repo => repo.AddVehicle(validVehicle), Times.Once);
        }
        
        [Fact]
        public void VehicleService_DeleteVehicle_ShouldReturnFalseForInvalidId()
        {            // Arrange
            _mockRepo.Setup(repo => repo.DeleteVehicle(It.Is<int>(id => id > 0))).Returns(true);
            var service = new BusBuddy.Business.VehicleService2(_mockRepo.Object);
            
            // Act & Assert
            Assert.False(service.DeleteVehicle(0)); // Invalid ID
            Assert.True(service.DeleteVehicle(1));  // Valid ID
            _mockRepo.Verify(repo => repo.DeleteVehicle(0), Times.Never);
            _mockRepo.Verify(repo => repo.DeleteVehicle(1), Times.Once);
        }
        
        [Fact]
        public void VehicleService_UpdateVehicle_ShouldValidateBeforeUpdating()
        {            // Arrange
            _mockRepo.Setup(repo => repo.UpdateVehicle(It.IsAny<BusBuddy.Models.Vehicle>())).Returns(true);
            var service = new BusBuddy.Business.VehicleService2(_mockRepo.Object);
            
            // Act & Assert - Invalid vehicle
            var invalidVehicle = new BusBuddy.Models.Vehicle { Id = 1, VehicleNumber = "", Year = 2022 };
            Assert.False(service.UpdateVehicle(invalidVehicle));
              // Act & Assert - Valid vehicle
            var validVehicle = new BusBuddy.Models.Vehicle { Id = 1, VehicleNumber = "BUS001", Year = 2022 };
            Assert.True(service.UpdateVehicle(validVehicle));
            _mockRepo.Verify(repo => repo.UpdateVehicle(validVehicle), Times.Once);
        }
    }
      // Test classes moved to proper project files
}