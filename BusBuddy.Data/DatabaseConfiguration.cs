using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BusBuddy.Data
{
    /// <summary>
    /// Centralized database configuration for production and test environments
    /// </summary>
    public static class DatabaseConfiguration
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string Environment => _configuration["Environment"] ?? "Production";
        public static string DatabaseProvider => _configuration["DatabaseProvider"] ?? "SqlServer";
        public static bool IsTestEnvironment => Environment.Equals("Test", StringComparison.OrdinalIgnoreCase);
        public static bool IsProductionEnvironment => Environment.Equals("Production", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Get the appropriate connection string based on environment
        /// </summary>
        public static string GetConnectionString()
        {
            string connectionName = IsTestEnvironment ? "TestConnection" : "BusBuddyDB";
            var connectionString = _configuration.GetConnectionString(connectionName);
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _configuration.GetConnectionString("BusBuddyDB") ?? throw new InvalidOperationException($"No connection string found for environment: {Environment}");
            }
            return $"{connectionString};Encrypt=False;TrustServerCertificate=True;";
        }

        /// <summary>
        /// Get the database name from connection string
        /// </summary>
        public static string GetDatabaseName()
        {
            var connectionString = GetConnectionString();
            if (connectionString.Contains("Initial Catalog="))
            {
                var start = connectionString.IndexOf("Initial Catalog=") + "Initial Catalog=".Length;
                var end = connectionString.IndexOf(";", start);
                return end == -1 ? connectionString.Substring(start) : connectionString.Substring(start, end - start);
            }
            return "BusBuddy";
        }

        /// <summary>
        /// Configure DbContextOptions for the current environment
        /// </summary>
        public static DbContextOptions<BusBuddyContext> GetDbContextOptions()
        {
            var builder = new DbContextOptionsBuilder<BusBuddyContext>();
            var connectionString = GetConnectionString();
            builder.UseSqlServer(connectionString, options =>
            {
                options.CommandTimeout(60);
                options.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                options.MinBatchSize(5);
                options.MaxBatchSize(100);
            });
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

