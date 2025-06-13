using System;
using BusBuddy.Data;

class TestRepositoriesWithData
{
    static void Main()
    {
        try
        {
            Console.WriteLine("Testing repositories with data...");

            // Test VehicleRepository
            var vehicleRepo = new VehicleRepository();
            var vehicles = vehicleRepo.GetAllVehicles();
            Console.WriteLine($"VehicleRepository found {vehicles?.Count ?? 0} vehicles");
            if (vehicles?.Count > 0)
            {
                Console.WriteLine($"First vehicle: {vehicles[0].VehicleNumber} - {vehicles[0].Make} {vehicles[0].Model}");
            }

            // Test RouteRepository
            var routeRepo = new RouteRepository();
            var routes = routeRepo.GetAllRoutes();
            Console.WriteLine($"RouteRepository found {routes?.Count ?? 0} routes");
            if (routes?.Count > 0)
            {
                Console.WriteLine($"First route: {routes[0].RouteName} on {routes[0].Date}");
            }

            // Test MaintenanceRepository
            var maintenanceRepo = new MaintenanceRepository();
            var maintenance = maintenanceRepo.GetAllMaintenanceRecords();
            Console.WriteLine($"MaintenanceRepository found {maintenance?.Count ?? 0} records");
            if (maintenance?.Count > 0)
            {
                Console.WriteLine($"First maintenance: {maintenance[0].MaintenanceCompleted} - ${maintenance[0].RepairCost}");
            }

            // Test FuelRepository
            var fuelRepo = new FuelRepository();
            var fuel = fuelRepo.GetAllFuelRecords();
            Console.WriteLine($"FuelRepository found {fuel?.Count ?? 0} records");
            if (fuel?.Count > 0)
            {
                Console.WriteLine($"First fuel: {fuel[0].FuelAmount} gallons at {fuel[0].FuelLocation}");
            }

            Console.WriteLine("\nAll repositories are working correctly!");
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
