using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{
    public class ActivityRepository : BaseRepository, IActivityRepository
    {
        public ActivityRepository() : base()
        {
        }

        public ActivityRepository(string connectionString, string providerName) : base(connectionString, providerName)
        {
        }

        public List<Activity> GetAllActivities()
        {
            // GRACEFUL DEGRADATION: Always return data, never throw exceptions
            try
            {
                if (!_isDatabaseAvailable)
                {
                    Console.WriteLine("📊 Database offline - returning sample activity data");
                    return GetSampleActivityData();
                }

                using (var connection = CreateConnection())
                {
                    connection.Open();
                    var activities = connection.Query<Activity>("SELECT * FROM Activities").AsList();
                    Console.WriteLine($"📊 Retrieved {activities.Count} activities from database");
                    return activities;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Database error in GetAllActivities: {ex.Message}. Returning sample data.");
                return GetSampleActivityData();
            }
        }

        private List<Activity> GetSampleActivityData()
        {
            return new List<Activity>
            {
                new Activity
                {
                    ActivityID = 1,
                    ActivityType = "Sports Trip",
                    Destination = "Regional Stadium",
                    Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    LeaveTime = "08:00",
                    ReturnTime = "16:00",
                    RequestedBy = "Athletic Department"
                },
                new Activity
                {
                    ActivityID = 2,
                    ActivityType = "Field Trip",
                    Destination = "Science Museum",
                    Date = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
                    LeaveTime = "09:00",
                    ReturnTime = "15:00",
                    RequestedBy = "Science Department"
                },
                new Activity
                {
                    ActivityID = 3,
                    ActivityType = "Competition",
                    Destination = "State Finals",
                    Date = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd"),
                    LeaveTime = "07:00",
                    ReturnTime = "18:00",
                    RequestedBy = "Athletic Department"
                },
                new Activity
                {
                    ActivityID = 4,
                    ActivityType = "Academic Trip",
                    Destination = "History Museum",
                    Date = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"),
                    LeaveTime = "10:00",
                    ReturnTime = "14:00",
                    RequestedBy = "History Department"
                }
            };
        }

        public Activity GetActivityById(int id)
        {
            try
            {
                if (!_isDatabaseAvailable)
                {
                    Console.WriteLine($"📊 Database offline - returning sample activity data for ID {id}");
                    return GetSampleActivityData().FirstOrDefault(a => a.ActivityID == id);
                }

                using (var connection = CreateConnection())
                {
                    connection.Open();
                    return connection.QuerySingleOrDefault<Activity>(
                        "SELECT * FROM Activities WHERE ActivityID = @ActivityID",
                        new { ActivityID = id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Database error in GetActivityById: {ex.Message}. Returning sample data.");
                return GetSampleActivityData().FirstOrDefault(a => a.ActivityID == id);
            }
        }

        public List<Activity> GetActivitiesByDate(DateTime date)
        {
            // Format the date string to match the storage format in the database
            string formattedDate = date.ToString("yyyy-MM-dd");
            using (var connection = CreateConnection())
            {
                connection.Open();
                var activities = connection.Query<Activity>(
                    "SELECT * FROM Activities WHERE Date = @Date",
                    new { Date = formattedDate }).AsList();
                return activities;
            }
        }

        public List<Activity> GetActivitiesByDriver(int DriverId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var activities = connection.Query<Activity>(
                    "SELECT * FROM Activities WHERE DriverId = @DriverId",
                    new { DriverId = DriverId }).AsList();
                return activities;
            }
        }

        public List<Activity> GetActivitiesByBus(int busId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var activities = connection.Query<Activity>(
                    "SELECT * FROM Activities WHERE AssignedVehicleID = @BusId",
                    new { BusId = busId }).AsList();
                return activities;
            }
        }

        public int AddActivity(Activity activity)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    INSERT INTO Activities (
                        Date, ActivityType, Destination,
                        LeaveTime, EventTime, RequestedBy,
                        AssignedVehicleID, DriverId
                    )
                    VALUES (
                        @Date, @ActivityType, @Destination,
                        @LeaveTime, @EventTime, @RequestedBy,
                        @AssignedVehicleID, @DriverId
                    );
                    SELECT SCOPE_IDENTITY()";
                return connection.QuerySingle<int>(sql, activity);
            }
        }

        public bool UpdateActivity(Activity activity)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    UPDATE Activities
                    SET Date = @Date,
                        ActivityType = @ActivityType,
                        Destination = @Destination,
                        LeaveTime = @LeaveTime,
                        EventTime = @EventTime,
                        RequestedBy = @RequestedBy,
                        AssignedVehicleID = @AssignedVehicleID,
                        DriverId = @DriverId
                    WHERE ActivityID = @ActivityID";
                var rowsAffected = connection.Execute(sql, activity);
                return rowsAffected > 0;
            }
        }

        public bool DeleteActivity(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = "DELETE FROM Activities WHERE ActivityID = @ActivityID";
                var rowsAffected = connection.Execute(sql, new { ActivityID = id });
                return rowsAffected > 0;
            }
        }
    }
}

