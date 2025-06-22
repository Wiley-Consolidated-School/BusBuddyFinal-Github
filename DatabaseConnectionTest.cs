using System;
using BusBuddy.Data;

namespace BusBuddy.DatabaseTest
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== BusBuddy Database Connection Test ===");
            Console.WriteLine($"Generated on: {DateTime.Now}");
            Console.WriteLine();

            try
            {
                // Run database diagnostics
                DatabaseDiagnostics.RunDiagnostics();

                Console.WriteLine("\n=== Repository Test ===");

                // Test vehicle repository
                var vehicleRepo = new VehicleRepository();
                var vehicles = vehicleRepo.GetAllVehicles();
                Console.WriteLine($"✅ Vehicle Repository: {vehicles?.Count ?? 0} vehicles found");

                // Test driver repository
                var driverRepo = new DriverRepository();
                var drivers = driverRepo.GetAllDrivers();
                Console.WriteLine($"✅ Driver Repository: {drivers?.Count ?? 0} drivers found");

                // Test route repository
                var routeRepo = new RouteRepository();
                var routes = routeRepo.GetAllRoutes();
                Console.WriteLine($"✅ Route Repository: {routes?.Count ?? 0} routes found");

                Console.WriteLine("\n✅ Database connectivity test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Database test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
