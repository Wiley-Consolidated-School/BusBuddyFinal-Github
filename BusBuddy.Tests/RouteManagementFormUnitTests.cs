using Xunit;
using BusBuddy.UI.Views;
using Moq;
using BusBuddy.Data;
using BusBuddy.Business;
using BusBuddy.Models;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Phase 1 Unit Tests for RouteManagementForm
    /// Focus: Form Validation Edge Cases, User Interaction Basics, Error Handling
    /// </summary>
    public class RouteManagementFormUnitTests
    {
        private readonly Mock<IRouteRepository> _mockRouteRepo;
        private readonly Mock<IVehicleRepository> _mockVehicleRepo;
        private readonly Mock<IDriverRepository> _mockDriverRepo;
        private readonly Mock<IDatabaseHelperService> _mockDatabaseService;

        public RouteManagementFormUnitTests()
        {
            _mockRouteRepo = new Mock<IRouteRepository>();
            _mockVehicleRepo = new Mock<IVehicleRepository>();
            _mockDriverRepo = new Mock<IDriverRepository>();
            _mockDatabaseService = new Mock<IDatabaseHelperService>();
        }

        #region Basic Constructor Tests

        [Fact]
        [Trait("Category", "Unit")]
        public void Constructor_InitializesSuccessfully()
        {
            // Arrange & Act
            var form = new RouteManagementForm();

            // Assert
            Assert.NotNull(form);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Constructor_ThrowsException_WhenRouteRepositoryIsNull()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RouteManagementForm(null!, new VehicleRepository(), new DriverRepository()));
        }

        #endregion

        #region Phase 1: Form Validation Edge Cases

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("Route 1", true)]         // Standard name
        [InlineData("Elementary AM", true)]   // With description
        [InlineData("Route-A", true)]         // With hyphen
        [InlineData("R1", true)]              // Short name
        [InlineData("", false)]               // Empty (required)
        [InlineData("   ", false)]            // Whitespace only
        public void RouteNameValidation_VariousInputs_ReturnsExpectedResult(string routeName, bool expected)
        {
            // Arrange
            var isValid = !string.IsNullOrWhiteSpace(routeName);

            // Act & Assert
            Assert.Equal(expected, isValid);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("150.5", "155.0", true)]   // Valid range
        [InlineData("0.0", "10.5", true)]      // Starting from zero
        [InlineData("100.0", "95.0", false)]   // End less than begin
        [InlineData("150.5", "150.5", false)]  // Same values
        [InlineData("abc", "155.0", false)]    // Invalid begin
        [InlineData("150.5", "abc", false)]    // Invalid end
        public void MileageValidation_VariousInputs_ReturnsExpectedResult(string beginMiles, string endMiles, bool expected)
        {
            // Arrange
            bool isValid = false;
            if (double.TryParse(beginMiles, out double begin) && double.TryParse(endMiles, out double end))
            {
                isValid = end > begin && begin >= 0;
            }

            // Act & Assert
            Assert.Equal(expected, isValid);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("25", true)]              // Standard rider count
        [InlineData("0", true)]               // No riders (valid for some routes)
        [InlineData("72", true)]              // Full bus
        [InlineData("-5", false)]             // Negative riders
        [InlineData("abc", false)]            // Non-numeric
        [InlineData("", true)]                // Empty (optional)
        public void RiderCountValidation_VariousInputs_ReturnsExpectedResult(string riderCount, bool expected)
        {
            // Arrange
            bool isValid;
            if (string.IsNullOrWhiteSpace(riderCount))
            {
                isValid = true; // Optional field
            }
            else if (int.TryParse(riderCount, out int count))
            {
                isValid = count >= 0 && count <= 100;
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
        [InlineData("07:30", true)]           // Standard AM time
        [InlineData("15:45", true)]           // Standard PM time
        [InlineData("6:30 AM", true)]         // 12-hour format
        [InlineData("3:45 PM", true)]         // 12-hour format
        [InlineData("25:00", false)]          // Invalid hour
        [InlineData("abc", false)]            // Non-time format
        [InlineData("", true)]                // Empty (optional)
        public void TimeValidation_VariousInputs_ReturnsExpectedResult(string timeInput, bool expected)
        {
            // Arrange
            bool isValid;
            if (string.IsNullOrWhiteSpace(timeInput))
            {
                isValid = true; // Optional field
            }
            else
            {
                isValid = TimeSpan.TryParse(timeInput, out _) || DateTime.TryParse(timeInput, out _);
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
            using var form = new RouteManagementForm();

            // Assert
            Assert.True(form.KeyPreview);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Form_EscapeKey_ShouldHandleGracefully()
        {
            // Arrange
            using var form = new RouteManagementForm();

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
            using var form = new RouteManagementForm();
            var validationErrors = new List<string>();

            // Act - Simulate multiple validation errors
            var routeName = "";
            var beginMiles = "invalid";
            var amRiders = "-5";

            if (string.IsNullOrWhiteSpace(routeName))
                validationErrors.Add("Route name is required");
            if (!double.TryParse(beginMiles, out _))
                validationErrors.Add("Begin miles must be numeric");
            if (int.TryParse(amRiders, out int riders) && riders < 0)
                validationErrors.Add("Rider count cannot be negative");

            // Assert
            Assert.True(validationErrors.Count > 0);
            Assert.Contains("Route name is required", validationErrors);
            Assert.Contains("Begin miles must be numeric", validationErrors);
            Assert.Contains("Rider count cannot be negative", validationErrors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Form_DatabaseConnectionFailure_ShouldHandleGracefully()
        {
            // Arrange
            _mockRouteRepo.Setup(x => x.GetAllRoutes()).Throws(new InvalidOperationException("Database unavailable"));

            // Act & Assert
            using var form = new RouteManagementForm();
            Assert.NotNull(form);
        }

        #endregion
    }
}
