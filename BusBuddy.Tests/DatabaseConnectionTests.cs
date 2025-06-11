using Xunit;
using System;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Configuration;
using Microsoft.Data.SqlClient;
using System.IO;

namespace BusBuddy.Tests
{
    public class DatabaseConnectionTests
    {
        [Fact]
        public void CanConnectToDatabase()
        {
            // Get the connection string from config
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ProviderName;

            // Fallback to default SQLite connection if config is not available
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Data Source=test_busbuddy.db";
                providerName = "Microsoft.Data.Sqlite";
            }

            Assert.NotNull(connectionString);
            Assert.NotNull(providerName);

            // Try to connect to the database
            IDbConnection? connection = null;
            try
            {
                if (providerName == "Microsoft.Data.Sqlite")
                {
                    connection = new SqliteConnection(connectionString!);
                    connection.Open();
                    Assert.Equal(ConnectionState.Open, connection.State);

                    // Try a simple query
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT sqlite_version();";
                        var result = cmd.ExecuteScalar();
                        Assert.NotNull(result);
                        Console.WriteLine($"SQLite Version: {result}");
                    }
                }
                else
                {
                    connection = new SqlConnection(connectionString!);
                    connection.Open();
                    Assert.Equal(ConnectionState.Open, connection.State);

                    // Try a simple query
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT @@VERSION;";
                        var result = cmd.ExecuteScalar();
                        Assert.NotNull(result);
                        Console.WriteLine($"SQL Server Version: {result}");
                    }
                }
            }
            finally
            {
                connection?.Close();
            }
        }

        [Fact]
        public void DatabaseFile_ShouldExist()
        {
            // Get the database path from the connection string
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ProviderName;

            // Fallback to default SQLite connection if config is not available
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Data Source=test_busbuddy.db";
                providerName = "Microsoft.Data.Sqlite";
            }

            if (providerName == "Microsoft.Data.Sqlite")
            {
                // Extract the database file path
                var dbPath = connectionString?.Replace("Data Source=", "").Trim() ?? "";
                Console.WriteLine($"Database path: {dbPath}");

                // Check if the file exists
                var exists = File.Exists(dbPath);
                Console.WriteLine($"Database file exists: {exists}");

                // If it doesn't exist in the current path, check the full path
                if (!exists && !Path.IsPathRooted(dbPath))
                {
                    var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbPath);
                    Console.WriteLine($"Checking full path: {fullPath}");
                    exists = File.Exists(fullPath);
                    Console.WriteLine($"Database file exists at full path: {exists}");
                }

                Assert.True(exists, $"Database file does not exist: {dbPath}");
            }
            else if (providerName == "Microsoft.Data.SqlClient")
            {
                // For SQL Server, skip file existence check (database is server-based)
                Assert.True(true, "SQL Server provider: skipping file existence check.");
            }
            else
            {
                // Unknown provider, fail the test
                Assert.True(false, $"Unknown provider: {providerName}");
            }
        }
    }
}
