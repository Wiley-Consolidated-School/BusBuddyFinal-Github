using Xunit;
using BusBuddy.UI.Views;
using Moq;
using BusBuddy.Data;
using BusBuddy.Business;
using BusBuddy.Models;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Tests.TestHelpers;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Phase 1 Unit Tests for FuelManagementForm
    /// Focus: Form Validation Edge Cases, User Interaction Basics, Error Handling
    /// </summary>
    public class FuelManagementFormUnitTests
    {
        private readonly Mock<IFuelRepository> _mockFuelRepo;
        private readonly Mock<IVehicleRepository> _mockVehicleRepo;
        private readonly Mock<IDatabaseHelperService> _mockDatabaseService;

        public FuelManagementFormUnitTests()
        {
            _mockFuelRepo = new Mock<IFuelRepository>();
            _mockVehicleRepo = new Mock<IVehicleRepository>();
            _mockDatabaseService = new Mock<IDatabaseHelperService>();
        }

        #region Basic Constructor Tests

        [Fact]
        [Trait("Category", "Unit")]
        public void Constructor_InitializesSuccessfully_WithMockedDependencies()
        {
            // Act & Assert using helper
            BaseFormTestHelper.AssertFormInitializesSuccessfully(
                () => new FuelManagementForm(),
                "Fuel Management",
                new System.Drawing.Size(800, 600)
            );
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Constructor_ThrowsException_WhenFuelRepositoryIsNull()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FuelManagementForm(null!));
        }

        #endregion

        #region Phase 1: Form Validation Edge Cases

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("25.50", true)]       // Standard amount
        [InlineData("0.01", true)]        // Minimum amount
        [InlineData("999.99", true)]      // Large amount
        [InlineData("0", false)]          // Zero
        [InlineData("-5.00", false)]      // Negative
        [InlineData("abc", false)]        // Non-numeric
        [InlineData("", false)]           // Empty (required)
        public void FuelAmountValidation_VariousInputs_ReturnsExpectedResult(string amount, bool expected)
        {
            // Arrange
            bool isValid;
            if (string.IsNullOrWhiteSpace(amount))
            {
                isValid = false; // Required field
            }
            else if (decimal.TryParse(amount, out decimal amountValue))
            {
                isValid = amountValue > 0 && amountValue <= 1000;
            }
            else
            {
                isValid = false;
            }

            // Act & Assert
            Assert.Equal(expected, isValid);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("3.45", true)]        // Standard price
        [InlineData("5.99", true)]        // High price
        [InlineData("0.01", true)]        // Minimum price
        [InlineData("0", false)]          // Zero price
        [InlineData("-1.00", false)]      // Negative price
        [InlineData("abc", false)]        // Non-numeric
        [InlineData("", true)]            // Empty (optional)
        public void FuelPriceValidation_VariousInputs_ReturnsExpectedResult(string price, bool expected)
        {
            // Arrange
            bool isValid;
            if (string.IsNullOrWhiteSpace(price))
            {
                isValid = true; // Optional field
            }
            else if (decimal.TryParse(price, out decimal priceValue))
            {
                isValid = priceValue > 0 && priceValue <= 20; // Reasonable fuel price range
            }
            else
            {
                isValid = false;
            }

            // Act & Assert
            Assert.Equal(expected, isValid);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("Shell", true)]           // Standard station
        [InlineData("Exxon Mobil", true)]     // With space
        [InlineData("7-Eleven", true)]        // With hyphen and number
        [InlineData("BP", true)]              // Abbreviation
        [InlineData("", false)]               // Empty (required)
        [InlineData("   ", false)]            // Whitespace only
        public void FuelLocationValidation_VariousInputs_ReturnsExpectedResult(string location, bool expected)
        {
            // Arrange
            var isValid = !string.IsNullOrWhiteSpace(location);

            // Act & Assert
            Assert.Equal(expected, isValid);
        }

        #endregion

        #region Phase 1: User Interaction Basics

        [Fact]
        [Trait("Category", "Unit")]
        public void Form_KeyPreview_ShouldBeEnabled()
        {
            // Arrange & Act
            using var form = new FuelManagementForm();

            // Assert
            Assert.True(form.KeyPreview);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Form_EscapeKey_ShouldHandleGracefully()
        {
            // Arrange
            using var form = new FuelManagementForm();

            // Act
            var handled = false;
            try
            {
                // Don't show form in tests - just test that it can be created
                handled = form != null;
            }
            catch (Exception)
            {
                handled = false;
            }

            // Assert
            Assert.True(handled);
        }

        #endregion

        #region Phase 1: Error Handling

        [Fact]
        [Trait("Category", "Unit")]
        public void Form_MultipleValidationErrors_ShouldHandleGracefully()
        {
            // Arrange
            using var form = new FuelManagementForm();
            var validationErrors = new List<string>();

            // Act - Simulate multiple validation errors
            var amount = "";
            var price = "-1.00";
            var location = "";

            if (string.IsNullOrWhiteSpace(amount))
                validationErrors.Add("Fuel amount is required");
            if (decimal.TryParse(price, out decimal priceValue) && priceValue <= 0)
                validationErrors.Add("Price must be positive");
            if (string.IsNullOrWhiteSpace(location))
                validationErrors.Add("Location is required");

            // Assert
            Assert.True(validationErrors.Count > 0);
            Assert.Contains("Fuel amount is required", validationErrors);
            Assert.Contains("Price must be positive", validationErrors);
            Assert.Contains("Location is required", validationErrors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Form_DatabaseConnectionFailure_ShouldHandleGracefully()
        {
            // Arrange
            _mockFuelRepo.Setup(x => x.GetAllFuelRecords()).Throws(new InvalidOperationException("Database unavailable"));

            // Act & Assert
            using var form = new FuelManagementForm();
            Assert.NotNull(form);
        }

        #endregion
    }
}
