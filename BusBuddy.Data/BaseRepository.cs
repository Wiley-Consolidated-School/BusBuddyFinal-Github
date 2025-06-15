using System;
using System.Data;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace BusBuddy.Data
{    public abstract class BaseRepository
    {
        protected readonly string _connectionString;
        protected readonly string _providerName;

        protected BaseRepository()
        {
            // Check if we're in a test environment first
            if (IsTestEnvironment())
            {
                _connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
                _providerName = "Microsoft.Data.SqlClient";
                Console.WriteLine("Using test environment connection string");
            }
            else
            {
                var conn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
                if (conn == null)
                {
                    // Fallback - use SQL Server Express for production
                    _connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB;Integrated Security=True;TrustServerCertificate=True;";
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

            // Initialize SQL Server database if needed
            EnsureSqlServerDatabase();
        }

        private void EnsureSqlServerDatabase()
        {
            try
            {
                var initializer = new SqlServerDatabaseInitializer(_connectionString);
                initializer.Initialize();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize SQL Server database: {ex.Message}", ex);
            }
        }

        private bool IsTestEnvironment()
        {
            // Check multiple indicators for test environment
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().FullName;

            return baseDirectory.Contains("test", StringComparison.OrdinalIgnoreCase) ||
                   baseDirectory.Contains("Test") ||
                   assemblyName.Contains("Test") ||
                   System.Diagnostics.Debugger.IsAttached ||
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
