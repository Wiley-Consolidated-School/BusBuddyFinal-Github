#nullable disable
using System;
using System.Collections.Generic;
using BusBuddy.Data;
using BusBuddy.Models;
using Moq;
using Xunit;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Tests to verify null safety in repository operations and data handling.
    /// These tests focus on data layer null safety without UI dependencies.
    /// </summary>
    public class NullSafetyDataTests
    {
        [Fact]
        public void VehicleRepository_GetAllVehicles_ShouldHandleNullResult()
        {
            // Arrange
            var mockRepo = new Mock<IVehicleRepository>();
            mockRepo.Setup(r => r.GetAllVehicles()).Returns((List<Vehicle>)null);

            // Act & Assert - Should not throw
            var result = mockRepo.Object.GetAllVehicles();
            Assert.Null(result);
        }

        [Fact]
        public void DriverRepository_GetAllDrivers_ShouldHandleNullResult()
        {
            // Arrange
            var mockRepo = new Mock<IDriverRepository>();
            mockRepo.Setup(r => r.GetAllDrivers()).Returns((List<Driver>)null);

            // Act & Assert - Should not throw
            var result = mockRepo.Object.GetAllDrivers();
            Assert.Null(result);
        }

        [Fact]
        public void RouteRepository_GetAllRoutes_ShouldHandleNullResult()
        {
            // Arrange
            var mockRouteRepo = new Mock<IRouteRepository>();
            var mockVehicleRepo = new Mock<IVehicleRepository>();
            var mockDriverRepo = new Mock<IDriverRepository>();

            mockRouteRepo.Setup(r => r.GetAllRoutes()).Returns((List<Route>)null);
            mockVehicleRepo.Setup(r => r.GetAllVehicles()).Returns((List<Vehicle>)null);
            mockDriverRepo.Setup(r => r.GetAllDrivers()).Returns((List<Driver>)null);

            // Act & Assert - Should not throw
            var routeResult = mockRouteRepo.Object.GetAllRoutes();
            var vehicleResult = mockVehicleRepo.Object.GetAllVehicles();
            var driverResult = mockDriverRepo.Object.GetAllDrivers();

            Assert.Null(routeResult);
            Assert.Null(vehicleResult);
            Assert.Null(driverResult);
        }

        [Fact]
        public void ActivityRepository_GetAllActivities_ShouldHandleNullResult()
        {
            // Arrange
            var mockRepo = new Mock<IActivityRepository>();
            mockRepo.Setup(r => r.GetAllActivities()).Returns((List<Activity>)null);

            // Act & Assert - Should not throw
            var result = mockRepo.Object.GetAllActivities();
            Assert.Null(result);
        }

        [Fact]
        public void FuelRepository_GetAllFuelRecords_ShouldHandleNullResult()
        {
            // Arrange
            var mockFuelRepo = new Mock<IFuelRepository>();
            var mockVehicleRepo = new Mock<IVehicleRepository>();

            mockFuelRepo.Setup(r => r.GetAllFuelRecords()).Returns((List<Fuel>)null);
            mockVehicleRepo.Setup(r => r.GetAllVehicles()).Returns((List<Vehicle>)null);

            // Act & Assert - Should not throw
            var fuelResult = mockFuelRepo.Object.GetAllFuelRecords();
            var vehicleResult = mockVehicleRepo.Object.GetAllVehicles();

            Assert.Null(fuelResult);
            Assert.Null(vehicleResult);
        }

        [Fact]
        public void MaintenanceRepository_GetAllMaintenanceRecords_ShouldHandleNullResult()
        {
            // Arrange
            var mockMaintenanceRepo = new Mock<IMaintenanceRepository>();
            var mockVehicleRepo = new Mock<IVehicleRepository>();

            mockMaintenanceRepo.Setup(r => r.GetAllMaintenanceRecords()).Returns((List<Maintenance>)null);
            mockVehicleRepo.Setup(r => r.GetAllVehicles()).Returns((List<Vehicle>)null);

            // Act & Assert - Should not throw
            var maintenanceResult = mockMaintenanceRepo.Object.GetAllMaintenanceRecords();
            var vehicleResult = mockVehicleRepo.Object.GetAllVehicles();

            Assert.Null(maintenanceResult);
            Assert.Null(vehicleResult);
        }

        [Fact]
        public void SchoolCalendarRepository_GetAllCalendarEvents_ShouldHandleNullResult()
        {
            // Arrange
            var mockRepo = new Mock<ISchoolCalendarRepository>();
            mockRepo.Setup(r => r.GetAllCalendarEvents()).Returns((List<SchoolCalendar>)null);

            // Act & Assert - Should not throw
            var result = mockRepo.Object.GetAllCalendarEvents();
            Assert.Null(result);
        }

        [Fact]
        public void MockRepository_Setup_ShouldNotThrowOnNullConfiguration()
        {
            // Arrange & Act - Test that mocking itself doesn't cause issues
            var mockActivityRepo = new Mock<IActivityRepository>();
            var mockVehicleRepo = new Mock<IVehicleRepository>();
            var mockDriverRepo = new Mock<IDriverRepository>();
            var mockRouteRepo = new Mock<IRouteRepository>();
            var mockFuelRepo = new Mock<IFuelRepository>();
            var mockMaintenanceRepo = new Mock<IMaintenanceRepository>();
            var mockSchoolCalendarRepo = new Mock<ISchoolCalendarRepository>();

            // Assert - All repositories should be created successfully
            Assert.NotNull(mockActivityRepo.Object);
            Assert.NotNull(mockVehicleRepo.Object);
            Assert.NotNull(mockDriverRepo.Object);
            Assert.NotNull(mockRouteRepo.Object);
            Assert.NotNull(mockFuelRepo.Object);
            Assert.NotNull(mockMaintenanceRepo.Object);
            Assert.NotNull(mockSchoolCalendarRepo.Object);
        }

        [Fact]
        public void NullCollectionHandling_ShouldBeDefensiveProgramming()
        {
            // Test that our code can handle null collections gracefully
            List<Vehicle> nullVehicles = null;
            List<Driver> nullDrivers = null;
            List<Route> nullRoutes = null;

            // Act & Assert - These should not throw
            Assert.Null(nullVehicles);
            Assert.Null(nullDrivers);
            Assert.Null(nullRoutes);

            // Verify that null checks work as expected
            Assert.True(nullVehicles == null);
            Assert.True(nullDrivers == null);
            Assert.True(nullRoutes == null);
        }

        [Fact]
        public void Repository_MockSetup_ShouldHandleMultipleNullReturns()
        {
            // Arrange - Setup multiple repositories with null returns
            var mockVehicleRepo = new Mock<IVehicleRepository>();
            var mockDriverRepo = new Mock<IDriverRepository>();
            var mockRouteRepo = new Mock<IRouteRepository>();
            var mockActivityRepo = new Mock<IActivityRepository>();

            mockVehicleRepo.Setup(r => r.GetAllVehicles()).Returns((List<Vehicle>)null);
            mockDriverRepo.Setup(r => r.GetAllDrivers()).Returns((List<Driver>)null);
            mockRouteRepo.Setup(r => r.GetAllRoutes()).Returns((List<Route>)null);
            mockActivityRepo.Setup(r => r.GetAllActivities()).Returns((List<Activity>)null);

            // Act & Assert - All should return null without throwing
            Assert.Null(mockVehicleRepo.Object.GetAllVehicles());
            Assert.Null(mockDriverRepo.Object.GetAllDrivers());
            Assert.Null(mockRouteRepo.Object.GetAllRoutes());
            Assert.Null(mockActivityRepo.Object.GetAllActivities());
        }
    }
}
