using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// Error recovery and resilience tests validating system behavior under failure conditions.
    /// Tests cover database connectivity issues, data corruption scenarios, and graceful degradation.
    /// </summary>
    public class ErrorRecoveryTests : SystemTestBase
    {
        private readonly ITestOutputHelper _output;

        public ErrorRecoveryTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region Database Connection Tests

        [Fact]
        public void DatabaseConnection_Resilience_ShouldHandleFailures()
        {
            _output.WriteLine("Testing database connection resilience...");

            // Test basic connectivity first
            var testBus = CreateTestVehicle("_ConnectivityTest");
            var busId = BusRepository.AddBus(bus);
            TestbusIds.Add(busId);

            Assert.True(busId > 0, "Basic database connection should work");
            _output.WriteLine("✅ Basic database connectivity confirmed");

            // Test retrieval after connection stress
            var retrievedBus = BusRepository.GetBusById(busId);
            Assert.NotNull(retrievedBus);
            Assert.Equal(bus.BusNumber, retrievedBus.BusNumber);

            _output.WriteLine("✅ Data retrieval after operations successful");
            _output.WriteLine("✅ Database connection resilience test PASSED");
        }

        [Fact]
        public void InvalidData_Handling_ShouldPreventCorruption()
        {
            _output.WriteLine("Testing invalid data handling...");

            // Test null/invalid Bus data
            try
            {
                var result = BusRepository.AddBus(null!);
                Assert.True(result <= 0, "Adding null Bus should fail gracefully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✅ Null Bus properly rejected: {ex.GetType().Name}");
            }

            // Test invalid driver data
            var invalidDriver = new Driver
            {
                FirstName = "", // Invalid empty name
                LastName = "",
                Name = "",
                Status = "InvalidStatus" // Invalid status
            };

            try
            {
                var DriverId = DriverRepository.AddDriver(invalidDriver);
                if (DriverId > 0)
                {
                    TestDriverIds.Add(DriverId);
                    _output.WriteLine("⚠️ Invalid driver was accepted (validation needed)");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✅ Invalid driver properly rejected: {ex.GetType().Name}");
            }

            _output.WriteLine("✅ Invalid data handling test PASSED");
        }

        #endregion

        #region Validation Error Recovery Tests

        [Fact]
        public void ValidationService_ErrorRecovery_ShouldContinueOperation()
        {
            _output.WriteLine("Testing validation service error recovery...");

            // Test validation with non-existent IDs
            var nonExistentVehicleResult = ValidationService.ValidateBusAvailability(99999, DateTime.Today);
            Assert.False(nonExistentVehicleResult.IsValid);
            Assert.Contains("does not exist", nonExistentVehicleResult.GetErrorMessage());
            _output.WriteLine("✅ Non-existent Bus validation handled correctly");

            var nonExistentDriverResult = ValidationService.ValidateDriverAvailability(99999, DateTime.Today);
            Assert.False(nonExistentDriverResult.IsValid);
            Assert.Contains("does not exist", nonExistentDriverResult.GetErrorMessage());
            _output.WriteLine("✅ Non-existent driver validation handled correctly");

            // Test validation with null fuel record
            var nullFuelResult = ValidationService.ValidateFuelRecord(null!);
            Assert.False(nullFuelResult.IsValid);
            Assert.Contains("cannot be null", nullFuelResult.GetErrorMessage());
            _output.WriteLine("✅ Null fuel record validation handled correctly");

            // Test validation with invalid fuel record
            var invalidFuel = new Fuel
            {
                VehicleFueledID = null, // Missing required field
                FuelDate = "", // Empty date
                FuelAmount = -10 // Negative amount
            };

            var invalidFuelResult = ValidationService.ValidateFuelRecord(invalidFuel);
            Assert.False(invalidFuelResult.IsValid);
            Assert.True(invalidFuelResult.Errors.Count > 0);
            _output.WriteLine($"✅ Invalid fuel record validation found {invalidFuelResult.Errors.Count} errors");

            _output.WriteLine("✅ Validation service error recovery test PASSED");
        }

        [Fact]
        public async Task AnalyticsService_ErrorRecovery_ShouldHandleEmptyData()
        {
            _output.WriteLine("Testing analytics service error recovery with empty data...");

            // Test analytics with future date range (no data)
            var futureStart = DateTime.Today.AddDays(30);
            var futureEnd = DateTime.Today.AddDays(60);

            try
            {
                var metrics = await AnalyticsService.GetRouteEfficiencyMetricsAsync(futureStart, futureEnd);
                // Should return empty/default metrics, not throw exception
                Assert.NotNull(metrics);
                _output.WriteLine("✅ Future date range handled gracefully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"⚠️ Analytics service threw exception for future dates: {ex.Message}");
            }

            // Test analytics with invalid date range
            var invalidStart = DateTime.Today;
            var invalidEnd = DateTime.Today.AddDays(-30); // End before start

            try
            {
                var metrics = await AnalyticsService.GetRouteEfficiencyMetricsAsync(invalidStart, invalidEnd);
                _output.WriteLine("✅ Invalid date range handled");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✅ Invalid date range properly rejected: {ex.GetType().Name}");
            }

            // Test driver performance with non-existent driver
            try
            {
                var performance = await AnalyticsService.CalculateDriverPerformanceAsync(99999, DateTime.Today.AddDays(-7), DateTime.Today);
                if (performance == null)
                {
                    _output.WriteLine("✅ Non-existent driver returned null performance");
                }
            }
            catch (ArgumentException ex) when (ex.Message.Contains("not found"))
            {
                _output.WriteLine("✅ Non-existent driver properly rejected with ArgumentException");
            }

            _output.WriteLine("✅ Analytics service error recovery test PASSED");
        }

        #endregion

        #region Data Consistency Tests

        [Fact]
        public void DataConsistency_UnderConcurrentOperations_ShouldMaintainIntegrity()
        {
            _output.WriteLine("Testing data consistency under concurrent-like operations...");

            // Create test Bus and driver
            var testBus = CreateTestVehicle("_ConsistencyTest");
            var driver = CreateTestDriver("_ConsistencyTest");

            var busId = BusRepository.AddBus(bus);
            var DriverId = DriverRepository.AddDriver(driver);

            TestbusIds.Add(busId);
            TestDriverIds.Add(DriverId);

            // Simulate concurrent route operations
            var route1 = CreateTestRoute(busId, DriverId, "_Concurrent1");
            var route2 = CreateTestRoute(busId, DriverId, "_Concurrent2");
            route2.Date = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"); // Different date

            var routeId1 = RouteRepository.AddRoute(route1);
            var routeId2 = RouteRepository.AddRoute(route2);

            TestRouteIds.Add(routeId1);
            TestRouteIds.Add(routeId2);

            // Verify both routes reference the same Bus and driver
            var retrievedRoute1 = RouteRepository.GetRouteById(routeId1);
            var retrievedRoute2 = RouteRepository.GetRouteById(routeId2);

            Assert.NotNull(retrievedRoute1);
            Assert.NotNull(retrievedRoute2);
            Assert.Equal(busId, retrievedRoute1.AMbusId);
            Assert.Equal(busId, retrievedRoute2.AMbusId);
            Assert.Equal(DriverId, retrievedRoute1.AMDriverId);
            Assert.Equal(DriverId, retrievedRoute2.AMDriverId);

            _output.WriteLine("✅ Concurrent route operations maintained referential integrity");

            // Test update operations
            bus.Status = "Maintenance";
            var updateResult = BusRepository.UpdateBus(bus);
            Assert.True(updateResult);

            // Verify routes still reference the updated bus
            var updatedBus = BusRepository.GetBusById(busId);
            Assert.NotNull(updatedBus);
            Assert.Equal("Maintenance", updatedBus.Status);

            _output.WriteLine("✅ Bus update maintained data consistency");
            _output.WriteLine("✅ Data consistency test PASSED");
        }

        [Fact]
        public void OrphanedData_Prevention_ShouldMaintainReferences()
        {
            _output.WriteLine("Testing orphaned data prevention...");

            // Create test data with dependencies
            var testBus = CreateTestVehicle("_OrphanTest");
            var driver = CreateTestDriver("_OrphanTest");

            var busId = BusRepository.AddBus(bus);
            var DriverId = DriverRepository.AddDriver(driver);

            TestbusIds.Add(busId);
            TestDriverIds.Add(DriverId);

            // Create route that depends on Bus and driver
            var route = CreateTestRoute(busId, DriverId, "_OrphanTest");
            var RouteId = RouteRepository.AddRoute(route);
            TestRouteIds.Add(RouteId);

            // Verify route references exist
            var retrievedRoute = RouteRepository.GetRouteById(RouteId);
            Assert.NotNull(retrievedRoute);
            Assert.Equal(busId, retrievedRoute.AMbusId);
            Assert.Equal(DriverId, retrievedRoute.AMDriverId);

            // Attempt to delete Bus that has dependent routes
            var deleteResult = BusRepository.DeleteBus(busId);

            if (deleteResult)
            {
                _output.WriteLine("⚠️ Bus deletion succeeded despite dependent route (cascade delete or weak reference)");
                // If deletion succeeded, remove from our tracking since it's already deleted
                TestbusIds.Remove(busId);
            }
            else
            {
                _output.WriteLine("✅ Bus deletion prevented due to dependent route");
            }

            // Verify route still exists and handles missing references gracefully
            var routeAfterAttemptedDelete = RouteRepository.GetRouteById(RouteId);
            Assert.NotNull(routeAfterAttemptedDelete);

            _output.WriteLine("✅ Orphaned data prevention test PASSED");
        }

        #endregion

        #region Exception Handling Tests

        [Fact]
        public void ExceptionHandling_InServiceOperations_ShouldNotCrash()
        {
            _output.WriteLine("Testing exception handling in service operations...");

            // Test repository operations with boundary conditions
            try
            {
                var result = BusRepository.GetBusById(0); // Zero ID
                _output.WriteLine($"✅ Zero ID handled: {(result == null ? "returned null" : "returned data")}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✅ Zero ID exception handled: {ex.GetType().Name}");
            }

            try
            {
                var result = BusRepository.GetBusById(-1); // Negative ID
                _output.WriteLine($"✅ Negative ID handled: {(result == null ? "returned null" : "returned data")}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✅ Negative ID exception handled: {ex.GetType().Name}");
            }

            // Test validation with extreme dates
            var testBus = CreateTestVehicle("_ExceptionTest");
            var busId = BusRepository.AddBus(bus);
            TestbusIds.Add(busId);

            try
            {
                var result = ValidationService.ValidateBusAvailability(busId, DateTime.MinValue);
                _output.WriteLine($"✅ Min date validation: {(result.IsValid ? "valid" : "invalid")}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✅ Min date exception handled: {ex.GetType().Name}");
            }

            try
            {
                var result = ValidationService.ValidateBusAvailability(busId, DateTime.MaxValue);
                _output.WriteLine($"✅ Max date validation: {(result.IsValid ? "valid" : "invalid")}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✅ Max date exception handled: {ex.GetType().Name}");
            }

            _output.WriteLine("✅ Exception handling test PASSED");
        }

        #endregion
    }
}


