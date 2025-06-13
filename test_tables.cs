using System;
using Microsoft.Data.Sqlite;
using System.Configuration;

class TestTables
{
    static void Main()
    {
        var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString ?? "Data Source=busbuddy.db";

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // Get list of tables
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("Tables in database:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"- {reader.GetString(0)}");
                    }
                }
            }

            // Check specific tables that are mentioned in the error
            string[] tablesToCheck = { "Vehicles", "Fuel", "Routes", "Drivers" };
            foreach (var table in tablesToCheck)
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT COUNT(*) FROM {table};";
                        var count = cmd.ExecuteScalar();
                        Console.WriteLine($"{table} table exists with {count} records");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{table} table error: {ex.Message}");
                }
            }
        }
    }
}
