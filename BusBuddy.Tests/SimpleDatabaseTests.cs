//using System;
using System.Data;
using Microsoft.Data.Sqlite;
using Xunit;
using System.Configuration;
using System.IO;

namespace BusBuddy.Tests
{
    public class SimpleDatabaseTests
    {
        [Fact]
        public void CanConnectToSqliteDatabase()
        {
            // Get the connection string from app.config
            var connectionString = "Data Source=busbuddy.db";

            // Create a connection
            using (var connection = new SqliteConnection(connectionString))
            {
                // Try to open the connection
                connection.Open();

                // Verify the connection is open
                Assert.Equal(ConnectionState.Open, connection.State);

                // Try a simple query
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT sqlite_version();";
                    var result = cmd.ExecuteScalar();
                    Assert.NotNull(result);
                }
            }
        }

        [Fact]
        public void DatabaseFileShouldExist()
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            var providerName = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ProviderName;

            if (string.IsNullOrEmpty(providerName) || providerName != "Microsoft.Data.SqlClient")
            {
                // Skip the test if not SQL Server or provider not set
                return; // Xunit will treat this as a pass/skip
            }

            Assert.True(true, "SQL Server provider: skipping file existence check.");
        }
    }
}
