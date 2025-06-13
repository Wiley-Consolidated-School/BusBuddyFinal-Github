using System;
using BusBuddy.Data;

class QuickTest
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing repository loading...");

        try
        {
            var vehicleRepo = new VehicleRepository();
            var vehicles = vehicleRepo.GetAllVehicles();
            Console.WriteLine($"✓ Vehicles loaded: {vehicles.Count} records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error loading vehicles: {ex.Message}");
        }

        try
        {
            var routeRepo = new RouteRepository();
            var routes = routeRepo.GetAllRoutes();
            Console.WriteLine($"✓ Routes loaded: {routes.Count} records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error loading routes: {ex.Message}");
        }

        try
        {
            var maintenanceRepo = new MaintenanceRepository();
            var maintenance = maintenanceRepo.GetAllMaintenanceRecords();
            Console.WriteLine($"✓ Maintenance loaded: {maintenance.Count} records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error loading maintenance: {ex.Message}");
        }

        try
        {
            var fuelRepo = new FuelRepository();
            var fuel = fuelRepo.GetAllFuelRecords();
            Console.WriteLine($"✓ Fuel records loaded: {fuel.Count} records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error loading fuel records: {ex.Message}");
        }

        try
        {
            var activityRepo = new ActivityRepository();
            var activities = activityRepo.GetAllActivities();
            Console.WriteLine($"✓ Activities loaded: {activities.Count} records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error loading activities: {ex.Message}");
        }

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}
