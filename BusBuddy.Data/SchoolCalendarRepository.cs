using System;
using System.Collections.Generic;
using System.Data;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{
    public class SchoolCalendarRepository : BaseRepository, ISchoolCalendarRepository
    {
        public SchoolCalendarRepository() : base()
        {
        }

        public List<SchoolCalendar> GetAllCalendarEntries()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var calendarEntries = connection.Query<SchoolCalendar>("SELECT * FROM SchoolCalendar").AsList();
                return calendarEntries;
            }
        }

        public SchoolCalendar GetCalendarEntryById(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return connection.QuerySingleOrDefault<SchoolCalendar>(
                    "SELECT * FROM SchoolCalendar WHERE CalendarID = @CalendarID",
                    new { CalendarID = id });
            }
        }

        public List<SchoolCalendar> GetCalendarEntriesByDateRange(DateTime startDate, DateTime endDate)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var calendarEntries = connection.Query<SchoolCalendar>(
                    @"SELECT * FROM SchoolCalendar 
                      WHERE (Date BETWEEN @StartDate AND @EndDate) 
                         OR (EndDate IS NOT NULL AND EndDate BETWEEN @StartDate AND @EndDate)
                         OR (Date <= @StartDate AND EndDate >= @EndDate)",
                    new { StartDate = startDate, EndDate = endDate }).AsList();
                return calendarEntries;
            }
        }

        public List<SchoolCalendar> GetCalendarEntriesByCategory(string category)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var calendarEntries = connection.Query<SchoolCalendar>(
                    "SELECT * FROM SchoolCalendar WHERE Category = @Category",
                    new { Category = category }).AsList();
                return calendarEntries;
            }
        }

        public List<SchoolCalendar> GetCalendarEntriesByRouteNeeded(bool routeNeeded)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var calendarEntries = connection.Query<SchoolCalendar>(
                    "SELECT * FROM SchoolCalendar WHERE RouteNeeded = @RouteNeeded",
                    new { RouteNeeded = routeNeeded }).AsList();
                return calendarEntries;
            }
        }

        public int AddCalendarEntry(SchoolCalendar calendarEntry)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    INSERT INTO SchoolCalendar (
                        Date, EndDate, Category, Description, RouteNeeded
                    )
                    VALUES (
                        @Date, @EndDate, @Category, @Description, @RouteNeeded
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT)";

                return connection.QuerySingle<int>(sql, calendarEntry);
            }
        }

        public bool UpdateCalendarEntry(SchoolCalendar calendarEntry)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    UPDATE SchoolCalendar 
                    SET Date = @Date, 
                        EndDate = @EndDate, 
                        Category = @Category, 
                        Description = @Description, 
                        RouteNeeded = @RouteNeeded
                    WHERE CalendarID = @CalendarID";

                var rowsAffected = connection.Execute(sql, calendarEntry);
                return rowsAffected > 0;
            }
        }

        public bool DeleteCalendarEntry(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = "DELETE FROM SchoolCalendar WHERE CalendarID = @CalendarID";
                var rowsAffected = connection.Execute(sql, new { CalendarID = id });
                return rowsAffected > 0;
            }
        }
    }
}
