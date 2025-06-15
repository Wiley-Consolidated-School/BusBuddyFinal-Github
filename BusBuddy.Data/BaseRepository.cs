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
            var conn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (conn == null)
            {
                // Fallback - use SQL Server Express
                _connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB;Integrated Security=True;TrustServerCertificate=True;";
                _providerName = "Microsoft.Data.SqlClient";
            }
            else
            {
                _connectionString = conn.ConnectionString;
                _providerName = conn.ProviderName ?? "Microsoft.Data.SqlClient";
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
            // Check if we're running in a test context
            return _connectionString.Contains("test_") ||
                   _connectionString.Contains("Test") ||
                   AppDomain.CurrentDomain.BaseDirectory.Contains("test") ||
                   System.Diagnostics.Debugger.IsAttached;
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
