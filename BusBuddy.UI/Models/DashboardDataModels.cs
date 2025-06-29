using System;
using System.Collections.Generic;
using System.Linq;
using BusBuddy.Models;

namespace BusBuddy.UI.Models
{
    /// <summary>
    /// Represents bus data for dashboard display
    /// </summary>
    public class VehicleData
    {
        public int BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string MaintenanceStatus { get; set; } = string.Empty;
        public DateTime? LastInspection { get; set; }
        public DateTime? NextServiceDue { get; set; }
        // Create from Bus model
        public static VehicleData FromBus(Bus bus)
        {
            return new VehicleData
            {
                BusId = bus.BusId,
                BusNumber = bus.BusNumber ?? string.Empty,
                Status = bus.Status ?? "Unknown",
                Year = bus.Year ?? 0, // Handle nullable Year
                Make = bus.Make ?? string.Empty,
                Model = bus.Model ?? string.Empty,
                LastInspection = bus.LastInspectionDate,
                MaintenanceStatus = DetermineMaintenanceStatus(bus),
                NextServiceDue = DetermineNextServiceDate(bus)
            };
        }
        private static string DetermineMaintenanceStatus(Bus bus)
        {
            if (bus.Status?.Equals("Maintenance", StringComparison.OrdinalIgnoreCase) == true)
                return "In Maintenance";
            if (bus.LastInspectionDate.HasValue &&
                (DateTime.Now - bus.LastInspectionDate.Value).TotalDays > 180)
                return "Inspection Due";
            return "OK";
        }
        private static DateTime? DetermineNextServiceDate(Bus bus)
        {
            if (!bus.LastInspectionDate.HasValue)
                return null;
            // Next service due 6 months after last inspection
            return bus.LastInspectionDate.Value.AddMonths(6);
        }
    }
    /// <summary>
    /// Represents route data for dashboard display
    /// </summary>
    public class RouteData
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string AMVehicleNumber { get; set; } = string.Empty;
        public string AMDriverName { get; set; } = string.Empty;
        public decimal? AMMiles { get; set; }
        public int? AMRiders { get; set; }
        public string PMVehicleNumber { get; set; } = string.Empty;
        public string PMDriverName { get; set; } = string.Empty;
        public decimal? PMMiles { get; set; }
        public int? PMRiders { get; set; }
        public string RouteType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        // Create from Route model
        public static RouteData FromRoute(Route route, List<Bus> buses, List<Driver> drivers)
        {
            var amVehicle = route.AMBusId.HasValue
                ? buses.FirstOrDefault(v => v.BusId == route.AMBusId)
                : null;
            var pmVehicle = route.PMBusId.HasValue
                ? buses.FirstOrDefault(v => v.BusId == route.PMBusId)
                : null;
            var amDriver = route.AMDriverId.HasValue
                ? drivers.FirstOrDefault(d => d.DriverId == route.AMDriverId)
                : null;
            var pmDriver = route.PMDriverId.HasValue
                ? drivers.FirstOrDefault(d => d.DriverId == route.PMDriverId)
                : null;
            return new RouteData
            {
                RouteId = route.RouteId,
                RouteName = route.RouteName ?? string.Empty,
                Date = route.DateAsDateTime,
                AMVehicleNumber = amVehicle?.BusNumber ?? string.Empty,
                AMDriverName = GetDriverFullName(amDriver),
                AMMiles = route.AMMiles,
                AMRiders = route.AMRiders,
                PMVehicleNumber = pmVehicle?.BusNumber ?? string.Empty,
                PMDriverName = GetDriverFullName(pmDriver),
                PMMiles = route.PMMiles,
                PMRiders = route.PMRiders,
                RouteType = route.RouteType ?? "REG",
                Notes = route.Notes ?? string.Empty
            };
        }
        private static string GetDriverFullName(Driver? driver)
        {
            if (driver == null)
                return string.Empty;
            return $"{driver.FirstName} {driver.LastName}".Trim();
        }
    }
    /// <summary>
    /// Represents activity data for dashboard display
    /// </summary>
    public class ActivityData
    {
        public int ActivityID { get; set; }
        public DateTime Date { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public TimeSpan? DepartureTime { get; set; }
        public TimeSpan? ReturnTime { get; set; }
        public string AssignedVehicleNumber { get; set; } = string.Empty;
        public string AssignedBusNumber { get; set; } = string.Empty;
        public string AssignedDriverName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int? ScheduledRiders { get; set; }
        public double Distance { get; set; }
        public string bus { get; set; } = string.Empty;
        // Create from Activity model
        public static ActivityData FromActivity(Activity activity, List<Bus> buses, List<Driver> drivers)
        {
            var bus = activity.AssignedBusID.HasValue
                ? buses.FirstOrDefault(v => v.BusId == activity.AssignedBusID)
                : null;
            var driver = activity.DriverId.HasValue
                ? drivers.FirstOrDefault(d => d.DriverId == activity.DriverId)
                : null;
            return new ActivityData
            {
                ActivityID = activity.ActivityID,
                Date = activity.DateAsDateTime ?? DateTime.Today,
                ActivityType = activity.ActivityType ?? string.Empty,
                Destination = activity.Destination ?? string.Empty,
                DepartureTime = ParseTimeSpan(activity.LeaveTime),
                ReturnTime = ParseTimeSpan(activity.EventTime),
                AssignedVehicleNumber = bus?.BusNumber ?? string.Empty,
                AssignedDriverName = GetDriverFullName(driver),
                Status = "Scheduled",
                Notes = activity.Notes ?? string.Empty,
                ScheduledRiders = 0, // Default value since it's not in the model
                Distance = 0 // Default value since it's not in the model
            };
        }
        private static TimeSpan? ParseTimeSpan(string? timeString)
        {
            if (string.IsNullOrEmpty(timeString))
                return null;
            if (TimeSpan.TryParse(timeString, out var result))
                return result;
            return null;
        }
        private static string GetDriverFullName(Driver? driver)
        {
            if (driver == null)
                return string.Empty;
            return $"{driver.FirstName} {driver.LastName}".Trim();
        }
    }
    /// <summary>
    /// Represents a data point for charting
    /// </summary>
    public class ChartDataPoint
    {
        public string Category { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        // Optional properties for different chart types
        public double? SecondaryValue { get; set; }
        public string Series { get; set; } = string.Empty;
        public int Index { get; set; }
    }
}

