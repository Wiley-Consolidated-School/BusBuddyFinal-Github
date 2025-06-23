using System;
using System.Data;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace BusBuddy.Data
{    public abstract class BaseRepository
    {
        protected readonly string _connectionString;
        protected readonly string _providerName;        protected BaseRepository()
        {
            // Check if we're in a test environment first
            if (IsTestEnvironment())
            {
                // Ensure test database exists before attempting to connect to it
                TestDatabaseInitializer.EnsureTestDatabaseExists();

                // Use App.config connection string if available
                var testConn = ConfigurationManager.ConnectionStrings["TestConnection"];
                if (testConn != null)
                {
                    _connectionString = testConn.ConnectionString;
                    _providerName = testConn.ProviderName;
                    Console.WriteLine("Using test connection string from App.config");
                }
                else
                {
                    // Fallback to hardcoded connection string
                    _connectionString = "Data Source=.\\SQLEXPRESS01;Initial Catalog=BusBuddy_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
                    _providerName = "Microsoft.Data.SqlClient";
                    Console.WriteLine("Using fallback test connection string");
                }
            }
            else
            {
                var conn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
                if (conn == null)
                {
                    // Fallback - use SQL Server Express for production
                    _connectionString = "Data Source=.\\SQLEXPRESS01;Initial Catalog=BusBuddy;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
                    _providerName = "Microsoft.Data.SqlClient";
                    Console.WriteLine("Using fallback production connection string");
                }
                else
                {
                    _connectionString = conn.ConnectionString;
                    _providerName = conn.ProviderName ?? "Microsoft.Data.SqlClient";
                    Console.WriteLine("Using configured connection string");
                }
            }

            // Test the connection but don't initialize database on every repository creation
            TestConnection();
        }        private void TestConnection()
        {
            int retryCount = 3;
            int currentRetry = 0;
            bool connected = false;

            while (!connected && currentRetry < retryCount)
            {
                try
                {
                    using var connection = CreateConnection();
                    connection.Open();
                    Console.WriteLine("✅ Database connection successful");
                    connected = true;
                }
                catch (Exception ex)
                {
                    currentRetry++;
                    Console.WriteLine($"❌ Database connection failed (attempt {currentRetry}/{retryCount}): {ex.Message}");

                    if (currentRetry >= retryCount)
                    {
                        Console.WriteLine("❌ All connection attempts failed");
                        throw new InvalidOperationException($"Failed to connect to database after {retryCount} attempts: {ex.Message}", ex);
                    }

                    // Wait before retrying
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        private bool IsTestEnvironment()
        {
            // Check multiple indicators for test environment - exclude debugger check for main app
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().FullName;

            return baseDirectory.Contains("test", StringComparison.OrdinalIgnoreCase) ||
                   baseDirectory.Contains("Test") ||
                   (assemblyName?.Contains("Test") ?? false) ||
                   Environment.CommandLine.Contains("testhost") ||
                   Environment.CommandLine.Contains("vstest") ||
                   AppDomain.CurrentDomain.FriendlyName.Contains("test", StringComparison.OrdinalIgnoreCase);
        }

        protected BaseRepository(string connectionString, string providerName)
        {
            _connectionString = connectionString;
            _providerName = providerName;
        }

        protected IDbConnection CreateConnection()
        {
            // Always use SQL Server
            return new SqlConnection(_connectionString);
        }
    }
}
