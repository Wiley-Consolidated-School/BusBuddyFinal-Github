using System;
using System.Configuration;
using Microsoft.Data.Sqlite;
using BusBuddy.Data;

class TestConnection
{
    static void Main()
    {
        try
        {
            Console.WriteLine("Testing database connection and loading issues...");

            var conn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (conn == null)
            {
                Console.WriteLine("ERROR: DefaultConnection not found in config");
                return;
            }

            Console.WriteLine($"Connection String: {conn.ConnectionString}");
            Console.WriteLine($"Provider: {conn.ProviderName}");

            using (var connection = new SqliteConnection(conn.ConnectionString))
            {
                connection.Open();
                Console.WriteLine("Database connection successful!");

                // Test all tables that are having loading issues
                TestTable(connection, "Vehicles");
                TestTable(connection, "Routes");
                TestTable(connection, "Maintenance");
                TestTable(connection, "Fuel");

                // Test repository operations
                TestRepositories();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void TestTable(SqliteConnection connection, string tableName)
    {
        try
        {
            Console.WriteLine($"\n--- Testing {tableName} table ---");

            // Check if table exists
            using (var cmd = new SqliteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'", connection))
            {
                var result = cmd.ExecuteScalar();
                if (result == null)
                {
                    Console.WriteLine($"ERROR: {tableName} table does not exist!");
                    return;
                }
            }

            // Get record count
            using (var cmd = new SqliteCommand($"SELECT COUNT(*) FROM {tableName}", connection))
            {
                var count = cmd.ExecuteScalar();
                Console.WriteLine($"{tableName} table has {count} records");
            }

            // Get table structure
            using (var cmd = new SqliteCommand($"PRAGMA table_info({tableName})", connection))
            using (var reader = cmd.ExecuteReader())
            {
                Console.WriteLine($"{tableName} table structure:");
                while (reader.Read())
                {
                    Console.WriteLine($"  {reader["name"]} ({reader["type"]}) - NOT NULL: {reader["notnull"]}");
                }
            }

            // Try to select a few records
            using (var cmd = new SqliteCommand($"SELECT * FROM {tableName} LIMIT 3", connection))
            using (var reader = cmd.ExecuteReader())
            {
                Console.WriteLine($"Sample {tableName} records:");
                while (reader.Read())
                {
                    Console.Write("  ");
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Console.Write($"{reader.GetName(i)}={reader[i]} ");
                    }
                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR testing {tableName}: {ex.Message}");
        }
    }

    static void TestRepositories()
    {
        Console.WriteLine("\n--- Testing Repository Operations ---");

        try
        {
            // Test VehicleRepository
            Console.WriteLine("Testing VehicleRepository...");
            var vehicleRepo = new VehicleRepository();
            var vehicles = vehicleRepo.GetAllVehicles();
            Console.WriteLine($"VehicleRepository.GetAllVehicles() returned {vehicles?.Count ?? 0} vehicles");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in VehicleRepository: {ex.Message}");
        }

        try
        {
            // Test RouteRepository
            Console.WriteLine("Testing RouteRepository...");
            var routeRepo = new RouteRepository();
            var routes = routeRepo.GetAllRoutes();
            Console.WriteLine($"RouteRepository.GetAllRoutes() returned {routes?.Count ?? 0} routes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in RouteRepository: {ex.Message}");
        }

        try
        {
            // Test MaintenanceRepository
            Console.WriteLine("Testing MaintenanceRepository...");
            var maintenanceRepo = new MaintenanceRepository();
            var maintenance = maintenanceRepo.GetAllMaintenanceRecords();
            Console.WriteLine($"MaintenanceRepository.GetAllMaintenanceRecords() returned {maintenance?.Count ?? 0} records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in MaintenanceRepository: {ex.Message}");
        }

        try
        {
            // Test FuelRepository
            Console.WriteLine("Testing FuelRepository...");
            var fuelRepo = new FuelRepository();
            var fuel = fuelRepo.GetAllFuelRecords();            Console.WriteLine($"FuelRepository.GetAllFuelRecords() returned {fuel?.Count ?? 0} records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in FuelRepository: {ex.Message}");
        }
    }
}
