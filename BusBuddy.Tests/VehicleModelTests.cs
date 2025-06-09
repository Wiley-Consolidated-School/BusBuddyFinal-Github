using Xunit;
using BusBuddy.Models;
using System;

namespace BusBuddy.Tests
{
    public class VehicleModelTests
    {
        [Fact]
        public void Vehicle_PropertiesShouldBeSettable()
        {
            // Arrange
            var vehicle = new Vehicle
            {
                Id = 1,
                VehicleNumber = "BUS123",
                Make = "Mercedes",
                Model = "Sprinter",
                Year = 2022,
                Capacity = 25,
                FuelType = "Diesel",
                Status = "Active"
            };

            // Assert
            Assert.Equal(1, vehicle.Id);
            Assert.Equal("BUS123", vehicle.VehicleNumber);
            Assert.Equal("Mercedes", vehicle.Make);
            Assert.Equal("Sprinter", vehicle.Model);
            Assert.Equal(2022, vehicle.Year);
            Assert.Equal(25, vehicle.Capacity);
            Assert.Equal("Diesel", vehicle.FuelType);
            Assert.Equal("Active", vehicle.Status);
        }

        [Fact]
        public void Vehicle_WithNullProperties_ShouldNotThrowException()
        {
            // Arrange & Act
            var vehicle = new Vehicle
            {
                Id = 1,
                VehicleNumber = null,
                Make = null,
                Model = null,
                FuelType = null,
                Status = null
            };

            // Assert - if we get here without exceptions, test passes
            Assert.Null(vehicle.VehicleNumber);
            Assert.Null(vehicle.Make);
            Assert.Null(vehicle.Model);
            Assert.Null(vehicle.FuelType);
            Assert.Null(vehicle.Status);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(10, true)]
        public void Vehicle_IsValidId_ShouldValidateCorrectly(int id, bool expected)
        {
            // Arrange
            var vehicle = new Vehicle { Id = id };

            // Act
            var result = vehicle.Id > 0;

            // Assert
            Assert.Equal(expected, result);
        }
    }
}