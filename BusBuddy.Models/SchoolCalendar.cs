using System;

namespace BusBuddy.Models
{
    public class SchoolCalendar
    {
        public int CalendarID { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Category { get; set; }  // School Day, Holiday, Thanksgiving Break, Christmas Break, Spring Break, Key Event
        public string? Description { get; set; }
        public int RouteNeeded { get; set; }
        public string? Notes { get; set; }
        
        // Helper property to convert between int and bool
        public bool IsRouteNeeded
        {
            get { return RouteNeeded > 0; }
            set { RouteNeeded = value ? 1 : 0; }
        }
    }
}
