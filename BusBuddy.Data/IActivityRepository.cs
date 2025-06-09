using System;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Data
{
    public interface IActivityRepository
    {
        List<Activity> GetAllActivities();
        Activity GetActivityById(int id);
        List<Activity> GetActivitiesByDate(DateTime date);
        List<Activity> GetActivitiesByDriver(int driverId);
        List<Activity> GetActivitiesByVehicle(int vehicleId);
        int AddActivity(Activity activity);
        bool UpdateActivity(Activity activity);
        bool DeleteActivity(int id);
    }
}
