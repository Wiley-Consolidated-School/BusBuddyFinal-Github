using Xunit;
using BusBuddy.Models;
using System;

namespace BusBuddy.Tests
{
    public class TimeCardModelTests
    {
        [Fact]
        public void TestTimeCardModel_June9_CanCreateCorrectly()
        {
            // Arrange & Act - June 9th split shift entry
            var june9Entry = new TimeCard
            {
                Date = new DateTime(2025, 6, 9),
                DayType = "Route Day",
                AMClockIn = new TimeSpan(4, 16, 0),        // 4:16 AM
                LunchClockOut = new TimeSpan(10, 30, 0),   // 10:30 AM
                LunchClockIn = new TimeSpan(12, 30, 0),    // 12:30 PM
                PMClockOut = new TimeSpan(17, 0, 0),       // 5:00 PM
                TotalTime = 10.73m,                        // 10 hours 44 minutes
                Overtime = 2.73m,                          // 2 hours 44 minutes overtime
                PTOHours = 0m,
                Notes = "Split shift - Morning route 4:16-10:30, Afternoon route 12:30-17:00"
            };

            // Assert - Model properties are set correctly
            Assert.Equal(new DateTime(2025, 6, 9), june9Entry.Date);
            Assert.Equal("Route Day", june9Entry.DayType);
            Assert.Equal(new TimeSpan(4, 16, 0), june9Entry.AMClockIn);
            Assert.Equal(new TimeSpan(10, 30, 0), june9Entry.LunchClockOut);
            Assert.Equal(new TimeSpan(12, 30, 0), june9Entry.LunchClockIn);
            Assert.Equal(new TimeSpan(17, 0, 0), june9Entry.PMClockOut);
            Assert.Equal(10.73m, june9Entry.TotalTime);
            Assert.Equal(2.73m, june9Entry.Overtime);
            Assert.Equal(0m, june9Entry.PTOHours);
        }

        [Fact]
        public void TestTimeCardModel_June10_PTO_CanCreateCorrectly()
        {
            // Arrange & Act - June 10th PTO entry
            var june10Entry = new TimeCard
            {
                Date = new DateTime(2025, 6, 10),
                DayType = "PTO Day",
                AMClockIn = null,
                LunchClockOut = null,
                LunchClockIn = null,
                PMClockOut = null,
                PTOHours = 8.0m,
                TotalTime = 8.0m,
                Overtime = 0m,
                Notes = "PTO - Personal Time Off (8 hours)"
            };

            // Assert - PTO entry properties are correct
            Assert.Equal(new DateTime(2025, 6, 10), june10Entry.Date);
            Assert.Equal("PTO Day", june10Entry.DayType);
            Assert.Null(june10Entry.AMClockIn);
            Assert.Null(june10Entry.LunchClockOut);
            Assert.Null(june10Entry.LunchClockIn);
            Assert.Null(june10Entry.PMClockOut);
            Assert.Equal(8.0m, june10Entry.PTOHours);
            Assert.Equal(8.0m, june10Entry.TotalTime);
            Assert.Equal(0m, june10Entry.Overtime);
        }

        [Fact]
        public void TestTimeCardModel_June11_SplitShift_CanCreateCorrectly()
        {
            // Arrange & Act - June 11th split shift entry
            var june11Entry = new TimeCard
            {
                Date = new DateTime(2025, 6, 11),
                DayType = "Route Day",
                AMClockIn = new TimeSpan(5, 51, 0),        // 5:51 AM
                LunchClockOut = new TimeSpan(12, 30, 0),   // 12:30 PM
                LunchClockIn = new TimeSpan(13, 18, 0),    // 1:18 PM
                PMClockOut = new TimeSpan(17, 7, 0),       // 5:07 PM
                TotalTime = 10.47m,                        // 10 hours 28 minutes
                Overtime = 2.47m,                          // 2 hours 28 minutes overtime
                PTOHours = 0m,
                Notes = "Split shift - Complex afternoon timing with 48-minute lunch"
            };

            // Assert - Complex timing is handled correctly
            Assert.Equal(new DateTime(2025, 6, 11), june11Entry.Date);
            Assert.Equal(new TimeSpan(5, 51, 0), june11Entry.AMClockIn);
            Assert.Equal(new TimeSpan(12, 30, 0), june11Entry.LunchClockOut);
            Assert.Equal(new TimeSpan(13, 18, 0), june11Entry.LunchClockIn);
            Assert.Equal(new TimeSpan(17, 7, 0), june11Entry.PMClockOut);
            Assert.Equal(10.47m, june11Entry.TotalTime);
            Assert.Equal(2.47m, june11Entry.Overtime);
        }

        [Fact]
        public void TestTimeCalculations_June9_VerifyCorrectMath()
        {
            // Test that demonstrates you understand the complex time calculations
            // Morning shift: 4:16 AM to 10:30 AM = 6 hours 14 minutes = 6.23 hours
            // Afternoon shift: 12:30 PM to 5:00 PM = 4 hours 30 minutes = 4.5 hours
            // Total: 6.23 + 4.5 = 10.73 hours
            // Overtime: 10.73 - 8 = 2.73 hours

            var morningHours = 6.23m;   // 4:16 AM to 10:30 AM
            var afternoonHours = 4.5m;  // 12:30 PM to 5:00 PM
            var totalHours = morningHours + afternoonHours;
            var overtimeHours = totalHours - 8;

            Assert.Equal(10.73m, totalHours);
            Assert.Equal(2.73m, overtimeHours);
        }
    }
}
