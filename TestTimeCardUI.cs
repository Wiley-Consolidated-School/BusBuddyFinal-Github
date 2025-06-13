using System;
using System.Collections.Generic;
using BusBuddy.Data;
using BusBuddy.Models;

class TestTimeCardUI
{
    static void Main()
    {
        Console.WriteLine("Testing TimeCard repository and data loading...");

        try
        {
            var timeCardRepo = new TimeCardRepository();
            var timeCards = timeCardRepo.GetAllTimeCards();

            Console.WriteLine($"Found {timeCards.Count} TimeCard records:");
            foreach (var tc in timeCards)
            {
                Console.WriteLine($"  ID: {tc.TimeCardID}, Date: {tc.Date}, Type: {tc.DayType}, Total: {tc.TotalTime}");
            }

            var activityRepo = new ActivityRepository();
            var activities = activityRepo.GetAllActivities();

            Console.WriteLine($"\nFound {activities.Count} Activity records:");
            foreach (var activity in activities)
            {
                Console.WriteLine($"  ID: {activity.ActivityID}, Date: {activity.Date}, Type: {activity.ActivityType}, Destination: {activity.Destination}");
            }

            var calendarRepo = new SchoolCalendarRepository();
            var calendarEntries = calendarRepo.GetAllCalendarEntries();

            Console.WriteLine($"\nFound {calendarEntries.Count} SchoolCalendar records:");
            foreach (var entry in calendarEntries)
            {
                Console.WriteLine($"  ID: {entry.CalendarID}, Date: {entry.Date}, Category: {entry.Category}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
