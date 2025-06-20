using System;
using System.Configuration;

namespace BusBuddy
{
    class DiagnosticRunner
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== BusBuddy Configuration Diagnostic ===");

                Console.WriteLine("\n1. Checking ConfigurationManager...");
                var connStrings = ConfigurationManager.ConnectionStrings;
                Console.WriteLine($"Connection strings count: {connStrings.Count}");

                foreach (ConnectionStringSettings conn in connStrings)
                {
                    Console.WriteLine($"  - Name: {conn.Name}");
                    Console.WriteLine($"    ConnectionString: {conn.ConnectionString}");
                    Console.WriteLine($"    ProviderName: {conn.ProviderName}");
                    Console.WriteLine();
                }

                Console.WriteLine("\n2. App Settings:");
                var allKeys = ConfigurationManager.AppSettings.AllKeys;
                if (allKeys != null)
                {
                    foreach (string key in allKeys)
                    {
                        Console.WriteLine($"  {key}: {ConfigurationManager.AppSettings[key]}");
                    }
                }
                else
                {
                    Console.WriteLine("  No app settings found");
                }

                Console.WriteLine("\n3. Testing DefaultConnection access...");
                var defaultConn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
                if (defaultConn != null)
                {
                    Console.WriteLine($"DefaultConnection found: {defaultConn.ConnectionString}");
                }
                else
                {
                    Console.WriteLine("DefaultConnection not found");
                }

                Console.WriteLine("\n✅ Diagnostic completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error during diagnostic: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
