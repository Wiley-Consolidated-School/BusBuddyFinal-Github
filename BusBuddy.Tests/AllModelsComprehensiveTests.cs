using Xunit;
using BusBuddy.Models;

namespace BusBuddy.Tests
{
    public class AllModelsComprehensiveTests
    {
        [Fact]
        public void Driver_AllProperties_SetAndGetCorrectly()
        {
            // Arrange
            var driver = new Driver();

            // Act
            driver.DriverID = 1;
            driver.DriverName = "John Doe";
            driver.DriverPhone = "555-1234";
            driver.DriverEmail = "john@example.com";
            driver.Address = "123 Main St";
            driver.City = "Anytown";
            driver.State = "ST";
            driver.Zip = "12345";
            driver.DriversLicenseType = "CDL";
            driver.TrainingComplete = 1;
            driver.Notes = "Excellent driver";

            // Assert
            Assert.Equal(1, driver.DriverID);
            Assert.Equal("John Doe", driver.DriverName);
            Assert.Equal("555-1234", driver.DriverPhone);
            Assert.Equal("john@example.com", driver.DriverEmail);
            Assert.Equal("123 Main St", driver.Address);
            Assert.Equal("Anytown", driver.City);
            Assert.Equal("ST", driver.State);
            Assert.Equal("12345", driver.Zip);
            Assert.Equal("CDL", driver.DriversLicenseType);
            Assert.Equal(1, driver.TrainingComplete);
            Assert.Equal("Excellent driver", driver.Notes);
        }

        [Fact]
        public void Driver_IsTrainingComplete_ConvertsCorrectly()
        {
            // Arrange
            var driver = new Driver();

            // Act & Assert - Test true case
            driver.IsTrainingComplete = true;
            Assert.Equal(1, driver.TrainingComplete);
            Assert.True(driver.IsTrainingComplete);

            // Act & Assert - Test false case
            driver.IsTrainingComplete = false;
            Assert.Equal(0, driver.TrainingComplete);
            Assert.False(driver.IsTrainingComplete);
        }

        [Fact]
        public void Route_AllProperties_SetAndGetCorrectly()
        {
            // Arrange
            var route = new Route();
            var testDate = new DateTime(2025, 6, 15);

            // Act
            route.RouteID = 1;
            route.Date = testDate;
            route.RouteName = "Elementary Route 1";
            route.AMVehicleID = 101;
            route.PMVehicleID = 102;
            route.AMDriverID = 201;
            route.PMDriverID = 202;
            route.AMBeginMiles = 12500.5m;
            route.AMEndMiles = 12550.8m;
            route.AMRiders = 25;
            route.PMBeginMiles = 12550.8m;
            route.PMEndMiles = 12600.2m;
            route.PMRiders = 22;
            route.Notes = "All routes completed on time";

            // Assert
            Assert.Equal(1, route.RouteID);
            Assert.Equal(testDate, route.Date);
            Assert.Equal("Elementary Route 1", route.RouteName);
            Assert.Equal(101, route.AMVehicleID);
            Assert.Equal(102, route.PMVehicleID);
            Assert.Equal(201, route.AMDriverID);
            Assert.Equal(202, route.PMDriverID);
            Assert.Equal(12500.5m, route.AMBeginMiles);
            Assert.Equal(12550.8m, route.AMEndMiles);
            Assert.Equal(25, route.AMRiders);
            Assert.Equal(12550.8m, route.PMBeginMiles);
            Assert.Equal(12600.2m, route.PMEndMiles);
            Assert.Equal(22, route.PMRiders);
            Assert.Equal("All routes completed on time", route.Notes);
        }

        [Fact]
        public void Activity_AllProperties_SetAndGetCorrectly()
        {
            // Arrange
            var activity = new Activity();
            var testDate = new DateTime(2025, 6, 15);

            // Act
            activity.ActivityID = 1;
            activity.Date = testDate;
            activity.ActivityType = "Field Trip";
            activity.Destination = "Science Museum";
            activity.LeaveTime = "08:00";
            activity.EventTime = "10:00";
            activity.ReturnTime = "15:00";
            activity.RequestedBy = "Ms. Smith";
            activity.AssignedVehicleID = 101;
            activity.DriverID = 201;
            activity.Notes = "Chaperones required";

            // Assert
            Assert.Equal(1, activity.ActivityID);
            Assert.Equal(testDate, activity.Date);
            Assert.Equal("Field Trip", activity.ActivityType);
            Assert.Equal("Science Museum", activity.Destination);
            Assert.Equal("08:00", activity.LeaveTime);
            Assert.Equal("10:00", activity.EventTime);
            Assert.Equal("15:00", activity.ReturnTime);
            Assert.Equal("Ms. Smith", activity.RequestedBy);
            Assert.Equal(101, activity.AssignedVehicleID);
            Assert.Equal(201, activity.DriverID);
            Assert.Equal("Chaperones required", activity.Notes);
        }

        [Fact]
        public void Fuel_AllProperties_SetAndGetCorrectly()
        {
            // Arrange
            var fuel = new Fuel();
            var testDate = new DateTime(2025, 6, 15);

            // Act
            fuel.FuelID = 1;
            fuel.FuelDate = testDate;
            fuel.FuelLocation = "Key Pumps";
            fuel.VehicleFueledID = 101;
            fuel.VehicleOdometerReading = 125000;
            fuel.FuelType = "Diesel";
            fuel.FuelAmount = 25.5m;
            fuel.FuelCost = 75.50m;
            fuel.Notes = "Filled before trip";

            // Assert
            Assert.Equal(1, fuel.FuelID);
            Assert.Equal(testDate, fuel.FuelDate);
            Assert.Equal("Key Pumps", fuel.FuelLocation);
            Assert.Equal(101, fuel.VehicleFueledID);
            Assert.Equal(125000, fuel.VehicleOdometerReading);
            Assert.Equal("Diesel", fuel.FuelType);
            Assert.Equal(25.5m, fuel.FuelAmount);
            Assert.Equal(75.50m, fuel.FuelCost);
            Assert.Equal("Filled before trip", fuel.Notes);
        }

        [Fact]
        public void Maintenance_AllProperties_SetAndGetCorrectly()
        {
            // Arrange
            var maintenance = new Maintenance();
            var testDate = new DateTime(2025, 6, 15);

            // Act
            maintenance.MaintenanceID = 1;
            maintenance.Date = testDate;
            maintenance.VehicleID = 101;
            maintenance.OdometerReading = 125000;
            maintenance.MaintenanceCompleted = "Oil Change";
            maintenance.Vendor = "QuickLube";
            maintenance.RepairCost = 125.00m;
            maintenance.Notes = "Regular oil change and filter replacement";

            // Assert
            Assert.Equal(1, maintenance.MaintenanceID);
            Assert.Equal(testDate, maintenance.Date);
            Assert.Equal(101, maintenance.VehicleID);
            Assert.Equal(125000, maintenance.OdometerReading);
            Assert.Equal("Oil Change", maintenance.MaintenanceCompleted);
            Assert.Equal("QuickLube", maintenance.Vendor);
            Assert.Equal(125.00m, maintenance.RepairCost);
            Assert.Equal("Regular oil change and filter replacement", maintenance.Notes);
        }

        [Fact]
        public void SchoolCalendar_AllProperties_SetAndGetCorrectly()
        {
            // Arrange
            var calendar = new SchoolCalendar();
            var testDate = new DateTime(2025, 6, 15);
            var endDate = new DateTime(2025, 6, 20);

            // Act
            calendar.CalendarID = 1;
            calendar.Date = testDate;
            calendar.EndDate = endDate;
            calendar.Category = "Holiday";
            calendar.Description = "Schools closed for summer";
            calendar.RouteNeeded = 1;
            calendar.Notes = "District-wide";

            // Assert
            Assert.Equal(1, calendar.CalendarID);
            Assert.Equal(testDate, calendar.Date);
            Assert.Equal(endDate, calendar.EndDate);
            Assert.Equal("Holiday", calendar.Category);
            Assert.Equal("Schools closed for summer", calendar.Description);
            Assert.Equal(1, calendar.RouteNeeded);
            Assert.Equal("District-wide", calendar.Notes);
            Assert.True(calendar.IsRouteNeeded);
        }

        [Fact]
        public void TimeCard_AllProperties_SetAndGetCorrectly()
        {
            // Arrange
            var timeCard = new TimeCard();
            var testDate = new DateTime(2025, 6, 15);
            var amIn = new TimeSpan(7, 0, 0);
            var pmOut = new TimeSpan(16, 0, 0);

            // Act
            timeCard.TimeCardID = 1;
            timeCard.DriverID = 201;
            timeCard.Date = testDate;
            timeCard.DayType = "Route Day";
            timeCard.AMClockIn = amIn;
            timeCard.PMClockOut = pmOut;
            timeCard.TotalTime = 8.0m;
            timeCard.Overtime = 0.5m;
            timeCard.WeeklyTotal = 40.0m;
            timeCard.MonthlyTotal = 160.0m;
            timeCard.Notes = "Regular shift";

            // Assert
            Assert.Equal(1, timeCard.TimeCardID);
            Assert.Equal(201, timeCard.DriverID);
            Assert.Equal(testDate, timeCard.Date);
            Assert.Equal("Route Day", timeCard.DayType);
            Assert.Equal(amIn, timeCard.AMClockIn);
            Assert.Equal(pmOut, timeCard.PMClockOut);
            Assert.Equal(8.0m, timeCard.TotalTime);
            Assert.Equal(0.5m, timeCard.Overtime);
            Assert.Equal(40.0m, timeCard.WeeklyTotal);
            Assert.Equal(160.0m, timeCard.MonthlyTotal);
            Assert.Equal("Regular shift", timeCard.Notes);
        }

        [Fact]
        public void ActivitySchedule_AllProperties_SetAndGetCorrectly()
        {
            // Arrange
            var schedule = new ActivitySchedule();
            var testDate = new DateTime(2025, 6, 15);
            var leaveTime = new TimeSpan(9, 0, 0);

            // Act
            schedule.ScheduleID = 1;
            schedule.Date = testDate;
            schedule.TripType = "Sports Trip";
            schedule.ScheduledVehicleID = 101;
            schedule.ScheduledDestination = "Stadium";
            schedule.ScheduledLeaveTime = leaveTime;
            schedule.ScheduledEventTime = new TimeSpan(10, 0, 0);
            schedule.ScheduledReturnTime = new TimeSpan(15, 0, 0);
            schedule.ScheduledRiders = 30;
            schedule.ScheduledDriverID = 201;
            schedule.Notes = "Varsity only";

            // Assert
            Assert.Equal(1, schedule.ScheduleID);
            Assert.Equal(testDate, schedule.Date);
            Assert.Equal("Sports Trip", schedule.TripType);
            Assert.Equal(101, schedule.ScheduledVehicleID);
            Assert.Equal("Stadium", schedule.ScheduledDestination);
            Assert.Equal(leaveTime, schedule.ScheduledLeaveTime);
            Assert.Equal(new TimeSpan(10, 0, 0), schedule.ScheduledEventTime);
            Assert.Equal(new TimeSpan(15, 0, 0), schedule.ScheduledReturnTime);
            Assert.Equal(30, schedule.ScheduledRiders);
            Assert.Equal(201, schedule.ScheduledDriverID);
            Assert.Equal("Varsity only", schedule.Notes);
        }

        [Fact]
        public void AllModels_WithNullValues_HandleGracefully()
        {
            // Test that all models can handle null string values without throwing
            var driver = new Driver { DriverName = null, DriverEmail = null };
            var route = new Route { RouteName = null };
            var activity = new Activity { ActivityType = null, Destination = null };
            var maintenance = new Maintenance { MaintenanceCompleted = null };
            var calendar = new SchoolCalendar { Category = null };
            var timeCard = new TimeCard { Notes = null };
            var schedule = new ActivitySchedule { Notes = null };

            // Assert - Should not throw exceptions
            Assert.Null(driver.DriverName);
            Assert.Null(route.RouteName);
            Assert.Null(activity.ActivityType);
            Assert.Null(maintenance.MaintenanceCompleted);
            Assert.Null(calendar.Category);
            Assert.Null(timeCard.Notes);
            Assert.Null(schedule.Notes);
        }
    }
}
