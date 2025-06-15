using System;
using Microsoft.Data.SqlClient;

namespace TestConnection
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== SQL Server Connection Test ===");

            // Test the exact connection string from your App.config
            var connectionString = "Server=ST-LPTF3-23\\SQLEXPRESS01;Database=master;Integrated Security=true;TrustServerCertificate=true;";

            Console.WriteLine($"Testing connection to: {connectionString}");
            Console.WriteLine();

            try
            {
                using var connection = new SqlConnection(connectionString);

                Console.WriteLine("Attempting to open connection...");
                connection.Open();

                Console.WriteLine("✅ Successfully connected to SQL Server!");
                Console.WriteLine($"Server Version: {connection.ServerVersion}");

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT @@SERVERNAME, DB_NAME(), @@VERSION";
                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    Console.WriteLine($"Server Name: {reader.GetString(0)}");
                    Console.WriteLine($"Database: {reader.GetString(1)}");
                    Console.WriteLine($"Version: {reader.GetString(2)}");
                }

                Console.WriteLine();
                Console.WriteLine("🎉 Connection test PASSED! Your SQL Server is accessible.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection failed: {ex.Message}");
                Console.WriteLine($"Error Type: {ex.GetType().Name}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                Console.WriteLine();
                Console.WriteLine("💡 Troubleshooting tips:");
                Console.WriteLine("1. Check if SQL Server Express service is running");
                Console.WriteLine("2. Verify the instance name: ST-LPTF3-23\\SQLEXPRESS01");
                Console.WriteLine("3. Ensure TCP/IP is enabled for SQL Server");
                Console.WriteLine("4. Check Windows Authentication permissions");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
