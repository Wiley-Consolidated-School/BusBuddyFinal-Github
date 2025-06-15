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
            using (var connection = CreateConnection())
            {
                connection.Open();
                var scheduledActivities = connection.Query<ActivitySchedule>("SELECT * FROM ActivitySchedule").AsList();
                return scheduledActivities;
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
            using (var connection = CreateConnection())
            {
                connection.Open();
                var scheduledActivities = connection.Query<ActivitySchedule>(
                    "SELECT * FROM ActivitySchedule WHERE Date = @Date",
                    new { Date = date }).AsList();
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
