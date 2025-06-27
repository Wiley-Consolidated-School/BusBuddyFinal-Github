using System;
using System.Collections.Generic;
using System.Data;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{    public class ActivityRepository : BaseRepository, IActivityRepository
    {
        public ActivityRepository() : base()
        {
        }

        public ActivityRepository(string connectionString, string providerName) : base(connectionString, providerName)
        {
        }

        public List<Activity> GetAllActivities()
        {
            // Return sample data when database is not available
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    var activities = connection.Query<Activity>("SELECT * FROM Activities").AsList();
                    return activities;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database not available: {ex.Message}. Returning sample data.");
                return new List<Activity>
                {
                    new Activity { ActivityID = 1, ActivityType = "Sports Trip", Destination = "Stadium", Date = DateTime.Today.ToString("yyyy-MM-dd"), LeaveTime = "08:00", ReturnTime = "16:00", RequestedBy = "Athletic Department" },
                    new Activity { ActivityID = 2, ActivityType = "Field Trip", Destination = "Science Museum", Date = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), LeaveTime = "09:00", ReturnTime = "15:00", RequestedBy = "Science Department" },
                    new Activity { ActivityID = 3, ActivityType = "Competition", Destination = "State Finals", Date = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd"), LeaveTime = "07:00", ReturnTime = "18:00", RequestedBy = "Athletic Department" }
                };
            }
        }

        public Activity GetActivityById(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return connection.QuerySingleOrDefault<Activity>(
                    "SELECT * FROM Activities WHERE ActivityID = @ActivityID",
                    new { ActivityID = id });
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

        public List<Activity> GetActivitiesByDriver(int driverId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var activities = connection.Query<Activity>(
                    "SELECT * FROM Activities WHERE DriverID = @DriverID",
                    new { DriverID = driverId }).AsList();
                return activities;
            }
        }

        public List<Activity> GetActivitiesByVehicle(int vehicleId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var activities = connection.Query<Activity>(
                    "SELECT * FROM Activities WHERE AssignedVehicleID = @VehicleID",
                    new { VehicleID = vehicleId }).AsList();
                return activities;
            }
        }        public int AddActivity(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));

            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    INSERT INTO Activities (
                        Date, ActivityType, Destination,
                        LeaveTime, EventTime, RequestedBy,
                        AssignedVehicleID, DriverID
                    )
                    VALUES (
                        @Date, @ActivityType, @Destination,
                        @LeaveTime, @EventTime, @RequestedBy,
                        @AssignedVehicleID, @DriverID
                    );
                    SELECT SCOPE_IDENTITY()";

                return connection.QuerySingle<int>(sql, activity);
            }
        }

        public bool UpdateActivity(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));

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
                        DriverID = @DriverID
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
