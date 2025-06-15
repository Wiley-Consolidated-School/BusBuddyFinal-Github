using BusBuddy.Models;
using BusBuddy.Business;
using BusBuddy.Data;

namespace BusBuddy.Tests;

/// <summary>
/// Test class for business logic and validation services
/// </summary>
public class BusinessLogicTests
{
    private readonly string _testConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
    private const string _sqlServerProvider = "Microsoft.Data.SqlClient";

    [Fact]
    public void ValidationService_ValidateVehicleAvailability_ShouldReturnFailedForNonExistentVehicle()
    {
        // Arrange
        var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
        var driverRepo = new DriverRepository(_testConnectionString, _sqlServerProvider);
        var maintenanceRepo = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);
        var fuelRepo = new FuelRepository(_testConnectionString, _sqlServerProvider);
        var validationService = new ValidationService(vehicleRepo, driverRepo, maintenanceRepo, fuelRepo);

        // Act
        var result = validationService.ValidateVehicleAvailability(999, DateTime.Now);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("does not exist", result.GetErrorMessage());
    }

    [Fact]
    public void ValidationService_ValidateDriverAvailability_ShouldReturnFailedForNonExistentDriver()
    {
        // Arrange
        var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
        var driverRepo = new DriverRepository(_testConnectionString, _sqlServerProvider);
        var maintenanceRepo = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);
        var fuelRepo = new FuelRepository(_testConnectionString, _sqlServerProvider);
        var validationService = new ValidationService(vehicleRepo, driverRepo, maintenanceRepo, fuelRepo);

        // Act
        var result = validationService.ValidateDriverAvailability(999, DateTime.Now);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("does not exist", result.GetErrorMessage());
    }

    [Fact]
    public void ValidationService_ValidateFuelRecord_ShouldReturnFailedForInvalidAmount()
    {
        // Arrange
        var validationService = new ValidationService();
        var invalidFuel = new Fuel
        {
            FuelDateAsDateTime = DateTime.Now,
            FuelLocation = "Test Station",
            FuelAmount = -10  // Invalid: negative amount
        };

        // Act
        var result = validationService.ValidateFuelRecord(invalidFuel);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Fuel amount", result.GetErrorMessage());
    }

    [Fact]
    public void ValidationService_ValidateFuelRecord_ShouldReturnFailedForExcessiveAmount()
    {
        // Arrange
        var validationService = new ValidationService();
        var invalidFuel = new Fuel
        {
            FuelDateAsDateTime = DateTime.Now,
            FuelLocation = "Test Station",
            FuelAmount = 300  // Invalid: too much
        };

        // Act
        var result = validationService.ValidateFuelRecord(invalidFuel);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Fuel amount", result.GetErrorMessage());
    }

    [Fact]
    public void ValidationService_ValidateMaintenanceRecord_ShouldReturnFailedForNegativeCost()
    {
        // Arrange
        var validationService = new ValidationService();
        var invalidMaintenance = new Maintenance
        {
            Date = DateTime.Now,
            RepairCost = -100  // Invalid: negative cost
        };

        // Act
        var result = validationService.ValidateMaintenanceRecord(invalidMaintenance);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Repair cost cannot be negative", result.GetErrorMessage());
    }

    [Fact]
    public void ValidationService_ValidateMaintenanceRecord_ShouldReturnFailedForNegativeOdometer()
    {
        // Arrange
        var validationService = new ValidationService();
        var invalidMaintenance = new Maintenance
        {
            Date = DateTime.Now,
            OdometerReading = -1000  // Invalid: negative odometer
        };

        // Act
        var result = validationService.ValidateMaintenanceRecord(invalidMaintenance);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Odometer reading cannot be negative", result.GetErrorMessage());
    }

    [Fact]
    public void ValidationResult_Success_ShouldReturnValidResult()
    {
        // Arrange & Act
        var result = ValidationResult.Success();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidationResult_Failed_ShouldReturnInvalidResult()
    {
        // Arrange & Act
        var result = ValidationResult.Failed("Test error");

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Test error", result.Errors.First());
    }

    [Fact]
    public void ValidationResult_Combine_ShouldCombineMultipleResults()
    {
        // Arrange
        var result1 = ValidationResult.Failed("Error 1");
        var result2 = ValidationResult.Failed("Error 2");
        var result3 = ValidationResult.Success();

        // Act
        var combinedResult = ValidationResult.Combine(new[] { result1, result2, result3 });

        // Assert
        Assert.False(combinedResult.IsValid);
        Assert.Equal(2, combinedResult.Errors.Count);
        Assert.Contains("Error 1", combinedResult.Errors);
        Assert.Contains("Error 2", combinedResult.Errors);
    }

    [Fact]
    public void ValidationResult_GetErrorMessage_ShouldJoinErrors()
    {
        // Arrange
        var result = ValidationResult.Failed(new[] { "Error 1", "Error 2" });

        // Act
        var errorMessage = result.GetErrorMessage();

        // Assert
        Assert.Contains("Error 1", errorMessage);
        Assert.Contains("Error 2", errorMessage);
    }
}
