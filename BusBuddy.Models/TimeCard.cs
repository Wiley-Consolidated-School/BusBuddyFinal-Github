using System;

namespace BusBuddy.Models
{
    public class TimeCard
    {
        public int TimeCardID { get; set; }
        public int? DriverID { get; set; }
        public DateTime? Date { get; set; }
        public string? DayType { get; set; }  // Normal Day, Route Day
        public TimeSpan? AMClockIn { get; set; }
        public TimeSpan? LunchClockOut { get; set; }
        public TimeSpan? LunchClockIn { get; set; }
        public TimeSpan? PMClockOut { get; set; }
        public TimeSpan? RouteAMClockOut { get; set; }
        public TimeSpan? RouteAMClockIn { get; set; }
        public TimeSpan? RoutePMClockOut { get; set; }
        public TimeSpan? RoutePMClockIn { get; set; }        public decimal? TotalTime { get; set; }
        public decimal? Overtime { get; set; }
        public decimal? PTOHours { get; set; }
        public decimal? WeeklyTotal { get; set; }
        public decimal? MonthlyTotal { get; set; }
        public string? Notes { get; set; }

        // Navigation property
        public Driver? Driver { get; set; }
    }
}
