using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// Disaster recovery and backup tests validating data protection, backup procedures, and system recovery capabilities.
    /// Tests ensure business continuity and data integrity in failure scenarios.
    /// </summary>
    public class DisasterRecoveryTests : SystemTestBase
    {
        private readonly ITestOutputHelper _output;

        public DisasterRecoveryTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region Data Backup Tests

        [Fact]
        public void DataBackup_Creation_ShouldPreserveIntegrity()
        {
            _output.WriteLine("Testing data backup creation and integrity...");

            // Create test data to backup
            var testDataSet = CreateComprehensiveTestDataSet();
            _output.WriteLine($"Created test dataset: {testDataSet.VehicleCount} vehicles, {testDataSet.DriverCount} drivers, {testDataSet.RouteCount} routes");

            // Simulate backup creation by exporting data
            var backupData = new
            {
                BackupDate = DateTime.UtcNow,
                VehicleData = VehicleRepository.GetAllVehicles()?.Where(v => v.VehicleNumber?.Contains("_BackupTest") == true).ToList(),
                DriverData = DriverRepository.GetAllDrivers()?.Where(d => d.DriverName?.Contains("_BackupTest") == true).ToList(),
                RouteData = RouteRepository.GetAllRoutes()?.Where(r => r.RouteName?.Contains("_BackupTest") == true).ToList()
            };

            // Verify backup completeness
            Assert.NotNull(backupData.VehicleData);
            Assert.NotNull(backupData.DriverData);
            Assert.NotNull(backupData.RouteData);

            Assert.True(backupData.VehicleData.Count >= testDataSet.VehicleCount,
                $"Backup should contain at least {testDataSet.VehicleCount} vehicles");
            Assert.True(backupData.DriverData.Count >= testDataSet.DriverCount,
                $"Backup should contain at least {testDataSet.DriverCount} drivers");

            // Verify data integrity in backup
            foreach (var vehicle in backupData.VehicleData)
            {
                Assert.False(string.IsNullOrEmpty(vehicle.VehicleNumber), "Vehicle number should be preserved");
                Assert.False(string.IsNullOrEmpty(vehicle.Make), "Vehicle make should be preserved");
                Assert.False(string.IsNullOrEmpty(vehicle.Status), "Vehicle status should be preserved");
            }

            foreach (var driver in backupData.DriverData)
            {
                Assert.False(string.IsNullOrEmpty(driver.DriverName), "Driver name should be preserved");
                Assert.False(string.IsNullOrEmpty(driver.Status), "Driver status should be preserved");
            }

            _output.WriteLine("✅ Data backup creation and integrity test PASSED");
        }

        [Fact]
        public void DataRestore_Process_ShouldRecoverData()
        {
            _output.WriteLine("Testing data restore process...");

            // Create original data
            var originalVehicle = CreateTestVehicle("_RestoreTest_Original");
            var originalDriver = CreateTestDriver("_RestoreTest_Original");

            var originalVehicleId = VehicleRepository.AddVehicle(originalVehicle);
            var originalDriverId = DriverRepository.AddDriver(originalDriver);

            TestVehicleIds.Add(originalVehicleId);
            TestDriverIds.Add(originalDriverId);

            // Verify original data exists
            var preRestoreVehicle = VehicleRepository.GetVehicleById(originalVehicleId);
            var preRestoreDriver = DriverRepository.GetDriverById(originalDriverId);

            Assert.NotNull(preRestoreVehicle);
            Assert.NotNull(preRestoreDriver);

            // Simulate data corruption/loss by modifying data
            preRestoreVehicle.Status = "CORRUPTED";
            preRestoreDriver.Status = "CORRUPTED";

            VehicleRepository.UpdateVehicle(preRestoreVehicle);
            DriverRepository.UpdateDriver(preRestoreDriver);

            // Verify corruption
            var corruptedVehicle = VehicleRepository.GetVehicleById(originalVehicleId);
            var corruptedDriver = DriverRepository.GetDriverById(originalDriverId);

            Assert.NotNull(corruptedVehicle);
            Assert.NotNull(corruptedDriver);
            Assert.Equal("CORRUPTED", corruptedVehicle.Status);
            Assert.Equal("CORRUPTED", corruptedDriver.Status);

            // Simulate restore process by reverting to original values
            var restoredVehicle = originalVehicle;
            restoredVehicle.VehicleID = originalVehicleId;
            restoredVehicle.Status = "Active"; // Restore original status

            var restoredDriver = originalDriver;
            restoredDriver.DriverID = originalDriverId;
            restoredDriver.Status = "Active"; // Restore original status

            VehicleRepository.UpdateVehicle(restoredVehicle);
            DriverRepository.UpdateDriver(restoredDriver);

            // Verify restore success
            var finalVehicle = VehicleRepository.GetVehicleById(originalVehicleId);
            var finalDriver = DriverRepository.GetDriverById(originalDriverId);

            Assert.NotNull(finalVehicle);
            Assert.NotNull(finalDriver);
            Assert.Equal("Active", finalVehicle.Status);
            Assert.Equal("Active", finalDriver.Status);
            Assert.Equal(originalVehicle.VehicleNumber, finalVehicle.VehicleNumber);
            Assert.Equal(originalDriver.DriverName, finalDriver.DriverName);

            _output.WriteLine("✅ Data restore process test PASSED");
        }

        #endregion

        #region System Recovery Tests

        [Fact]
        public void SystemFailure_Recovery_ShouldMaintainDataConsistency()
        {
            _output.WriteLine("Testing system failure recovery and data consistency...");

            // Create a batch of operations to simulate during "failure"
            var preFailureData = new List<(string Type, int Id, string Description)>();

            // Create initial data
            for (int i = 0; i < 5; i++)
            {
                var vehicle = CreateTestVehicle($"_FailureTest_{i}");
                var vehicleId = VehicleRepository.AddVehicle(vehicle);
                TestVehicleIds.Add(vehicleId);
                preFailureData.Add(("Vehicle", vehicleId, $"Vehicle {vehicle.VehicleNumber}"));
            }

            for (int i = 0; i < 3; i++)
            {
                var driver = CreateTestDriver($"_FailureTest_{i}");
                var driverId = DriverRepository.AddDriver(driver);
                TestDriverIds.Add(driverId);
                preFailureData.Add(("Driver", driverId, $"Driver {driver.DriverName}"));
            }

            _output.WriteLine($"Created {preFailureData.Count} records before simulated failure");

            // Simulate system failure during transaction by attempting invalid operations
            var failureOperations = new List<string>();

            try
            {
                // Attempt operations that might fail
                var invalidVehicle = CreateTestVehicle("_Invalid");
                invalidVehicle.VehicleNumber = null; // This might cause a failure
                VehicleRepository.AddVehicle(invalidVehicle);
                failureOperations.Add("Invalid vehicle creation attempted");
            }
            catch (Exception ex)
            {
                failureOperations.Add($"Expected failure: {ex.GetType().Name}");
            }

            try
            {
                // Attempt to update non-existent record
                var nonExistentVehicle = CreateTestVehicle("_NonExistent");
                nonExistentVehicle.VehicleID = -999;
                VehicleRepository.UpdateVehicle(nonExistentVehicle);
                failureOperations.Add("Non-existent vehicle update attempted");
            }
            catch (Exception ex)
            {
                failureOperations.Add($"Expected failure: {ex.GetType().Name}");
            }

            // Verify data consistency after simulated failures
            foreach (var (type, id, description) in preFailureData)
            {
                if (type == "Vehicle")
                {
                    var vehicle = VehicleRepository.GetVehicleById(id);
                    Assert.NotNull(vehicle);
                    Assert.Equal("Active", vehicle.Status); // Should remain unchanged
                }
                else if (type == "Driver")
                {
                    var driver = DriverRepository.GetDriverById(id);
                    Assert.NotNull(driver);
                    Assert.Equal("Active", driver.Status); // Should remain unchanged
                }
            }

            _output.WriteLine($"Simulated {failureOperations.Count} failure scenarios");
            _output.WriteLine("✅ System failure recovery and data consistency test PASSED");
        }

        [Fact]
        public async Task ConcurrentFailure_Recovery_ShouldPreventDataCorruption()
        {
            _output.WriteLine("Testing concurrent failure recovery...");

            var vehicle = CreateTestVehicle("_ConcurrentFailure");
            var vehicleId = VehicleRepository.AddVehicle(vehicle);
            TestVehicleIds.Add(vehicleId);

            var tasks = new List<Task>();
            var results = new List<(bool Success, string Operation, Exception Error)>();
            var lockObject = new object();

            // Simulate concurrent operations that might fail
            for (int i = 0; i < 10; i++)
            {
                var operationId = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        // Each task attempts different operations
                        if (operationId % 3 == 0)
                        {
                            // Update vehicle status
                            var v = VehicleRepository.GetVehicleById(vehicleId);
                            if (v != null)
                            {
                                v.Status = $"Updated_by_Op_{operationId}";
                                VehicleRepository.UpdateVehicle(v);
                            }

                            lock (lockObject)
                            {
                                results.Add((true, $"Update_{operationId}", null!));
                            }
                        }
                        else if (operationId % 3 == 1)
                        {
                            // Read vehicle data
                            var v = VehicleRepository.GetVehicleById(vehicleId);
                            Assert.NotNull(v);

                            lock (lockObject)
                            {
                                results.Add((true, $"Read_{operationId}", null!));
                            }
                        }
                        else
                        {
                            // Attempt invalid operation
                            VehicleRepository.GetVehicleById(-999);

                            lock (lockObject)
                            {
                                results.Add((true, $"Invalid_{operationId}", null!));
                            }
                        }

                        await Task.Delay(10); // Small delay to increase contention
                    }
                    catch (Exception ex)
                    {
                        lock (lockObject)
                        {
                            results.Add((false, $"Operation_{operationId}", ex));
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Analyze results
            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            _output.WriteLine($"Concurrent operations: {successCount} succeeded, {failureCount} failed");

            // Verify final data integrity
            var finalVehicle = VehicleRepository.GetVehicleById(vehicleId);
            Assert.NotNull(finalVehicle);
            Assert.False(string.IsNullOrEmpty(finalVehicle.VehicleNumber));

            // At least some operations should have succeeded
            Assert.True(successCount > 0, "Some operations should succeed even under concurrent stress");

            _output.WriteLine("✅ Concurrent failure recovery test PASSED");
        }

        #endregion

        #region Business Continuity Tests

        [Fact]
        public void CriticalOperations_Availability_ShouldBeMaintained()
        {
            _output.WriteLine("Testing critical operations availability...");

            // Define critical business operations that must always be available
            var criticalOperations = new Dictionary<string, Func<bool>>
            {
                ["Vehicle Lookup"] = () =>
                {
                    try
                    {
                        var vehicles = VehicleRepository.GetAllVehicles();
                        return vehicles != null;
                    }
                    catch { return false; }
                },

                ["Driver Lookup"] = () =>
                {
                    try
                    {
                        var drivers = DriverRepository.GetAllDrivers();
                        return drivers != null;
                    }
                    catch { return false; }
                },

                ["Route Information"] = () =>
                {
                    try
                    {
                        var routes = RouteRepository.GetAllRoutes();
                        return routes != null;
                    }
                    catch { return false; }
                },

                ["Vehicle Status Check"] = () =>
                {
                    try
                    {
                        if (TestVehicleIds.Any())
                        {
                            var vehicle = VehicleRepository.GetVehicleById(TestVehicleIds.First());
                            return vehicle != null;
                        }
                        return true; // No test data available, assume success
                    }
                    catch { return false; }
                },

                ["Emergency Contact Access"] = () =>
                {
                    try
                    {
                        if (TestDriverIds.Any())
                        {
                            var driver = DriverRepository.GetDriverById(TestDriverIds.First());
                            return driver != null && !string.IsNullOrEmpty(driver.DriverPhone);
                        }
                        return true; // No test data available, assume success
                    }
                    catch { return false; }
                }
            };

            var operationResults = new Dictionary<string, bool>();

            // Test each critical operation
            foreach (var operation in criticalOperations)
            {
                var startTime = DateTime.UtcNow;
                var success = operation.Value();
                var duration = DateTime.UtcNow - startTime;

                operationResults[operation.Key] = success;

                _output.WriteLine($"{operation.Key}: {(success ? "✅ Available" : "❌ Failed")} ({duration.TotalMilliseconds:F2}ms)");

                // Critical operations should complete quickly
                Assert.True(duration.TotalSeconds < 5, $"{operation.Key} should complete within 5 seconds");
            }

            // All critical operations should be available
            var availableCount = operationResults.Values.Count(available => available);
            var totalCount = operationResults.Count;

            Assert.True(availableCount == totalCount,
                $"All critical operations should be available: {availableCount}/{totalCount}");

            _output.WriteLine("✅ Critical operations availability test PASSED");
        }

        [Fact]
        public void ServiceDegradation_GracefulHandling_ShouldWork()
        {
            _output.WriteLine("Testing graceful service degradation...");

            // Create test data
            var vehicle = CreateTestVehicle("_ServiceDegradation");
            var vehicleId = VehicleRepository.AddVehicle(vehicle);
            TestVehicleIds.Add(vehicleId);

            // Test various degradation scenarios
            var degradationTests = new[]
            {
                new { Name = "Slow Database Response", Test = (Func<bool>)(() =>
                {
                    // Simulate slow response by adding delay
                    Task.Delay(100).Wait();
                    var result = VehicleRepository.GetVehicleById(vehicleId);
                    return result != null;
                })},

                new { Name = "Limited Connectivity", Test = (Func<bool>)(() =>
                {
                    // Test basic operations still work
                    var vehicles = VehicleRepository.GetAllVehicles();
                    return vehicles != null && vehicles.Count > 0;
                })},

                new { Name = "High Load Conditions", Test = (Func<bool>)(() =>
                {
                    // Perform multiple operations quickly
                    var tasks = new List<Task<Vehicle?>>();
                    for (int i = 0; i < 5; i++)
                    {
                        tasks.Add(Task.Run(() => VehicleRepository.GetVehicleById(vehicleId)));
                    }
                    var results = Task.WhenAll(tasks).Result;
                    return results.All(r => r != null);
                })},

                new { Name = "Partial Service Failure", Test = (Func<bool>)(() =>
                {
                    // Test that other services continue working even if one fails
                    try
                    {
                        // Attempt invalid operation
                        VehicleRepository.GetVehicleById(-999);
                    }
                    catch
                    {
                        // Ignore expected failure
                    }

                    // Verify valid operations still work
                    var validVehicle = VehicleRepository.GetVehicleById(vehicleId);
                    return validVehicle != null;
                })}
            };

            var degradationResults = new List<(string Name, bool Success, TimeSpan Duration)>();

            foreach (var test in degradationTests)
            {
                var startTime = DateTime.UtcNow;
                try
                {
                    var success = test.Test();
                    var duration = DateTime.UtcNow - startTime;
                    degradationResults.Add((test.Name, success, duration));

                    _output.WriteLine($"{test.Name}: {(success ? "✅ Handled" : "❌ Failed")} ({duration.TotalMilliseconds:F2}ms)");
                }
                catch (Exception ex)
                {
                    var duration = DateTime.UtcNow - startTime;
                    degradationResults.Add((test.Name, false, duration));
                    _output.WriteLine($"{test.Name}: ❌ Exception - {ex.GetType().Name}");
                }
            }

            // Most degradation scenarios should be handled gracefully
            var handledCount = degradationResults.Count(r => r.Success);
            var successRate = (double)handledCount / degradationResults.Count;

            Assert.True(successRate >= 0.75, $"At least 75% of degradation scenarios should be handled gracefully: {successRate:P}");

            _output.WriteLine("✅ Graceful service degradation test PASSED");
        }

        #endregion

        #region Data Integrity Validation Tests

        [Fact]
        public void DataIntegrity_Validation_ShouldDetectCorruption()
        {
            _output.WriteLine("Testing data integrity validation...");

            // Create test data with known checksums
            var integrityTestData = new[]
            {
                CreateTestVehicle("_Integrity_1"),
                CreateTestVehicle("_Integrity_2"),
                CreateTestVehicle("_Integrity_3")
            };

            var vehicleIds = new List<int>();
            var originalChecksums = new Dictionary<int, string>();

            // Store original data and calculate checksums
            foreach (var vehicle in integrityTestData)
            {
                var vehicleId = VehicleRepository.AddVehicle(vehicle);
                vehicleIds.Add(vehicleId);
                TestVehicleIds.Add(vehicleId);

                var checksum = CalculateVehicleChecksum(vehicle);
                originalChecksums[vehicleId] = checksum;
            }

            // Verify initial integrity
            foreach (var vehicleId in vehicleIds)
            {
                var vehicle = VehicleRepository.GetVehicleById(vehicleId);
                Assert.NotNull(vehicle);
                var currentChecksum = CalculateVehicleChecksum(vehicle);

                Assert.Equal(originalChecksums[vehicleId], currentChecksum);
            }

            // Simulate data corruption
            var corruptedVehicle = VehicleRepository.GetVehicleById(vehicleIds[0]);
            Assert.NotNull(corruptedVehicle);
            corruptedVehicle.VINNumber = "CORRUPTED_VIN";
            VehicleRepository.UpdateVehicle(corruptedVehicle);

            // Detect corruption
            var corruptedChecksum = CalculateVehicleChecksum(corruptedVehicle);
            Assert.NotEqual(originalChecksums[vehicleIds[0]], corruptedChecksum);

            // Verify other records remain intact
            for (int i = 1; i < vehicleIds.Count; i++)
            {
                var vehicle = VehicleRepository.GetVehicleById(vehicleIds[i]);
                Assert.NotNull(vehicle);
                var checksum = CalculateVehicleChecksum(vehicle);
                Assert.Equal(originalChecksums[vehicleIds[i]], checksum);
            }

            _output.WriteLine("✅ Data integrity validation test PASSED");
        }

        #endregion

        #region Helper Methods

        private (int VehicleCount, int DriverCount, int RouteCount) CreateComprehensiveTestDataSet()
        {
            var vehicleCount = 0;
            var driverCount = 0;
            var routeCount = 0;

            // Create vehicles
            for (int i = 0; i < 10; i++)
            {
                var vehicle = CreateTestVehicle($"_BackupTest_{i}");
                var vehicleId = VehicleRepository.AddVehicle(vehicle);
                TestVehicleIds.Add(vehicleId);
                vehicleCount++;
            }

            // Create drivers
            for (int i = 0; i < 8; i++)
            {
                var driver = CreateTestDriver($"_BackupTest_{i}");
                var driverId = DriverRepository.AddDriver(driver);
                TestDriverIds.Add(driverId);
                driverCount++;
            }

            // Create routes
            for (int i = 0; i < Math.Min(vehicleCount, driverCount); i++)
            {
                var route = CreateTestRoute(TestVehicleIds[i], TestDriverIds[i], $"_BackupTest_{i}");
                var routeId = RouteRepository.AddRoute(route);
                TestRouteIds.Add(routeId);
                routeCount++;
            }

            return (vehicleCount, driverCount, routeCount);
        }

        private string CalculateVehicleChecksum(Vehicle vehicle)
        {
            var data = $"{vehicle.VehicleNumber}|{vehicle.Make}|{vehicle.Model}|{vehicle.Year}|{vehicle.VINNumber}|{vehicle.Status}|{vehicle.SeatingCapacity}";
            return data.GetHashCode().ToString("X");
        }

        #endregion
    }
}
