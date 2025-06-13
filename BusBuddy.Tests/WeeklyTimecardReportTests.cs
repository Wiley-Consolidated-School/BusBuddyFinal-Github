using Xunit;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.Business;
using System;

namespace BusBuddy.Tests
{
    public class WeeklyTimecardReportTests
    {
        [Fact]
        public void TestWeeklyReport_June9Week_GeneratesCorrectFormat()
        {
            // Arrange
            var reportGenerator = new WeeklyTimecardReport();
            var june9th = new DateTime(2025, 6, 9);

            // Act
            var report = reportGenerator.GenerateJune9WeekReport("Steve McKitrick");

            // Assert
            Assert.NotNull(report);
            Assert.Contains("WEEKLY TIMECARD REPORT", report);
            Assert.Contains("Steve McKitrick", report);
            Assert.Contains("06/09/2025", report); // June 9th should be in the report
            Assert.Contains("Day Type", report);
            Assert.Contains("Clock In", report);
            Assert.Contains("WEEKLY TOTALS", report);
        }

        [Fact]
        public void TestWeekStartDate_June9th_ReturnsCorrectMonday()
        {
            // Arrange
            var reportGenerator = new WeeklyTimecardReport();
            var june9th = new DateTime(2025, 6, 9); // This is a Monday

            // Act
            var weekStart = reportGenerator.GetWeekStartDate(june9th);

            // Assert
            Assert.Equal(new DateTime(2025, 6, 9), weekStart); // June 9th is already Monday
            Assert.Equal(DayOfWeek.Monday, weekStart.DayOfWeek);
        }

        [Fact]
        public void TestWeekEndDate_June9th_ReturnsCorrectSunday()
        {
            // Arrange
            var reportGenerator = new WeeklyTimecardReport();
            var june9th = new DateTime(2025, 6, 9); // Monday

            // Act
            var weekEnd = reportGenerator.GetWeekEndDate(june9th);

            // Assert
            Assert.Equal(new DateTime(2025, 6, 15), weekEnd); // Should be Sunday June 15th
            Assert.Equal(DayOfWeek.Sunday, weekEnd.DayOfWeek);
        }

        [Fact]
        public void TestWeekStartDate_MidWeek_ReturnsCorrectMonday()
        {
            // Arrange
            var reportGenerator = new WeeklyTimecardReport();
            var june11th = new DateTime(2025, 6, 11); // This is a Wednesday

            // Act
            var weekStart = reportGenerator.GetWeekStartDate(june11th);

            // Assert
            Assert.Equal(new DateTime(2025, 6, 9), weekStart); // Should be Monday June 9th
            Assert.Equal(DayOfWeek.Monday, weekStart.DayOfWeek);
        }

        [Fact]
        public void TestWeekStartDate_Sunday_ReturnsCorrectMonday()
        {
            // Arrange
            var reportGenerator = new WeeklyTimecardReport();
            var june15th = new DateTime(2025, 6, 15); // This is a Sunday

            // Act
            var weekStart = reportGenerator.GetWeekStartDate(june15th);

            // Assert
            Assert.Equal(new DateTime(2025, 6, 9), weekStart); // Should be Monday June 9th
            Assert.Equal(DayOfWeek.Monday, weekStart.DayOfWeek);
        }

        [Fact]
        public void TestReportFormat_ContainsExpectedStructure()
        {
            // Arrange
            var reportGenerator = new WeeklyTimecardReport();
            var testDate = new DateTime(2025, 6, 9);

            // Act
            var report = reportGenerator.GenerateWeeklyReport(testDate, "Test Employee");

            // Assert
            // Check for proper formatting
            Assert.Contains("Day        Date      Day Type", report);
            Assert.Contains("Clock In  Lunch Out Lunch In  Clock Out", report);
            Assert.Contains("Total   OT    PTO", report);
            Assert.Contains("Mon 06/09/2025", report);
            Assert.Contains("Tue 06/10/2025", report);
            Assert.Contains("Wed 06/11/2025", report);
            Assert.Contains("Thu 06/12/2025", report);
            Assert.Contains("Fri 06/13/2025", report);
            Assert.Contains("Sat 06/14/2025", report);
            Assert.Contains("Sun 06/15/2025", report);
        }

        [Fact]
        public void TestReportCalculations_WithSampleData()
        {
            // This test demonstrates how the report would look with your real data
            // The actual data would come from the database, but this shows the concept

            // Arrange
            var reportGenerator = new WeeklyTimecardReport();
            var june9th = new DateTime(2025, 6, 9);

            // Act
            var report = reportGenerator.GenerateWeeklyReport(june9th, "Steve McKitrick");

            // Assert
            Assert.Contains("SUMMARY:", report);
            Assert.Contains("Regular Hours:", report);
            Assert.Contains("Overtime Hours:", report);
            Assert.Contains("PTO Hours:", report);
            Assert.Contains("Total Paid Hours:", report);
        }
    }
}
