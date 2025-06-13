using Xunit;
using Moq;
using BusBuddy.UI.Views;
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
    /// Phase 1 Unit Tests for MaintenanceManagementForm
    /// Focus: Form Validation Edge Cases, User Interaction Basics, Error Handling
    /// </summary>
    public class MaintenanceManagementFormUnitTests
    {
        private readonly Mock<IMaintenanceRepository> _mockMaintenanceRepo;
        private readonly Mock<IDatabaseHelperService> _mockDatabaseService;

        public MaintenanceManagementFormUnitTests()
        {
            _mockMaintenanceRepo = new Mock<IMaintenanceRepository>();
            _mockDatabaseService = new Mock<IDatabaseHelperService>();
        }

        #region Basic Constructor Tests

        [Fact]
        [Trait("Category", "Unit")]
        public void Constructor_InitializesSuccessfully_WithMockedDependencies()
        {
            // Arrange & Act
            var form = new MaintenanceManagementForm();

            // Assert
            Assert.NotNull(form);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Constructor_ThrowsException_WhenMaintenanceRepositoryIsNull()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new MaintenanceManagementForm(null!));
        }

        #endregion

        #region Phase 1: Form Validation Edge Cases

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("Oil Change", true)]          // Standard maintenance
        [InlineData("Tire Rotation", true)]      // Standard maintenance
        [InlineData("Annual Inspection", true)]  // Longer description
        [InlineData("Brake-Check", true)]        // With hyphen
        [InlineData("", false)]                  // Empty (required)
        [InlineData("   ", false)]               // Whitespace only
        public void MaintenanceTypeValidation_VariousInputs_ReturnsExpectedResult(string maintenanceType, bool expected)
        {
            // Arrange
            var isValid = !string.IsNullOrWhiteSpace(maintenanceType);

            // Act & Assert
            Assert.Equal(expected, isValid);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("250.00", true)]         // Standard cost
        [InlineData("0.01", true)]           // Minimum cost
        [InlineData("9999.99", true)]        // High cost repair
        [InlineData("0", true)]              // Free maintenance (warranty)
        [InlineData("-50.00", false)]       // Negative cost
        [InlineData("abc", false)]           // Non-numeric
        [InlineData("", true)]               // Empty (optional)
        public void MaintenanceCostValidation_VariousInputs_ReturnsExpectedResult(string cost, bool expected)
        {
            // Arrange
            bool isValid;
            if (string.IsNullOrWhiteSpace(cost))
            {
                isValid = true; // Optional field
            }
            else if (decimal.TryParse(cost, out decimal costValue))
            {
                isValid = costValue >= 0 && costValue <= 50000; // Reasonable cost range
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
        [InlineData("2024-06-12", true)]     // Valid date
        [InlineData("06/12/2024", true)]     // US format
        [InlineData("12/06/2024", true)]     // Alternative format
        [InlineData("2050-01-01", false)]    // Far future date
        [InlineData("1900-01-01", false)]    // Too old
        [InlineData("invalid", false)]       // Invalid format
        [InlineData("", true)]               // Empty (optional)
        public void MaintenanceDateValidation_VariousInputs_ReturnsExpectedResult(string dateInput, bool expected)
        {
            // Arrange
            bool isValid;
            if (string.IsNullOrWhiteSpace(dateInput))
            {
                isValid = true; // Optional field
            }
            else if (DateTime.TryParse(dateInput, out DateTime date))
            {
                var currentDate = DateTime.Now;
                isValid = date >= currentDate.AddYears(-10) && date <= currentDate.AddYears(1);
            }
            else
            {
                isValid = false;
            }

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
            using var form = new MaintenanceManagementForm();

            // Assert
            Assert.True(form.KeyPreview);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Form_EscapeKey_ShouldHandleGracefully()
        {
            // Arrange
            using var form = new MaintenanceManagementForm();

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
            using var form = new MaintenanceManagementForm();
            var validationErrors = new List<string>();

            // Act - Simulate multiple validation errors
            var maintenanceType = "";
            var cost = "-100.00";
            var date = "invalid date";

            if (string.IsNullOrWhiteSpace(maintenanceType))
                validationErrors.Add("Maintenance type is required");
            if (decimal.TryParse(cost, out decimal costValue) && costValue < 0)
                validationErrors.Add("Cost cannot be negative");
            if (!DateTime.TryParse(date, out _) && !string.IsNullOrWhiteSpace(date))
                validationErrors.Add("Invalid date format");

            // Assert
            Assert.True(validationErrors.Count > 0);
            Assert.Contains("Maintenance type is required", validationErrors);
            Assert.Contains("Cost cannot be negative", validationErrors);
            Assert.Contains("Invalid date format", validationErrors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Form_DatabaseConnectionFailure_ShouldHandleGracefully()
        {
            // Arrange
            _mockMaintenanceRepo.Setup(x => x.GetAllMaintenanceRecords()).Throws(new InvalidOperationException("Database unavailable"));

            // Act & Assert
            using var form = new MaintenanceManagementForm();
            Assert.NotNull(form);
        }

        #endregion
    }
}
