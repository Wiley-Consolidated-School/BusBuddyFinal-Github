using System;
using System.Collections.Generic;
using System.Globalization;

namespace BusBuddy.Models
{
    public class SchoolCalendar
    {
        public int CalendarID { get; set; }
        public DateTime CalendarDate { get; set; }
        public string? DayType { get; set; } // School Day, Holiday, Weekend, etc.
        public string? Category { get; set; } // Regular, Holiday, Break, etc.
        public string? Description { get; set; }
        public bool IsSchoolDay { get; set; } = true;
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Legacy properties for backward compatibility
        public string? Date
        {
            get => CalendarDate.ToString("yyyy-MM-dd");
            set
            {
                if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    CalendarDate = result;
                else if (DateTime.TryParse(value, out var fallbackResult))
                    CalendarDate = fallbackResult;
            }
        }
        public DateTime? DateAsDateTime
        {
            get => CalendarDate;
            set => CalendarDate = value ?? DateTime.Today;
        }
        public string? EndDate { get; set; } // For multi-day events
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
        public int RouteNeeded { get; set; }

        // Alias properties for form compatibility
        public int Id => CalendarID;
        public string? EventName => Description;
        public string? EventDate => Date;
        public string? EventType => Category;
        public string? EventDescription => Description;
        public string? EventLocation => Notes;
        public bool IsAllDay => true;
        public string? StartTime => "08:00";
        public string? EndTime => "15:00";

        // Helper property to convert between int and bool
        public bool IsRouteNeeded
        {
            get { return RouteNeeded > 0; }
            set { RouteNeeded = value ? 1 : 0; }
        }

        // Navigation property
        public ICollection<ActivitySchedule> ActivitySchedules { get; set; } = new List<ActivitySchedule>();
    }
}

