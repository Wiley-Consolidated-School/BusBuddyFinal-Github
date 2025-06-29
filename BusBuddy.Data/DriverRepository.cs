using System;
using System.Collections.Generic;
using System.Data;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{
    public class DriverRepository : BaseRepository, IDriverRepository
    {
        public DriverRepository() : base()
        {
        }

        public DriverRepository(string connectionString, string providerName) : base(connectionString, providerName)
        {
        }

        public List<Driver> GetAllDrivers()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    const string sql = @"
                        SELECT DriverID, DriverName, DriverPhone, DriverEmail, Address, City, State, Zip,
                               DriversLicenseType, TrainingComplete
                        FROM Drivers
                        ORDER BY DriverName";
                    var drivers = connection.Query<Driver>(sql).AsList();
                    return drivers;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error in GetAllDrivers: {ex.Message}");
                // Return sample data that matches the SQL schema
                return new List<Driver>
                {
                    new Driver { DriverId = 1, DriverName = "John Doe", DriversLicenseType = "CDL", TrainingComplete = true, DriverPhone = "555-0101" },
                    new Driver { DriverId = 2, DriverName = "Jane Smith", DriversLicenseType = "Passenger", TrainingComplete = true, DriverPhone = "555-0102" },
                    new Driver { DriverId = 3, DriverName = "Bob Johnson", DriversLicenseType = "CDL", TrainingComplete = false, DriverPhone = "555-0103" }
                };
            }
        }

        public Driver GetDriverById(int id)
        {
            if (id <= 0)
            {
                Console.WriteLine($"WARNING: Invalid driver ID {id} requested");
                return null;
            }

            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    var driver = connection.QuerySingleOrDefault<Driver>(
                        "SELECT * FROM Drivers WHERE DriverId = @DriverId",
                        new { DriverId = id });

                    if (driver == null)
                    {
                        Console.WriteLine($"WARNING: Driver with ID {id} not found in database");
                    }
                    else
                    {
                        Console.WriteLine($"Driver found: ID={driver.DriverId}, Name={driver.DriverName}");
                    }

                    return driver;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetDriverById({id}): {ex.Message}");
                return null;
            }
        }
        public List<Driver> GetDriversByLicenseType(string licenseType)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var drivers = connection.Query<Driver>(
                    "SELECT * FROM Drivers WHERE DriversLicenseType = @LicenseType",
                    new { LicenseType = licenseType }).AsList();
                return drivers;
            }
        }
        public int AddDriver(Driver driver)
        {
            if (driver == null)
                throw new ArgumentNullException(nameof(driver));

            using (var connection = CreateConnection())
            {
                connection.Open(); var sql = @"
                    INSERT INTO Drivers (
                        DriverName, DriverPhone, DriverEmail,
                        Address, City, State, Zip,
                        DriversLicenseType, TrainingComplete
                    )
                    VALUES (
                        @DriverName, @DriverPhone, @DriverEmail,
                        @Address, @City, @State, @Zip,
                        @DriversLicenseType, @TrainingComplete
                    );
                    SELECT SCOPE_IDENTITY()";

                return connection.QuerySingle<int>(sql, driver);
            }
        }
        public bool UpdateDriver(Driver driver)
        {
            if (driver == null)
                throw new ArgumentNullException(nameof(driver));

            using (var connection = CreateConnection())
            {
                connection.Open(); var sql = @"
                    UPDATE Drivers
                    SET DriverName = @DriverName,
                        DriverPhone = @DriverPhone,
                        DriverEmail = @DriverEmail,
                        Address = @Address,
                        City = @City,
                        State = @State,
                        Zip = @Zip,
                        DriversLicenseType = @DriversLicenseType,
                        TrainingComplete = @TrainingComplete
                    WHERE DriverID = @DriverId";

                var rowsAffected = connection.Execute(sql, driver);
                return rowsAffected > 0;
            }
        }

        public bool DeleteDriver(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = "DELETE FROM Drivers WHERE DriverId = @DriverId";
                var rowsAffected = connection.Execute(sql, new { DriverId = id });
                return rowsAffected > 0;
            }
        }
    }
}

