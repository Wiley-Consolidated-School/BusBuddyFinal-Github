using System;
using System.Collections.Generic;
using System.Data;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{
    public class ActivityRepository : BaseRepository, IActivityRepository
    {
        public ActivityRepository() : base()
        {
        }

        public List<Activity> GetAllActivities()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var activities = connection.Query<Activity>("SELECT * FROM Activities").AsList();
                return activities;
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
            using (var connection = CreateConnection())
            {
                connection.Open();
                var activities = connection.Query<Activity>(
                    "SELECT * FROM Activities WHERE Date = @Date",
                    new { Date = date }).AsList();
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
        }

        public int AddActivity(Activity activity)
        {
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
                    SELECT CAST(SCOPE_IDENTITY() AS INT)";

                return connection.QuerySingle<int>(sql, activity);
            }
        }

        public bool UpdateActivity(Activity activity)
        {
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
