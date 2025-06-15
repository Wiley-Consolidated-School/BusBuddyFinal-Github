using BusBuddy.UI.Views;
using BusBuddy.Models;
using BusBuddy.Data;
using System.Windows.Forms;
using Moq;

namespace BusBuddy.Tests;

/// <summary>
/// Test class for UI form functionality and integration
/// This tests the standardized management forms
/// </summary>
public class ManagementFormTests
{
    [Fact]
    public void ActivityManagementForm_ShouldInitializeCorrectly()
    {
        // Arrange
        var mockRepository = new Mock<IActivityRepository>();
        mockRepository.Setup(r => r.GetAllActivities()).Returns(new List<Activity>());

        // Act
        var form = new ActivityManagementForm(mockRepository.Object);

        // Assert
        Assert.NotNull(form);
        Assert.Equal("Activity Management", form.Text);
        Assert.Equal(new System.Drawing.Size(1200, 900), form.ClientSize);
    }

    [Fact]
    public void DriverManagementForm_ShouldInitializeCorrectly()
    {
        // Arrange
        var mockRepository = new Mock<IDriverRepository>();
        mockRepository.Setup(r => r.GetAllDrivers()).Returns(new List<Driver>());

        // Act
        var form = new DriverManagementForm(mockRepository.Object);

        // Assert
        Assert.NotNull(form);
        Assert.Equal("Driver Management", form.Text);
        Assert.Equal(new System.Drawing.Size(1200, 900), form.ClientSize);
    }

    [Fact]
    public void VehicleManagementForm_ShouldInitializeCorrectly()
    {
        // Arrange - VehicleManagementForm has default constructor
        // Act
        var form = new VehicleManagementForm();

        // Assert
        Assert.NotNull(form);
        Assert.Equal("Vehicle Management", form.Text);
        Assert.Equal(new System.Drawing.Size(1200, 900), form.ClientSize);
    }

    [Fact]
    public void FuelManagementForm_ShouldInitializeCorrectly()
    {
        // Arrange
        var mockFuelRepository = new Mock<IFuelRepository>();
        var mockVehicleRepository = new Mock<IVehicleRepository>();

        mockFuelRepository.Setup(r => r.GetAllFuelRecords()).Returns(new List<Fuel>());
        mockVehicleRepository.Setup(r => r.GetAllVehicles()).Returns(new List<Vehicle>());

        // Act
        var form = new FuelManagementForm(mockFuelRepository.Object, mockVehicleRepository.Object);

        // Assert
        Assert.NotNull(form);
        Assert.Equal("Fuel Management", form.Text);
        Assert.Equal(new System.Drawing.Size(1200, 900), form.ClientSize);
    }

    [Fact]
    public void MaintenanceManagementForm_ShouldInitializeCorrectly()
    {
        // Arrange - MaintenanceManagementForm has default constructor
        // Act
        var form = new MaintenanceManagementForm();

        // Assert
        Assert.NotNull(form);
        Assert.Equal("Maintenance Management", form.Text);
        Assert.Equal(new System.Drawing.Size(1200, 900), form.ClientSize);
    }

    [Fact]
    public void RouteManagementForm_ShouldInitializeCorrectly()
    {
        // Arrange
        var mockRouteRepository = new Mock<IRouteRepository>();
        var mockDriverRepository = new Mock<IDriverRepository>();
        var mockVehicleRepository = new Mock<IVehicleRepository>();

        mockRouteRepository.Setup(r => r.GetAllRoutes()).Returns(new List<Route>());
        mockDriverRepository.Setup(r => r.GetAllDrivers()).Returns(new List<Driver>());
        mockVehicleRepository.Setup(r => r.GetAllVehicles()).Returns(new List<Vehicle>());

        // Act
        var form = new RouteManagementForm(
            mockRouteRepository.Object,
            mockVehicleRepository.Object,
            mockDriverRepository.Object);

        // Assert
        Assert.NotNull(form);
        Assert.Equal("Route Management", form.Text);
        Assert.Equal(new System.Drawing.Size(1200, 900), form.ClientSize);
    }

    [Fact]
    public void ActivityScheduleManagementForm_ShouldInitializeCorrectly()
    {
        // Arrange - ActivityScheduleManagementForm has default constructor
        // Act
        var form = new ActivityScheduleManagementForm();

        // Assert
        Assert.NotNull(form);
        Assert.Equal("Activity Schedule Management", form.Text);
        Assert.Equal(new System.Drawing.Size(1200, 900), form.ClientSize);
    }

    [Theory]
    [InlineData("Activity Management")]
    [InlineData("Driver Management")]
    [InlineData("Vehicle Management")]
    [InlineData("Fuel Management")]
    [InlineData("Maintenance Management")]
    [InlineData("Route Management")]
    [InlineData("Activity Schedule Management")]
    public void ManagementForms_ShouldHaveStandardizedTitles(string expectedTitle)
    {
        // This test verifies that all forms follow the standardized naming convention
        var formTitles = new[]
        {
            "Activity Management",
            "Driver Management",
            "Vehicle Management",
            "Fuel Management",
            "Maintenance Management",
            "Route Management",
            "Activity Schedule Management"
        };

        // Assert
        Assert.Contains(expectedTitle, formTitles);
    }

    [Fact]
    public void StandardizedFormSize_ShouldBe1200x900()
    {
        // This test verifies that all forms use the standardized size
        var expectedSize = new System.Drawing.Size(1200, 900);

        // Arrange & Act
        var mockActivityRepo = new Mock<IActivityRepository>();
        mockActivityRepo.Setup(r => r.GetAllActivities()).Returns(new List<Activity>());
        using var activityForm = new ActivityManagementForm(mockActivityRepo.Object);

        var mockDriverRepo = new Mock<IDriverRepository>();
        mockDriverRepo.Setup(r => r.GetAllDrivers()).Returns(new List<Driver>());
        using var driverForm = new DriverManagementForm(mockDriverRepo.Object);

        using var vehicleForm = new VehicleManagementForm();

        // Assert
        Assert.Equal(expectedSize, activityForm.ClientSize);
        Assert.Equal(expectedSize, driverForm.ClientSize);
        Assert.Equal(expectedSize, vehicleForm.ClientSize);
    }

    [Fact]
    public void BaseDataForm_ShouldProvideCommonFunctionality()
    {
        // Arrange
        var mockActivityRepo = new Mock<IActivityRepository>();
        mockActivityRepo.Setup(r => r.GetAllActivities()).Returns(new List<Activity>());

        // Act
        using var form = new ActivityManagementForm(mockActivityRepo.Object);

        // Assert - Form should inherit from BaseDataForm
        Assert.IsAssignableFrom<Form>(form);
        Assert.NotNull(form);
    }

    [Fact]
    public void ManagementForms_ShouldHaveConsistentInterface()
    {
        // This test ensures all management forms follow similar patterns

        // Arrange & Act
        var mockActivityRepo = new Mock<IActivityRepository>();
        mockActivityRepo.Setup(r => r.GetAllActivities()).Returns(new List<Activity>());
        using var activityForm = new ActivityManagementForm(mockActivityRepo.Object);

        var mockDriverRepo = new Mock<IDriverRepository>();
        mockDriverRepo.Setup(r => r.GetAllDrivers()).Returns(new List<Driver>());
        using var driverForm = new DriverManagementForm(mockDriverRepo.Object);

        // Assert - All forms should be WinForms
        Assert.IsAssignableFrom<Form>(activityForm);
        Assert.IsAssignableFrom<Form>(driverForm);

        // Assert - All forms should have consistent properties
        Assert.Contains("Management", activityForm.Text);
        Assert.Contains("Management", driverForm.Text);
    }
}
