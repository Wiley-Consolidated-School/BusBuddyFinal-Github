using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Data;
using BusBuddy.Models;
using Microsoft.Data.SqlClient;
using Xunit;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Comprehensive tests for database connectivity, CRUD operations, concurrency, error handling, and performance
    /// </summary>
    public class DatabaseConnectivityTests
    {
        public DatabaseConnectivityTests()
        {
            // Ensure database is online before running tests or app logic
            EnsureDatabaseOnline();
        }

        [Fact]
        [Trait("Category", "Database")]
        public void CanConnectToDatabase()
        {
            // Arrange & Act & Assert
            var busRepository = new BusRepository();

            // This should not throw an exception if database is properly configured
            var buses = busRepository.GetAllBuses();

            Assert.NotNull(buses);
            Assert.True(buses.Count() >= 0, "Should be able to retrieve buses collection");
        }

        [Fact]
        [Trait("Category", "Database")]
        public void CanRetrieveBusData()
        {
            // Arrange
            var busRepository = new BusRepository();

            // Act
            var buses = busRepository.GetAllBuses();

            // Assert
            Assert.NotNull(buses);

            if (buses.Any())
            {
                var firstBus = buses.First();
                Assert.NotNull(firstBus.BusNumber);
                Assert.True(firstBus.BusId > 0, "Bus should have valid ID");
            }
        }

        [Fact]
        [Trait("Category", "Database")]
        public void CanRetrieveDriverData()
        {
            // Arrange
            var driverRepository = new DriverRepository();

            // Act
            var drivers = driverRepository.GetAllDrivers();

            // Assert
            Assert.NotNull(drivers);

            if (drivers.Any())
            {
                var firstDriver = drivers.First();
                Assert.NotNull(firstDriver.DriverName);
                Assert.True(firstDriver.DriverId > 0, "Driver should have valid ID");
            }
        }

        [Fact]
        [Trait("Category", "Database")]
        public void DatabaseSchemaIsValid()
        {
            // Test that we can create database context without errors
            try
            {
                using var context = new BusBuddyContext();

                // Test basic table access - Updated to use correct DbSet names
                var busCount = context.Buses.Count(); // Now correctly references Buses DbSet
                var driverCount = context.Drivers.Count();
                var routeCount = context.Routes.Count();

                Assert.True(busCount >= 0, "Should be able to count buses");
                Assert.True(driverCount >= 0, "Should be able to count drivers");
                Assert.True(routeCount >= 0, "Should be able to count routes");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Database schema validation failed: {ex.Message}");
            }
        }

        #region CRUD Operation Tests

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "CRUD")]
        public void CanInsertNewBus()
        {
            // Arrange
            var busRepository = new BusRepository();
            var testBus = new Bus
            {
                BusNumber = $"TEST-{DateTime.Now.Ticks}",
                Year = 2023,
                Make = "Test Manufacturer",
                Model = "Test Model",
                Capacity = 72,
                VIN = $"TEST{DateTime.Now.Ticks}",
                LicenseNumber = $"TST{DateTime.Now.Ticks % 10000}",
                LastInspectionDate = DateTime.Now.AddDays(-30),
                Status = "Active"
            };

            try
            {
                // Act
                busRepository.AddBus(testBus);

                // Assert - Verify bus was inserted
                var allBuses = busRepository.GetAllBuses().ToList();
                var insertedBus = allBuses.FirstOrDefault(b => b.BusNumber == testBus.BusNumber);


                Assert.NotNull(insertedBus);
                Assert.Equal(testBus.BusNumber, insertedBus.BusNumber);
                Assert.Equal(testBus.Make, insertedBus.Make);
                Assert.Equal(testBus.Model, insertedBus.Model);
                Assert.Equal(testBus.Capacity, insertedBus.Capacity);
            }
            finally
            {
                // Cleanup - Remove test bus
                CleanupTestBus(busRepository, testBus.BusNumber);
            }
        }

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "CRUD")]
        public void CanUpdateExistingBus()
        {
            // Arrange
            var busRepository = new BusRepository();
            var testBus = CreateTestBus();


            try
            {
                // Insert test bus
                busRepository.AddBus(testBus);
                var allBuses = busRepository.GetAllBuses().ToList();
                var insertedBus = allBuses.FirstOrDefault(b => b.BusNumber == testBus.BusNumber);
                Assert.NotNull(insertedBus);

                // Act - Update the bus
                insertedBus.Make = "Updated Manufacturer";
                insertedBus.Model = "Updated Model";
                insertedBus.Capacity = 80;
                insertedBus.Status = "Maintenance";


                busRepository.UpdateBus(insertedBus);

                // Assert - Verify updates
                var updatedBus = busRepository.GetBusById(insertedBus.BusId);
                Assert.NotNull(updatedBus);
                Assert.Equal("Updated Manufacturer", updatedBus.Make);
                Assert.Equal("Updated Model", updatedBus.Model);
                Assert.Equal(80, updatedBus.Capacity);
                Assert.Equal("Maintenance", updatedBus.Status);
            }
            finally
            {
                CleanupTestBus(busRepository, testBus.BusNumber);
            }
        }

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "CRUD")]
        public void CanDeleteBus()
        {
            // Arrange
            var busRepository = new BusRepository();
            var testBus = CreateTestBus();

            // Insert test bus

            busRepository.AddBus(testBus);
            var allBuses = busRepository.GetAllBuses().ToList();
            var insertedBus = allBuses.FirstOrDefault(b => b.BusNumber == testBus.BusNumber);
            Assert.NotNull(insertedBus);

            // Act - Delete the bus
            busRepository.DeleteBus(insertedBus.BusId);

            // Assert - Verify deletion
            var deletedBus = busRepository.GetBusById(insertedBus.BusId);
            Assert.Null(deletedBus);
        }

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "CRUD")]
        public void CanRetrieveBusById()
        {
            // Arrange
            var busRepository = new BusRepository();
            var testBus = CreateTestBus();


            try
            {
                // Insert test bus
                busRepository.AddBus(testBus);
                var allBuses = busRepository.GetAllBuses().ToList();
                var insertedBus = allBuses.FirstOrDefault(b => b.BusNumber == testBus.BusNumber);
                Assert.NotNull(insertedBus);

                // Act
                var retrievedBus = busRepository.GetBusById(insertedBus.BusId);

                // Assert
                Assert.NotNull(retrievedBus);
                Assert.Equal(insertedBus.BusId, retrievedBus.BusId);
                Assert.Equal(insertedBus.BusNumber, retrievedBus.BusNumber);
                Assert.Equal(insertedBus.Make, retrievedBus.Make);
                Assert.Equal(insertedBus.Model, retrievedBus.Model);
            }
            finally
            {
                CleanupTestBus(busRepository, testBus.BusNumber);
            }
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "ErrorHandling")]
        public void HandlesInvalidBusIdGracefully()
        {
            // Arrange
            var busRepository = new BusRepository();
            const int invalidBusId = -9999;

            // Act & Assert
            var result = busRepository.GetBusById(invalidBusId);
            Assert.Null(result);
        }

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "ErrorHandling")]
        public void HandlesNullBusInsertGracefully()
        {
            // Arrange
            var busRepository = new BusRepository();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => busRepository.AddBus(null));
        }

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "ErrorHandling")]
        public void HandlesDuplicateBusNumberInsert()
        {
            // Arrange
            var busRepository = new BusRepository();
            var testBus1 = CreateTestBus();
            var testBus2 = CreateTestBus();
            testBus2.BusNumber = testBus1.BusNumber; // Same bus number

            try
            {
                // Insert first bus
                busRepository.AddBus(testBus1);

                // Act & Assert - Try to insert duplicate
                Assert.Throws<SqlException>(() => busRepository.AddBus(testBus2));
            }
            finally
            {
                CleanupTestBus(busRepository, testBus1.BusNumber);
            }
        }

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "ErrorHandling")]
        public void HandlesUpdateOfNonexistentBus()
        {
            // Arrange
            var busRepository = new BusRepository();
            var nonexistentBus = new Bus
            {
                BusId = -9999,
                BusNumber = "NONEXISTENT",
                Make = "Test",
                Model = "Test"
            };

            // Act - This should not throw, but also should not affect any rows
            busRepository.UpdateBus(nonexistentBus);

            // Assert - Verify bus still doesn't exist
            var result = busRepository.GetBusById(-9999);
            Assert.Null(result);
        }

        #endregion

        #region Concurrency Tests

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "Concurrency")]
        public async Task HandlesConcurrentInserts()
        {
            // Arrange
            const int concurrentOperations = 5;
            var tasks = new List<Task>();
            var testBusNumbers = new List<string>();

            try
            {
                // Act - Perform concurrent inserts
                for (int i = 0; i < concurrentOperations; i++)
                {
                    var busNumber = $"CONCURRENT-{DateTime.Now.Ticks}-{i}";
                    testBusNumbers.Add(busNumber);


                    tasks.Add(Task.Run(() =>
                    {
                        var busRepository = new BusRepository();
                        var testBus = CreateTestBus();
                        testBus.BusNumber = busNumber;
                        busRepository.AddBus(testBus);
                    }));
                }

                await Task.WhenAll(tasks);

                // Assert - Verify all buses were inserted
                var busRepository = new BusRepository();
                var allBuses = busRepository.GetAllBuses().ToList();


                foreach (var busNumber in testBusNumbers)
                {
                    var insertedBus = allBuses.FirstOrDefault(b => b.BusNumber == busNumber);
                    Assert.NotNull(insertedBus);
                }
            }
            finally
            {
                // Cleanup
                var busRepository = new BusRepository();
                foreach (var busNumber in testBusNumbers)
                {
                    CleanupTestBus(busRepository, busNumber);
                }
            }
        }

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "Concurrency")]
        public async Task HandlesConcurrentReads()
        {
            // Arrange
            const int concurrentReads = 10;
            var tasks = new List<Task<IEnumerable<Bus>>>();

            // Act - Perform concurrent reads
            for (int i = 0; i < concurrentReads; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var busRepository = new BusRepository();
                    return busRepository.GetAllBuses();
                }));
            }

            var results = await Task.WhenAll(tasks);

            // Assert - All reads should succeed and return consistent results
            Assert.All(results, buses => Assert.NotNull(buses));

            // All results should have the same count (assuming no writes during test)

            var firstCount = results[0].Count();
            Assert.All(results, buses => Assert.Equal(firstCount, buses.Count()));
        }

        #endregion

        #region Performance Tests

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "Performance")]
        public void DatabaseQueryPerformanceIsAcceptable()
        {
            // Arrange
            var busRepository = new BusRepository();
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var buses = busRepository.GetAllBuses().ToList();
            stopwatch.Stop();

            // Assert - Query should complete within reasonable time (5 seconds)
            Assert.True(stopwatch.ElapsedMilliseconds < 5000,

                $"Query took {stopwatch.ElapsedMilliseconds}ms, which exceeds 5000ms threshold");
            Assert.NotNull(buses);
        }

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "Performance")]
        public void CanHandleLargeDatasetRetrieval()
        {
            // Arrange
            var busRepository = new BusRepository();
            var testBuses = new List<string>();

            try
            {
                // Insert multiple test buses for performance testing
                for (int i = 0; i < 10; i++)
                {
                    var testBus = CreateTestBus();
                    testBus.BusNumber = $"PERF-TEST-{DateTime.Now.Ticks}-{i}";
                    testBuses.Add(testBus.BusNumber);
                    busRepository.AddBus(testBus);
                }

                var stopwatch = new Stopwatch();

                // Act
                stopwatch.Start();
                var allBuses = busRepository.GetAllBuses().ToList();
                stopwatch.Stop();

                // Assert
                Assert.True(allBuses.Count >= 10, "Should retrieve at least the test buses");
                Assert.True(stopwatch.ElapsedMilliseconds < 3000,

                    $"Large dataset query took {stopwatch.ElapsedMilliseconds}ms, which exceeds 3000ms threshold");
            }
            finally
            {
                // Cleanup
                foreach (var busNumber in testBuses)
                {
                    CleanupTestBus(busRepository, busNumber);
                }
            }
        }

        #endregion

        #region Transaction Tests

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "Transaction")]
        public void CanPerformTransactionalOperations()
        {
            // This test demonstrates transaction-like behavior using multiple operations
            var busRepository = new BusRepository();
            var testBus = CreateTestBus();


            try
            {
                // Act - Perform multiple operations that should all succeed or all fail
                busRepository.AddBus(testBus);


                var allBuses = busRepository.GetAllBuses().ToList();
                var insertedBus = allBuses.FirstOrDefault(b => b.BusNumber == testBus.BusNumber);
                Assert.NotNull(insertedBus);

                // Update the bus
                insertedBus.Status = "In Service";
                busRepository.UpdateBus(insertedBus);

                // Verify update
                var updatedBus = busRepository.GetBusById(insertedBus.BusId);
                Assert.Equal("In Service", updatedBus.Status);

                // Clean up as part of "transaction"
                busRepository.DeleteBus(insertedBus.BusId);

                // Verify deletion

                var deletedBus = busRepository.GetBusById(insertedBus.BusId);
                Assert.Null(deletedBus);
            }
            catch
            {
                // In a real transaction, we would rollback here
                CleanupTestBus(busRepository, testBus.BusNumber);
                throw;
            }
        }

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "Transaction")]
        public void CanRollbackFailedTransaction()
        {
            // This test demonstrates handling of failed operations
            var busRepository = new BusRepository();
            var testBus = CreateTestBus();


            try
            {
                // Insert test bus
                busRepository.AddBus(testBus);
                var allBuses = busRepository.GetAllBuses().ToList();
                var insertedBus = allBuses.FirstOrDefault(b => b.BusNumber == testBus.BusNumber);
                Assert.NotNull(insertedBus);

                // Simulate a failed operation by trying to insert duplicate
                var duplicateBus = CreateTestBus();
                duplicateBus.BusNumber = testBus.BusNumber;

                // This should fail
                Assert.Throws<SqlException>(() => busRepository.AddBus(duplicateBus));

                // Verify original bus is still there (transaction didn't affect existing data)
                var stillExistsBus = busRepository.GetBusById(insertedBus.BusId);
                Assert.NotNull(stillExistsBus);
                Assert.Equal(testBus.BusNumber, stillExistsBus.BusNumber);
            }
            finally
            {
                CleanupTestBus(busRepository, testBus.BusNumber);
            }
        }

        #endregion

        #region Data Integrity Tests

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "DataIntegrity")]
        public void MaintainsDataIntegrityWithSpecialCharacters()
        {
            // Arrange
            var busRepository = new BusRepository();
            var testBus = new Bus
            {
                BusNumber = $"SPECIAL-{DateTime.Now.Ticks}",
                Year = 2023,
                Make = "Test's \"Quoted\" Manufacturer & Co.",
                Model = "Model with Special Chars: @#$%^&*()",
                Capacity = 72,
                VIN = $"SP3C1AL{DateTime.Now.Ticks}",
                LicenseNumber = $"SP-{DateTime.Now.Ticks % 10000}",
                LastInspectionDate = DateTime.Now.AddDays(-15),
                Status = "Active"
            };

            try
            {
                // Act
                busRepository.AddBus(testBus);

                // Assert
                var allBuses = busRepository.GetAllBuses().ToList();
                var retrievedBus = allBuses.FirstOrDefault(b => b.BusNumber == testBus.BusNumber);


                Assert.NotNull(retrievedBus);
                Assert.Equal(testBus.Make, retrievedBus.Make);
                Assert.Equal(testBus.Model, retrievedBus.Model);
            }
            finally
            {
                CleanupTestBus(busRepository, testBus.BusNumber);
            }
        }

        [Fact]
        [Trait("Category", "Database")]
        [Trait("Category", "DataIntegrity")]
        public void HandlesDateTimeFieldsCorrectly()
        {
            // Arrange
            var busRepository = new BusRepository();
            var testBus = CreateTestBus();
            var specificDate = new DateTime(2023, 6, 15, 10, 30, 45);
            testBus.LastInspectionDate = specificDate;

            try
            {
                // Act
                busRepository.AddBus(testBus);

                // Assert
                var allBuses = busRepository.GetAllBuses().ToList();
                var retrievedBus = allBuses.FirstOrDefault(b => b.BusNumber == testBus.BusNumber);


                Assert.NotNull(retrievedBus);
                Assert.NotNull(retrievedBus.LastInspectionDate);

                // Check date components (ignoring milliseconds for SQL Server compatibility)

                Assert.Equal(specificDate.Date, retrievedBus.LastInspectionDate.Value.Date);
                Assert.Equal(specificDate.Hour, retrievedBus.LastInspectionDate.Value.Hour);
                Assert.Equal(specificDate.Minute, retrievedBus.LastInspectionDate.Value.Minute);
                Assert.Equal(specificDate.Second, retrievedBus.LastInspectionDate.Value.Second);
            }
            finally
            {
                CleanupTestBus(busRepository, testBus.BusNumber);
            }
        }

        #endregion

        #region Helper Methods

        private Bus CreateTestBus()
        {
            var timestamp = DateTime.Now.Ticks;
            return new Bus
            {
                BusNumber = $"TEST-{timestamp}",
                Year = 2022,
                Make = "Test Manufacturer",
                Model = "Test Model",
                Capacity = 72,
                VIN = $"TEST{timestamp}",
                LicenseNumber = $"TST{timestamp % 10000}",
                LastInspectionDate = DateTime.Now.AddDays(-30),
                Status = "Active"
            };
        }

        private void CleanupTestBus(BusRepository repository, string busNumber)
        {
            try
            {
                var allBuses = repository.GetAllBuses().ToList();
                var testBus = allBuses.FirstOrDefault(b => b.BusNumber == busNumber);
                if (testBus != null)
                {
                    repository.DeleteBus(testBus.BusId);
                }
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }

        private void EnsureDatabaseOnline()
        {
            // This method checks if the BusBuddy database is online and brings it online if needed
            var connectionString = "Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;";
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'BusBuddy' AND state_desc = 'OFFLINE') " +
                "ALTER DATABASE [BusBuddy] SET ONLINE;", connection);
            command.ExecuteNonQuery();
        }

        #endregion
    }
}
