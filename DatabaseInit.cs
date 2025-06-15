using System;
using System.Configuration;
using BusBuddy.Data;

namespace BusBuddy
{
    class DatabaseInitProgram
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Initializing BusBuddy databases...");

                // Initialize production database
                var prodConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                var prodInitializer = new SqlServerDatabaseInitializer(prodConnectionString);
                prodInitializer.Initialize();
                Console.WriteLine("Production database initialized successfully.");

                // Initialize test database
                var testConnectionString = ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString;
                var testInitializer = new SqlServerDatabaseInitializer(testConnectionString);
                testInitializer.Initialize();
                Console.WriteLine("Test database initialized successfully.");

                Console.WriteLine("All databases initialized successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing databases: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
