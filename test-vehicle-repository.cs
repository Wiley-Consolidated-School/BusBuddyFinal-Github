// This simple test app directly initializes the ServiceContainerSingleton
// and then tries to access the VehicleRepository to verify it works
using System;
using BusBuddy.UI.Services;
using BusBuddy.Data;

namespace BusBuddy
{
    class TestVehicleRepository
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Initializing ServiceContainerSingleton...");

                // Force initialization of the ServiceContainerSingleton
                if (!ServiceContainerSingleton.IsInitialized)
                {
                    ServiceContainerSingleton.Initialize();
                }

                // Get a vehicle repository from the singleton
                var vehicleRepository = ServiceContainerSingleton.Instance.GetService<IVehicleRepository>();

                if (vehicleRepository == null)
                {
                    Console.WriteLine("ERROR: Failed to get IVehicleRepository from ServiceContainerSingleton");
                    return;
                }

                Console.WriteLine("Testing VehicleRepository by calling GetAllVehicles()...");

                // Test the repository
                var vehicles = vehicleRepository.GetAllVehicles();
                var count = vehicles?.Count ?? 0;

                Console.WriteLine($"SUCCESS: Found {count} vehicles in the database");

                // Print the vehicles
                Console.WriteLine("\nVehicles:");
                foreach (var vehicle in vehicles)
                {
                    Console.WriteLine($"  ID: {vehicle.Id}, Number: {vehicle.VehicleNumber}, Make: {vehicle.Make}, Model: {vehicle.Model}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
