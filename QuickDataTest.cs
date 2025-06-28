using System;
using System.Collections.Generic;
using BusBuddy.Data;
using BusBuddy.Models;

namespace BusBuddy.Tests
{
    public class QuickDataTest
    {
        public static int Main()
        {
            Console.WriteLine("=== Quick Vehicle Data Retrieval Test ===");

            try
            {
                var repository = new VehicleRepository();

                Console.WriteLine("Running diagnostics...");
                var diagnostics = repository.DiagnoseDataRetrieval();
                Console.WriteLine(diagnostics);

                Console.WriteLine("\nTesting GetAllVehicles...");
                var vehicles = repository.GetAllVehicles();
                Console.WriteLine($"Retrieved {vehicles.Count} vehicles");

                if (vehicles.Count > 0)
                {
                    Console.WriteLine("Sample vehicles:");
                    for (int i = 0; i < Math.Min(3, vehicles.Count); i++)
                    {
                        var v = vehicles[i];
                        Console.WriteLine($"  {i+1}. ID={v.VehicleID}, Number={v.VehicleNumber}, Make={v.Make}");
                    }
                }

                Console.WriteLine("\n✅ Test completed successfully");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return 1;
            }
        }
    }
}
