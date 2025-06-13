using System;
using System.Configuration;
using BusBuddy.Data;

class QuickRepositoryTest
{
    static void Main()
    {
        try
        {
            Console.WriteLine("Testing individual repositories...");

            // Test VehicleRepository
            Console.WriteLine("\n=== Testing VehicleRepository ===");
            var vehicleRepo = new VehicleRepository();
            var vehicles = vehicleRepo.GetAllVehicles();
            Console.WriteLine($"VehicleRepository.GetAllVehicles() returned {vehicles?.Count ?? 0} vehicles");

            // Test RouteRepository
            Console.WriteLine("\n=== Testing RouteRepository ===");
            var routeRepo = new RouteRepository();
            var routes = routeRepo.GetAllRoutes();
            Console.WriteLine($"RouteRepository.GetAllRoutes() returned {routes?.Count ?? 0} routes");

            // Test MaintenanceRepository
            Console.WriteLine("\n=== Testing MaintenanceRepository ===");
            var maintenanceRepo = new MaintenanceRepository();
            var maintenance = maintenanceRepo.GetAllMaintenanceRecords();
            Console.WriteLine($"MaintenanceRepository.GetAllMaintenanceRecords() returned {maintenance?.Count ?? 0} records");
              // Test FuelRepository
            Console.WriteLine("\n=== Testing FuelRepository ===");
            var fuelRepo = new FuelRepository();
            var fuel = fuelRepo.GetAllFuelRecords();
            Console.WriteLine($"FuelRepository.GetAllFuelRecords() returned {fuel?.Count ?? 0} records");

            // Test ActivityScheduleRepository
            Console.WriteLine("\n=== Testing ActivityScheduleRepository ===");
            var activityScheduleRepo = new ActivityScheduleRepository();
            var activitySchedules = activityScheduleRepo.GetAllScheduledActivities();
            Console.WriteLine($"ActivityScheduleRepository.GetAllScheduledActivities() returned {activitySchedules?.Count ?? 0} records");

            if (activitySchedules != null && activitySchedules.Count > 0)
            {
                var first = activitySchedules[0];
                Console.WriteLine($"  First record: ID={first.ScheduleID}, Date={first.Date}, Type={first.TripType}");
                Console.WriteLine($"  Destination={first.ScheduledDestination}, LeaveTime={first.ScheduledLeaveTime}");
            }

            // Test TimeCardRepository
            Console.WriteLine("\n=== Testing TimeCardRepository ===");
            var timeCardRepo = new TimeCardRepository();
            var timeCards = timeCardRepo.GetAllTimeCards();
            Console.WriteLine($"TimeCardRepository.GetAllTimeCards() returned {timeCards?.Count ?? 0} records");

            // Test ActivityRepository
            Console.WriteLine("\n=== Testing ActivityRepository ===");
            var activityRepo = new ActivityRepository();
            var activities = activityRepo.GetAllActivities();
            Console.WriteLine($"ActivityRepository.GetAllActivities() returned {activities?.Count ?? 0} records");

            // Test SchoolCalendarRepository
            Console.WriteLine("\n=== Testing SchoolCalendarRepository ===");
            var calendarRepo = new SchoolCalendarRepository();
            var calendar = calendarRepo.GetAllCalendarEntries();
            Console.WriteLine($"SchoolCalendarRepository.GetAllCalendarEntries() returned {calendar?.Count ?? 0} records");

            Console.WriteLine("\nAll repository tests completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
