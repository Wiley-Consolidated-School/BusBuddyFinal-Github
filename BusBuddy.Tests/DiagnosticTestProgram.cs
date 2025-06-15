using System;
using System.Configuration;
using BusBuddy.Data;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Standalone diagnostic program to test configuration and database connectivity
    /// </summary>
    public class DiagnosticTestProgram
    {
        public static void RunDiagnostics()
        {
            Console.WriteLine("=== BusBuddy Configuration and Database Diagnostics ===");
            Console.WriteLine($"Test run at: {DateTime.Now}");
            Console.WriteLine();

            // Test 1: Configuration Access
            TestConfigurationAccess();

            // Test 2: Database Diagnostics
            Console.WriteLine("\n" + new string('=', 50));
            DatabaseDiagnostics.RunDiagnostics();

            Console.WriteLine("\n=== Diagnostics Complete ===");
        }        private static void TestConfigurationAccess()
        {
            Console.WriteLine("--- Configuration Access Test ---");

            try
            {
                // Initialize configuration
                TestConfigurationHelper.InitializeConfiguration();

                // Test DefaultConnection
                var defaultConn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
                Console.WriteLine($"✓ DefaultConnection found: {defaultConn != null}");
                if (defaultConn != null)
                {
                    Console.WriteLine($"  Connection String: {defaultConn.ConnectionString}");
                    Console.WriteLine($"  Provider: {defaultConn.ProviderName}");
                }
                else
                {
                    Console.WriteLine("✗ DefaultConnection not found!");
                    LogAllConnectionStrings();
                }

                // Test TestConnection
                var testConn = ConfigurationManager.ConnectionStrings["TestConnection"];
                Console.WriteLine($"✓ TestConnection found: {testConn != null}");
                if (testConn != null)
                {
                    Console.WriteLine($"  Connection String: {testConn.ConnectionString}");
                    Console.WriteLine($"  Provider: {testConn.ProviderName}");
                }

                // Test AppSettings
                var environment = ConfigurationManager.AppSettings["Environment"];
                var dbProvider = ConfigurationManager.AppSettings["DatabaseProvider"];
                Console.WriteLine($"✓ Environment: {environment}");
                Console.WriteLine($"✓ DatabaseProvider: {dbProvider}");

                // Verify configuration file location
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                Console.WriteLine($"✓ Configuration file path: {configFile.FilePath}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Configuration test failed: {ex.Message}");
                Console.WriteLine($"Full error: {ex}");
            }
        }

        private static void LogAllConnectionStrings()
        {
            Console.WriteLine("\nAll available connection strings:");
            var connectionStrings = ConfigurationManager.ConnectionStrings;
            Console.WriteLine($"Total count: {connectionStrings.Count}");

            foreach (ConnectionStringSettings connString in connectionStrings)
            {
                Console.WriteLine($"  Name: {connString.Name}");
                Console.WriteLine($"  Provider: {connString.ProviderName}");
                Console.WriteLine($"  Connection: {connString.ConnectionString}");
                Console.WriteLine("  ---");
            }
        }
    }
}
