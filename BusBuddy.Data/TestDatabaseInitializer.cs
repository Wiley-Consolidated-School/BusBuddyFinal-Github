using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace BusBuddy.Data
{
    /// <summary>
    /// Initializes the test database for development and testing
    /// </summary>
    public static class TestDatabaseInitializer
    {
        private const string DefaultConnectionString =
            "Data Source=.\\SQLEXPRESS01;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";

        private const string TestDatabaseName = "BusBuddy_Test";

        /// <summary>
        /// Ensures the test database exists and has the required schema
        /// </summary>
        public static void EnsureTestDatabaseExists()
        {
            try
            {
                Console.WriteLine("Checking test database existence...");

                // First check if the database exists
                if (!DatabaseExists())
                {
                    Console.WriteLine($"Test database '{TestDatabaseName}' does not exist. Creating...");
                    CreateDatabase();
                    Console.WriteLine($"Test database '{TestDatabaseName}' created successfully.");
                }
                else
                {
                    Console.WriteLine($"Test database '{TestDatabaseName}' already exists.");
                }

                // Now that we know the database exists, we can create the schema if needed
                CreateTablesIfNeeded();

                Console.WriteLine("Test database initialization complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing test database: {ex.Message}");
                Console.WriteLine("Please run the create-test-database.ps1 script as administrator to create the database manually.");

                // This will allow the application to continue with fallback options
                // rather than crashing entirely if the database setup fails
            }
        }

        private static bool DatabaseExists()
        {
            using (var connection = new SqlConnection(DefaultConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = $"SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name = '{TestDatabaseName}'";
                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }

        private static void CreateDatabase()
        {
            using (var connection = new SqlConnection(DefaultConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = $"CREATE DATABASE [{TestDatabaseName}]";
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void CreateTablesIfNeeded()
        {
            // Get the test database connection string
            string testConnectionString = $"Data Source=.\\SQLEXPRESS01;Initial Catalog={TestDatabaseName};Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";

            using (var connection = new SqlConnection(testConnectionString))
            {
                connection.Open();

                // Check if the Vehicles table exists
                bool vehiclesTableExists = TableExists(connection, "Vehicles");
                bool activitiesTableExists = TableExists(connection, "Activities");
                bool activityScheduleTableExists = TableExists(connection, "ActivitySchedule");

                if (!vehiclesTableExists)
                {
                    // Create basic schema for testing
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;

                        // Create Vehicles table
                        command.CommandText = @"
                            CREATE TABLE Vehicles (
                                VehicleID INT PRIMARY KEY IDENTITY(1,1),
                                VehicleNumber NVARCHAR(50) NOT NULL,
                                Make NVARCHAR(50),
                                Model NVARCHAR(50),
                                Year INT,
                                Capacity INT,
                                Status NVARCHAR(20),
                                LastMaintenance DATETIME,
                                Notes NVARCHAR(MAX)
                            )";
                        command.ExecuteNonQuery();

                        // Create Drivers table
                        command.CommandText = @"
                            CREATE TABLE Drivers (
                                DriverID INT PRIMARY KEY IDENTITY(1,1),
                                FirstName NVARCHAR(50) NOT NULL,
                                LastName NVARCHAR(50) NOT NULL,
                                LicenseType NVARCHAR(20),
                                CDLExpiry DATETIME,
                                PhoneNumber NVARCHAR(20),
                                Email NVARCHAR(100),
                                Status NVARCHAR(20),
                                Notes NVARCHAR(MAX)
                            )";
                        command.ExecuteNonQuery();

                        // Create Routes table
                        command.CommandText = @"
                            CREATE TABLE Routes (
                                RouteID INT PRIMARY KEY IDENTITY(1,1),
                                RouteName NVARCHAR(50) NOT NULL,
                                Description NVARCHAR(200),
                                StartLocation NVARCHAR(100),
                                EndLocation NVARCHAR(100),
                                EstimatedDuration INT,
                                Distance DECIMAL(10,2),
                                Notes NVARCHAR(MAX)
                            )";
                        command.ExecuteNonQuery();

                        Console.WriteLine("Basic schema created successfully.");
                    }
                }
                else
                {
                    Console.WriteLine("Basic tables already exist in the test database.");
                }

                // Create Activities table if it doesn't exist
                if (!activitiesTableExists)
                {
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;

                        // Create Activities table
                        command.CommandText = @"
                            CREATE TABLE Activities (
                                ActivityID INT IDENTITY(1,1) PRIMARY KEY,
                                Date NVARCHAR(50),
                                ActivityType NVARCHAR(100),
                                Destination NVARCHAR(200),
                                LeaveTime NVARCHAR(50),
                                EventTime NVARCHAR(50),
                                ReturnTime NVARCHAR(50),
                                RequestedBy NVARCHAR(100),
                                AssignedVehicleID INT,
                                DriverID INT,
                                Notes NVARCHAR(MAX)
                            )";
                        command.ExecuteNonQuery();

                        Console.WriteLine("Activities table created successfully.");
                    }
                }

                // Create ActivitySchedule table if it doesn't exist
                if (!activityScheduleTableExists)
                {
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;

                        // Create ActivitySchedule table
                        command.CommandText = @"
                            CREATE TABLE ActivitySchedule (
                                ScheduleID INT IDENTITY(1,1) PRIMARY KEY,
                                Date NVARCHAR(50),
                                TripType NVARCHAR(100),
                                ScheduledVehicleID INT,
                                ScheduledDestination NVARCHAR(200),
                                ScheduledLeaveTime NVARCHAR(50),
                                ScheduledEventTime NVARCHAR(50),
                                ScheduledReturnTime NVARCHAR(50),
                                ScheduledRiders INT,
                                ScheduledDriverID INT,
                                Notes NVARCHAR(MAX)
                            )";
                        command.ExecuteNonQuery();

                        Console.WriteLine("ActivitySchedule table created successfully.");
                    }
                }
            }
        }

        private static bool TableExists(SqlConnection connection, string tableName)
        {
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
                return (int)command.ExecuteScalar() > 0;
            }
        }
    }
}
