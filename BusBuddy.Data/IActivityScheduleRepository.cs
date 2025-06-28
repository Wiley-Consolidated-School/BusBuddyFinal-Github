using System;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Data
{
    public interface IActivityScheduleRepository
    {
        List<ActivitySchedule> GetAllScheduledActivities();
        ActivitySchedule GetScheduledActivityById(int id);
        List<ActivitySchedule> GetScheduledActivitiesByDate(DateTime date);
        List<ActivitySchedule> GetScheduledActivitiesByDriver(int DriverId);
        List<ActivitySchedule> GetScheduledActivitiesByBus(int busId);
        List<ActivitySchedule> GetScheduledActivitiesByTripType(string tripType);
        int AddScheduledActivity(ActivitySchedule scheduledActivity);
        bool UpdateScheduledActivity(ActivitySchedule scheduledActivity);
        bool DeleteScheduledActivity(int id);
    }
}

