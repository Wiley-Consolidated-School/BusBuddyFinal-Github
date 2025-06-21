using System;
using BusBuddy.Data;

namespace BusBuddy.DebugTest
{
    /// <summary>
    /// Quick debug test for repository initialization
    /// </summary>
    public class RepositoryDebugTest
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("🔍 Testing repository initialization...");

            try
            {
                Console.WriteLine("1. Testing VehicleRepository constructor...");
                var vehicleRepo = new VehicleRepository();
                Console.WriteLine("✅ VehicleRepository created successfully");

                Console.WriteLine("2. Testing repository method...");
                var vehicles = vehicleRepo.GetAllVehicles();
                Console.WriteLine($"✅ GetAllVehicles returned {vehicles?.Count ?? 0} vehicles");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Repository test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
