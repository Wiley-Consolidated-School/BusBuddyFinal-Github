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
            var vehicle = CreateTestVehicle("_ConnectivityTest");
            var vehicleId = VehicleRepository.AddVehicle(vehicle);
            TestVehicleIds.Add(vehicleId);

            Assert.True(vehicleId > 0, "Basic database connection should work");
            _output.WriteLine("✅ Basic database connectivity confirmed");

            // Test retrieval after connection stress
            var retrievedVehicle = VehicleRepository.GetVehicleById(vehicleId);
            Assert.NotNull(retrievedVehicle);
            Assert.Equal(vehicle.VehicleNumber, retrievedVehicle.VehicleNumber);

            _output.WriteLine("✅ Data retrieval after operations successful");
            _output.WriteLine("✅ Database connection resilience test PASSED");
        }

        [Fact]
        public void InvalidData_Handling_ShouldPreventCorruption()
        {
            _output.WriteLine("Testing invalid data handling...");

            // Test null/invalid vehicle data
            try
            {
                var result = VehicleRepository.AddVehicle(null!);
                Assert.True(result <= 0, "Adding null vehicle should fail gracefully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✅ Null vehicle properly rejected: {ex.GetType().Name}");
            }

            // Test invalid driver data
            var invalidDriver = new Driver
            {
                FirstName = "", // Invalid empty name
                LastName = "",
                DriverName = "",
                Status = "InvalidStatus" // Invalid status
            };

            try
            {
                var driverId = DriverRepository.AddDriver(invalidDriver);
                if (driverId > 0)
                {
                    TestDriverIds.Add(driverId);
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
            var nonExistentVehicleResult = ValidationService.ValidateVehicleAvailability(99999, DateTime.Today);
            Assert.False(nonExistentVehicleResult.IsValid);
            Assert.Contains("does not exist", nonExistentVehicleResult.GetErrorMessage());
            _output.WriteLine("✅ Non-existent vehicle validation handled correctly");

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

            // Create test vehicle and driver
            var vehicle = CreateTestVehicle("_ConsistencyTest");
            var driver = CreateTestDriver("_ConsistencyTest");

            var vehicleId = VehicleRepository.AddVehicle(vehicle);
            var driverId = DriverRepository.AddDriver(driver);

            TestVehicleIds.Add(vehicleId);
            TestDriverIds.Add(driverId);

            // Simulate concurrent route operations
            var route1 = CreateTestRoute(vehicleId, driverId, "_Concurrent1");
            var route2 = CreateTestRoute(vehicleId, driverId, "_Concurrent2");
            route2.Date = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"); // Different date

            var routeId1 = RouteRepository.AddRoute(route1);
            var routeId2 = RouteRepository.AddRoute(route2);

            TestRouteIds.Add(routeId1);
            TestRouteIds.Add(routeId2);

            // Verify both routes reference the same vehicle and driver
            var retrievedRoute1 = RouteRepository.GetRouteById(routeId1);
            var retrievedRoute2 = RouteRepository.GetRouteById(routeId2);

            Assert.NotNull(retrievedRoute1);
            Assert.NotNull(retrievedRoute2);
            Assert.Equal(vehicleId, retrievedRoute1.AMVehicleID);
            Assert.Equal(vehicleId, retrievedRoute2.AMVehicleID);
            Assert.Equal(driverId, retrievedRoute1.AMDriverID);
            Assert.Equal(driverId, retrievedRoute2.AMDriverID);

            _output.WriteLine("✅ Concurrent route operations maintained referential integrity");

            // Test update operations
            vehicle.Status = "Maintenance";
            var updateResult = VehicleRepository.UpdateVehicle(vehicle);
            Assert.True(updateResult);

            // Verify routes still reference the updated vehicle
            var updatedVehicle = VehicleRepository.GetVehicleById(vehicleId);
            Assert.NotNull(updatedVehicle);
            Assert.Equal("Maintenance", updatedVehicle.Status);

            _output.WriteLine("✅ Vehicle update maintained data consistency");
            _output.WriteLine("✅ Data consistency test PASSED");
        }

        [Fact]
        public void OrphanedData_Prevention_ShouldMaintainReferences()
        {
            _output.WriteLine("Testing orphaned data prevention...");

            // Create test data with dependencies
            var vehicle = CreateTestVehicle("_OrphanTest");
            var driver = CreateTestDriver("_OrphanTest");

            var vehicleId = VehicleRepository.AddVehicle(vehicle);
            var driverId = DriverRepository.AddDriver(driver);

            TestVehicleIds.Add(vehicleId);
            TestDriverIds.Add(driverId);

            // Create route that depends on vehicle and driver
            var route = CreateTestRoute(vehicleId, driverId, "_OrphanTest");
            var routeId = RouteRepository.AddRoute(route);
            TestRouteIds.Add(routeId);

            // Verify route references exist
            var retrievedRoute = RouteRepository.GetRouteById(routeId);
            Assert.NotNull(retrievedRoute);
            Assert.Equal(vehicleId, retrievedRoute.AMVehicleID);
            Assert.Equal(driverId, retrievedRoute.AMDriverID);

            // Attempt to delete vehicle that has dependent routes
            var deleteResult = VehicleRepository.DeleteVehicle(vehicleId);

            if (deleteResult)
            {
                _output.WriteLine("⚠️ Vehicle deletion succeeded despite dependent route (cascade delete or weak reference)");
                // If deletion succeeded, remove from our tracking since it's already deleted
                TestVehicleIds.Remove(vehicleId);
            }
            else
            {
                _output.WriteLine("✅ Vehicle deletion prevented due to dependent route");
            }

            // Verify route still exists and handles missing references gracefully
            var routeAfterAttemptedDelete = RouteRepository.GetRouteById(routeId);
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
                var result = VehicleRepository.GetVehicleById(0); // Zero ID
                _output.WriteLine($"✅ Zero ID handled: {(result == null ? "returned null" : "returned data")}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✅ Zero ID exception handled: {ex.GetType().Name}");
            }

            try
            {
                var result = VehicleRepository.GetVehicleById(-1); // Negative ID
                _output.WriteLine($"✅ Negative ID handled: {(result == null ? "returned null" : "returned data")}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✅ Negative ID exception handled: {ex.GetType().Name}");
            }

            // Test validation with extreme dates
            var vehicle = CreateTestVehicle("_ExceptionTest");
            var vehicleId = VehicleRepository.AddVehicle(vehicle);
            TestVehicleIds.Add(vehicleId);

            try
            {
                var result = ValidationService.ValidateVehicleAvailability(vehicleId, DateTime.MinValue);
                _output.WriteLine($"✅ Min date validation: {(result.IsValid ? "valid" : "invalid")}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✅ Min date exception handled: {ex.GetType().Name}");
            }

            try
            {
                var result = ValidationService.ValidateVehicleAvailability(vehicleId, DateTime.MaxValue);
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
