using System;
using Xunit;
using BusBuddy.Models;

namespace BusBuddy.Tests.Improved
{
    /// <summary>
    /// Demonstration of validation testing following the Employee validation example
    /// from the Medium article - proper boundary testing and edge cases
    /// </summary>
    public class VehicleValidationTests
    {
        #region VehicleNumber Validation Tests

        [Fact]
        public void ValidateVehicleNumber_NullValue_ReturnsFalse()
        {
            // Arrange
            var vehicleNumber = (string?)null;

            // Act
            var isValid = IsValidVehicleNumber(vehicleNumber);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateVehicleNumber_EmptyString_ReturnsFalse()
        {
            // Arrange
            var vehicleNumber = string.Empty;

            // Act
            var isValid = IsValidVehicleNumber(vehicleNumber);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateVehicleNumber_OnlyWhitespace_ReturnsFalse()
        {
            // Arrange
            var vehicleNumber = "   ";

            // Act
            var isValid = IsValidVehicleNumber(vehicleNumber);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateVehicleNumber_TwoCharacters_ReturnsFalse()
        {
            // Arrange
            var vehicleNumber = "AB";

            // Act
            var isValid = IsValidVehicleNumber(vehicleNumber);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateVehicleNumber_ThreeCharacters_ReturnsFalse()
        {
            // Arrange
            var vehicleNumber = "ABC";

            // Act
            var isValid = IsValidVehicleNumber(vehicleNumber);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateVehicleNumber_FourCharacters_ReturnsTrue()
        {
            // Arrange
            var vehicleNumber = "ABCD";

            // Act
            var isValid = IsValidVehicleNumber(vehicleNumber);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateVehicleNumber_TwentyCharacters_ReturnsTrue()
        {
            // Arrange
            var vehicleNumber = "ABCDEFGHIJ1234567890"; // 20 characters

            // Act
            var isValid = IsValidVehicleNumber(vehicleNumber);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateVehicleNumber_TwentyOneCharacters_ReturnsFalse()
        {
            // Arrange
            var vehicleNumber = "ABCDEFGHIJ12345678901"; // 21 characters

            // Act
            var isValid = IsValidVehicleNumber(vehicleNumber);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateVehicleNumber_ValidFormat_ReturnsTrue()
        {
            // Arrange
            var vehicleNumber = "BUS123";

            // Act
            var isValid = IsValidVehicleNumber(vehicleNumber);

            // Assert
            Assert.True(isValid);
        }

        #endregion

        #region Year Validation Tests

        [Fact]
        public void ValidateYear_Null_ReturnsFalse()
        {
            // Arrange
            int? year = null;

            // Act
            var isValid = IsValidYear(year);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateYear_1899_ReturnsFalse()
        {
            // Arrange
            int year = 1899;

            // Act
            var isValid = IsValidYear(year);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateYear_1900_ReturnsTrue()
        {
            // Arrange
            int year = 1900;

            // Act
            var isValid = IsValidYear(year);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateYear_CurrentYear_ReturnsTrue()
        {
            // Arrange
            int year = DateTime.Now.Year;

            // Act
            var isValid = IsValidYear(year);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateYear_NextYear_ReturnsTrue()
        {
            // Arrange
            int year = DateTime.Now.Year + 1;

            // Act
            var isValid = IsValidYear(year);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateYear_TwoYearsFromNow_ReturnsFalse()
        {
            // Arrange
            int year = DateTime.Now.Year + 2;

            // Act
            var isValid = IsValidYear(year);

            // Assert
            Assert.False(isValid);
        }

        #endregion

        #region Capacity Validation Tests

        [Fact]
        public void ValidateCapacity_Null_ReturnsFalse()
        {
            // Arrange
            int? capacity = null;

            // Act
            var isValid = IsValidCapacity(capacity);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateCapacity_Zero_ReturnsFalse()
        {
            // Arrange
            int capacity = 0;

            // Act
            var isValid = IsValidCapacity(capacity);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateCapacity_NegativeOne_ReturnsFalse()
        {
            // Arrange
            int capacity = -1;

            // Act
            var isValid = IsValidCapacity(capacity);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateCapacity_One_ReturnsTrue()
        {
            // Arrange
            int capacity = 1;

            // Act
            var isValid = IsValidCapacity(capacity);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateCapacity_OneHundred_ReturnsTrue()
        {
            // Arrange
            int capacity = 100;

            // Act
            var isValid = IsValidCapacity(capacity);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateCapacity_OneHundredOne_ReturnsFalse()
        {
            // Arrange
            int capacity = 101;

            // Act
            var isValid = IsValidCapacity(capacity);

            // Assert
            Assert.False(isValid);
        }

        #endregion

        #region Helper Methods (Simulated Validation Logic)

        /// <summary>
        /// Simulated validation method for vehicle numbers
        /// Rules: 4-20 characters, not null/empty/whitespace
        /// </summary>
        private static bool IsValidVehicleNumber(string? vehicleNumber)
        {
            if (string.IsNullOrWhiteSpace(vehicleNumber))
                return false;

            if (vehicleNumber.Length < 4)
                return false;

            if (vehicleNumber.Length > 20)
                return false;

            return true;
        }

        /// <summary>
        /// Simulated validation method for year
        /// Rules: Between 1900 and next year, not null
        /// </summary>
        private static bool IsValidYear(int? year)
        {
            if (!year.HasValue)
                return false;

            if (year < 1900)
                return false;

            if (year > DateTime.Now.Year + 1)
                return false;

            return true;
        }

        /// <summary>
        /// Simulated validation method for capacity
        /// Rules: Between 1 and 100, not null
        /// </summary>
        private static bool IsValidCapacity(int? capacity)
        {
            if (!capacity.HasValue)
                return false;

            if (capacity <= 0)
                return false;

            if (capacity > 100)
                return false;

            return true;
        }

        #endregion
    }
}
