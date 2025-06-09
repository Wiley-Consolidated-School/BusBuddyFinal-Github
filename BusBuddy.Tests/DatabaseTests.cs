using Xunit;
using System;
using System.Collections.Generic;
using BusBuddy.Models;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Linq;
using Dapper;

namespace BusBuddy.Tests
{
    [Collection("Database collection")]
    public class DatabaseTests
    {
        private readonly DatabaseFixture _fixture;

        public DatabaseTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Database_ShouldHaveAllRequiredTables()
        {
            // Arrange
            var requiredTables = new[]
            {
                "Vehicles", "Drivers", "Routes", "Activities", "Fuel",
                "Maintenance", "SchoolCalendar", "ActivitySchedule", "TimeCard"
            };

            // Act & Assert
            foreach (var table in requiredTables)
            {
                Assert.True(_fixture.TableExists(table), $"Table {table} does not exist");
            }
        }

        [Fact]
        public void Database_ShouldHaveAllRequiredIndexes()
        {
            // Arrange
            var requiredIndexes = new[]
            {
                "idx_routes_date", "idx_routes_driver", "idx_routes_vehicle",
                "idx_activities_date", "idx_activities_driver", "idx_activities_vehicle",
                "idx_fuel_date", "idx_fuel_vehicle",
                "idx_maintenance_date", "idx_maintenance_vehicle",
                "idx_calendar_date", "idx_calendar_enddate", "idx_calendar_category",
                "idx_activityschedule_date", "idx_activityschedule_driver", "idx_activityschedule_vehicle",
                "idx_timecard_date", "idx_timecard_daytype", "idx_timecard_driver"
            };

            // Act & Assert
            foreach (var index in requiredIndexes)
            {
                Assert.True(_fixture.IndexExists(index), $"Index {index} does not exist");
            }
        }

        [Fact]
        public void Database_ShouldRetrieveAllVehicles()
        {
            // Act
            var vehicles = _fixture.Connection.Query<Vehicle>("SELECT * FROM Vehicles").ToList();

            // Assert
            Assert.Equal(3, vehicles.Count);
            Assert.Equal("BUS001", vehicles[0].VehicleNumber);
            Assert.Equal("BUS002", vehicles[1].VehicleNumber);
            Assert.Equal("VAN001", vehicles[2].VehicleNumber);
        }

        [Fact]
        public void Database_ShouldRetrieveVehicleById()
        {
            // Act
            var vehicle = _fixture.Connection.QuerySingleOrDefault<Vehicle>(
                "SELECT * FROM Vehicles WHERE Id = @Id", new { Id = 1 });

            // Assert
            Assert.NotNull(vehicle);
            Assert.Equal("BUS001", vehicle.VehicleNumber);
            Assert.Equal("Mercedes", vehicle.Make);
        }

        [Fact]
        public void Database_ShouldInsertNewVehicle()
        {
            // Arrange
            var newVehicle = new
            {
                VehicleNumber = "BUS003",
                BusNumber = "B003",
                Make = "Volvo",
                Model = "9700",
                Year = 2022,
                SeatingCapacity = 50,
                FuelType = "Diesel",
                Status = "Active"
            };

            // Act
            var rowsAffected = _fixture.Connection.Execute(@"
                INSERT INTO Vehicles (VehicleNumber, BusNumber, Make, Model, Year, SeatingCapacity, FuelType, Status)
                VALUES (@VehicleNumber, @BusNumber, @Make, @Model, @Year, @SeatingCapacity, @FuelType, @Status)",
                newVehicle);

            var insertedVehicle = _fixture.Connection.QuerySingleOrDefault<Vehicle>(
                "SELECT * FROM Vehicles WHERE VehicleNumber = @VehicleNumber",
                new { VehicleNumber = "BUS003" });

            // Assert
            Assert.Equal(1, rowsAffected);
            Assert.NotNull(insertedVehicle);
            Assert.Equal("Volvo", insertedVehicle.Make);
            Assert.Equal(50, insertedVehicle.SeatingCapacity);
        }

        [Fact]
        public void Database_ShouldUpdateExistingVehicle()
        {
            // Arrange
            var updateVehicle = new
            {
                Id = 2,
                Status = "Active",
                SeatingCapacity = 20
            };

            // Act
            var rowsAffected = _fixture.Connection.Execute(@"
                UPDATE Vehicles 
                SET Status = @Status, SeatingCapacity = @SeatingCapacity
                WHERE Id = @Id",
                updateVehicle);

            var updatedVehicle = _fixture.Connection.QuerySingleOrDefault<Vehicle>(
                "SELECT * FROM Vehicles WHERE Id = @Id", new { Id = 2 });

            // Assert
            Assert.Equal(1, rowsAffected);
            Assert.NotNull(updatedVehicle);
            Assert.Equal("Active", updatedVehicle.Status);
            Assert.Equal(20, updatedVehicle.SeatingCapacity);
        }

        [Fact]
        public void Database_ShouldDeleteVehicle()
        {
            // Act
            var rowsAffected = _fixture.Connection.Execute(
                "DELETE FROM Vehicles WHERE Id = @Id", new { Id = 3 });

            var deletedVehicle = _fixture.Connection.QuerySingleOrDefault<Vehicle>(
                "SELECT * FROM Vehicles WHERE Id = @Id", new { Id = 3 });

            // Assert
            Assert.Equal(1, rowsAffected);
            Assert.Null(deletedVehicle);
        }

        [Fact]
        public void Database_ShouldFilterVehiclesByStatus()
        {
            // Act
            var activeVehicles = _fixture.Connection.Query<Vehicle>(
                "SELECT * FROM Vehicles WHERE Status = @Status",
                new { Status = "Active" }).ToList();

            // Assert
            Assert.True(activeVehicles.Count > 0);
            foreach (var vehicle in activeVehicles)
            {
                Assert.Equal("Active", vehicle.Status);
            }
        }

        [Fact]
        public void Database_ShouldRetrieveAllDrivers()
        {
            // Act
            var drivers = _fixture.Connection.Query<Driver>("SELECT * FROM Drivers").ToList();

            // Assert
            Assert.Equal(2, drivers.Count);
            Assert.Equal("John Smith", drivers[0].DriverName);
            Assert.Equal("Mary Johnson", drivers[1].DriverName);
        }

        [Fact]
        public void Database_ShouldCreateRoute()
        {
            // Arrange
            var newRoute = new
            {
                Date = "2025-06-06",
                RouteName = "Morning Route 1",
                AMVehicleID = 1,
                AMDriverID = 1,
                AMBeginMiles = 10000.5,
                AMEndMiles = 10050.5,
                AMRiders = 20,
                PMVehicleID = 1,
                PMDriverID = 1,
                PMBeginMiles = 10050.5,
                PMEndMiles = 10100.5,
                PMRiders = 18,
                Notes = "Test route"
            };

            // Act
            var rowsAffected = _fixture.Connection.Execute(@"
                INSERT INTO Routes (Date, RouteName, AMVehicleID, AMDriverID, AMBeginMiles, AMEndMiles, AMRiders, 
                                    PMVehicleID, PMDriverID, PMBeginMiles, PMEndMiles, PMRiders, Notes)
                VALUES (@Date, @RouteName, @AMVehicleID, @AMDriverID, @AMBeginMiles, @AMEndMiles, @AMRiders,
                        @PMVehicleID, @PMDriverID, @PMBeginMiles, @PMEndMiles, @PMRiders, @Notes)",
                newRoute);

            var insertedRoute = _fixture.Connection.QuerySingleOrDefault<Route>(
                "SELECT * FROM Routes WHERE Date = @Date AND RouteName = @RouteName",
                new { Date = "2025-06-06", RouteName = "Morning Route 1" });

            // Assert
            Assert.Equal(1, rowsAffected);
            Assert.NotNull(insertedRoute);
            Assert.Equal(1, insertedRoute.AMVehicleID);
            Assert.Equal(1, insertedRoute.AMDriverID);
            Assert.Equal(20, insertedRoute.AMRiders);
        }

        [Fact]
        public void Database_ShouldEnforceForeignKeyConstraints()
        {
            // Arrange
            var newRoute = new
            {
                Date = "2025-06-06",
                RouteName = "Invalid Route",
                AMVehicleID = 999, // Non-existent vehicle ID
                AMDriverID = 1,
                Notes = "This should fail"
            };

            // Act & Assert
            var ex = Assert.Throws<SqliteException>(() =>
            {
                _fixture.Connection.Execute(@"
                    INSERT INTO Routes (Date, RouteName, AMVehicleID, AMDriverID, Notes)
                    VALUES (@Date, @RouteName, @AMVehicleID, @AMDriverID, @Notes)",
                    newRoute);
            });

            Assert.Contains("FOREIGN KEY constraint failed", ex.Message);
        }
        [Fact]
        public void Database_ShouldInsertAndRetrieveActivity()
        {
            // Arrange
            var newActivity = new
            {
                Date = "2025-06-10",
                ActivityType = "Field Trip",
                Destination = "Science Museum",
                LeaveTime = "09:00",
                EventTime = "10:00",
                ReturnTime = "14:00",
                RequestedBy = "Mr. Johnson",
                AssignedVehicleID = 1,
                DriverID = 1,
                Notes = "Annual science trip"
            };

            // Act
            var rowsAffected = _fixture.Connection.Execute(@"
                INSERT INTO Activities (Date, ActivityType, Destination, LeaveTime, EventTime, ReturnTime, 
                                        RequestedBy, AssignedVehicleID, DriverID, Notes)
                VALUES (@Date, @ActivityType, @Destination, @LeaveTime, @EventTime, @ReturnTime,
                        @RequestedBy, @AssignedVehicleID, @DriverID, @Notes)",
                newActivity);

            var insertedActivity = _fixture.Connection.QuerySingleOrDefault<Activity>(
                "SELECT * FROM Activities WHERE Date = @Date AND Destination = @Destination",
                new { Date = "2025-06-10", Destination = "Science Museum" });

            // Assert
            Assert.Equal(1, rowsAffected);
            Assert.NotNull(insertedActivity);
            Assert.Equal("Field Trip", insertedActivity.ActivityType);
            Assert.Equal(1, insertedActivity.AssignedVehicleID);
            Assert.Equal(1, insertedActivity.DriverID);
        }

        [Fact]
        public void Database_ShouldHaveCorrectVehicleTableSchema()
        {
            // Act
            var tableInfo = _fixture.Connection.Query<dynamic>("PRAGMA table_info(Vehicles)").ToList();

            // Assert
            Assert.Equal(13, tableInfo.Count); // Check column count

            // Verify specific columns
            Assert.Contains(tableInfo, col => col.name == "Id" && col.type == "INTEGER" && col.pk == 1);
            Assert.Contains(tableInfo, col => col.name == "VehicleNumber" && col.type == "TEXT" && col.notnull == 1);
            Assert.Contains(tableInfo, col => col.name == "BusNumber" && col.type == "TEXT");
            Assert.Contains(tableInfo, col => col.name == "Make" && col.type == "TEXT");
            Assert.Contains(tableInfo, col => col.name == "Model" && col.type == "TEXT");
            Assert.Contains(tableInfo, col => col.name == "Year" && col.type == "INTEGER");
            Assert.Contains(tableInfo, col => col.name == "SeatingCapacity" && col.type == "INTEGER");
            Assert.Contains(tableInfo, col => col.name == "VINNumber" && col.type == "TEXT");
            Assert.Contains(tableInfo, col => col.name == "LicenseNumber" && col.type == "TEXT");
            Assert.Contains(tableInfo, col => col.name == "DateLastInspection" && col.type == "TEXT");
            Assert.Contains(tableInfo, col => col.name == "FuelType" && col.type == "TEXT");
            Assert.Contains(tableInfo, col => col.name == "Status" && col.type == "TEXT");
            Assert.Contains(tableInfo, col => col.name == "Notes" && col.type == "TEXT");
        }

        [Fact]
        public void Database_ShouldHaveCorrectForeignKeysInRoutesTable()
        {
            // Act
            var foreignKeys = _fixture.Connection.Query<dynamic>("PRAGMA foreign_key_list(Routes)").ToList();

            // Assert
            Assert.Equal(4, foreignKeys.Count); // Check foreign key count

            // Verify specific foreign keys
            Assert.Contains(foreignKeys, fk => fk.table == "Vehicles" && fk.from == "AMVehicleID" && fk.to == "Id");
            Assert.Contains(foreignKeys, fk => fk.table == "Drivers" && fk.from == "AMDriverID" && fk.to == "DriverID");
            Assert.Contains(foreignKeys, fk => fk.table == "Vehicles" && fk.from == "PMVehicleID" && fk.to == "Id");
            Assert.Contains(foreignKeys, fk => fk.table == "Drivers" && fk.from == "PMDriverID" && fk.to == "DriverID");
        }

        [Fact]
        public void Database_ShouldHaveCorrectForeignKeyConstraints()
        {
            // Arrange - Using the fixture's connection

            // Verify Routes table foreign keys
            var routeForeignKeys = GetForeignKeys("Routes");
            Assert.Contains(routeForeignKeys, fk => fk.Table == "Vehicles" && fk.From == "AMVehicleID" && fk.To == "Id");
            Assert.Contains(routeForeignKeys, fk => fk.Table == "Drivers" && fk.From == "AMDriverID" && fk.To == "DriverID");
            Assert.Contains(routeForeignKeys, fk => fk.Table == "Vehicles" && fk.From == "PMVehicleID" && fk.To == "Id");
            Assert.Contains(routeForeignKeys, fk => fk.Table == "Drivers" && fk.From == "PMDriverID" && fk.To == "DriverID");

            // Verify Activities table foreign keys
            var activitiesForeignKeys = GetForeignKeys("Activities");
            Assert.Contains(activitiesForeignKeys, fk => fk.Table == "Vehicles" && fk.From == "AssignedVehicleID" && fk.To == "Id");
            Assert.Contains(activitiesForeignKeys, fk => fk.Table == "Drivers" && fk.From == "DriverID" && fk.To == "DriverID");

            // Verify Fuel table foreign keys
            var fuelForeignKeys = GetForeignKeys("Fuel");
            Assert.Contains(fuelForeignKeys, fk => fk.Table == "Vehicles" && fk.From == "VehicleFueledID" && fk.To == "Id");

            // Verify Maintenance table foreign keys
            var maintenanceForeignKeys = GetForeignKeys("Maintenance");
            Assert.Contains(maintenanceForeignKeys, fk => fk.Table == "Vehicles" && fk.From == "VehicleID" && fk.To == "Id");

            // Verify ActivitySchedule table foreign keys
            var schedForeignKeys = GetForeignKeys("ActivitySchedule");
            Assert.Contains(schedForeignKeys, fk => fk.Table == "Vehicles" && fk.From == "ScheduledVehicleID" && fk.To == "Id");
            Assert.Contains(schedForeignKeys, fk => fk.Table == "Drivers" && fk.From == "ScheduledDriverID" && fk.To == "DriverID");

            // Verify TimeCard table foreign keys
            var timecardForeignKeys = GetForeignKeys("TimeCard");
            Assert.Contains(timecardForeignKeys, fk => fk.Table == "Drivers" && fk.From == "DriverID" && fk.To == "DriverID");
        }

        private List<ForeignKeyInfo> GetForeignKeys(string tableName)
        {
            var foreignKeys = new List<ForeignKeyInfo>();
            using (var cmd = _fixture.Connection.CreateCommand())
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
    }
}
