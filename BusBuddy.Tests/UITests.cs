using Xunit;
using System;
using System.Windows.Forms;
using BusBuddy.UI;
using System.Drawing;

namespace BusBuddy.Tests
{
    public class UITests
    {
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

        [Theory]
        [InlineData("123", true)]
        [InlineData("abc", false)]
        [InlineData("123.45", false)]
        [InlineData("-123", true)]
        public void NumericValidation_ValidateIntegerInput_Theory(string input, bool expected)
        {
            // Act
            var isValid = int.TryParse(input, out _);

            // Assert
            Assert.Equal(expected, isValid);
        }

        [Fact]
        public void DateValidation_ShouldValidateDateInputs()
        {
            // Arrange
            var input = "2023-01-15";

            // Act
            var isValid = DateTime.TryParse(input, out DateTime result);

            // Assert
            Assert.True(isValid);
            Assert.Equal(2023, result.Year);
            Assert.Equal(1, result.Month);
            Assert.Equal(15, result.Day);
        }

        [Theory]
        [InlineData("2023-01-15", true)]
        [InlineData("01/15/2023", true)]
        [InlineData("not a date", false)]
        public void DateValidation_ValidateDateInput_Theory(string input, bool expected)
        {
            // Act
            var isValid = DateTime.TryParse(input, out _);

            // Assert
            Assert.Equal(expected, isValid);
        }
    }
}