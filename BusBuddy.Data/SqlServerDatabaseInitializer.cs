using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;

namespace BusBuddy.Data
{
    /// <summary>
    /// SQL Server database initializer for creating and initializing the BusBuddy database
    /// Thread-safe singleton implementation to prevent concurrent initialization conflicts
    /// </summary>
    public class SqlServerDatabaseInitializer
    {
        private readonly string _connectionString;
        private static readonly ConcurrentDictionary<string, bool> _initializedDatabases = new();
        private static readonly object _lockObject = new object();

        public SqlServerDatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public void Initialize()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            var databaseName = builder.InitialCatalog;

            // Check if this database has already been initialized
            if (_initializedDatabases.ContainsKey(databaseName))
            {
                return;
            }

            lock (_lockObject)
            {
                // Double-check inside lock
                if (_initializedDatabases.ContainsKey(databaseName))
                {
                    return;
                }

                try
                {
                    Console.WriteLine("🔍 Initializing SQL Server database...");

                    // Create connection string to master database for database creation
                    var masterBuilder = new SqlConnectionStringBuilder(_connectionString)
                    {
                        InitialCatalog = "master"
                    };

                    // Check if database exists, create if it doesn't
                    using (var masterConnection = new SqlConnection(masterBuilder.ConnectionString))
                    {
                        masterConnection.Open();
                        var checkDbCommand = new SqlCommand(
                            "SELECT COUNT(*) FROM sys.databases WHERE name = @databaseName",
                            masterConnection);
                        checkDbCommand.Parameters.AddWithValue("@databaseName", databaseName);
                        var dbExists = (int)checkDbCommand.ExecuteScalar() > 0;

                        if (!dbExists)
                        {
                            Console.WriteLine($"Creating database {databaseName}...");
                            var createDbCommand = new SqlCommand($"CREATE DATABASE [{databaseName}]", masterConnection);
                            createDbCommand.ExecuteNonQuery();
                            Console.WriteLine($"✅ Database {databaseName} created successfully!");
                        }
                        else
                        {
                            Console.WriteLine($"✅ Database {databaseName} already exists.");
                        }
                    }

                    // Initialize schema and data
                    InitializeSchema();

                    // Mark this database as initialized
                    _initializedDatabases.TryAdd(databaseName, true);
                    Console.WriteLine("✅ SQL Server database initialized successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ SQL Server database initialization failed: {ex.Message}");
                    throw new InvalidOperationException($"Failed to initialize SQL Server database: {ex.Message}", ex);
                }
            }
        }

        private void InitializeSchema()
        {
            // Try to find and execute the SQL Server schema script
            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "BusBuddy.Data", "DatabaseScript.SqlServer.sql");
            if (!File.Exists(scriptPath))
            {
                // Try alternative path for deployed version
                scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatabaseScript.SqlServer.sql");
            }
            if (!File.Exists(scriptPath))
            {
                Console.WriteLine("⚠️ SQL Server schema script not found, creating basic tables...");
                CreateBasicTables();
                return;
            }

            Console.WriteLine($"📄 Reading SQL script from: {scriptPath}");
            var script = File.ReadAllText(scriptPath);
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            try
            {
                // Execute the script (split by GO statements for SQL Server)
                var batches = script.Split(new[] { "\nGO\n", "\nGO\r\n", "\r\nGO\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var batch in batches)
                {
                    if (!string.IsNullOrWhiteSpace(batch))
                    {
                        var trimmedBatch = batch.Trim();
                        if (!string.IsNullOrEmpty(trimmedBatch))
                        {
                            try
                            {
                                var command = new SqlCommand(trimmedBatch, connection);
                                command.ExecuteNonQuery();
                            }
                            catch (SqlException sqlEx) when (
                                sqlEx.Message.Contains("already an object named") ||
                                sqlEx.Message.Contains("does not exist or you do not have permission"))
                            {
                                // These are expected errors during schema initialization, log but continue
                                Console.WriteLine($"⚠️ Schema warning: {sqlEx.Message}");
                            }
                        }
                    }
                }
                Console.WriteLine("✅ Schema initialized successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Schema initialization error: {ex.Message}");
                throw;
            }
        }

        private void CreateBasicTables()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var createTablesScript = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Vehicles' AND xtype='U')
                CREATE TABLE Vehicles (
                    Id int IDENTITY(1,1) PRIMARY KEY,
                    BusNumber nvarchar(50) NOT NULL,
                    Make nvarchar(50),
                    Model nvarchar(50),
                    Year int,
                    SeatingCapacity int,
                    FuelType nvarchar(20),
                    Status nvarchar(20),
                    VINNumber nvarchar(50),
                    LicenseNumber nvarchar(20),
                    LastInspectionDate datetime
                );
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Drivers' AND xtype='U')
                CREATE TABLE Drivers (
                    DriverId int IDENTITY(1,1) PRIMARY KEY,
                    Name nvarchar(100),
                    DriverPhone nvarchar(20),
                    DriverEmail nvarchar(100),
                    Address nvarchar(200),
                    City nvarchar(50),
                    State nvarchar(2),
                    Zip nvarchar(10),
                    DriversLicenseType nvarchar(10),
                    IsTrainingComplete int NOT NULL DEFAULT 0,
                    Notes nvarchar(max),
                    Status nvarchar(50),
                    FirstName nvarchar(100),
                    LastName nvarchar(100),
                    CDLExpirationDate datetime
                );
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Activities' AND xtype='U')
                CREATE TABLE Activities (
                    ActivityID int IDENTITY(1,1) PRIMARY KEY,
                    Date datetime NOT NULL,
                    ActivityType nvarchar(50),
                    Destination nvarchar(200),
                    LeaveTime nvarchar(10),
                    EventTime nvarchar(10),
                    RequestedBy nvarchar(100),
                    AssignedVehicleID int,
                    DriverId int
                );
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Routes' AND xtype='U')
                CREATE TABLE Routes (
                    RouteId int IDENTITY(1,1) PRIMARY KEY,
                    RouteName nvarchar(100),
                    StartLocation nvarchar(200),
                    EndLocation nvarchar(200),
                    Distance float,
                    EstimatedTime nvarchar(20),
                    Notes nvarchar(max)
                );
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Fuel' AND xtype='U')
                CREATE TABLE Fuel (
                    FuelID int IDENTITY(1,1) PRIMARY KEY,
                    FuelDate datetime NOT NULL,
                    FuelLocation nvarchar(100),
                    VehicleFueledID int,
                    VehicleOdometerReading float,
                    FuelType nvarchar(20),
                    FuelAmount float,
                    FuelCost money,
                    Notes nvarchar(max)
                );
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Maintenance' AND xtype='U')
                CREATE TABLE Maintenance (
                    MaintenanceID int IDENTITY(1,1) PRIMARY KEY,
                    Date datetime,
                    BusId int,
                    OdometerReading float,
                    MaintenanceCompleted nvarchar(max),
                    Vendor nvarchar(100),
                    RepairCost money,
                    Notes nvarchar(max)
                );
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SchoolCalendar' AND xtype='U')
                CREATE TABLE SchoolCalendar (
                    CalendarID int IDENTITY(1,1) PRIMARY KEY,
                    Date datetime NOT NULL,
                    EventName nvarchar(200),
                    EventType nvarchar(50),
                    IsSchoolDay bit,
                    Notes nvarchar(max)
                );
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ActivitySchedule' AND xtype='U')
                CREATE TABLE ActivitySchedule (
                    ActivityScheduleID int IDENTITY(1,1) PRIMARY KEY,
                    ActivityID int,
                    ScheduleDate datetime,
                    StartTime nvarchar(10),
                    EndTime nvarchar(10),
                    RecurrencePattern nvarchar(50),
                    Notes nvarchar(max)
                );
            ";
            var command = new SqlCommand(createTablesScript, connection);
            command.ExecuteNonQuery();
            Console.WriteLine("✅ All basic tables created successfully!");
        }

        /// <summary>
        /// Resets the initialization state for testing purposes
        /// </summary>
        public static void ResetInitializationState()
        {
            _initializedDatabases.Clear();
        }

        /// <summary>
        /// Forces re-initialization of a specific database (for testing)
        /// </summary>
        public static void ForceReinitialization(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            _initializedDatabases.TryRemove(databaseName, out _);
        }
    }
}

