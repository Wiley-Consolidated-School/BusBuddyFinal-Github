using System;
using System.Configuration;
using BusBuddy.Data;

namespace BusBuddy
{
    class DebugConfig
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== Configuration Debug ===");

                var conn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
                if (conn == null)
                {
                    Console.WriteLine("DefaultConnection is NULL");
                }
                else
                {
                    Console.WriteLine($"ConnectionString: {conn.ConnectionString}");
                    Console.WriteLine($"ProviderName: {conn.ProviderName}");
                }

                Console.WriteLine("\nAll connection strings:");
                foreach (ConnectionStringSettings connStr in ConfigurationManager.ConnectionStrings)
                {
                    Console.WriteLine($"  Name: {connStr.Name}");
                    Console.WriteLine($"  ConnectionString: {connStr.ConnectionString}");
                    Console.WriteLine($"  ProviderName: {connStr.ProviderName}");
                    Console.WriteLine();
                }

                Console.WriteLine("App Settings:");
                var allKeys = ConfigurationManager.AppSettings.AllKeys;
                if (allKeys != null)
                {
                    foreach (string? key in allKeys)
                    {
                        if (key != null)
                        {
                            Console.WriteLine($"  {key}: {ConfigurationManager.AppSettings[key] ?? "null"}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
