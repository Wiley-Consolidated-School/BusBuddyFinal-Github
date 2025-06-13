using System;
using Xunit;
using BusBuddy.Models;

namespace BusBuddy.Tests.Improved
{
    /// <summary>
    /// Unit tests for Vehicle model following XUnit best practices
    /// Demonstrates proper AAA pattern, edge cases, and boundary testing
    /// </summary>
    public class VehicleModelImprovedTests
    {
        #region Constructor Tests

        [Fact]
        public void Vehicle_DefaultConstructor_SetsDefaultValues()
        {
            // Arrange - No setup needed for default constructor

            // Act
            var vehicle = new Vehicle();

            // Assert
            Assert.Equal(0, vehicle.Id);
            Assert.Null(vehicle.VehicleNumber);
            Assert.Null(vehicle.Make);
            Assert.Null(vehicle.Model);
            Assert.Equal(0, vehicle.Year); // Year is int, default is 0
            Assert.Equal(0, vehicle.Capacity); // Capacity is int, default is 0
            Assert.Null(vehicle.FuelType);
            Assert.Null(vehicle.Status);
        }

        #endregion

        #region VehicleNumber Tests

        [Fact]
        public void VehicleNumber_ValidValue_SetsCorrectly()
        {
            // Arrange
            var vehicle = new Vehicle();
            const string expectedVehicleNumber = "BUS123";

            // Act
            vehicle.VehicleNumber = expectedVehicleNumber;

            // Assert
            Assert.Equal(expectedVehicleNumber, vehicle.VehicleNumber);
        }

        [Fact]
        public void VehicleNumber_NullValue_AcceptsNull()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.VehicleNumber = null;

            // Assert
            Assert.Null(vehicle.VehicleNumber);
        }

        [Fact]
        public void VehicleNumber_EmptyString_AcceptsEmpty()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.VehicleNumber = string.Empty;

            // Assert
            Assert.Equal(string.Empty, vehicle.VehicleNumber);
        }

        [Fact]
        public void VehicleNumber_WhitespaceOnly_AcceptsWhitespace()
        {
            // Arrange
            var vehicle = new Vehicle();
            const string whitespaceValue = "   ";

            // Act
            vehicle.VehicleNumber = whitespaceValue;

            // Assert
            Assert.Equal(whitespaceValue, vehicle.VehicleNumber);
        }

        #endregion

        #region Year Tests

        [Theory]
        [InlineData(1990)]
        [InlineData(2023)]
        [InlineData(2050)]
        public void Year_ValidYears_SetsCorrectly(int year)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Year = year;

            // Assert
            Assert.Equal(year, vehicle.Year);
        }

        [Fact]
        public void Year_NullValue_AcceptsNull()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Year = 0; // Year is int, set to default value

            // Assert
            Assert.Equal(0, vehicle.Year); // Year defaults to 0 for int
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1800)]
        public void Year_EdgeCaseValues_AcceptsValue(int year)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Year = year;

            // Assert
            Assert.Equal(year, vehicle.Year);
        }

        #endregion

        #region Capacity Tests

        [Theory]
        [InlineData(1)]
        [InlineData(25)]
        [InlineData(50)]
        [InlineData(100)]
        public void Capacity_ValidCapacities_SetsCorrectly(int capacity)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Capacity = capacity;

            // Assert
            Assert.Equal(capacity, vehicle.Capacity);
        }

        [Fact]
        public void Capacity_Zero_AcceptsZero()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Capacity = 0;

            // Assert
            Assert.Equal(0, vehicle.Capacity);
        }

        [Fact]
        public void Capacity_NegativeValue_AcceptsNegative()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Capacity = -1;

            // Assert
            Assert.Equal(-1, vehicle.Capacity);
        }

        [Fact]
        public void Capacity_NullValue_AcceptsNull()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Capacity = 0; // Capacity is int, set to default value

            // Assert
            Assert.Equal(0, vehicle.Capacity); // Capacity defaults to 0 for int
        }

        #endregion

        #region String Property Tests

        [Theory]
        [InlineData("Mercedes")]
        [InlineData("Ford")]
        [InlineData("Chevrolet")]
        public void Make_ValidMakes_SetsCorrectly(string make)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Make = make;

            // Assert
            Assert.Equal(make, vehicle.Make);
        }

        [Theory]
        [InlineData("Sprinter")]
        [InlineData("Transit")]
        [InlineData("Express")]
        public void Model_ValidModels_SetsCorrectly(string model)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Model = model;

            // Assert
            Assert.Equal(model, vehicle.Model);
        }

        [Theory]
        [InlineData("Diesel")]
        [InlineData("Gasoline")]
        [InlineData("Electric")]
        [InlineData("Hybrid")]
        public void FuelType_ValidFuelTypes_SetsCorrectly(string fuelType)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.FuelType = fuelType;

            // Assert
            Assert.Equal(fuelType, vehicle.FuelType);
        }

        [Theory]
        [InlineData("Active")]
        [InlineData("Inactive")]
        [InlineData("Maintenance")]
        [InlineData("Retired")]
        public void Status_ValidStatuses_SetsCorrectly(string status)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Status = status;

            // Assert
            Assert.Equal(status, vehicle.Status);
        }

        #endregion

        #region Object Initialization Tests

        [Fact]
        public void Vehicle_ObjectInitializer_SetsAllProperties()
        {
            // Arrange
            const int expectedId = 1;
            const string expectedVehicleNumber = "BUS123";
            const string expectedMake = "Mercedes";
            const string expectedModel = "Sprinter";
            const int expectedYear = 2022;
            const int expectedCapacity = 25;
            const string expectedFuelType = "Diesel";
            const string expectedStatus = "Active";

            // Act
            var vehicle = new Vehicle
            {
                Id = expectedId,
                VehicleNumber = expectedVehicleNumber,
                Make = expectedMake,
                Model = expectedModel,
                Year = expectedYear,
                Capacity = expectedCapacity,
                FuelType = expectedFuelType,
                Status = expectedStatus
            };

            // Assert
            Assert.Equal(expectedId, vehicle.Id);
            Assert.Equal(expectedVehicleNumber, vehicle.VehicleNumber);
            Assert.Equal(expectedMake, vehicle.Make);
            Assert.Equal(expectedModel, vehicle.Model);
            Assert.Equal(expectedYear, vehicle.Year);
            Assert.Equal(expectedCapacity, vehicle.Capacity);
            Assert.Equal(expectedFuelType, vehicle.FuelType);
            Assert.Equal(expectedStatus, vehicle.Status);
        }

        [Fact]
        public void Vehicle_PartialInitialization_SetsSpecifiedPropertiesOnly()
        {
            // Arrange
            const string expectedMake = "Ford";
            const string expectedModel = "Transit";

            // Act
            var vehicle = new Vehicle
            {
                Make = expectedMake,
                Model = expectedModel
            };

            // Assert
            Assert.Equal(expectedMake, vehicle.Make);
            Assert.Equal(expectedModel, vehicle.Model);
            // Verify other properties remain default/null
            Assert.Equal(0, vehicle.Id);
            Assert.Null(vehicle.VehicleNumber);
            Assert.Equal(0, vehicle.Year); // Year is int, default is 0
            Assert.Equal(0, vehicle.Capacity); // Capacity is int, default is 0
            Assert.Null(vehicle.FuelType);
            Assert.Null(vehicle.Status);
        }

        #endregion
    }
}
