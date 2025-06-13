using System;
using System.IO;
using System.Data;
using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using System.Reflection;
using BusBuddy.Db;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using Moq;
using System.Threading;

namespace BusBuddy.Tests
{
            public class DatabaseInitializerTests
            {
            private readonly string TestSqliteDbPath;

            public DatabaseInitializerTests()
            {
            // Use a unique filename for each test instance to avoid locking issues
            TestSqliteDbPath = $"test_busbuddy_{Guid.NewGuid():N}.db";
            }
            [Fact]
            public void DatabaseInitializer_ShouldCreateSqliteTables()
            {
            // Arrange
            if (File.Exists(TestSqliteDbPath))
            {
            File.Delete(TestSqliteDbPath);
            }

            string connectionString = $"Data Source={TestSqliteDbPath}";
            // Don't override the configuration - let it use the App.config

            try
            {
            // Act
            DatabaseInitializer.InitializeDatabase(connectionString, "Microsoft.Data.Sqlite");

            // Assert
            using (var connection = new SqliteConnection(connectionString))
            {
                    connection.Open();

                    // Verify tables exist
                    var requiredTables = new List<string> {
                        "Vehicles", "Drivers", "Routes", "Activities", "Fuel",
                        "Maintenance", "SchoolCalendar", "ActivitySchedule", "TimeCard"
                    };

                    foreach (var table in requiredTables)
                    {
                        var tableExists = TableExists(connection, table);
                        Assert.True(tableExists, $"Table {table} does not exist");
                    }

                    // Verify indexes exist
                    var requiredIndexes = new List<string> {
                        "idx_routes_date", "idx_routes_driver", "idx_routes_vehicle",
                        "idx_activities_date", "idx_activities_driver", "idx_activities_vehicle",
                        "idx_fuel_date", "idx_fuel_vehicle",
                        "idx_maintenance_date", "idx_maintenance_vehicle",
                        "idx_calendar_date", "idx_calendar_enddate", "idx_calendar_category",
                        "idx_activityschedule_date", "idx_activityschedule_driver", "idx_activityschedule_vehicle",
                        "idx_timecard_date", "idx_timecard_daytype", "idx_timecard_driver"
                    };

                    foreach (var index in requiredIndexes)
                    {
                        var indexExists = IndexExists(connection, index);
                        Assert.True(indexExists, $"Index {index} does not exist");
                    }

                    // Verify vehicle table schema
                    var vehicleColumns = GetTableColumns(connection, "Vehicles");
                    Assert.Contains(vehicleColumns, c => c.Name == "Id" && c.Type == "INTEGER" && c.IsPrimaryKey);
                    Assert.Contains(vehicleColumns, c => c.Name == "VehicleNumber" && c.Type == "TEXT" && c.NotNull);
                    Assert.Contains(vehicleColumns, c => c.Name == "Make" && c.Type == "TEXT");
                    Assert.Contains(vehicleColumns, c => c.Name == "Model" && c.Type == "TEXT");
                    Assert.Contains(vehicleColumns, c => c.Name == "Year" && c.Type == "INTEGER");
                    Assert.Contains(vehicleColumns, c => c.Name == "SeatingCapacity" && c.Type == "INTEGER"); Assert.Contains(vehicleColumns, c => c.Name == "FuelType" && c.Type == "TEXT");
                    Assert.Contains(vehicleColumns, c => c.Name == "Status" && c.Type == "TEXT");
            }
            }
            finally
            {
            // Cleanup
            CleanupTestDb(TestSqliteDbPath);
            }
            }

            [Fact]
            public void DatabaseInitializer_ShouldCreateSqlServerTables()
            {
            // Arrange - Using SQL Server LocalDB or skip if not available
            string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=BusBuddyTest;Trusted_Connection=True;TrustServerCertificate=True;";

            // First check if LocalDB is available
            bool canConnectToSqlServer = false;
            try
            {
            using (var testConnection = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                    testConnection.Open();
                    canConnectToSqlServer = true;
            }
            }
            catch (Exception)
            {
            // LocalDB not available, skip the test
            return;
            }

            if (!canConnectToSqlServer)
            {
            return; // Skip test if SQL Server is not available
            }

            try
            {
            // Create test database
            using (var masterConnection = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                    masterConnection.Open();
                    using (var cmd = masterConnection.CreateCommand())
                    {
                        cmd.CommandText = "IF EXISTS (SELECT * FROM sys.databases WHERE name = 'BusBuddyTest') DROP DATABASE BusBuddyTest; CREATE DATABASE BusBuddyTest;";
                        cmd.ExecuteNonQuery();
                    }
            }

            // Act
            DatabaseInitializer.InitializeDatabase(connectionString, "Microsoft.Data.SqlClient");

            // Assert
            using (var connection = new SqlConnection(connectionString))
            {
                    connection.Open();

                    // Verify tables exist
                    var tableCount = GetSqlServerTableCount(connection);
                    Assert.Equal(9, tableCount); // 9 tables should exist

                    // Verify specific tables exist
                    var requiredTables = new[] {
                        "Vehicles", "Drivers", "Routes", "Activities", "Fuel",
                        "Maintenance", "SchoolCalendar", "ActivitySchedule", "TimeCard"
                    };

                    foreach (var table in requiredTables)
                    {
                        var exists = TableExistsInSqlServer(connection, table);
                        Assert.True(exists, $"Table {table} should exist in SQL Server database");
                    }
            }
            }
            finally
            {
            // Cleanup - drop test database
            try
            {
                    using (var connection = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"))
                    {
                        connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "IF EXISTS (SELECT * FROM sys.databases WHERE name = 'BusBuddyTest') DROP DATABASE BusBuddyTest";
                            cmd.ExecuteNonQuery();
                        }
                    }
            }
            catch
            {
                    // Ignore cleanup errors
            }
            }
            }
            [Fact]
            public void DatabaseInitializer_ShouldVerifyForeignKeyConstraints_InSqlite()
            {
            // Arrange
            if (File.Exists(TestSqliteDbPath))
            {
            File.Delete(TestSqliteDbPath);
            }

            string connectionString = $"Data Source={TestSqliteDbPath}";
            // Don't override the configuration - let it use the App.config

            try
            {
            // Act
            DatabaseInitializer.InitializeDatabase(connectionString, "Microsoft.Data.Sqlite");

            // Assert
            using (var connection = new SqliteConnection(connectionString))
            {
                    connection.Open();

                    // Verify foreign keys are enabled
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA foreign_keys";
                        var foreignKeysEnabled = Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                        Assert.True(foreignKeysEnabled, "Foreign keys should be enabled");
                    }

                    // Verify Routes table foreign keys
                    var routeForeignKeys = GetForeignKeys(connection, "Routes");
                    Assert.Contains(routeForeignKeys, fk => fk.Table == "Vehicles" && fk.From == "AMVehicleID" && fk.To == "Id");
                    Assert.Contains(routeForeignKeys, fk => fk.Table == "Drivers" && fk.From == "AMDriverID" && fk.To == "DriverID");
                    Assert.Contains(routeForeignKeys, fk => fk.Table == "Vehicles" && fk.From == "PMVehicleID" && fk.To == "Id");
                    Assert.Contains(routeForeignKeys, fk => fk.Table == "Drivers" && fk.From == "PMDriverID" && fk.To == "DriverID");

                    // Verify Activities table foreign keys
                    var activitiesForeignKeys = GetForeignKeys(connection, "Activities");
                    Assert.Contains(activitiesForeignKeys, fk => fk.Table == "Vehicles" && fk.From == "AssignedVehicleID" && fk.To == "Id");
                    Assert.Contains(activitiesForeignKeys, fk => fk.Table == "Drivers" && fk.From == "DriverID" && fk.To == "DriverID");

                    // Verify Fuel table foreign keys
                    var fuelForeignKeys = GetForeignKeys(connection, "Fuel");
                    Assert.Contains(fuelForeignKeys, fk => fk.Table == "Vehicles" && fk.From == "VehicleFueledID" && fk.To == "Id");

                    // Verify Maintenance table foreign keys
                    var maintenanceForeignKeys = GetForeignKeys(connection, "Maintenance");
                    Assert.Contains(maintenanceForeignKeys, fk => fk.Table == "Vehicles" && fk.From == "VehicleID" && fk.To == "Id");

                    // Verify ActivitySchedule table foreign keys
                    var schedForeignKeys = GetForeignKeys(connection, "ActivitySchedule");
                    Assert.Contains(schedForeignKeys, fk => fk.Table == "Vehicles" && fk.From == "ScheduledVehicleID" && fk.To == "Id");
                    Assert.Contains(schedForeignKeys, fk => fk.Table == "Drivers" && fk.From == "ScheduledDriverID" && fk.To == "DriverID");

                    // Verify TimeCard table foreign keys
                    var timecardForeignKeys = GetForeignKeys(connection, "TimeCard");
                    Assert.Contains(timecardForeignKeys, fk => fk.Table == "Drivers" && fk.From == "DriverID" && fk.To == "DriverID");
            }
            }
            finally
            {
            // Cleanup
            CleanupTestDb(TestSqliteDbPath);
            }
            }
            [Fact]
            public void DatabaseInitializer_ShouldVerifyAllTablesAndIndexes_InSqlite()
            {
            // Arrange
            if (File.Exists(TestSqliteDbPath))
            {
            File.Delete(TestSqliteDbPath);
            }

            string connectionString = $"Data Source={TestSqliteDbPath}";
            // Don't override the configuration - let it use the App.config

            try
            {
            // Act
            DatabaseInitializer.InitializeDatabase(connectionString, "Microsoft.Data.Sqlite");

            // Assert
            using (var connection = new SqliteConnection(connectionString))
            {
                    connection.Open();

                    // Get all tables in the database
                    var tables = GetAllTables(connection);

                    // Check for all required tables
                    var requiredTables = new[] {
                        "Vehicles", "Drivers", "Routes", "Activities", "Fuel",
                        "Maintenance", "SchoolCalendar", "ActivitySchedule", "TimeCard"
                    };

                    foreach (var table in requiredTables)
                    {
                        Assert.Contains(table, tables);
                    }

                    // Check for all required indexes
                    var requiredIndexes = new[] {
                        "idx_routes_date", "idx_routes_driver", "idx_routes_vehicle",
                        "idx_activities_date", "idx_activities_driver", "idx_activities_vehicle",
                        "idx_fuel_date", "idx_fuel_vehicle",
                        "idx_maintenance_date", "idx_maintenance_vehicle",
                        "idx_calendar_date", "idx_calendar_enddate", "idx_calendar_category",
                        "idx_activityschedule_date", "idx_activityschedule_driver", "idx_activityschedule_vehicle",
                        "idx_timecard_date", "idx_timecard_daytype", "idx_timecard_driver"
                    };

                    var indexes = GetAllIndexes(connection);
                    foreach (var index in requiredIndexes)
                    {
                        Assert.Contains(index, indexes);
                    }

                    // Check all tables have correct structure
                    VerifyVehiclesTableStructure(connection);
                    VerifyDriversTableStructure(connection);
                    VerifyRoutesTableStructure(connection);
                    VerifyActivitiesTableStructure(connection);
                    VerifyFuelTableStructure(connection);
                    VerifyMaintenanceTableStructure(connection);
                    VerifySchoolCalendarTableStructure(connection);
                    VerifyActivityScheduleTableStructure(connection);
                    VerifyTimeCardTableStructure(connection);
            }
            }
            finally
            {
            // Cleanup
            CleanupTestDb(TestSqliteDbPath);
            }
            }

            // SQL Server helper methods
            private static int GetSqlServerTableCount(SqlConnection connection)
            {
            using (var cmd = connection.CreateCommand())
            {
            cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            return (int)cmd.ExecuteScalar();
            }
            }

            private static bool TableExistsInSqlServer(SqlConnection connection, string tableName)
            {
            using (var cmd = connection.CreateCommand())
            {
            cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName AND TABLE_TYPE = 'BASE TABLE'";
            cmd.Parameters.Add(new SqlParameter("@tableName", tableName));
            var count = (int)cmd.ExecuteScalar();
            return count > 0;
            }
            }

            // SQLite helper methods
            private static string[] GetAllTables(SqliteConnection connection)
            {
            var tables = new List<string>();
            using (var cmd = connection.CreateCommand())
            {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
            using (var reader = cmd.ExecuteReader())
            {
                    while (reader.Read())
                    {
                        tables.Add(reader["name"]?.ToString() ?? string.Empty);
                    }
            }
            }
            return tables.ToArray();
            }

            private static string[] GetAllIndexes(SqliteConnection connection)
            {
            var indexes = new List<string>();
            using (var cmd = connection.CreateCommand())
            {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='index' AND name NOT LIKE 'sqlite_%'";
            using (var reader = cmd.ExecuteReader())
            {
                    while (reader.Read())
                    {
                        indexes.Add(reader["name"]?.ToString() ?? string.Empty);
                    }
            }
            }
            return indexes.ToArray();
            }

            private static void VerifyVehiclesTableStructure(SqliteConnection connection)
            {
            var columns = GetTableColumns(connection, "Vehicles");

            Assert.Equal(13, columns.Count);
            Assert.Contains(columns, c => c.Name == "Id" && c.Type == "INTEGER" && c.IsPrimaryKey);
            Assert.Contains(columns, c => c.Name == "VehicleNumber" && c.Type == "TEXT" && c.NotNull);
            Assert.Contains(columns, c => c.Name == "BusNumber" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "Make" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "Model" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "Year" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "SeatingCapacity" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "VINNumber" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "LicenseNumber" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "DateLastInspection" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "FuelType" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "Status" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "Notes" && c.Type == "TEXT");
            }

            private static void VerifyDriversTableStructure(SqliteConnection connection)
            {
            var columns = GetTableColumns(connection, "Drivers");

            Assert.Equal(11, columns.Count);
            Assert.Contains(columns, c => c.Name == "DriverID" && c.Type == "INTEGER" && c.IsPrimaryKey);
            Assert.Contains(columns, c => c.Name == "DriverName" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "DriverPhone" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "DriverEmail" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "Address" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "City" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "State" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "Zip" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "DriversLicenseType" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "TrainingComplete" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "Notes" && c.Type == "TEXT");
            }

            private static void VerifyRoutesTableStructure(SqliteConnection connection)
            {
            var columns = GetTableColumns(connection, "Routes");

            Assert.Equal(14, columns.Count);
            Assert.Contains(columns, c => c.Name == "RouteID" && c.Type == "INTEGER" && c.IsPrimaryKey);
            Assert.Contains(columns, c => c.Name == "Date" && c.Type == "TEXT" && c.NotNull);
            Assert.Contains(columns, c => c.Name == "RouteName" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "AMVehicleID" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "AMBeginMiles" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "AMEndMiles" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "AMRiders" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "AMDriverID" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "PMVehicleID" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "PMBeginMiles" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "PMEndMiles" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "PMRiders" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "PMDriverID" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "Notes" && c.Type == "TEXT");

            // Verify foreign keys
            var foreignKeys = GetForeignKeys(connection, "Routes");
            Assert.Contains(foreignKeys, fk => fk.Table == "Vehicles" && fk.From == "AMVehicleID");
            Assert.Contains(foreignKeys, fk => fk.Table == "Drivers" && fk.From == "AMDriverID");
            Assert.Contains(foreignKeys, fk => fk.Table == "Vehicles" && fk.From == "PMVehicleID");
            Assert.Contains(foreignKeys, fk => fk.Table == "Drivers" && fk.From == "PMDriverID");
            }

            private static void VerifyActivitiesTableStructure(SqliteConnection connection)
            {
            var columns = GetTableColumns(connection, "Activities");

            Assert.Equal(11, columns.Count);
            Assert.Contains(columns, c => c.Name == "ActivityID" && c.Type == "INTEGER" && c.IsPrimaryKey);
            Assert.Contains(columns, c => c.Name == "Date" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "ActivityType" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "Destination" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "LeaveTime" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "EventTime" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "ReturnTime" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "RequestedBy" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "AssignedVehicleID" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "DriverID" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "Notes" && c.Type == "TEXT");

            // Verify foreign keys
            var foreignKeys = GetForeignKeys(connection, "Activities");
            Assert.Contains(foreignKeys, fk => fk.Table == "Vehicles" && fk.From == "AssignedVehicleID");
            Assert.Contains(foreignKeys, fk => fk.Table == "Drivers" && fk.From == "DriverID");
            }

            private static void VerifyFuelTableStructure(SqliteConnection connection)
            {
            var columns = GetTableColumns(connection, "Fuel");

            Assert.Equal(9, columns.Count);
            Assert.Contains(columns, c => c.Name == "FuelID" && c.Type == "INTEGER" && c.IsPrimaryKey);
            Assert.Contains(columns, c => c.Name == "FuelDate" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "FuelLocation" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "VehicleFueledID" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "VehicleOdometerReading" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "FuelType" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "FuelAmount" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "FuelCost" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "Notes" && c.Type == "TEXT");

            // Verify foreign keys
            var foreignKeys = GetForeignKeys(connection, "Fuel");
            Assert.Contains(foreignKeys, fk => fk.Table == "Vehicles" && fk.From == "VehicleFueledID");
            }

            private static void VerifyMaintenanceTableStructure(SqliteConnection connection)
            {
            var columns = GetTableColumns(connection, "Maintenance");

            Assert.Equal(8, columns.Count);
            Assert.Contains(columns, c => c.Name == "MaintenanceID" && c.Type == "INTEGER" && c.IsPrimaryKey);
            Assert.Contains(columns, c => c.Name == "Date" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "VehicleID" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "OdometerReading" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "MaintenanceCompleted" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "Vendor" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "RepairCost" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "Notes" && c.Type == "TEXT");

            // Verify foreign keys
            var foreignKeys = GetForeignKeys(connection, "Maintenance");
            Assert.Contains(foreignKeys, fk => fk.Table == "Vehicles" && fk.From == "VehicleID");
            }

            private static void VerifySchoolCalendarTableStructure(SqliteConnection connection)
            {
            var columns = GetTableColumns(connection, "SchoolCalendar");

            Assert.Equal(7, columns.Count);
            Assert.Contains(columns, c => c.Name == "CalendarID" && c.Type == "INTEGER" && c.IsPrimaryKey);
            Assert.Contains(columns, c => c.Name == "Date" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "EndDate" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "Category" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "Description" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "RouteNeeded" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "Notes" && c.Type == "TEXT");
            }

            private static void VerifyActivityScheduleTableStructure(SqliteConnection connection)
            {
            var columns = GetTableColumns(connection, "ActivitySchedule");

            Assert.Equal(11, columns.Count);
            Assert.Contains(columns, c => c.Name == "ScheduleID" && c.Type == "INTEGER" && c.IsPrimaryKey);
            Assert.Contains(columns, c => c.Name == "Date" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "TripType" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "ScheduledVehicleID" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "ScheduledDestination" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "ScheduledLeaveTime" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "ScheduledEventTime" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "ScheduledReturnTime" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "ScheduledRiders" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "ScheduledDriverID" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "Notes" && c.Type == "TEXT");

            // Verify foreign keys
            var foreignKeys = GetForeignKeys(connection, "ActivitySchedule");
            Assert.Contains(foreignKeys, fk => fk.Table == "Vehicles" && fk.From == "ScheduledVehicleID");
            Assert.Contains(foreignKeys, fk => fk.Table == "Drivers" && fk.From == "ScheduledDriverID");
            }

            private static void VerifyTimeCardTableStructure(SqliteConnection connection)
            {
            var columns = GetTableColumns(connection, "TimeCard");

            Assert.Equal(18, columns.Count);
            Assert.Contains(columns, c => c.Name == "TimeCardID" && c.Type == "INTEGER" && c.IsPrimaryKey);
            Assert.Contains(columns, c => c.Name == "DriverID" && c.Type == "INTEGER");
            Assert.Contains(columns, c => c.Name == "Date" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "DayType" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "AMClockIn" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "LunchClockOut" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "LunchClockIn" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "PMClockOut" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "RouteAMClockOut" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "RouteAMClockIn" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "RoutePMClockOut" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "RoutePMClockIn" && c.Type == "TEXT");
            Assert.Contains(columns, c => c.Name == "TotalTime" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "Overtime" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "WeeklyTotal" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "WeeklyHours" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "MonthlyTotal" && c.Type == "REAL");
            Assert.Contains(columns, c => c.Name == "Notes" && c.Type == "TEXT");

            // Verify foreign keys
            var foreignKeys = GetForeignKeys(connection, "TimeCard");
            Assert.Contains(foreignKeys, fk => fk.Table == "Drivers" && fk.From == "DriverID");
            }

            #region Helper Methods

            private static void CleanupTestDb(string dbPath)
            {
            try
            {
            if (File.Exists(dbPath))
            {
                    // Force garbage collection to release any connections
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Try to delete with retries
                    int retries = 3;
                    while (retries > 0)
                    {
                        try
                        {
                            File.Delete(dbPath);
                            break;
                        }
                        catch (IOException)
                        {
                            retries--;
                            if (retries > 0)
                            {
                                Thread.Sleep(100);
                            }
                        }
                    }
            }
            }
            catch
            {
            // Ignore cleanup errors in tests
            }
            }

            private static void SetupConnectionString(string providerName, string connectionString)
            {
            // This is a simplified approach for testing
            // In a real environment, you might use a test configuration or mocking
            var mockConnectionStringSettings = new ConnectionStringSettings("DefaultConnection", connectionString, providerName);

            // Use reflection to modify the static ConfigurationManager settings
            var configType = typeof(ConfigurationManager);
            var connectionsProperty = configType.GetProperty("ConnectionStrings", BindingFlags.Static | BindingFlags.NonPublic);

            if (connectionsProperty != null)
            {
            var connectionsCollection = new ConnectionStringSettingsCollection();
            connectionsCollection.Add(mockConnectionStringSettings);

            var internalCollection = configType.GetNestedType("ConnectionStringSettingsCollection", BindingFlags.NonPublic);
            if (internalCollection != null)
            {
                    var instance = Activator.CreateInstance(internalCollection, connectionsCollection);
                    connectionsProperty.SetValue(null, instance);
            }
            }
            }

            private static bool TableExists(SqliteConnection connection, string tableName)
            {
            using (var cmd = connection.CreateCommand())
            {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@name";
            cmd.Parameters.Add(new SqliteParameter("@name", tableName));
            var result = cmd.ExecuteScalar();
            return result != null;
            }
            }

            private static bool IndexExists(SqliteConnection connection, string indexName)
            {
            using (var cmd = connection.CreateCommand())
            {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='index' AND name=@name";
            cmd.Parameters.Add(new SqliteParameter("@name", indexName));
            var result = cmd.ExecuteScalar();
            return result != null;
            }
            }

            private static List<ColumnInfo> GetTableColumns(SqliteConnection connection, string tableName)
            {
            var columns = new List<ColumnInfo>();
            using (var cmd = connection.CreateCommand())
            {
            cmd.CommandText = $"PRAGMA table_info({tableName})";
            using (var reader = cmd.ExecuteReader())
            {
                    while (reader.Read())
                    {
                        columns.Add(new ColumnInfo
                        {
                            Name = reader["name"]?.ToString() ?? string.Empty,
                            Type = reader["type"]?.ToString() ?? string.Empty,
                            NotNull = Convert.ToInt32(reader["notnull"]) == 1,
                            IsPrimaryKey = Convert.ToInt32(reader["pk"]) == 1
                        });
                    }
            }
            }
            return columns;
            }

            private static List<ForeignKeyInfo> GetForeignKeys(SqliteConnection connection, string tableName)
            {
            var foreignKeys = new List<ForeignKeyInfo>();
            using (var cmd = connection.CreateCommand())
            {
            cmd.CommandText = $"PRAGMA foreign_key_list({tableName})";
            using (var reader = cmd.ExecuteReader())
            {
                    while (reader.Read())
                    {
                        foreignKeys.Add(new ForeignKeyInfo
                        {
                            Id = Convert.ToInt32(reader["id"] ?? 0),
                            Table = reader["table"]?.ToString() ?? string.Empty,
                            From = reader["from"]?.ToString() ?? string.Empty,
                            To = reader["to"]?.ToString() ?? string.Empty
                        });
                    }
            }
            }
            return foreignKeys;
            }
            private class ForeignKeyInfo
            {
            public int Id { get; set; }
            public string Table { get; set; } = string.Empty;
            public string From { get; set; } = string.Empty;
            public string To { get; set; } = string.Empty;
            }
            private class ColumnInfo
            {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public bool NotNull { get; set; }
            public bool IsPrimaryKey { get; set; }
            }

            #endregion
            }
}
