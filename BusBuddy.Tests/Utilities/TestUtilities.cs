using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Moq;

namespace BusBuddy.Tests.Utilities
{
    /// <summary>
    /// Test priority attribute for controlling test execution order
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TestPriorityAttribute : Attribute
    {
        public int Priority { get; }

        public TestPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }

    /// <summary>
    /// Custom test case orderer that respects TestPriority attributes
    /// Ensures end-to-end tests run in logical sequence
    /// </summary>
    public class PriorityOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            var sortedMethods = new SortedDictionary<int, List<TTestCase>>();

            foreach (var testCase in testCases)
            {
                var priority = 0;

                foreach (var attr in testCase.TestMethod.Method.GetCustomAttributes((typeof(TestPriorityAttribute)).AssemblyQualifiedName))
                {
                    priority = attr.GetNamedArgument<int>("Priority");
                    break;
                }

                GetOrCreate(sortedMethods, priority).Add(testCase);
            }

            foreach (var list in sortedMethods.Keys.Select(priority => sortedMethods[priority]))
            {
                list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name));
                foreach (var testCase in list)
                    yield return testCase;
            }
        }

        private static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            if (dictionary.TryGetValue(key, out var result))
                return result;

            result = new TValue();
            dictionary[key] = result;
            return result;
        }
    }

    /// <summary>
    /// Test data builder for creating realistic test scenarios
    /// </summary>
    public static class TestDataBuilder
    {
        /// <summary>
        /// Creates a set of test vehicles with realistic data
        /// </summary>
        public static List<TestVehicle> CreateTestVehicles(int count = 5)
        {
            var vehicles = new List<TestVehicle>();
            var makes = new[] { "Blue Bird", "IC Bus", "Thomas Built", "Collins", "Starcraft" };
            var models = new[] { "Vision", "CE Series", "Saf-T-Liner", "NexBus", "StarTrans" };

            for (int i = 1; i <= count; i++)
            {
                vehicles.Add(new TestVehicle
                {
                    Id = i,
                    VehicleNumber = $"BUS{i:D3}",
                    Make = makes[i % makes.Length],
                    Model = models[i % models.Length],
                    Year = 2020 + (i % 4),
                    Capacity = 20 + (i % 6) * 10, // 20, 30, 40, 50, 60, 70
                    IsActive = i <= count * 0.9, // 90% active
                    Mileage = 10000 + i * 5000,
                    LastMaintenanceDate = DateTime.Now.AddDays(-30 - i * 10)
                });
            }

            return vehicles;
        }

        /// <summary>
        /// Creates a set of test drivers with realistic data
        /// </summary>
        public static List<TestDriver> CreateTestDrivers(int count = 8)
        {
            var firstNames = new[] { "John", "Sarah", "Michael", "Lisa", "David", "Jennifer", "Robert", "Amanda" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis" };

            var drivers = new List<TestDriver>();

            for (int i = 1; i <= count; i++)
            {
                drivers.Add(new TestDriver
                {
                    Id = i,
                    FirstName = firstNames[(i - 1) % firstNames.Length],
                    LastName = lastNames[(i - 1) % lastNames.Length],
                    LicenseNumber = $"DL{1000000 + i}",
                    LicenseExpiration = DateTime.Now.AddMonths(6 + i * 3),
                    PhoneNumber = $"555-{100 + i:D3}-{1000 + i:D4}",
                    Email = $"{firstNames[(i - 1) % firstNames.Length].ToLower()}.{lastNames[(i - 1) % lastNames.Length].ToLower()}@busbuddy.test",
                    HireDate = DateTime.Now.AddMonths(-12 - i * 2),
                    IsActive = true,
                    YearsExperience = Math.Max(1, 15 - i),
                    SafetyRating = 4.0 + (i % 6) * 0.15 // 4.0 to 4.75
                });
            }

            return drivers;
        }

        /// <summary>
        /// Creates a set of test routes with realistic data
        /// </summary>
        public static List<TestRoute> CreateTestRoutes(int count = 6)
        {
            var routeNames = new[]
            {
                "Elementary Route A", "Elementary Route B", "Middle School North",
                "Middle School South", "High School Express", "Special Needs Route"
            };

            var routes = new List<TestRoute>();

            for (int i = 1; i <= Math.Min(count, routeNames.Length); i++)
            {
                routes.Add(new TestRoute
                {
                    Id = i,
                    RouteName = routeNames[i - 1],
                    Description = $"Route serving {routeNames[i - 1].Split(' ')[0]} students",
                    Distance = 8.5 + i * 2.3, // 8.5 to 22.3 miles
                    EstimatedDuration = 35 + i * 8, // 35 to 83 minutes
                    StopCount = 12 + i * 2, // 12 to 24 stops
                    StudentCount = 25 + i * 8, // 25 to 73 students
                    IsActive = true,
                    StartTime = TimeSpan.FromHours(7) + TimeSpan.FromMinutes(i * 15),
                    EndTime = TimeSpan.FromHours(8) + TimeSpan.FromMinutes(i * 15),
                    AssignedVehicleId = i <= count ? i : null,
                    AssignedDriverId = i <= count ? i : null
                });
            }

            return routes;
        }

        /// <summary>
        /// Creates maintenance records for testing
        /// </summary>
        public static List<TestMaintenanceRecord> CreateMaintenanceRecords(int vehicleCount = 5)
        {
            var maintenanceTypes = new[]
            {
                "Oil Change", "Tire Rotation", "Brake Inspection", "Engine Service",
                "Safety Inspection", "Transmission Service", "Air Filter Replacement"
            };

            var records = new List<TestMaintenanceRecord>();
            var recordId = 1;

            for (int vehicleId = 1; vehicleId <= vehicleCount; vehicleId++)
            {
                // Each vehicle gets 3-5 maintenance records
                var recordCount = 3 + vehicleId % 3;

                for (int i = 0; i < recordCount; i++)
                {
                    records.Add(new TestMaintenanceRecord
                    {
                        Id = recordId++,
                        VehicleId = vehicleId,
                        MaintenanceType = maintenanceTypes[i % maintenanceTypes.Length],
                        Description = $"{maintenanceTypes[i % maintenanceTypes.Length]} for vehicle BUS{vehicleId:D3}",
                        ServiceDate = DateTime.Now.AddDays(-90 + i * 30),
                        NextServiceDue = DateTime.Now.AddDays(-60 + i * 30 + 90),
                        Cost = 150 + i * 50 + vehicleId * 25,
                        Mileage = 10000 + vehicleId * 5000 + i * 2000,
                        TechnicianName = $"Tech {(i % 3) + 1}",
                        IsCompleted = i < recordCount - 1 // Last record is pending
                    });
                }
            }

            return records;
        }

        /// <summary>
        /// Creates analytics test data for performance testing
        /// </summary>
        public static AnalyticsTestData CreateAnalyticsTestData()
        {
            return new AnalyticsTestData
            {
                TotalVehicles = 25,
                ActiveVehicles = 23,
                TotalDrivers = 30,
                ActiveDrivers = 28,
                TotalRoutes = 15,
                ActiveRoutes = 14,
                AverageUtilization = 87.5,
                FuelEfficiencyAvg = 8.2,
                OnTimePerformance = 94.3,
                MaintenanceDueCount = 5,
                SafetyIncidents = 0,
                GeneratedDate = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Test assertion helpers for common validation patterns
    /// </summary>
    public static class TestAssertions
    {
        /// <summary>
        /// Asserts that a vehicle object has valid data
        /// </summary>
        public static void AssertValidVehicle(TestVehicle vehicle)
        {
            if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));

            Assert.True(vehicle.Id > 0, "Vehicle ID should be positive");
            Assert.False(string.IsNullOrEmpty(vehicle.VehicleNumber), "Vehicle number should not be empty");
            Assert.False(string.IsNullOrEmpty(vehicle.Make), "Vehicle make should not be empty");
            Assert.False(string.IsNullOrEmpty(vehicle.Model), "Vehicle model should not be empty");
            Assert.True(vehicle.Year >= 2000 && vehicle.Year <= DateTime.Now.Year + 1, "Vehicle year should be reasonable");
            Assert.True(vehicle.Capacity > 0, "Vehicle capacity should be positive");
            Assert.True(vehicle.Mileage >= 0, "Vehicle mileage should not be negative");
        }

        /// <summary>
        /// Asserts that a driver object has valid data
        /// </summary>
        public static void AssertValidDriver(TestDriver driver)
        {
            if (driver == null) throw new ArgumentNullException(nameof(driver));

            Assert.True(driver.Id > 0, "Driver ID should be positive");
            Assert.False(string.IsNullOrEmpty(driver.FirstName), "First name should not be empty");
            Assert.False(string.IsNullOrEmpty(driver.LastName), "Last name should not be empty");
            Assert.False(string.IsNullOrEmpty(driver.LicenseNumber), "License number should not be empty");
            Assert.True(driver.LicenseExpiration > DateTime.Now, "License should not be expired");
            Assert.True(driver.YearsExperience >= 0, "Years of experience should not be negative");
            Assert.True(driver.SafetyRating >= 0 && driver.SafetyRating <= 5, "Safety rating should be between 0 and 5");
        }

        /// <summary>
        /// Asserts that performance metrics are within acceptable ranges
        /// </summary>
        public static void AssertPerformanceMetrics(long elapsedMs, long maxExpectedMs, string operation)
        {
            Assert.True(elapsedMs <= maxExpectedMs,
                $"{operation} took {elapsedMs}ms, which exceeds the maximum expected {maxExpectedMs}ms");
        }

        /// <summary>
        /// Asserts that memory usage is within acceptable bounds
        /// </summary>
        public static void AssertMemoryUsage(long memoryBytes, long maxExpectedBytes, string operation)
        {
            Assert.True(memoryBytes <= maxExpectedBytes,
                $"{operation} used {memoryBytes / (1024 * 1024)}MB, which exceeds the maximum expected {maxExpectedBytes / (1024 * 1024)}MB");
        }
    }

    // Test data classes
    public class TestVehicle
    {
        public int Id { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public int Mileage { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
    }

    public class TestDriver
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiration { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }
        public int YearsExperience { get; set; }
        public double SafetyRating { get; set; }
    }

    public class TestRoute
    {
        public int Id { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Distance { get; set; }
        public int EstimatedDuration { get; set; }
        public int StopCount { get; set; }
        public int StudentCount { get; set; }
        public bool IsActive { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? AssignedVehicleId { get; set; }
        public int? AssignedDriverId { get; set; }
    }

    public class TestMaintenanceRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public DateTime NextServiceDue { get; set; }
        public decimal Cost { get; set; }
        public int Mileage { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }

    public class AnalyticsTestData
    {
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int TotalDrivers { get; set; }
        public int ActiveDrivers { get; set; }
        public int TotalRoutes { get; set; }
        public int ActiveRoutes { get; set; }
        public double AverageUtilization { get; set; }
        public double FuelEfficiencyAvg { get; set; }
        public double OnTimePerformance { get; set; }
        public int MaintenanceDueCount { get; set; }
        public int SafetyIncidents { get; set; }
        public DateTime GeneratedDate { get; set; }
    }

    /// <summary>
    /// Extension methods for database mock setup
    /// </summary>
    public static class DatabaseMockExtensions
    {
        public static Mock<T> SetupReturnsAsync<T>(this Mock<T> mock, object returnValue) where T : class
        {
            // Helper for setting up async mock returns
            return mock;
        }
    }
}
