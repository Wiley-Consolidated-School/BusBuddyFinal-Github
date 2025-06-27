using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using BusBuddy.Data;
using BusBuddy.Business;
using BusBuddy.Models;

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// Base class for system-level integration tests.
    /// Provides common setup and teardown for end-to-end testing scenarios.
    /// </summary>
    public abstract class SystemTestBase : IDisposable
    {
        protected readonly IVehicleRepository VehicleRepository;
        protected readonly IDriverRepository DriverRepository;
        protected readonly IRouteRepository RouteRepository;
        protected readonly IMaintenanceRepository MaintenanceRepository;
        protected readonly IFuelRepository FuelRepository;
        protected readonly ValidationService ValidationService;
        protected readonly RouteAnalyticsService AnalyticsService;

        protected readonly List<int> TestVehicleIds;
        protected readonly List<int> TestDriverIds;
        protected readonly List<int> TestRouteIds;

        protected SystemTestBase()
        {
            // Initialize repositories
            VehicleRepository = new VehicleRepository();
            DriverRepository = new DriverRepository();
            RouteRepository = new RouteRepository();
            MaintenanceRepository = new MaintenanceRepository();
            FuelRepository = new FuelRepository();

            // Initialize services
            ValidationService = new ValidationService(
                VehicleRepository,
                DriverRepository,
                MaintenanceRepository,
                FuelRepository);

            AnalyticsService = new RouteAnalyticsService(
                RouteRepository,
                VehicleRepository,
                DriverRepository);

            // Track test data for cleanup
            TestVehicleIds = new List<int>();
            TestDriverIds = new List<int>();
            TestRouteIds = new List<int>();
        }

        protected Vehicle CreateTestVehicle(string suffix = "")
        {
            var timestamp = DateTime.Now.Ticks;
            return new Vehicle
            {
                VehicleNumber = $"TEST{timestamp}{suffix}",
                Make = "Test Make",
                Model = "Test Model",
                Year = 2020,
                SeatingCapacity = 72,
                FuelType = "Diesel",
                Status = "Active",
                VINNumber = $"VIN{timestamp}",
                LicenseNumber = $"LIC{timestamp}"
            };
        }

        protected Driver CreateTestDriver(string suffix = "")
        {
            var timestamp = DateTime.Now.Ticks;
            return new Driver
            {
                FirstName = "Test",
                LastName = $"Driver{timestamp}{suffix}",
                DriverName = $"Test Driver{timestamp}{suffix}",
                DriversLicenseType = "CDL",
                Status = "Active",
                DriverPhone = "555-0123",
                DriverEmail = $"test{timestamp}@example.com",
                CDLExpirationDate = DateTime.Today.AddYears(2),
                TrainingComplete = 1
            };
        }

        protected Route CreateTestRoute(int? vehicleId = null, int? driverId = null, string suffix = "")
        {
            var timestamp = DateTime.Now.Ticks;
            return new Route
            {
                RouteName = $"TestRoute{timestamp}{suffix}",
                Date = DateTime.Today.ToString("yyyy-MM-dd"),
                AMVehicleID = vehicleId,
                AMDriverID = driverId,
                AMBeginMiles = 1000,
                AMEndMiles = 1050,
                AMRiders = 25,
                RouteType = "CDL"
            };
        }

        public virtual void Dispose()
        {
            // Cleanup test data in reverse order due to dependencies
            foreach (var id in TestRouteIds)
            {
                try { RouteRepository.DeleteRoute(id); } catch { }
            }

            foreach (var id in TestVehicleIds)
            {
                try { VehicleRepository.DeleteVehicle(id); } catch { }
            }

            foreach (var id in TestDriverIds)
            {
                try { DriverRepository.DeleteDriver(id); } catch { }
            }
        }
    }
}
