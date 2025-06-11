using System;
using Microsoft.Data.Sqlite;
using System.IO;

namespace BusBuddy.Tests
{
    public class ConnectionTest
    {
        public static void Main(string[] args)
        {
            try
            {
                // Check if database file exists
                bool dbFileExists = File.Exists("busbuddy.db");
                Console.WriteLine($"Database file exists: {dbFileExists}");

                // Create the file if it doesn't exist
                if (!dbFileExists)
                {
                    Console.WriteLine("Creating database file...");
                    using (var connection = new SqliteConnection("Data Source=busbuddy.db"))
                    {
                        connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "CREATE TABLE IF NOT EXISTS TestConnection (Id INTEGER PRIMARY KEY, Name TEXT)";
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO TestConnection (Name) VALUES ('Test')";
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "SELECT * FROM TestConnection";
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Console.WriteLine($"Id: {reader["Id"]}, Name: {reader["Name"]}");
                                }
                            }
                        }
                    }
                    Console.WriteLine("Database created and connection test successful");
                }
                else
                {
                    Console.WriteLine("Connecting to existing database...");
                    using (var connection = new SqliteConnection("Data Source=busbuddy.db"))
                    {
                        connection.Open();
                        Console.WriteLine("Connection state: " + connection.State);

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "SELECT sqlite_version()";
                            var versionResult = cmd.ExecuteScalar();
                            string version = versionResult?.ToString() ?? "Unknown";
                            Console.WriteLine($"SQLite version: {version}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
