using System;
using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
    {
        string connectionString = "Data Source=busbuddy.db";

        try
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected to database successfully");

                // Check what tables exist
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("Tables in database:");
                        while (reader.Read())
                        {
                            Console.WriteLine($"- {reader["name"]}");
                        }
                    }
                }

                // Check if Vehicles table has data
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Vehicles";
                    var count = cmd.ExecuteScalar();
                    Console.WriteLine($"Vehicles table has {count} rows");
                }

                // Check if Fuel table exists and has data
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Fuel";
                    var count = cmd.ExecuteScalar();
                    Console.WriteLine($"Fuel table has {count} rows");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
