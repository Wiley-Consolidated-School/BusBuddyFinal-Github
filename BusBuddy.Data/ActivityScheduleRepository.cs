using System;
using System.Collections.Generic;
using System.Data;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{
    public class ActivityScheduleRepository : BaseRepository, IActivityScheduleRepository
    {
        public ActivityScheduleRepository() : base()
        {
        }

        public List<ActivitySchedule> GetAllScheduledActivities()
        {
            // Return sample data when database is not available
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    var schedules = connection.Query<ActivitySchedule>("SELECT * FROM ActivitySchedules").AsList();
                    return schedules;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database not available: {ex.Message}. Returning sample data.");
                return new List<ActivitySchedule>
                {
                    new ActivitySchedule { ScheduleID = 1, TripType = "Field Trip", ScheduledDestination = "Park", Date = DateTime.Today.ToString("yyyy-MM-dd"), ScheduledLeaveTime = TimeSpan.FromHours(9), ScheduledReturnTime = TimeSpan.FromHours(15) },
                    new ActivitySchedule { ScheduleID = 2, TripType = "Sports Trip", ScheduledDestination = "Sports Complex", Date = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"), ScheduledLeaveTime = TimeSpan.FromHours(8.5), ScheduledReturnTime = TimeSpan.FromHours(17) },
                    new ActivitySchedule { ScheduleID = 3, TripType = "Academic Trip", ScheduledDestination = "University", Date = DateTime.Today.AddDays(5).ToString("yyyy-MM-dd"), ScheduledLeaveTime = TimeSpan.FromHours(8), ScheduledReturnTime = TimeSpan.FromHours(16.5) }
                };
            }
        }

        public ActivitySchedule GetScheduledActivityById(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return connection.QuerySingleOrDefault<ActivitySchedule>(
                    "SELECT * FROM ActivitySchedule WHERE ScheduleID = @ScheduleID",
                    new { ScheduleID = id });
            }
        }

        public List<ActivitySchedule> GetScheduledActivitiesByDate(DateTime date)
        {
            // Format the date string to match the storage format in the database
            string formattedDate = date.ToString("yyyy-MM-dd");

            using (var connection = CreateConnection())
            {
                connection.Open();
                var scheduledActivities = connection.Query<ActivitySchedule>(
                    "SELECT * FROM ActivitySchedule WHERE Date = @Date",
                    new { Date = formattedDate }).AsList();
                return scheduledActivities;
            }
        }

        public List<ActivitySchedule> GetScheduledActivitiesByDriver(int driverId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var scheduledActivities = connection.Query<ActivitySchedule>(
                    "SELECT * FROM ActivitySchedule WHERE ScheduledDriverID = @DriverID",
                    new { DriverID = driverId }).AsList();
                return scheduledActivities;
            }
        }

        public List<ActivitySchedule> GetScheduledActivitiesByVehicle(int vehicleId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var scheduledActivities = connection.Query<ActivitySchedule>(
                    "SELECT * FROM ActivitySchedule WHERE ScheduledVehicleID = @VehicleID",
                    new { VehicleID = vehicleId }).AsList();
                return scheduledActivities;
            }
        }

        public List<ActivitySchedule> GetScheduledActivitiesByTripType(string tripType)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var scheduledActivities = connection.Query<ActivitySchedule>(
                    "SELECT * FROM ActivitySchedule WHERE TripType = @TripType",
                    new { TripType = tripType }).AsList();
                return scheduledActivities;
            }
        }

        public int AddScheduledActivity(ActivitySchedule scheduledActivity)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    INSERT INTO ActivitySchedule (
                        Date, TripType, ScheduledVehicleID,
                        ScheduledDestination, ScheduledLeaveTime,
                        ScheduledEventTime, ScheduledRiders, ScheduledDriverID
                    )
                    VALUES (
                        @Date, @TripType, @ScheduledVehicleID,
                        @ScheduledDestination, @ScheduledLeaveTime,
                        @ScheduledEventTime, @ScheduledRiders, @ScheduledDriverID
                    );
                    SELECT SCOPE_IDENTITY();";

                return connection.QuerySingle<int>(sql, scheduledActivity);
            }
        }

        public bool UpdateScheduledActivity(ActivitySchedule scheduledActivity)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    UPDATE ActivitySchedule
                    SET Date = @Date,
                        TripType = @TripType,
                        ScheduledVehicleID = @ScheduledVehicleID,
                        ScheduledDestination = @ScheduledDestination,
                        ScheduledLeaveTime = @ScheduledLeaveTime,
                        ScheduledEventTime = @ScheduledEventTime,
                        ScheduledRiders = @ScheduledRiders,
                        ScheduledDriverID = @ScheduledDriverID
                    WHERE ScheduleID = @ScheduleID";

                var rowsAffected = connection.Execute(sql, scheduledActivity);
                return rowsAffected > 0;
            }
        }

        public bool DeleteScheduledActivity(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = "DELETE FROM ActivitySchedule WHERE ScheduleID = @ScheduleID";
                var rowsAffected = connection.Execute(sql, new { ScheduleID = id });
                return rowsAffected > 0;
            }
        }
    }
}
