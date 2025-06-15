using BusBuddy.Models;
using BusBuddy.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.IO;

namespace BusBuddy.Tests;

/// <summary>
/// Test class for repository layer functionality
/// </summary>
public class RepositoryTests : IAsyncLifetime
{
    private readonly string _testConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
    private const string _sqlServerProvider = "Microsoft.Data.SqlClient";

    public async Task InitializeAsync()
    {
        await InitializeTestDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    private async Task InitializeTestDatabaseAsync()
    {
        try
        {
            // Use our custom database initializer to create tables with correct schema
            var initializer = new SqlServerDatabaseInitializer(_testConnectionString);
            initializer.Initialize();

            // Clean up existing data
            using var context = CreateTestContext();

            // Clean up in dependency order
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
            Console.WriteLine($"Test setup warning: {ex.Message}");
        }
    }

    private async Task CreateTestDatabaseIfNotExistsAsync()
    {
        var masterConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";

        using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();

        var checkDbCommand = new SqlCommand("SELECT COUNT(*) FROM sys.databases WHERE name = 'BusBuddyDB_Test'", connection);
        var result = await checkDbCommand.ExecuteScalarAsync();
        var dbExists = (result != null) && (int)result > 0;

        if (!dbExists)
        {
            var createDbCommand = new SqlCommand("CREATE DATABASE BusBuddyDB_Test", connection);
            await createDbCommand.ExecuteNonQueryAsync();
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
    public void VehicleRepository_AddVehicle_ShouldAddSuccessfully()
    {
        // Arrange
        var repository = new VehicleRepository(_testConnectionString, _sqlServerProvider);
        var vehicle = new Vehicle
        {
            VehicleNumber = "REPO001",
            Make = "Test Motors",
            Model = "Repository Test",
            Year = 2023,
            Capacity = 45,
            LicenseNumber = "REPO123"
        };

        // Act
        repository.AddVehicle(vehicle);

        // Assert
        var vehicles = repository.GetAllVehicles();
        Assert.Contains(vehicles, v => v.VehicleNumber == "REPO001");
    }

    [Fact]
    public void DriverRepository_UpdateDriver_ShouldUpdateSuccessfully()
    {
        // Arrange
        var repository = new DriverRepository(_testConnectionString, _sqlServerProvider);
        var driver = new Driver
        {
            DriverName = "John Tester",
            DriverEmail = "john.tester@test.com",
            DriverPhone = "555-1234",
            TrainingComplete = 1  // Required field for SQL Server
        };

        repository.AddDriver(driver);
        var addedDriver = repository.GetAllDrivers().First(d => d.DriverEmail == "john.tester@test.com");

        // Act
        addedDriver.DriverPhone = "555-5678";
        repository.UpdateDriver(addedDriver);

        // Assert
        var updatedDriver = repository.GetDriverById(addedDriver.DriverID);
        Assert.Equal("555-5678", updatedDriver?.DriverPhone);
    }

    [Fact]
    public void ActivityRepository_GetActivitiesByDateRange_ShouldFilterCorrectly()
    {
        // Arrange
        var repository = new ActivityRepository(_testConnectionString, _sqlServerProvider);

        var activity1 = new Activity
        {
            DateAsDateTime = DateTime.Now.AddDays(-5),
            ActivityType = "Field Trip",
            Destination = "Museum"
        };

        var activity2 = new Activity
        {
            DateAsDateTime = DateTime.Now.AddDays(5),
            ActivityType = "Sports Event",
            Destination = "Stadium"
        };

        repository.AddActivity(activity1);
        repository.AddActivity(activity2);

        // Act
        var startDate = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd");
        var endDate = DateTime.Now.ToString("yyyy-MM-dd");
        var recentActivities = repository.GetAllActivities()
            .Where(a => string.Compare(a.Date, startDate) >= 0 && string.Compare(a.Date, endDate) <= 0)
            .ToList();

        // Assert
        Assert.Single(recentActivities);
        Assert.Equal("Field Trip", recentActivities.First().ActivityType);
    }

    [Fact]
    public void FuelRepository_GetFuelRecordsByVehicle_ShouldFilterCorrectly()
    {
        // Arrange
        var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
        var fuelRepo = new FuelRepository(_testConnectionString, _sqlServerProvider);

        var vehicle = new Vehicle
        {
            VehicleNumber = "FUEL001",
            Make = "Test",
            Model = "Fuel Test"
        };
        vehicleRepo.AddVehicle(vehicle);
        var addedVehicle = vehicleRepo.GetAllVehicles().First(v => v.VehicleNumber == "FUEL001");

        var fuelRecord = new Fuel
        {
            VehicleFueledID = addedVehicle.Id,
            FuelDateAsDateTime = DateTime.Now,
            FuelLocation = "Test Station",
            FuelType = "Diesel"
        };
        fuelRepo.AddFuelRecord(fuelRecord);

        // Act
        var fuelRecords = fuelRepo.GetAllFuelRecords()
            .Where(f => f.VehicleFueledID == addedVehicle.Id)
            .ToList();

        // Assert
        Assert.Single(fuelRecords);
        Assert.Equal("Test Station", fuelRecords.First().FuelLocation);
    }

    [Fact]
    public void MaintenanceRepository_DeleteMaintenance_ShouldRemoveSuccessfully()
    {
        // Arrange
        var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
        var maintenanceRepo = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);

        var vehicle = new Vehicle
        {
            VehicleNumber = "MAINT001",
            Make = "Test",
            Model = "Maintenance Test"
        };
        vehicleRepo.AddVehicle(vehicle);
        var addedVehicle = vehicleRepo.GetAllVehicles().First(v => v.VehicleNumber == "MAINT001");

        var maintenance = new Maintenance
        {
            VehicleID = addedVehicle.Id,
            DateAsDateTime = DateTime.Now,
            MaintenanceCompleted = "Oil Change",
            Notes = "Routine maintenance"
        };
        maintenanceRepo.AddMaintenance(maintenance);

        var addedMaintenance = maintenanceRepo.GetAllMaintenanceRecords()
            .First(m => m.MaintenanceCompleted == "Oil Change");

        // Act
        maintenanceRepo.DeleteMaintenance(addedMaintenance.MaintenanceID);

        // Assert
        var deletedMaintenance = maintenanceRepo.GetMaintenanceById(addedMaintenance.MaintenanceID);
        Assert.Null(deletedMaintenance);
    }
}
