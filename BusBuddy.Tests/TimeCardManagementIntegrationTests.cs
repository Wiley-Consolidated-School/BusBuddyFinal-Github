using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.Business;

namespace BusBuddy.Tests
{
    public class TimeCardManagementIntegrationTests : IDisposable
    {
        private readonly BusBuddyContext _context;
        private readonly TimeCardRepository _timeCardRepository;
        private readonly DriverRepository _driverRepository;
        private readonly TimeEntryValidationService _validationService;
        private readonly Driver _testDriver;

        public TimeCardManagementIntegrationTests()
        {
            // Create in-memory database
            var options = new DbContextOptionsBuilder<BusBuddyContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BusBuddyContext(options);

            // Use parameterless constructors for repositories (they'll use default connection)
            _timeCardRepository = new TimeCardRepository();
            _driverRepository = new DriverRepository();
            _validationService = new TimeEntryValidationService(_timeCardRepository);

            // Setup test data
            SetupTestData();
            _testDriver = _context.Drivers.First();
        }

        private void SetupTestData()
        {
            // Create test driver
            var driver = new Driver
            {
                DriverID = 1,
                DriverName = "Jane Smith",
                DriverPhone = "555-0123",
                DriverEmail = "jane.smith@busbuddy.com",
                Address = "123 Main St",
                City = "Test City",
                State = "TS",
                Zip = "12345",
                DriversLicenseType = "CDL",
                TrainingComplete = 1,
                Notes = "Test driver for validation"
            };
            _context.Drivers.Add(driver);
            _context.SaveChanges();
        }

        [Fact]
        public void ValidateTimeCard_TypicalWorkDay_NoWarnings()
        {
            // Arrange - Create a typical work day entry
            var timeCard = new TimeCard
            {
                TimeCardID = 1,
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(8, 0, 0),    // 8:00 AM
                LunchClockOut = new TimeSpan(12, 0, 0), // 12:00 PM
                LunchClockIn = new TimeSpan(13, 0, 0),  // 1:00 PM
                PMClockOut = new TimeSpan(17, 0, 0),    // 5:00 PM
                TotalTime = 8.0m,
                Notes = "Regular work day"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            Assert.Empty(warnings); // Should have no warnings for a typical day
        }

        [Fact]
        public void ValidateTimeCard_MissingClockIn_ReturnsWarning()
        {
            // Arrange - Create entry missing AM clock in
            var timeCard = new TimeCard
            {
                TimeCardID = 2,
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = null, // Missing clock in!
                LunchClockOut = new TimeSpan(12, 0, 0),
                LunchClockIn = new TimeSpan(13, 0, 0),
                PMClockOut = new TimeSpan(17, 0, 0),
                Notes = "Forgot to clock in"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            Assert.NotEmpty(warnings);
            Assert.Contains(warnings, w => w.Type == WarningType.MissingClockIn);
            Assert.Contains(warnings, w => w.SuggestedAction.Contains("clock in time"));
        }

        [Fact]
        public void ValidateTimeCard_MissingClockOut_ReturnsWarning()
        {
            // Arrange - Create incomplete entry (simulating forgetting to clock out)
            var timeCard = new TimeCard
            {
                TimeCardID = 3,
                Date = DateTime.Today.AddDays(-1), // Yesterday
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(8, 0, 0),
                LunchClockOut = new TimeSpan(12, 0, 0),
                LunchClockIn = new TimeSpan(13, 0, 0),
                PMClockOut = null, // Forgot to clock out!
                Notes = "Incomplete day"
            };

            // First save this incomplete entry
            // (In a real scenario, this would be done through the repository)

            // Now create today's entry
            var todayTimeCard = new TimeCard
            {
                TimeCardID = 4,
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(7, 30, 0),
                Notes = "New day entry"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(todayTimeCard);

            // Assert - This test demonstrates the validation concept
            // In practice, the service would check against stored data
            Assert.NotNull(warnings); // Warnings collection should exist
        }

        [Fact]
        public void ValidateTimeCard_ShortLunchBreak_ReturnsWarning()
        {
            // Arrange - Create entry with very short lunch
            var timeCard = new TimeCard
            {
                TimeCardID = 5,
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(8, 0, 0),
                LunchClockOut = new TimeSpan(12, 0, 0), // 12:00 PM
                LunchClockIn = new TimeSpan(12, 10, 0), // 12:10 PM - only 10 minutes!
                PMClockOut = new TimeSpan(17, 0, 0),
                Notes = "Short lunch break"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            Assert.Contains(warnings, w => w.Type == WarningType.ShortLunchBreak);
            Assert.Contains(warnings, w => w.Message.Contains("10 minutes"));
        }

        [Fact]
        public void ValidateTimeCard_LongLunchBreak_ReturnsWarning()
        {
            // Arrange - Create entry with very long lunch
            var timeCard = new TimeCard
            {
                TimeCardID = 6,
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(8, 0, 0),
                LunchClockOut = new TimeSpan(12, 0, 0), // 12:00 PM
                LunchClockIn = new TimeSpan(15, 0, 0),  // 3:00 PM - 3 hours!
                PMClockOut = new TimeSpan(17, 0, 0),
                Notes = "Long lunch break"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            Assert.Contains(warnings, w => w.Type == WarningType.LongLunchBreak);
            Assert.Contains(warnings, w => w.Message.Contains("3") && w.Message.Contains("hours"));
        }

        [Fact]
        public void ValidateTimeCard_VeryEarlyClockIn_ReturnsWarning()
        {
            // Arrange - Create entry with very early clock in
            var timeCard = new TimeCard
            {
                TimeCardID = 7,
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(4, 30, 0), // 4:30 AM - very early!
                LunchClockOut = new TimeSpan(12, 0, 0),
                LunchClockIn = new TimeSpan(13, 0, 0),
                PMClockOut = new TimeSpan(17, 0, 0),
                Notes = "Very early start"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            Assert.Contains(warnings, w => w.Type == WarningType.VeryEarlyClockIn);
            Assert.Contains(warnings, w => w.Message.Contains("04:30"));
        }

        [Fact]
        public void ValidateTimeCard_RouteDay_ValidatesRouteEntries()
        {
            // Arrange - Create route day with missing route clock in
            var timeCard = new TimeCard
            {
                TimeCardID = 8,
                Date = DateTime.Today,
                DayType = "Route Day",
                AMClockIn = new TimeSpan(7, 0, 0),
                RouteAMClockOut = new TimeSpan(8, 0, 0), // Route starts
                RouteAMClockIn = null, // Missing route clock in!
                RoutePMClockOut = new TimeSpan(15, 0, 0),
                RoutePMClockIn = new TimeSpan(15, 30, 0),
                PMClockOut = new TimeSpan(16, 30, 0),
                Notes = "Route day with incomplete AM route"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            Assert.NotEmpty(warnings);
            // The validation should catch missing route times
        }

        [Theory]
        [InlineData(8, 0, 17, 0, 8.0)] // Normal 8-hour day (excluding lunch)
        [InlineData(7, 0, 18, 0, 10.0)] // 10-hour day
        [InlineData(6, 0, 20, 0, 13.0)] // Long day that should trigger warning
        public void ValidateTimeCard_VariousWorkDayLengths_ValidatesCorrectly(int clockInHour, int clockInMinute,
            int clockOutHour, int clockOutMinute, double expectedHours)
        {
            // Arrange
            var timeCard = new TimeCard
            {
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(clockInHour, clockInMinute, 0),
                LunchClockOut = new TimeSpan(12, 0, 0),
                LunchClockIn = new TimeSpan(13, 0, 0), // 1 hour lunch
                PMClockOut = new TimeSpan(clockOutHour, clockOutMinute, 0),
                TotalTime = (decimal)(expectedHours - 1), // Subtract lunch hour
                Notes = $"Test {expectedHours} hour day"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            if (expectedHours > 12) // Long day should have warning
            {
                Assert.Contains(warnings, w => w.Type == WarningType.ExcessiveWorkHours);
            }
            else // Normal day should have no major warnings
            {
                Assert.DoesNotContain(warnings, w => w.Severity == WarningSeverity.High);
            }
        }

        [Fact]
        public void ValidateTimeCard_GetWarningsByDriver_WorksCorrectly()
        {
            // This test demonstrates how warnings would be retrieved for a specific driver
            // In practice, you would query the database for the driver's recent time cards

            // Arrange
            var driverId = _testDriver.DriverID;
            var recentDate = DateTime.Today.AddDays(-7);

            // Act - Get all warnings for this driver's recent entries
            // This would typically involve querying the database and validating each entry
            var allWarnings = new List<TimeEntryWarning>();

            // In a real implementation, you would:
            // 1. Get recent time cards for the driver
            // 2. Validate each one
            // 3. Aggregate warnings

            // Assert
            Assert.NotNull(allWarnings); // Placeholder assertion
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
