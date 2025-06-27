using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using BusBuddy.Data;
using BusBuddy.Models;

// Disable nullable reference types for testing null arguments
#nullable disable

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// Tests for VehicleRepository ensuring proper CRUD operations and data integrity.
    /// Tests cover database operations, validation, and error handling.
    /// </summary>
    public class VehicleRepositoryTest : IDisposable
    {
        private readonly VehicleRepository _repository;
        private readonly List<int> _testVehicleIds;

        public VehicleRepositoryTest()
        {
            _repository = new VehicleRepository();
            _testVehicleIds = new List<int>();
        }

        public void Dispose()
        {
            // Cleanup test vehicles
            foreach (var id in _testVehicleIds)
            {
                try
                {
                    _repository.DeleteVehicle(id);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        #region GetAllVehicles Tests

        [Fact]
        public void GetAllVehicles_ShouldReturnListOfVehicles()
        {
            // Act
            var vehicles = _repository.GetAllVehicles();

            // Assert
            Assert.NotNull(vehicles);
            Assert.IsType<List<Vehicle>>(vehicles);
        }

        #endregion

        #region GetVehicleById Tests

        [Fact]
        public void GetVehicleById_WithValidId_ShouldReturnVehicle()
        {
            // Arrange
            var testVehicle = CreateTestVehicle();
            var vehicleId = _repository.AddVehicle(testVehicle);
            _testVehicleIds.Add(vehicleId);

            // Act
            var result = _repository.GetVehicleById(vehicleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testVehicle.VehicleNumber, result.VehicleNumber);
            Assert.Equal(testVehicle.Make, result.Make);
            Assert.Equal(testVehicle.Model, result.Model);
        }

        [Fact]
        public void GetVehicleById_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = _repository.GetVehicleById(-1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetVehicleById_WithZeroId_ShouldReturnNull()
        {
            // Act
            var result = _repository.GetVehicleById(0);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddVehicle Tests

        [Fact]
        public void AddVehicle_WithValidVehicle_ShouldReturnPositiveId()
        {
            // Arrange
            var testVehicle = CreateTestVehicle();

            // Act
            var vehicleId = _repository.AddVehicle(testVehicle);
            _testVehicleIds.Add(vehicleId);

            // Assert
            Assert.True(vehicleId > 0);

            // Verify the vehicle was actually added
            var retrievedVehicle = _repository.GetVehicleById(vehicleId);
            Assert.NotNull(retrievedVehicle);
            Assert.Equal(testVehicle.VehicleNumber, retrievedVehicle.VehicleNumber);
        }

        [Fact]
        public void AddVehicle_WithNullVehicle_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _repository.AddVehicle(null));
        }

        #endregion

        #region UpdateVehicle Tests

        [Fact]
        public void UpdateVehicle_WithValidChanges_ShouldReturnTrue()
        {
            // Arrange
            var testVehicle = CreateTestVehicle();
            var vehicleId = _repository.AddVehicle(testVehicle);
            _testVehicleIds.Add(vehicleId);

            var vehicleToUpdate = _repository.GetVehicleById(vehicleId);
            vehicleToUpdate.Make = "Updated Make";
            vehicleToUpdate.Model = "Updated Model";

            // Act
            var result = _repository.UpdateVehicle(vehicleToUpdate);

            // Assert
            Assert.True(result);

            // Verify the changes were persisted
            var updatedVehicle = _repository.GetVehicleById(vehicleId);
            Assert.Equal("Updated Make", updatedVehicle.Make);
            Assert.Equal("Updated Model", updatedVehicle.Model);
        }

        [Fact]
        public void UpdateVehicle_WithNonExistentVehicle_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentVehicle = CreateTestVehicle();
            nonExistentVehicle.Id = 99999; // Non-existent ID

            // Act
            var result = _repository.UpdateVehicle(nonExistentVehicle);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region DeleteVehicle Tests

        [Fact]
        public void DeleteVehicle_WithExistingVehicle_ShouldReturnTrue()
        {
            // Arrange
            var testVehicle = CreateTestVehicle();
            var vehicleId = _repository.AddVehicle(testVehicle);

            // Act
            var result = _repository.DeleteVehicle(vehicleId);

            // Assert
            Assert.True(result);

            // Verify the vehicle was actually deleted
            var deletedVehicle = _repository.GetVehicleById(vehicleId);
            Assert.Null(deletedVehicle);
        }

        [Fact]
        public void DeleteVehicle_WithNonExistentId_ShouldReturnFalse()
        {
            // Act
            var result = _repository.DeleteVehicle(99999);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Helper Methods

        private Vehicle CreateTestVehicle()
        {
            var timestamp = DateTime.Now.Ticks;
            return new Vehicle
            {
                VehicleNumber = $"TEST{timestamp}",
                Make = "Test Make",
                Model = "Test Model",
                Year = 2023,
                SeatingCapacity = 72,
                FuelType = "Diesel",
                Status = "Active",
                VINNumber = $"TEST{timestamp}VIN",
                LicenseNumber = $"TEST{timestamp}LIC",
                DateLastInspection = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd")
            };
        }

        #endregion
    }
}
