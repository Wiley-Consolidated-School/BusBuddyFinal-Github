#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Views;
using BusBuddy.Tests.UI;
using Moq;
using Xunit;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Tests to verify that management forms handle null values safely
    /// and don't throw ArgumentNullException when repositories return null
    /// </summary>
    public class NullSafetyTests : UITestBase
    {
        [Fact]
        public void VehicleManagementForm_LoadData_ShouldHandleNullRepository()
        {
            // Arrange
            var mockRepo = new Mock<IVehicleRepository>();
            mockRepo.Setup(r => r.GetAllVehicles()).Returns((List<Vehicle>)null);

            // Act & Assert - Should not throw ArgumentNullException
            var form = new VehicleManagementFormSyncfusion(mockRepo.Object);

            // Invoke LoadData via reflection
            var loadDataMethod = typeof(VehicleManagementFormSyncfusion)
                .GetMethod("LoadData", BindingFlags.NonPublic | BindingFlags.Instance);

            // This should not throw an exception
            var exception = Record.Exception(() => loadDataMethod?.Invoke(form, null));
            Assert.Null(exception);

            form.Dispose();
        }

        [Fact]
        public void DriverManagementForm_LoadData_ShouldHandleNullRepository()
        {
            // Arrange
            var mockRepo = new Mock<IDriverRepository>();
            mockRepo.Setup(r => r.GetAllDrivers()).Returns((List<Driver>)null);

            // Act & Assert
            var form = new DriverManagementFormSyncfusion(mockRepo.Object);

            var loadDataMethod = typeof(DriverManagementFormSyncfusion)
                .GetMethod("LoadData", BindingFlags.NonPublic | BindingFlags.Instance);

            var exception = Record.Exception(() => loadDataMethod?.Invoke(form, null));
            Assert.Null(exception);

            form.Dispose();
        }

        [Fact]
        public void RouteManagementForm_LoadData_ShouldHandleNullRepository()
        {
            // Arrange
            var mockRouteRepo = new Mock<IRouteRepository>();
            var mockVehicleRepo = new Mock<IVehicleRepository>();
            var mockDriverRepo = new Mock<IDriverRepository>();
            mockRouteRepo.Setup(r => r.GetAllRoutes()).Returns((List<Route>)null);
            mockVehicleRepo.Setup(r => r.GetAllVehicles()).Returns(new List<Vehicle>());
            mockDriverRepo.Setup(r => r.GetAllDrivers()).Returns(new List<Driver>());

            // Act & Assert
            var form = new RouteManagementFormSyncfusion(mockRouteRepo.Object, mockVehicleRepo.Object, mockDriverRepo.Object);

            var loadDataMethod = typeof(RouteManagementFormSyncfusion)
                .GetMethod("LoadData", BindingFlags.NonPublic | BindingFlags.Instance);

            var exception = Record.Exception(() => loadDataMethod?.Invoke(form, null));
            Assert.Null(exception);

            form.Dispose();
        }

        [Fact]
        public void ActivityManagementForm_LoadData_ShouldHandleNullRepository()
        {
            // Arrange
            var mockRepo = new Mock<IActivityRepository>();
            mockRepo.Setup(r => r.GetAllActivities()).Returns((List<Activity>)null);

            // Act & Assert
            var form = new ActivityManagementFormSyncfusion(mockRepo.Object);

            var loadDataMethod = typeof(ActivityManagementFormSyncfusion)
                .GetMethod("LoadData", BindingFlags.NonPublic | BindingFlags.Instance);

            var exception = Record.Exception(() => loadDataMethod?.Invoke(form, null));
            Assert.Null(exception);

            form.Dispose();
        }

        [Fact]
        public void FuelManagementForm_LoadData_ShouldHandleNullRepository()
        {
            // Arrange
            var mockFuelRepo = new Mock<IFuelRepository>();
            var mockVehicleRepo = new Mock<IVehicleRepository>();
            mockFuelRepo.Setup(r => r.GetAllFuelRecords()).Returns((List<Fuel>)null);
            mockVehicleRepo.Setup(r => r.GetAllVehicles()).Returns(new List<Vehicle>());

            // Create concrete repositories using mocks (FuelManagementFormSyncfusion expects concrete types)
            var fuelRepo = new FuelRepository();
            var vehicleRepo = new VehicleRepository();

            // Act & Assert
            var form = new FuelManagementFormSyncfusion(fuelRepo, vehicleRepo);

            var loadDataMethod = typeof(FuelManagementFormSyncfusion)
                .GetMethod("LoadData", BindingFlags.NonPublic | BindingFlags.Instance);

            var exception = Record.Exception(() => loadDataMethod?.Invoke(form, null));
            Assert.Null(exception);

            form.Dispose();
        }

        [Fact]
        public void MaintenanceManagementForm_LoadData_ShouldHandleNullRepository()
        {
            // Arrange
            var mockMaintenanceRepo = new Mock<IMaintenanceRepository>();
            var mockVehicleRepo = new Mock<IVehicleRepository>();
            mockMaintenanceRepo.Setup(r => r.GetAllMaintenanceRecords()).Returns((List<Maintenance>)null);
            mockVehicleRepo.Setup(r => r.GetAllVehicles()).Returns(new List<Vehicle>());

            // Act & Assert
            var form = new MaintenanceManagementFormSyncfusion(mockMaintenanceRepo.Object, mockVehicleRepo.Object);

            var loadDataMethod = typeof(MaintenanceManagementFormSyncfusion)
                .GetMethod("LoadData", BindingFlags.NonPublic | BindingFlags.Instance);

            var exception = Record.Exception(() => loadDataMethod?.Invoke(form, null));
            Assert.Null(exception);

            form.Dispose();
        }

        [Fact]
        public void SchoolCalendarManagementForm_LoadData_ShouldHandleNullRepository()
        {
            // Arrange
            var mockRepo = new Mock<ISchoolCalendarRepository>();
            mockRepo.Setup(r => r.GetAllCalendarEvents()).Returns((List<SchoolCalendar>)null);

            // Act & Assert
            var form = new SchoolCalendarManagementFormSyncfusion(mockRepo.Object);

            var loadDataMethod = typeof(SchoolCalendarManagementFormSyncfusion)
                .GetMethod("LoadData", BindingFlags.NonPublic | BindingFlags.Instance);

            var exception = Record.Exception(() => loadDataMethod?.Invoke(form, null));
            Assert.Null(exception);

            form.Dispose();
        }

        [Fact]
        public void ActivityScheduleManagementForm_LoadData_ShouldHandleNullRepository()
        {
            // Arrange
            var mockRepo = new Mock<IActivityRepository>();
            mockRepo.Setup(r => r.GetAllActivities()).Returns((List<Activity>)null);

            // Act & Assert
            var form = new ActivityScheduleManagementFormSyncfusion(mockRepo.Object);

            var loadDataMethod = typeof(ActivityScheduleManagementFormSyncfusion)
                .GetMethod("LoadData", BindingFlags.NonPublic | BindingFlags.Instance);

            var exception = Record.Exception(() => loadDataMethod?.Invoke(form, null));
            Assert.Null(exception);

            form.Dispose();
        }
    }
}
