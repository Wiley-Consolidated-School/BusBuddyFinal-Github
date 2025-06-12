using System.Data.SQLite;
using Dapper;
using System.IO;

namespace BusBuddy.Tests
{
    /// <summary>
    /// TestBase sets up and seeds the SQLite test database using Dapper.
    /// - Ensures schema matches DatabaseScript.sql
    /// - Seeds required data for all tests
    /// - No EF Core/DbContext usage
    ///
    /// To update schema, edit DatabaseScript.sql and sync here.
    /// To update seed data, edit TestSeedData.sql or the seeding logic below.
    ///
    /// Last updated: June 12, 2025
    /// </summary>
    public class TestBase
    {
        protected SQLiteConnection CreateConnection()
        {
            // Use a unique file for each test run for isolation, or use :memory: for in-memory
            var dbPath = Path.GetTempFileName() + ".db";
            var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();
            // Run schema script
            var schemaSql = File.ReadAllText("..\\BusBuddy.Data\\DatabaseScript.sql");
            connection.Execute(schemaSql);
            return connection;
        }

        public void SeedTestData(SQLiteConnection connection)
        {
            // Seed all tables using the comprehensive TestSeedData.sql
            var seedSql = File.ReadAllText("..\\BusBuddy.Data\\TestSeedData.sql");
            connection.Execute(seedSql);
        }
    }
}
