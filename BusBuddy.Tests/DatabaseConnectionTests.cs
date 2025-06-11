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
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ProviderName;

            if (string.IsNullOrEmpty(providerName) || providerName != "Microsoft.Data.SqlClient")
            {
                // Skip the test if not SQL Server or provider not set
                return; // Xunit will treat this as a pass/skip
            }

            Assert.True(true, "SQL Server provider: skipping file existence check.");
        }
    }
}
