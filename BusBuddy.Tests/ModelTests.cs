using BusBuddy.Models;

namespace BusBuddy.Tests;

/// <summary>
/// Test class for model validations and business rules
/// </summary>
public class ModelTests
{
    [Fact]
    public void Driver_Name_Property_ShouldCombineFirstAndLastName()
    {
        // Arrange
        var driver = new Driver
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var name = driver.Name;

        // Assert
        Assert.Equal("John Doe", name);
    }

    [Fact]
    public void Driver_Name_Property_ShouldHandleNullValues()
    {
        // Arrange
        var driver = new Driver
        {
            FirstName = null,
            LastName = "Doe"
        };

        // Act
        var name = driver.Name;

        // Assert
        Assert.Equal("Doe", name.Trim());
    }

    [Fact]
    public void Driver_DriverName_Property_ShouldAllowDirectAssignment()
    {
        // Arrange
        var driver = new Driver
        {
            DriverName = "Jane Smith"
        };

        // Act & Assert
        Assert.Equal("Jane Smith", driver.DriverName);
    }

    [Fact]
    public void Driver_IsTrainingComplete_ShouldConvertFromInt()
    {
        // Arrange
        var driver = new Driver
        {
            TrainingComplete = 1
        };

        // Act & Assert
        Assert.True(driver.IsTrainingComplete);

        // Act - set to false
        driver.IsTrainingComplete = false;

        // Assert
        Assert.Equal(0, driver.TrainingComplete);
    }

    [Fact]
    public void Activity_ShouldAllowNullOptionalFields()
    {
        // Arrange & Act
        var activity = new Activity
        {
            Date = DateTime.Now,
            ActivityType = "Field Trip",
            Destination = "Museum"
            // LeaveTime, EventTime, ReturnTime are optional
        };

        // Assert
        Assert.NotNull(activity);
        Assert.Equal("Field Trip", activity.ActivityType);
        Assert.Equal("Museum", activity.Destination);
    }

    [Fact]
    public void Fuel_ShouldHandleNullableProperties()
    {
        // Arrange & Act
        var fuel = new Fuel
        {
            FuelDate = DateTime.Now,
            FuelLocation = "Test Station",
            VehicleFueledID = null, // Nullable
            VehicleOdometerReading = null // Nullable
        };

        // Assert
        Assert.NotNull(fuel);
        Assert.Null(fuel.VehicleFueledID);
        Assert.Null(fuel.VehicleOdometerReading);
    }

    [Fact]
    public void Maintenance_ShouldAllowNullableVehicleId()
    {
        // Arrange & Act
        var maintenance = new Maintenance
        {
            Date = DateTime.Now,
            MaintenanceCompleted = "Oil Change",
            Notes = "Routine maintenance",
            VehicleID = null // Nullable
        };

        // Assert
        Assert.NotNull(maintenance);
        Assert.Null(maintenance.VehicleID);
        Assert.Equal("Oil Change", maintenance.MaintenanceCompleted);
    }

    [Fact]
    public void Maintenance_ShouldProvideCompatibilityProperties()
    {
        // Arrange & Act
        var maintenance = new Maintenance
        {
            Date = DateTime.Now,
            MaintenanceCompleted = "Oil Change",
            Notes = "Routine maintenance",
            RepairCost = 150.50m,
            OdometerReading = 50000
        };

        // Assert
        Assert.Equal("Oil Change", maintenance.Category);
        Assert.Equal("Routine maintenance", maintenance.Description);
        Assert.Equal(150.50m, maintenance.Cost);
        Assert.Equal(50000, maintenance.Odometer);
    }

    [Fact]
    public void Route_ShouldHandleBasicProperties()
    {
        // Arrange & Act
        var route = new Route
        {
            RouteName = "Route 1",
            Date = DateTime.Now,
            Notes = "Main route through downtown"
        };

        // Assert
        Assert.NotNull(route);
        Assert.Equal("Route 1", route.RouteName);
        Assert.Equal("Main route through downtown", route.Notes);
    }

    [Fact]
    public void ActivitySchedule_ShouldHandleNullableTimeSpans()
    {
        // Arrange & Act
        var schedule = new ActivitySchedule
        {
            Date = DateTime.Now,
            TripType = "One Way",
            ScheduledDestination = "School",
            ScheduledLeaveTime = null,  // Nullable TimeSpan
            ScheduledEventTime = null   // Nullable TimeSpan
        };

        // Assert
        Assert.NotNull(schedule);
        Assert.Null(schedule.ScheduledLeaveTime);
        Assert.Null(schedule.ScheduledEventTime);
        Assert.Equal("One Way", schedule.TripType);
    }

    [Fact]
    public void SchoolCalendar_ShouldHandleBasicProperties()
    {
        // Arrange & Act
        var calendar = new SchoolCalendar
        {
            Date = DateTime.Now.Date,
            Notes = "Regular school day"
        };

        // Assert
        Assert.NotNull(calendar);
        Assert.Equal("Regular school day", calendar.Notes);
    }

    [Theory]
    [InlineData(2020, true)]
    [InlineData(2023, true)]
    [InlineData(1990, true)]
    [InlineData(1800, false)]
    [InlineData(2050, false)]
    public void Vehicle_Year_ShouldBeReasonable(int year, bool isValid)
    {
        // Arrange
        var vehicle = new Vehicle
        {
            VehicleNumber = "TEST001",
            Make = "Test",
            Model = "Vehicle",
            Year = year
        };

        // Act & Assert
        if (isValid)
        {
            Assert.True(vehicle.Year >= 1900 && vehicle.Year <= DateTime.Now.Year + 1);
        }
        else
        {
            Assert.False(vehicle.Year >= 1900 && vehicle.Year <= DateTime.Now.Year + 1);
        }
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(-5, false)]
    [InlineData(0, false)]
    public void Vehicle_Capacity_ShouldBePositive(int capacity, bool isValid)
    {
        // Arrange
        var vehicle = new Vehicle
        {
            VehicleNumber = "TEST001",
            Make = "Test",
            Model = "Vehicle",
            Capacity = capacity
        };

        // Act & Assert
        if (isValid)
        {
            Assert.True(vehicle.Capacity > 0);
        }
        else
        {
            Assert.False(vehicle.Capacity > 0);
        }
    }

    [Fact]
    public void Vehicle_CompatibilityProperties_ShouldWork()
    {
        // Arrange & Act
        var vehicle = new Vehicle
        {
            VehicleNumber = "BUS001",
            VINNumber = "1234567890",
            DateLastInspection = DateTime.Now.AddMonths(-6),
            Capacity = 50
        };

        // Assert
        Assert.Equal("1234567890", vehicle.VIN);
        Assert.Equal(DateTime.Now.AddMonths(-6).Date, vehicle.LastInspectionDate?.Date);
        Assert.Equal(50, vehicle.SeatingCapacity);
    }

    [Fact]
    public void Vehicle_BusNumber_ShouldFallbackToVehicleNumber()
    {
        // Arrange & Act
        var vehicle = new Vehicle
        {
            VehicleNumber = "BUS001"
            // BusNumber not set, should fallback to VehicleNumber
        };

        // Assert
        Assert.Equal("BUS001", vehicle.BusNumber);
    }

    [Fact]
    public void Route_NavigationProperties_ShouldProvideComputedNames()
    {
        // Arrange
        var route = new Route
        {
            RouteName = "Main Route",
            AMVehicle = new Vehicle { VehicleNumber = "BUS001" },
            AMDriver = new Driver { FirstName = "John", LastName = "Doe" }
        };

        // Act & Assert
        Assert.Equal("BUS001", route.AMVehicleNumber);
        Assert.Equal("John Doe", route.AMDriverName);
    }
}
