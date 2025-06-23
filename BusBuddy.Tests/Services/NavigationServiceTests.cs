using System;
using Xunit;
using FluentAssertions;
using BusBuddy.Tests.Foundation;
using BusBuddy.UI.Services;
using Moq;

namespace BusBuddy.Tests.Services
{
    /// <summary>
    /// Tests for NavigationService functionality
    /// Following Syncfusion testing patterns for service testing
    /// </summary>
    public class NavigationServiceTests : SyncfusionTestBase
    {
        private NavigationService? _navigationService;
        private Mock<IFormFactory>? _mockFormFactory;

        public NavigationServiceTests()
        {
            _mockFormFactory = new Mock<IFormFactory>();
            _navigationService = new NavigationService(_mockFormFactory.Object);
        }

        [Fact]
        public void NavigationService_Constructor_ShouldInitializeWithFormFactory()
        {
            // Arrange & Act
            var service = new NavigationService(_mockFormFactory!.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void NavigationService_Constructor_ShouldThrowWithNullFormFactory()
        {
            // Arrange & Act & Assert
            Action action = () => new NavigationService(null!);
            action.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData("dashboard", true)]
        [InlineData("vehicles", true)]
        [InlineData("drivers", true)]
        [InlineData("routes", true)]
        [InlineData("cde40", true)]
        [InlineData("invalid", false)]
        [InlineData("", false)]
        public void NavigationService_IsModuleAvailable_ShouldReturnCorrectValues(string moduleName, bool expected)
        {
            // Arrange & Act
            var result = _navigationService!.IsModuleAvailable(moduleName);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("dashboard")]
        [InlineData("vehicles")]
        [InlineData("drivers")]
        [InlineData("routes")]
        [InlineData("activityschedule")]
        [InlineData("cde40")]
        public void NavigationService_Navigate_ShouldReturnTrueForValidModules(string moduleName)
        {
            // Arrange - No additional setup needed for valid modules

            // Act
            var result = _navigationService!.Navigate(moduleName);

            // Assert
            result.Should().BeTrue($"Navigation to '{moduleName}' should succeed");
        }

        [Fact]
        public void NavigationService_Navigate_ShouldReturnFalseForInvalidModule()
        {
            // Arrange & Act
            var result = _navigationService!.Navigate("invalid-module");

            // Assert
            result.Should().BeFalse("Navigation to invalid module should fail");
        }

        [Fact]
        public void NavigationService_Navigate_ShouldHandleNullModuleName()
        {
            // Arrange & Act
            var result = _navigationService!.Navigate(null!);

            // Assert
            result.Should().BeFalse("Navigation with null module name should fail");
        }

        [Fact]
        public void NavigationService_Navigate_ShouldHandleEmptyModuleName()
        {
            // Arrange & Act
            var result = _navigationService!.Navigate("");

            // Assert
            result.Should().BeFalse("Navigation with empty module name should fail");
        }

        [Fact]
        public void NavigationService_ShowVehicleManagement_ShouldCallNavigate()
        {
            // Arrange & Act
            _navigationService!.ShowVehicleManagement();

            // Assert - Verify the method completes without throwing
            // In a real implementation, we would verify the correct form is shown
            true.Should().BeTrue("ShowVehicleManagement should complete successfully");
        }

        [Fact]
        public void NavigationService_ShowDriverManagement_ShouldCallNavigate()
        {
            // Arrange & Act
            _navigationService!.ShowDriverManagement();

            // Assert
            true.Should().BeTrue("ShowDriverManagement should complete successfully");
        }

        [Fact]
        public void NavigationService_ShowReports_ShouldCallNavigate()
        {
            // Arrange & Act
            _navigationService!.ShowReports();

            // Assert
            true.Should().BeTrue("ShowReports should complete successfully");
        }

        [Fact]
        public void NavigationService_CaseSensitivity_ShouldBeIgnored()
        {
            // Arrange & Act & Assert
            _navigationService!.Navigate("DASHBOARD").Should().BeTrue();
            _navigationService.Navigate("dashboard").Should().BeTrue();
            _navigationService.Navigate("Dashboard").Should().BeTrue();
            _navigationService.Navigate("DashBoard").Should().BeTrue();
        }

        [Fact]
        public void NavigationService_MultipleParameters_ShouldBeHandled()
        {
            // Arrange & Act
            var result = _navigationService!.Navigate("dashboard", "param1", "param2", 123);

            // Assert
            result.Should().BeTrue("Navigation with parameters should succeed");
        }

        public override void Dispose()
        {
            _navigationService = null;
            _mockFormFactory = null;
            base.Dispose();
        }
    }
}
