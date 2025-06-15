using BusBuddy.Models;
using BusBuddy.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Tests;

public class UnitTest1 : IAsyncLifetime
{
    private readonly string _testConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";

    public async Task InitializeAsync()
    {
        // Set up test environment and clean database before tests
        try
        {
            await CleanupTestDatabaseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test setup warning: {ex.Message}");
        }
    }

    public async Task DisposeAsync()
    {
        // Clean up after tests if needed
        await Task.CompletedTask;
    }

    private async Task CleanupTestDatabaseAsync()
    {
        try
        {
            using var context = CreateTestContext();

            // Ensure database exists
            await context.Database.EnsureCreatedAsync();

            // Clean up data
            context.PTOBalances.RemoveRange(context.PTOBalances);
            context.ActivitySchedules.RemoveRange(context.ActivitySchedules);
            context.Activities.RemoveRange(context.Activities);
            context.Maintenances.RemoveRange(context.Maintenances);
            context.Fuels.RemoveRange(context.Fuels);
            context.SchoolCalendars.RemoveRange(context.SchoolCalendars);
            context.Routes.RemoveRange(context.Routes);
            context.Drivers.RemoveRange(context.Drivers);
            context.Vehicles.RemoveRange(context.Vehicles);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test cleanup warning: {ex.Message}");
        }
    }

    private BusBuddyContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<BusBuddyContext>()
            .UseSqlServer(_testConnectionString)
            .Options;
        return new BusBuddyContext(options);
    }

    [Fact]
    public void Test_Basic_Vehicle_Creation()
    {
        // Arrange & Act
        var vehicle = new Vehicle
        {
            VehicleNumber = "BUS001",
            LicenseNumber = "ABC123",
            Make = "Blue Bird",
            Model = "Vision"
        };

        // Assert
        Assert.Equal("BUS001", vehicle.VehicleNumber);
        Assert.Equal("ABC123", vehicle.LicenseNumber);
        Assert.Equal("Blue Bird", vehicle.Make);
        Assert.Equal("Vision", vehicle.Model);
    }

    [Fact]
    public void Test_Driver_Model_Creation()
    {
        // Arrange & Act
        var driver = new Driver
        {
            FirstName = "John",
            LastName = "Doe",
            DriverEmail = "john.doe@example.com"
        };

        // Assert
        Assert.Equal("John", driver.FirstName);
        Assert.Equal("Doe", driver.LastName);
        Assert.Equal("john.doe@example.com", driver.DriverEmail);
        Assert.Equal("John Doe", driver.Name);
    }

    [Fact]
    public async Task Test_Database_Vehicle_CRUD_Operations()
    {
        // Arrange
        using var context = CreateTestContext();

        var vehicle = new Vehicle
        {
            VehicleNumber = "TEST001",
            BusNumber = "T001",
            Make = "Test Motors",
            Model = "Test Bus",
            Year = 2023,
            Capacity = 50,
            LicenseNumber = "TEST123"
        };

        // Act - Create
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        // Assert - Read
        var savedVehicle = await context.Vehicles.FindAsync(vehicle.Id);
        Assert.NotNull(savedVehicle);
        Assert.Equal("TEST001", savedVehicle.VehicleNumber);
        Assert.Equal("T001", savedVehicle.BusNumber);
        Assert.Equal("Test Motors", savedVehicle.Make);

        // Act - Update
        savedVehicle.Year = 2024;
        await context.SaveChangesAsync();

        // Assert - Updated
        var updatedVehicle = await context.Vehicles.FindAsync(vehicle.Id);
        Assert.Equal(2024, updatedVehicle!.Year);

        // Act - Delete
        context.Vehicles.Remove(updatedVehicle);
        await context.SaveChangesAsync();

        // Assert - Deleted
        var deletedVehicle = await context.Vehicles.FindAsync(vehicle.Id);
        Assert.Null(deletedVehicle);
    }

    [Fact]
    public async Task Test_Database_Driver_CRUD_Operations()
    {
        // Arrange
        using var context = CreateTestContext();

        var driver = new Driver
        {
            FirstName = "Test",
            LastName = "Driver",
            DriverEmail = "test.driver@test.com"
        };

        // Act - Create
        context.Drivers.Add(driver);
        await context.SaveChangesAsync();

        // Assert - Read
        var savedDriver = await context.Drivers.FindAsync(driver.DriverID);
        Assert.NotNull(savedDriver);
        Assert.Equal("Test", savedDriver.FirstName);
        Assert.Equal("Driver", savedDriver.LastName);
        Assert.Equal("Test Driver", savedDriver.Name);

        // Act - Update
        savedDriver.DriverEmail = "updated@test.com";
        await context.SaveChangesAsync();

        // Assert - Updated
        var updatedDriver = await context.Drivers.FindAsync(driver.DriverID);
        Assert.Equal("updated@test.com", updatedDriver!.DriverEmail);

        // Act - Delete
        context.Drivers.Remove(updatedDriver);
        await context.SaveChangesAsync();

        // Assert - Deleted
        var deletedDriver = await context.Drivers.FindAsync(driver.DriverID);
        Assert.Null(deletedDriver);
    }

    [Fact]
    public async Task Test_Database_Connection_And_Configuration()
    {
        // Arrange & Act
        using var context = CreateTestContext();

        // Assert
        Assert.NotNull(context);

        // Test that we can connect to the database
        var canConnect = await context.Database.CanConnectAsync();
        Assert.True(canConnect);

        // Test basic operations
        var vehicleCount = await context.Vehicles.CountAsync();
        Assert.True(vehicleCount >= 0); // Should be 0 or more (cleaned up)
    }

    [Fact]
    public void RunDatabaseDiagnostics()
    {
        // Run database diagnostics to verify configuration
        Console.WriteLine("Running Database Diagnostics...");
        DatabaseDiagnostics.RunDiagnostics();
        Console.WriteLine("Database diagnostics completed.");
    }
}
