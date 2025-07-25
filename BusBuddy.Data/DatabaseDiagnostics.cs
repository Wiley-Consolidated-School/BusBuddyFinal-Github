using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace BusBuddy.Data
{
    public static class DatabaseDiagnostics
    {
        public static void RunDiagnostics()
        {
            Console.WriteLine("=== Database Diagnostics ===");

            // Check if we're in test environment
            var isTest = IsTestEnvironment();
            Console.WriteLine($"Test environment detected: {isTest}");

            // Check configuration
            var defaultConn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            Console.WriteLine($"DefaultConnection found: {defaultConn != null}");
            if (defaultConn != null)
            {
                Console.WriteLine($"Connection String: {defaultConn.ConnectionString}");
                Console.WriteLine($"Provider: {defaultConn.ProviderName}");
            }
            else if (isTest)
            {
                Console.WriteLine("Using test environment fallback connection");
            }

            // Test SQL Server connection
            TestSqlServerConnection();

            // Test repository creation
            TestRepositoryCreation();
        }

        private static bool IsTestEnvironment()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return baseDirectory.Contains("test", StringComparison.OrdinalIgnoreCase) ||
                   baseDirectory.Contains("Test") ||
                   Environment.CommandLine.Contains("testhost") ||
                   Environment.CommandLine.Contains("vstest");
        }

        private static void TestSqlServerConnection()
        {
            Console.WriteLine("\n--- SQL Server Connection Test ---");
            var connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                Console.WriteLine("✓ Connected to SQL Server Express");

                // Check if BusBuddyDB exists
                var checkDbCommand = new SqlCommand("SELECT COUNT(*) FROM sys.databases WHERE name = 'BusBuddyDB'", connection);
                var dbExists = (int)checkDbCommand.ExecuteScalar() > 0;
                Console.WriteLine($"BusBuddyDB exists: {dbExists}");

                if (!dbExists)
                {
                    Console.WriteLine("Creating BusBuddyDB database...");
                    var createDbCommand = new SqlCommand("CREATE DATABASE BusBuddyDB", connection);
                    createDbCommand.ExecuteNonQuery();
                    Console.WriteLine("✓ Database created");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ SQL Server connection failed: {ex.Message}");
                Console.WriteLine($"Full error: {ex}");
            }
        }

        private static void TestRepositoryCreation()
        {
            Console.WriteLine("\n--- Repository Test ---");
            try
            {
                var baseRepo = new TestRepository();
                Console.WriteLine($"Repository connection string: {baseRepo.GetConnectionString()}");
                Console.WriteLine($"Repository provider: {baseRepo.GetProviderName()}");

                using var connection = baseRepo.TestCreateConnection();
                connection.Open();
                Console.WriteLine("✓ Repository connection successful");

                // Test if Vehicles table exists - SQL Server only
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Vehicles'";

                var tableExists = Convert.ToInt32(command.ExecuteScalar()) > 0;
                Console.WriteLine($"Vehicles table exists: {tableExists}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Repository test failed: {ex.Message}");
            }
        }
    }

    // Test class to access BaseRepository internals
    public class TestRepository : BaseRepository
    {
        public string GetConnectionString() => _connectionString;
        public string GetProviderName() => _providerName;
        public IDbConnection TestCreateConnection() => CreateConnection();
    }
}
