using System;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Data
{
    public interface ISchoolCalendarRepository
    {
        List<SchoolCalendar> GetAllCalendarEntries();
        SchoolCalendar GetCalendarEntryById(int id);
        List<SchoolCalendar> GetCalendarEntriesByDateRange(DateTime startDate, DateTime endDate);
        List<SchoolCalendar> GetCalendarEntriesByCategory(string category);
        List<SchoolCalendar> GetCalendarEntriesByRouteNeeded(bool routeNeeded);
        int AddCalendarEntry(SchoolCalendar calendarEntry);
        bool UpdateCalendarEntry(SchoolCalendar calendarEntry);
        bool DeleteCalendarEntry(int id);

        // Additional methods for form compatibility
        int Add(SchoolCalendar calendarEntry);
        bool Update(SchoolCalendar calendarEntry);
        bool Delete(int id);
    }
}
