using System;
using System.Globalization;

namespace BusBuddy.Models
{
    public class SchoolCalendar
    {
        public int CalendarID { get; set; }
        public string? Date { get; set; }
        public string? EndDate { get; set; }
        public string? Category { get; set; }  // School Day, Holiday, Thanksgiving Break, Christmas Break, Spring Break, Key Event

        // Helper properties to get/set Date as DateTime
        public DateTime? DateAsDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(Date)) return null;
                if (DateTime.TryParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    return result;
                if (DateTime.TryParse(Date, out var fallbackResult))
                    return fallbackResult;
                return null;
            }
            set => Date = value?.ToString("yyyy-MM-dd");
        }

        public DateTime? EndDateAsDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(EndDate)) return null;
                if (DateTime.TryParseExact(EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    return result;
                if (DateTime.TryParse(EndDate, out var fallbackResult))
                    return fallbackResult;
                return null;
            }
            set => EndDate = value?.ToString("yyyy-MM-dd");
        }
        public string? Description { get; set; }
        public int RouteNeeded { get; set; }
        public string? Notes { get; set; }

        // Additional properties for form compatibility
        public int Id => CalendarID;
        public string? DayType => Category;

        // Helper property to convert between int and bool
        public bool IsRouteNeeded
        {
            get { return RouteNeeded > 0; }
            set { RouteNeeded = value ? 1 : 0; }
        }
    }
}
