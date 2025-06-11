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
            var dbPath = "busbuddy.db";
            var exists = File.Exists(dbPath);

            // If not found in the current directory, check the full path
            if (!exists)
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), dbPath);
                exists = File.Exists(fullPath);
            }

            Assert.True(exists, $"Database file '{dbPath}' not found");
        }
    }
}
