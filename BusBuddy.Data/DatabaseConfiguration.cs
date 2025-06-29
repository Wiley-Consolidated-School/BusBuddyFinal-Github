using System;
using System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Data
{
    /// <summary>
    /// Centralized database configuration for production and test environments
    /// </summary>
    public static class DatabaseConfiguration
    {
        public static string Environment => ConfigurationManager.AppSettings["Environment"] ?? "Production";
        public static string DatabaseProvider => ConfigurationManager.AppSettings["DatabaseProvider"] ?? "SqlServer";
        public static bool IsTestEnvironment => Environment.Equals("Test", StringComparison.OrdinalIgnoreCase);
        public static bool IsProductionEnvironment => Environment.Equals("Production", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Get the appropriate connection string based on environment
        /// </summary>
        public static string GetConnectionString()
        {
            string connectionName = IsTestEnvironment ? "TestConnection" : "DefaultConnection";
            var connectionString = ConfigurationManager.ConnectionStrings[connectionName]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback to DefaultConnection if specific environment connection not found
                connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            }
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"No connection string found for environment: {Environment}");
            }
            return connectionString;
        }

        /// <summary>
        /// Get the database name from connection string
        /// </summary>
        public static string GetDatabaseName()
        {
            var connectionString = GetConnectionString();
            // Extract database name from SQL Server connection string
            if (connectionString.Contains("Initial Catalog="))
            {
                var start = connectionString.IndexOf("Initial Catalog=") + "Initial Catalog=".Length;
                var end = connectionString.IndexOf(";", start);
                if (end == -1) end = connectionString.Length;
                return connectionString.Substring(start, end - start);
            }
            return "BusBuddyDB";
        }

        /// <summary>
        /// Configure DbContextOptions for the current environment
        /// </summary>
        public static DbContextOptions<BusBuddyContext> GetDbContextOptions()
        {
            var builder = new DbContextOptionsBuilder<BusBuddyContext>();
            var connectionString = GetConnectionString();
            // Configure for SQL Server with enhanced connection settings
            builder.UseSqlServer(connectionString, options =>
            {
                options.CommandTimeout(60); // Increased from 30 seconds to 60 seconds
                options.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                // Configure connection resiliency
                options.MinBatchSize(5);
                options.MaxBatchSize(100);
            });
            // Enable logging for test environment
            if (IsTestEnvironment)
            {
                builder.EnableSensitiveDataLogging();
                builder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            }
            return builder.Options;
        }

        /// <summary>
        /// Create a new BusBuddyContext with proper configuration
        /// </summary>
        public static BusBuddyContext CreateContext()
        {
            return new BusBuddyContext(GetDbContextOptions());
        }
    }
}

