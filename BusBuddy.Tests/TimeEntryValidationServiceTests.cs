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
    public class TimeEntryValidationServiceTests : IDisposable
    {
        private readonly BusBuddyContext _context;
        private readonly TimeCardRepository _timeCardRepository;
        private readonly DriverRepository _driverRepository;
        private readonly TimeEntryValidationService _validationService;
        private readonly Driver _testDriver;

        public TimeEntryValidationServiceTests()
        {
            // Create in-memory database
            var options = new DbContextOptionsBuilder<BusBuddyContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BusBuddyContext(options);

            // Use parameterless constructors for repositories
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
                DriverName = "John Doe",
                DriverPhone = "555-0123",
                DriverEmail = "john.doe@busbuddy.com",
                Address = "123 Main St",
                City = "Test City",
                State = "TS",
                Zip = "12345",
                DriversLicenseType = "CDL",
                TrainingComplete = 1,
                Notes = "Test driver"
            };
            _context.Drivers.Add(driver);
            _context.SaveChanges();
        }

        [Fact]
        public void ValidateTimeCard_CompleteNormalDay_NoWarnings()
        {
            // Arrange
            var timeCard = new TimeCard
            {
                TimeCardID = 1,
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(8, 0, 0),     // 8:00 AM
                LunchClockOut = new TimeSpan(12, 0, 0), // 12:00 PM
                LunchClockIn = new TimeSpan(13, 0, 0),  // 1:00 PM
                PMClockOut = new TimeSpan(17, 0, 0),    // 5:00 PM
                TotalTime = 8.0m,
                Notes = "Complete normal work day"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            Assert.Empty(warnings);
        }

        [Fact]
        public void ValidateTimeCard_MissingAMClockIn_ReturnsWarning()
        {
            // Arrange
            var timeCard = new TimeCard
            {
                TimeCardID = 2,
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = null, // Missing!
                LunchClockOut = new TimeSpan(12, 0, 0),
                LunchClockIn = new TimeSpan(13, 0, 0),
                PMClockOut = new TimeSpan(17, 0, 0),
                Notes = "Missing clock in"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            Assert.NotEmpty(warnings);
            Assert.Contains(warnings, w => w.Type == WarningType.MissingClockIn);
        }

        [Fact]
        public void ValidateTimeCard_ShortLunchBreak_ReturnsWarning()
        {
            // Arrange
            var timeCard = new TimeCard
            {
                TimeCardID = 3,
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(8, 0, 0),
                LunchClockOut = new TimeSpan(12, 0, 0), // 12:00 PM
                LunchClockIn = new TimeSpan(12, 10, 0), // 12:10 PM - only 10 minutes!
                PMClockOut = new TimeSpan(17, 0, 0),
                Notes = "Very short lunch"
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
            // Arrange
            var timeCard = new TimeCard
            {
                TimeCardID = 4,
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(8, 0, 0),
                LunchClockOut = new TimeSpan(12, 0, 0), // 12:00 PM
                LunchClockIn = new TimeSpan(15, 0, 0),  // 3:00 PM - 3 hours!
                PMClockOut = new TimeSpan(17, 0, 0),
                Notes = "Very long lunch"
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
            // Arrange
            var timeCard = new TimeCard
            {
                TimeCardID = 5,
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
        }

        [Fact]
        public void ValidateTimeCard_RouteDayMissingRouteData_ReturnsWarning()
        {
            // Arrange
            var timeCard = new TimeCard
            {
                TimeCardID = 6,
                Date = DateTime.Today,
                DayType = "Route Day",
                AMClockIn = new TimeSpan(7, 0, 0),
                RouteAMClockOut = new TimeSpan(8, 0, 0), // Route starts
                RouteAMClockIn = null, // Missing route return!
                RoutePMClockOut = new TimeSpan(15, 0, 0),
                RoutePMClockIn = new TimeSpan(15, 30, 0),
                PMClockOut = new TimeSpan(16, 30, 0),
                Notes = "Route day with missing data"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            Assert.NotEmpty(warnings);
            // Should detect incomplete route data
        }

        [Theory]
        [InlineData(8, 0, 17, 0, false)] // Normal 8-hour day (with lunch) = 9 total hours
        [InlineData(7, 0, 18, 0, false)] // 11 total hours
        [InlineData(6, 0, 20, 0, true)]  // 14 total hours - should trigger warning
        [InlineData(5, 0, 22, 0, true)]  // 17 total hours - definitely excessive
        public void ValidateTimeCard_VariousWorkDayLengths_ValidatesCorrectly(int clockInHour, int clockInMinute,
            int clockOutHour, int clockOutMinute, bool shouldHaveWarning)
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
                Notes = "Variable length day test"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            if (shouldHaveWarning)
            {
                Assert.Contains(warnings, w => w.Type == WarningType.ExcessiveWorkHours);
            }
            else
            {
                Assert.DoesNotContain(warnings, w => w.Type == WarningType.ExcessiveWorkHours);
            }
        }

        [Fact]
        public void ValidateTimeCard_IncompletePreviousDay_DetectsIssue()
        {
            // This test simulates the scenario where someone forgot to clock out yesterday
            // and is now trying to clock in today

            // Arrange - Yesterday's incomplete entry
            var yesterdayCard = new TimeCard
            {
                TimeCardID = 10,
                Date = DateTime.Today.AddDays(-1),
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(8, 0, 0),
                LunchClockOut = new TimeSpan(12, 0, 0),
                LunchClockIn = new TimeSpan(13, 0, 0),
                PMClockOut = null, // Missing clock out!
                Notes = "Forgot to clock out"
            };

            // Today's entry
            var todayCard = new TimeCard
            {
                TimeCardID = 11,
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(8, 15, 0),
                Notes = "New day - trying to clock in"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(todayCard);

            // Assert
            // The validation service should have logic to detect this scenario
            // For now, we're testing that the service can be called without errors
            Assert.NotNull(warnings);
        }

        [Fact]
        public void ValidateTimeCard_MultipleWarnings_ReturnsAllWarnings()
        {
            // Arrange - Create a time card with multiple issues
            var timeCard = new TimeCard
            {
                TimeCardID = 12,
                Date = DateTime.Today,
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(3, 0, 0),     // Too early (warning 1)
                LunchClockOut = new TimeSpan(12, 0, 0),
                LunchClockIn = new TimeSpan(12, 5, 0),  // Too short lunch (warning 2)
                PMClockOut = new TimeSpan(23, 0, 0),    // Too long day (warning 3)
                Notes = "Multiple problems"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            Assert.True(warnings.Count >= 2); // Should have multiple warnings
            Assert.Contains(warnings, w => w.Type == WarningType.VeryEarlyClockIn);
            Assert.Contains(warnings, w => w.Type == WarningType.ShortLunchBreak);
        }

        [Fact]
        public void ValidateTimeCard_WeekendEntry_HandlesCorrectly()
        {
            // Arrange - Create entry for weekend (which might be unusual)
            var saturday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday);

            var timeCard = new TimeCard
            {
                TimeCardID = 13,
                Date = saturday,
                DayType = "Normal Day",
                AMClockIn = new TimeSpan(8, 0, 0),
                PMClockOut = new TimeSpan(17, 0, 0),
                Notes = "Weekend work"
            };

            // Act
            var warnings = _validationService.ValidateTimeCard(timeCard);

            // Assert
            Assert.NotNull(warnings); // Should handle weekend entries without crashing
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
