using System;
using BusBuddy.Data;
using BusBuddy.Models;

class TestActivitySchedule
{
    static void Main()
    {
        try
        {
            Console.WriteLine("Testing ActivitySchedule Repository...");

            var repository = new ActivityScheduleRepository();
            var activities = repository.GetAllScheduledActivities();

            Console.WriteLine($"Found {activities.Count} activity schedules:");

            foreach (var activity in activities)
            {
                Console.WriteLine($"- ID: {activity.ScheduleID}");
                Console.WriteLine($"  Date: {activity.Date}");
                Console.WriteLine($"  TripType: {activity.TripType}");
                Console.WriteLine($"  Destination: {activity.ScheduledDestination}");
                Console.WriteLine($"  LeaveTime: {activity.ScheduledLeaveTime}");
                Console.WriteLine($"  EventTime: {activity.ScheduledEventTime}");
                Console.WriteLine($"  ReturnTime: {activity.ScheduledReturnTime}");
                Console.WriteLine($"  Riders: {activity.ScheduledRiders}");
                Console.WriteLine($"  DriverID: {activity.ScheduledDriverID}");
                Console.WriteLine($"  VehicleID: {activity.ScheduledVehicleID}");
                Console.WriteLine($"  Notes: {activity.Notes}");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}
