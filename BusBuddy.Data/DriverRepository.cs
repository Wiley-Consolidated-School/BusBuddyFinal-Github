using System;
using System.Collections.Generic;
using System.Data;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{    public class DriverRepository : BaseRepository, IDriverRepository
    {
        public DriverRepository() : base()
        {
        }

        public DriverRepository(string connectionString, string providerName) : base(connectionString, providerName)
        {
        }

        public List<Driver> GetAllDrivers()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var drivers = connection.Query<Driver>("SELECT * FROM Drivers").AsList();
                return drivers;
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
                        "SELECT * FROM Drivers WHERE DriverID = @DriverID",
                        new { DriverID = id });

                    if (driver == null)
                    {
                        Console.WriteLine($"WARNING: Driver with ID {id} not found in database");
                    }
                    else
                    {
                        Console.WriteLine($"Driver found: ID={driver.DriverID}, Name={driver.Name}");
                    }

                    return driver;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetDriverById({id}): {ex.Message}");
                return null;
            }
        }        public List<Driver> GetDriversByLicenseType(string licenseType)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var drivers = connection.Query<Driver>(
                    "SELECT * FROM Drivers WHERE LicenseType = @LicenseType",
                    new { LicenseType = licenseType }).AsList();
                return drivers;
            }
        }public int AddDriver(Driver driver)
        {
            if (driver == null)
                throw new ArgumentNullException(nameof(driver));

            using (var connection = CreateConnection())
            {
                connection.Open();                var sql = @"
                    INSERT INTO Drivers (
                        DriverName, DriverPhone, DriverEmail,
                        Address, City, State, Zip,
                        LicenseType, TrainingComplete, Notes,
                        Status, FirstName, LastName, CDLExpirationDate
                    )
                    VALUES (
                        @DriverName, @DriverPhone, @DriverEmail,
                        @Address, @City, @State, @Zip,
                        @LicenseType, @TrainingComplete, @Notes,
                        @Status, @FirstName, @LastName, @CDLExpirationDate
                    );
                    SELECT SCOPE_IDENTITY()";

                return connection.QuerySingle<int>(sql, driver);
            }
        }        public bool UpdateDriver(Driver driver)
        {
            if (driver == null)
                throw new ArgumentNullException(nameof(driver));

            using (var connection = CreateConnection())
            {
                connection.Open();                var sql = @"                    UPDATE Drivers
                    SET DriverName = @DriverName,
                        DriverPhone = @DriverPhone,
                        DriverEmail = @DriverEmail,
                        Address = @Address,
                        City = @City,
                        State = @State,
                        Zip = @Zip,
                        LicenseType = @LicenseType,
                        TrainingComplete = @TrainingComplete,
                        Notes = @Notes,
                        Status = @Status,
                        FirstName = @FirstName,
                        LastName = @LastName,
                        CDLExpirationDate = @CDLExpirationDate
                    WHERE DriverID = @DriverID";

                var rowsAffected = connection.Execute(sql, driver);
                return rowsAffected > 0;
            }
        }

        public bool DeleteDriver(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = "DELETE FROM Drivers WHERE DriverID = @DriverID";
                var rowsAffected = connection.Execute(sql, new { DriverID = id });
                return rowsAffected > 0;
            }
        }
    }
}
