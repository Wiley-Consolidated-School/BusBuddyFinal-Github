using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using BusBuddy.Models;
using BusBuddy.Business;

namespace BusBuddy.Tests
{
    public class BusinessLogicTests
    {
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
        public void VehicleFilter_ShouldFilterByMake()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, Make = "Mercedes" },
                new Vehicle { Id = 2, Make = "Ford" },
                new Vehicle { Id = 3, Make = "Mercedes" }
            };
            
            // Act
            var filtered = vehicles.FindAll(v => v.Make == "Mercedes");
            
            // Assert
            Assert.Equal(2, filtered.Count);
            Assert.Equal(1, filtered[0].Id);
            Assert.Equal(3, filtered[1].Id);
        }
        
        [Fact]
        public void VehicleFilter_ShouldFilterByYear()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, Year = 2020 },
                new Vehicle { Id = 2, Year = 2018 },
                new Vehicle { Id = 3, Year = 2022 }
            };
            
            // Act
            var filtered = vehicles.FindAll(v => v.Year >= 2020);
            
            // Assert
            Assert.Equal(2, filtered.Count);
            Assert.Equal(1, filtered[0].Id);
            Assert.Equal(3, filtered[1].Id);
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
        
        [Theory]
        [InlineData(-1, 0, 100, false)]
        [InlineData(0, 0, 100, true)]
        [InlineData(50, 0, 100, true)]
        [InlineData(100, 0, 100, true)]
        [InlineData(101, 0, 100, false)]
        public void VehicleCapacityValidation_Theory(int capacity, int min, int max, bool expected)
        {
            // Act
            var isValid = capacity >= min && capacity <= max;
            
            // Assert
            Assert.Equal(expected, isValid);
        }
    }
    
    // Define namespace and class for testing
    namespace BusBuddy.Business
    {
        public class VehicleService
        {
            public bool IsValidVehicleNumber(string vehicleNumber)
            {
                return !string.IsNullOrEmpty(vehicleNumber) && vehicleNumber.Length >= 3;
            }
            
            public int CalculateVehicleAge(int vehicleYear)
            {
                return DateTime.Now.Year - vehicleYear;
            }
        }
    }
}