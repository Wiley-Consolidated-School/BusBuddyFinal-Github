using System;
using System.ComponentModel.DataAnnotations;

namespace BusBuddy.Models
{
    /// <summary>
    /// Represents a time card entry for tracking driver hours
    /// Supports complex time clock scheme with normal work hours and route-specific hours
    /// </summary>
    public class TimeCard
    {
        [Key]
        public int TimeCardId { get; set; }

        [Required]
        public DateTime? Date { get; set; }

        // Normal work hours
        public TimeSpan? ClockIn { get; set; }
        public TimeSpan? LunchOut { get; set; }
        public TimeSpan? LunchIn { get; set; }
        public TimeSpan? ClockOut { get; set; }

        // Route-specific hours (for Truck Plaza route)
        public TimeSpan? RouteAMOut { get; set; }
        public TimeSpan? RouteAMIn { get; set; }
        public TimeSpan? RoutePMOut { get; set; }
        public TimeSpan? RoutePMIn { get; set; }

        // Calculated fields
        public double TotalTime { get; set; }
        public double Overtime { get; set; }

        // Route Day flag - distinguishes between regular work days and route days
        public bool IsRouteDay { get; set; }

        // Driver information
        [Required]
        public int DriverId { get; set; }
        public virtual Driver Driver { get; set; }

        // Audit fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

        // Additional fields for enhanced tracking
        public string Notes { get; set; }
        public bool IsPaidTimeOff { get; set; }
        public double PTOHours { get; set; }

        /// <summary>
        /// Calculate total normal work hours (excluding route hours)
        /// </summary>
        public double CalculateNormalHours()
        {
            if (!ClockIn.HasValue || !ClockOut.HasValue)
                return 0;

            var totalMinutes = (ClockOut.Value - ClockIn.Value).TotalMinutes;

            // Subtract lunch time if both lunch out and lunch in are recorded
            if (LunchOut.HasValue && LunchIn.HasValue)
            {
                var lunchMinutes = (LunchIn.Value - LunchOut.Value).TotalMinutes;
                totalMinutes -= lunchMinutes;
            }

            return Math.Max(0, totalMinutes / 60.0);
        }

        /// <summary>
        /// Calculate total route hours
        /// </summary>
        public double CalculateRouteHours()
        {
            double routeHours = 0;

            // AM Route hours
            if (RouteAMOut.HasValue && RouteAMIn.HasValue)
            {
                routeHours += (RouteAMIn.Value - RouteAMOut.Value).TotalHours;
            }

            // PM Route hours
            if (RoutePMOut.HasValue && RoutePMIn.HasValue)
            {
                routeHours += (RoutePMIn.Value - RoutePMOut.Value).TotalHours;
            }

            return Math.Max(0, routeHours);
        }

        /// <summary>
        /// Calculate total hours for the day
        /// </summary>
        public void CalculateTotalHours()
        {
            var normalHours = CalculateNormalHours();
            var routeHours = CalculateRouteHours();

            TotalTime = normalHours + routeHours + PTOHours;

            // Calculate overtime (anything over 8 hours per day)
            Overtime = Math.Max(0, TotalTime - 8.0);
        }

        /// <summary>
        /// Validate time card entries for logical consistency
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!Date.HasValue)
            {
                errorMessage = "Date is required.";
                return false;
            }

            // Validate normal work hours sequence
            if (ClockIn.HasValue && ClockOut.HasValue && ClockOut.Value <= ClockIn.Value)
            {
                errorMessage = "Clock out time must be after clock in time.";
                return false;
            }

            // Validate lunch times
            if (LunchOut.HasValue && LunchIn.HasValue)
            {
                if (LunchIn.Value <= LunchOut.Value)
                {
                    errorMessage = "Lunch in time must be after lunch out time.";
                    return false;
                }

                if (ClockIn.HasValue && LunchOut.Value < ClockIn.Value)
                {
                    errorMessage = "Lunch out time must be after clock in time.";
                    return false;
                }

                if (ClockOut.HasValue && LunchIn.Value > ClockOut.Value)
                {
                    errorMessage = "Lunch in time must be before clock out time.";
                    return false;
                }
            }

            // Validate route times
            if (RouteAMOut.HasValue && RouteAMIn.HasValue && RouteAMIn.Value <= RouteAMOut.Value)
            {
                errorMessage = "Route AM in time must be after route AM out time.";
                return false;
            }

            if (RoutePMOut.HasValue && RoutePMIn.HasValue && RoutePMIn.Value <= RoutePMOut.Value)
            {
                errorMessage = "Route PM in time must be after route PM out time.";
                return false;
            }

            return true;
        }
    }
}
