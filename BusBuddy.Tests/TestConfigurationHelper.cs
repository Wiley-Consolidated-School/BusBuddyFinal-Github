using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Helper class to ensure proper configuration loading for tests
    /// </summary>
    public static class TestConfigurationHelper
    {
        private static bool _configurationInitialized = false;

        /// <summary>
        /// Initialize configuration for tests by setting up the proper config file
        /// </summary>
        public static void InitializeConfiguration()
        {
            if (_configurationInitialized)
                return;            try
            {
                // Get the directory where the test assembly is located
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

                if (assemblyDirectory == null)
                {
                    Console.WriteLine("Warning: Could not determine assembly directory");
                    return;
                }

                // Look for App.config file
                var appConfigPath = Path.Combine(assemblyDirectory, "App.config");
                var dllConfigPath = Path.Combine(assemblyDirectory, "BusBuddy.Tests.dll.config");

                Console.WriteLine($"Assembly directory: {assemblyDirectory}");
                Console.WriteLine($"App.config exists: {File.Exists(appConfigPath)}");
                Console.WriteLine($"DLL config exists: {File.Exists(dllConfigPath)}");

                // Try to force configuration refresh
                typeof(ConfigurationManager)
                    .GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static)
                    ?.SetValue(null, 0);

                typeof(ConfigurationManager)
                    .GetField("s_configSystem", BindingFlags.NonPublic | BindingFlags.Static)
                    ?.SetValue(null, null);

                typeof(ConfigurationManager)
                    .Assembly.GetType("System.Configuration.ClientConfigurationSystem")
                    ?.GetField("s_current", BindingFlags.NonPublic | BindingFlags.Static)
                    ?.SetValue(null, null);

                _configurationInitialized = true;

                Console.WriteLine("Configuration initialization completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Configuration initialization failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get connection string with fallback logic
        /// </summary>
        public static string GetConnectionString(string name)
        {
            InitializeConfiguration();

            var connectionString = ConfigurationManager.ConnectionStrings[name];
            if (connectionString != null)
            {
                return connectionString.ConnectionString;
            }

            // Fallback to hardcoded test connection strings
            switch (name.ToLowerInvariant())
            {
                case "defaultconnection":
                    return "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
                case "testconnection":
                    return "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
                default:
                    throw new InvalidOperationException($"Connection string '{name}' not found");
            }
        }        /// <summary>
        /// Get app setting with fallback logic
        /// </summary>
        public static string? GetAppSetting(string key)
        {
            InitializeConfiguration();

            var value = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            // Fallback to hardcoded test values
            switch (key.ToLowerInvariant())
            {
                case "environment":
                    return "Test";
                case "databaseprovider":
                    return "SqlServer";
                default:
                    return null;
            }
        }
    }
}
