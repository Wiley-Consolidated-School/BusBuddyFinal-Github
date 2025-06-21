using System;
using BusBuddy.UI.Services;
using BusBuddy.Data;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing repository initialization...");

        try
        {
            var serviceContainer = new ServiceContainer();
            var vehicleRepository = serviceContainer.GetService<IVehicleRepository>();

            if (vehicleRepository == null)
            {
                Console.WriteLine("❌ VehicleRepository is null");
                return;
            }

            Console.WriteLine("✅ VehicleRepository created successfully");

            var vehicles = vehicleRepository.GetAllVehicles();
            Console.WriteLine($"✅ Retrieved {vehicles.Count()} vehicles from database");

            foreach (var vehicle in vehicles.Take(3))
            {
                Console.WriteLine($"   - Vehicle: {vehicle.VehicleNumber} ({vehicle.Make} {vehicle.Model})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
