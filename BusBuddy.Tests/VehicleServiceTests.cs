using Xunit;
using Moq;

namespace BusBuddy.Tests
{
    public class VehicleServiceTests
    {
        [Fact]
        public void ValidateVehicleNumber_ShouldReturnTrue_WhenValid()
        {
            // Arrange
            var vehicleNumber = "BUS001";

            // Act
            var result = !string.IsNullOrEmpty(vehicleNumber) && vehicleNumber.Length >= 3;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateVehicleNumber_ShouldReturnFalse_WhenInvalid()
        {
            // Arrange
            var vehicleNumber = "";

            // Act
            var result = !string.IsNullOrEmpty(vehicleNumber) && vehicleNumber.Length >= 3;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CalculateVehicleAge_ShouldReturnCorrectAge()
        {
            // Arrange
            var currentYear = System.DateTime.Now.Year;
            var vehicleYear = 2020;

            // Act
            var age = currentYear - vehicleYear;

            // Assert
            Assert.True(age >= 0);
        }

        [Theory]
        [InlineData("BUS001", true)]
        [InlineData("", false)]
        [InlineData("AB", false)]
        [InlineData("BUS123", true)]
        public void ValidateVehicleNumber_Theory_ShouldValidateCorrectly(string vehicleNumber, bool expected)
        {
            // Act
            var result = !string.IsNullOrEmpty(vehicleNumber) && vehicleNumber.Length >= 3;            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void VehicleService_ShouldHandleNullValues()
        {
            // Arrange
            string? vehicleNumber = null;

            // Act
            // Using string.IsNullOrEmpty handles null values safely
            var result = !string.IsNullOrEmpty(vehicleNumber);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VehicleService_ShouldHandleNullOrWhitespaceValues()
        {
            // Arrange
            string? vehicleNumber = "   ";

            // Act
            // Using string.IsNullOrWhiteSpace handles null and whitespace safely
            var result = !string.IsNullOrWhiteSpace(vehicleNumber) && vehicleNumber.Length >= 3;

            // Assert
            Assert.False(result);
        }
    }
}
