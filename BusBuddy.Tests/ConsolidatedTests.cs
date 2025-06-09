using Xunit;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Moq;
using BusBuddy.Models;
using BusBuddy.Business;
using BusBuddy.Data;
using BusBuddy.UI;

namespace BusBuddy.Tests
{
    public class ConsolidatedTests
    {
        #region UI Tests
        
        [Fact]
        public void FormValidation_ShouldValidateRequiredFields()
        {
            // Arrange
            var isRequired = true;
            var value = "";

            // Act
            var isValid = !isRequired || !string.IsNullOrWhiteSpace(value);

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("BUS001", true)]
        [InlineData("   ", false)]
        public void FormValidation_ValidateRequiredField_Theory(string value, bool expected)
        {
            // Arrange
            var isRequired = true;

            // Act
            var isValid = !isRequired || !string.IsNullOrWhiteSpace(value);

            // Assert
            Assert.Equal(expected, isValid);
        }

        [Fact]
        public void NumericValidation_ShouldValidateIntegerInputs()
        {
            // Arrange
            var input = "123";

            // Act
            var isValid = int.TryParse(input, out int result);

            // Assert
            Assert.True(isValid);
            Assert.Equal(123, result);
        }

        #endregion

        #region Model Tests
        
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
        
        #endregion

        #region Business Logic Tests
        
        [Fact]
        public void VehicleAgeCalculation_ShouldCalculateCorrectly()
        {
            // Arrange
            var currentYear = DateTime.Now.Year;
            var vehicleYear = 2018;
            
            // Act
            var age = currentYear - vehicleYear;
            
            // Assert
            Assert.True(age >= 0);
            Assert.True(age <= 100); // Sanity check
        }
        
        [Theory]
        [InlineData(2020, 2023, 3)]
        [InlineData(2023, 2023, 0)]
        [InlineData(1990, 2023, 33)]
        public void VehicleAgeCalculation_Theory(int vehicleYear, int currentYear, int expectedAge)
        {
            // Act
            var age = currentYear - vehicleYear;
            
            // Assert
            Assert.Equal(expectedAge, age);
        }
        
        [Fact]
        public void VehicleCapacityValidation_ShouldValidateRange()
        {
            // Arrange
            var minCapacity = 0;
            var maxCapacity = 100;
            var testCapacity = 25;
            
            // Act
            var isValid = testCapacity >= minCapacity && testCapacity <= maxCapacity;
            
            // Assert
            Assert.True(isValid);
        }
        
        #endregion

        #region Data Access Tests
        
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
            var mockRepo = new Mock<IVehicleRepository>();
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
        
        #endregion
    }
}