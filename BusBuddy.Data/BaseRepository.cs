using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace BusBuddy.Data
{
    public abstract class BaseRepository
    {
        protected readonly string _connectionString;
        protected readonly string _providerName;

        // Track database connectivity status for graceful degradation
        protected bool _isDatabaseAvailable = false;

        /// <summary>
        /// Gets whether the database connection is available
        /// </summary>
        public bool IsDatabaseAvailable => _isDatabaseAvailable; protected BaseRepository()
        {
            // Check if we're in a test environment first
            if (IsTestEnvironment())
            {
                // Ensure test database exists before attempting to connect to it
                SimpleDatabaseCheck.CheckAndFixDatabaseAsync().GetAwaiter().GetResult();

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
                    // Fallback to hardcoded connection string - Updated to match actual SQL Server instance
                    _connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=BusBuddy_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
                    _providerName = "Microsoft.Data.SqlClient";
                    Console.WriteLine("Using fallback test connection string");
                }
            }
            else
            {
                var conn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
                if (conn == null)
                {
                    // Fallback - use SQL Server Express for production - Updated to match actual SQL Server instance
                    _connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=BusBuddy;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
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
        }
        private void TestConnection()
        {
            int retryCount = 5; // Increased from 3 to 5
            int currentRetry = 0;
            bool connected = false;

            Console.WriteLine($"Testing database connection to {_connectionString}");

            while (!connected && currentRetry < retryCount)
            {
                try
                {
                    using var connection = CreateConnection();
                    connection.Open();
                    Console.WriteLine("✅ Database connection successful");
                    connected = true;
                    _isDatabaseAvailable = true;
                }
                catch (Exception ex)
                {
                    currentRetry++;
                    Console.WriteLine($"⚠️ Database connection failed (attempt {currentRetry}/{retryCount}): {ex.Message}");
                    Console.WriteLine($"Connection string used: {_connectionString}");

                    // Log the connection details to help diagnose the issue
                    var parts = _connectionString.Split(';');
                    foreach (var part in parts)
                    {
                        Console.WriteLine($"  Connection param: {part}");
                    }

                    if (currentRetry >= retryCount)
                    {
                        Console.WriteLine("⚠️ Database unavailable - application will run in offline mode with sample data");
                        Console.WriteLine("To fix this issue, please check that:");
                        Console.WriteLine("1. SQL Server Express is installed and running (service MSSQL$SQLEXPRESS01)");
                        Console.WriteLine("2. The BusBuddy database exists");
                        Console.WriteLine("3. Your Windows account has permissions to access the database");
                        // GRACEFUL DEGRADATION: Log warning instead of throwing exception
                        // This allows the application to continue with sample/mock data
                        _isDatabaseAvailable = false;
                        return;
                    }

                    // Wait before retrying - increased timeout between attempts
                    Console.WriteLine($"Waiting 1 second before retry {currentRetry + 1}/{retryCount}...");
                    System.Threading.Thread.Sleep(1000);
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

        /// <summary>
        /// Creates and returns a database connection, attempting to recover from offline state if needed
        /// </summary>
        protected IDbConnection CreateConnectionWithRecovery()
        {
            try
            {
                // First try normal connection
                var connection = CreateConnection();

                try
                {
                    // Try to open the connection
                    connection.Open();
                    _isDatabaseAvailable = true;
                    connection.Close();
                    return connection;
                }
                catch (SqlException ex) when (ex.Number == 942 || // Offline database error
                                            ex.Number == 4060 || // Cannot open database
                                            ex.Number == 233)    // Server not found
                {
                    Console.WriteLine($"Database offline or unavailable error: {ex.Message}");
                    Console.WriteLine("Attempting to repair database connection...");

                    // Attempt database recovery
                    var recovered = SimpleDatabaseCheck.CheckAndFixDatabaseAsync().GetAwaiter().GetResult();

                    if (recovered)
                    {
                        Console.WriteLine("Database recovery successful! Reconnecting...");
                        // Try connection again after recovery
                        connection = CreateConnection();
                        connection.Open();
                        _isDatabaseAvailable = true;
                        connection.Close();
                        return connection;
                    }
                    else
                    {
                        Console.WriteLine("Database recovery failed.");
                        _isDatabaseAvailable = false;
                        throw new InvalidOperationException("Database is offline and automatic recovery failed. Please contact IT support.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating database connection: {ex.Message}");
                _isDatabaseAvailable = false;
                throw;
            }
        }
    }
}

