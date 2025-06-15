using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace BusBuddy.Data
{
    /// <summary>
    /// SQLite database initializer for fallback when SQL Server is not available
    /// </summary>
    public static class SqliteDatabaseInitializer
    {
        public static async Task InitializeSqliteDatabaseAsync()
        {
            try
            {
                Console.WriteLine("🔍 Initializing SQLite database as fallback...");

                var connectionString = "Data Source=busbuddy.db";

                // Read the SQLite schema script
                var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "BusBuddy.Data", "DatabaseScript.sql");
                Console.WriteLine($"🔍 Looking for script at: {scriptPath}");

                if (!File.Exists(scriptPath))
                {
                    // Try alternative path for deployed version
                    scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatabaseScript.sql");
                    Console.WriteLine($"🔍 Trying alternative path: {scriptPath}");
                }

                if (!File.Exists(scriptPath))
                {
                    Console.WriteLine("⚠️ SQLite schema script not found, creating basic tables...");
                    await CreateBasicTablesAsync(connectionString);
                    return;
                }

                Console.WriteLine($"📄 Reading SQL script from: {scriptPath}");
                var script = await File.ReadAllTextAsync(scriptPath);

                Console.WriteLine($"📝 Script content preview (first 500 chars):");
                Console.WriteLine(script.Length > 500 ? script.Substring(0, 500) + "..." : script);

                // Check if TimeCard is mentioned in the script
                if (script.Contains("TimeCard", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("⚠️ WARNING: Script contains TimeCard references!");
                }
                else
                {
                    Console.WriteLine("✅ No TimeCard references found in script");
                }

                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                // Execute the script
                Console.WriteLine("⚡ Executing SQL script...");
                var command = new SqliteCommand(script, connection);
                await command.ExecuteNonQueryAsync();

                Console.WriteLine("✅ SQLite database initialized successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SQLite database initialization failed: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");

                // Fall back to basic table creation
                Console.WriteLine("🔄 Falling back to basic table creation...");
                try
                {
                    await CreateBasicTablesAsync("Data Source=busbuddy.db");
                    Console.WriteLine("✅ Basic tables created successfully!");
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"❌ Fallback table creation failed: {fallbackEx.Message}");
                    throw;
                }
            }
        }

        private static async Task CreateBasicTablesAsync(string connectionString)
        {
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            var createTablesScript = @"
                CREATE TABLE IF NOT EXISTS Vehicles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    VehicleNumber TEXT NOT NULL,
                    Make TEXT,
                    Model TEXT,
                    Year INTEGER,
                    SeatingCapacity INTEGER,
                    FuelType TEXT,
                    Status TEXT,
                    VINNumber TEXT,
                    LicenseNumber TEXT,
                    DateLastInspection TEXT
                );

                CREATE TABLE IF NOT EXISTS Drivers (
                    DriverID INTEGER PRIMARY KEY AUTOINCREMENT,
                    DriverName TEXT,
                    DriverPhone TEXT,
                    DriverEmail TEXT,
                    Address TEXT,
                    City TEXT,
                    State TEXT,
                    Zip TEXT,
                    DriversLicenseType TEXT,
                    TrainingComplete INTEGER,
                    Notes TEXT
                );

                CREATE TABLE IF NOT EXISTS Activities (
                    ActivityID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT NOT NULL,
                    ActivityType TEXT,
                    Destination TEXT,
                    LeaveTime TEXT,
                    EventTime TEXT,
                    RequestedBy TEXT,
                    AssignedVehicleID INTEGER,
                    DriverID INTEGER
                );

                CREATE TABLE IF NOT EXISTS ActivitySchedule (
                    ActivityScheduleID INTEGER PRIMARY KEY AUTOINCREMENT,
                    ActivityID INTEGER,
                    ScheduleDate TEXT,
                    StartTime TEXT,
                    EndTime TEXT,
                    RecurrencePattern TEXT,
                    Notes TEXT
                );

                CREATE TABLE IF NOT EXISTS Routes (
                    RouteID INTEGER PRIMARY KEY AUTOINCREMENT,
                    RouteName TEXT,
                    StartLocation TEXT,
                    EndLocation TEXT,
                    Distance REAL,
                    EstimatedTime TEXT,
                    Notes TEXT
                );

                CREATE TABLE IF NOT EXISTS SchoolCalendar (
                    CalendarID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT NOT NULL,
                    EventName TEXT,
                    EventType TEXT,
                    IsSchoolDay INTEGER,
                    Notes TEXT
                );

                CREATE TABLE IF NOT EXISTS Fuel (
                    FuelID INTEGER PRIMARY KEY AUTOINCREMENT,
                    FuelDate TEXT NOT NULL,
                    FuelLocation TEXT,
                    VehicleFueledID INTEGER,
                    VehicleOdometerReading REAL,
                    FuelType TEXT,
                    FuelAmount REAL,
                    FuelCost REAL,
                    Notes TEXT
                );

                CREATE TABLE IF NOT EXISTS Maintenance (
                    MaintenanceID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT,
                    VehicleID INTEGER,
                    OdometerReading REAL,
                    MaintenanceCompleted TEXT,
                    Vendor TEXT,
                    RepairCost REAL,
                    Notes TEXT
                );
            ";

            var command = new SqliteCommand(createTablesScript, connection);
            await command.ExecuteNonQueryAsync();

            Console.WriteLine("✅ All basic tables created successfully!");
        }
    }
}
