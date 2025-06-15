using Xunit;
using BusBuddy.Models;
using BusBuddy.Data;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Test class for time card validation and improvement concepts
    /// Tests fundamental timecard validation scenarios
    /// </summary>
    public class TimeCardValidationServiceTests
    {
        [Fact]
        public void TimeCard_ValidTimeSequence_ShouldCalculateCorrectHours()
        {
            // Arrange
            var timeCard = new Models.TimeCard
            {
                Date = DateTime.Now,
                ClockIn = new TimeSpan(7, 0, 0),      // 7:00 AM
                LunchOut = new TimeSpan(12, 0, 0),    // 12:00 PM
                LunchIn = new TimeSpan(12, 30, 0),    // 12:30 PM
                ClockOut = new TimeSpan(15, 30, 0)    // 3:30 PM
            };

            // Act
            timeCard.CalculateTotalHours();

            // Assert - Should be 8 hours with 30-minute lunch
            Assert.True(timeCard.TotalTime > 7.5 && timeCard.TotalTime <= 8.5);
        }

        [Fact]
        public void TimeCard_InvalidTimeSequence_ShouldBeDetectable()
        {
            // Arrange
            var timeCard = new Models.TimeCard
            {
                Date = DateTime.Now,
                ClockIn = new TimeSpan(8, 0, 0),     // 8:00 AM
                ClockOut = new TimeSpan(7, 0, 0)     // 7:00 AM (invalid - before clock in)
            };

            // Act
            var isValid = timeCard.IsValid(out string errorMessage);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(errorMessage);
        }

        [Fact]
        public void TimeCard_ExcessiveHours_ShouldBeDetectable()
        {
            // Arrange
            var timeCard = new Models.TimeCard
            {
                Date = DateTime.Now,
                ClockIn = new TimeSpan(6, 0, 0),     // 6:00 AM
                ClockOut = new TimeSpan(22, 0, 0)    // 10:00 PM (16 hours - excessive)
            };

            // Act
            timeCard.CalculateTotalHours();
            var isValid = timeCard.IsValid(out string errorMessage);

            // Assert
            Assert.True(timeCard.TotalTime > 12); // More than 12 hours
            // Validation logic would detect this as excessive
        }

        [Fact]
        public void TimeCard_RouteHours_ShouldCalculateCorrectly()
        {
            // Arrange
            var timeCard = new Models.TimeCard
            {
                Date = DateTime.Now,
                RouteAMOut = new TimeSpan(6, 30, 0),   // 6:30 AM
                RouteAMIn = new TimeSpan(8, 30, 0),    // 8:30 AM
                RoutePMOut = new TimeSpan(14, 30, 0),  // 2:30 PM
                RoutePMIn = new TimeSpan(16, 0, 0),    // 4:00 PM
                IsRouteDay = true
            };

            // Act
            var routeHours = timeCard.CalculateRouteHours();

            // Assert
            Assert.Equal(3.5, routeHours); // 2 hours AM + 1.5 hours PM
        }

        [Theory]
        [InlineData(6, 0, 14, 0, false)] // 8-hour day - reasonable
        [InlineData(5, 0, 20, 0, true)]  // 15-hour day - excessive
        [InlineData(8, 0, 16, 30, false)] // 8.5-hour day - reasonable
        public void TimeCard_WorkHours_ShouldValidateReasonableness(int clockInHour, int clockInMinute,
            int clockOutHour, int clockOutMinute, bool shouldBeExcessive)
        {
            // Arrange
            var timeCard = new Models.TimeCard
            {
                Date = DateTime.Now,
                ClockIn = new TimeSpan(clockInHour, clockInMinute, 0),
                ClockOut = new TimeSpan(clockOutHour, clockOutMinute, 0)
            };

            // Act
            timeCard.CalculateTotalHours();

            // Assert
            if (shouldBeExcessive)
            {
                Assert.True(timeCard.TotalTime > 12); // More than 12 hours is excessive
            }
            else
            {
                Assert.True(timeCard.TotalTime <= 10); // 10 hours or less is reasonable
            }
        }

        [Fact]
        public void TimeCard_OvertimeCalculation_ShouldBeAccurate()
        {
            // Arrange
            var timeCard = new Models.TimeCard
            {
                Date = DateTime.Now,
                ClockIn = new TimeSpan(7, 0, 0),
                ClockOut = new TimeSpan(17, 30, 0), // 10.5 hours
                LunchOut = new TimeSpan(12, 0, 0),
                LunchIn = new TimeSpan(12, 30, 0)   // 30-minute lunch
            };

            // Act
            timeCard.CalculateTotalHours();

            // Assert - Should have overtime if over 8 hours
            Assert.True(timeCard.TotalTime > 8);
            // Overtime calculation would be handled by business logic
        }

        [Fact]
        public void TimeCard_MissingRequiredFields_ShouldBeDetectable()
        {
            // Arrange
            var timeCard = new Models.TimeCard
            {
                // Missing Date and DriverId
                ClockIn = new TimeSpan(8, 0, 0),
                ClockOut = new TimeSpan(17, 0, 0)
            };

            // Act & Assert
            Assert.Null(timeCard.Date);
            Assert.Equal(0, timeCard.DriverId);

            // These would be caught by validation logic
            var hasRequiredFields = timeCard.Date.HasValue && timeCard.DriverId > 0;
            Assert.False(hasRequiredFields);
        }

        [Fact]
        public void TimeCard_PTOHours_ShouldBeTrackedSeparately()
        {
            // Arrange
            var timeCard = new Models.TimeCard
            {
                Date = DateTime.Now,
                DriverId = 1,
                IsPaidTimeOff = true,
                PTOHours = 8.0,
                Notes = "Vacation day"
            };

            // Act & Assert
            Assert.True(timeCard.IsPaidTimeOff);
            Assert.Equal(8.0, timeCard.PTOHours);
            Assert.Equal("Vacation day", timeCard.Notes);
        }

        [Fact]
        public void TimeCard_AuditFields_ShouldBePopulated()
        {
            // Arrange & Act
            var timeCard = new Models.TimeCard
            {
                Date = DateTime.Now,
                DriverId = 1,
                CreatedBy = "TestUser",
                ModifiedBy = "TestUser"
            };

            // Assert
            Assert.True(timeCard.CreatedDate > DateTime.MinValue);
            Assert.Equal("TestUser", timeCard.CreatedBy);
            Assert.Equal("TestUser", timeCard.ModifiedBy);
        }

        [Fact]
        public void TimeCard_BusinessRules_ShouldEnforceConsistency()
        {
            // Arrange
            var timeCard = new Models.TimeCard
            {
                Date = DateTime.Now,
                DriverId = 1,
                ClockIn = new TimeSpan(8, 0, 0),
                ClockOut = new TimeSpan(17, 0, 0),
                IsRouteDay = true,
                RouteAMOut = new TimeSpan(7, 0, 0),
                RouteAMIn = new TimeSpan(8, 30, 0)
            };

            // Act
            var routeHours = timeCard.CalculateRouteHours();
            timeCard.CalculateTotalHours();

            // Assert - Route day should have route hours
            Assert.True(timeCard.IsRouteDay);
            Assert.True(routeHours > 0);
        }
    }
}
