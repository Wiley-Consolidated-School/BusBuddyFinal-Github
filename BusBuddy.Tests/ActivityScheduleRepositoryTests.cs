using System;
using System.Collections.Generic;
using System.Linq;
using BusBuddy.Data;
using BusBuddy.Models;
using Xunit;

namespace BusBuddy.Tests
{
    public class ActivityScheduleRepositoryTests
    {
        [Fact]
        public void CanInsertAndRetrieveScheduledActivity()
        {
            var repo = new ActivityScheduleRepository();
            var testActivity = new ActivitySchedule
            {
                Date = DateTime.Today.AddDays(1),
                ActivityName = "Test Activity",
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(12),
                VehicleID = 1,
                DriverID = 1,
                Notes = "Unit test note",
                CalendarID = 1
            };
            int id = repo.AddScheduledActivity(testActivity);
            var retrieved = repo.GetScheduledActivityById(id);
            Assert.NotNull(retrieved);
            Assert.Equal("Test Activity", retrieved.ActivityName);
            // Cleanup
            repo.DeleteScheduledActivity(id);
        }

        [Fact]
        public void CanUpdateScheduledActivity()
        {
            var repo = new ActivityScheduleRepository();
            var testActivity = new ActivitySchedule
            {
                Date = DateTime.Today.AddDays(2),
                ActivityName = "Update Test",
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(9),
                VehicleID = 1,
                DriverID = 1,
                Notes = "Before update",
                CalendarID = 1
            };
            int id = repo.AddScheduledActivity(testActivity);
            var toUpdate = repo.GetScheduledActivityById(id);
            toUpdate.Notes = "After update";
            bool updated = repo.UpdateScheduledActivity(toUpdate);
            var updatedEntity = repo.GetScheduledActivityById(id);
            Assert.True(updated);
            Assert.Equal("After update", updatedEntity.Notes);
            // Cleanup
            repo.DeleteScheduledActivity(id);
        }

        [Fact]
        public void CanDeleteScheduledActivity()
        {
            var repo = new ActivityScheduleRepository();
            var testActivity = new ActivitySchedule
            {
                Date = DateTime.Today.AddDays(3),
                ActivityName = "Delete Test",
                StartTime = TimeSpan.FromHours(13),
                EndTime = TimeSpan.FromHours(14),
                VehicleID = 1,
                DriverID = 1,
                Notes = "To be deleted",
                CalendarID = 1
            };
            int id = repo.AddScheduledActivity(testActivity);
            bool deleted = repo.DeleteScheduledActivity(id);
            var shouldBeNull = repo.GetScheduledActivityById(id);
            Assert.True(deleted);
            Assert.Null(shouldBeNull);
        }
    }
}
