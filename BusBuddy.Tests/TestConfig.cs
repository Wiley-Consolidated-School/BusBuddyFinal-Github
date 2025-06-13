using System;
using System.Collections.Generic;
using System.IO;

namespace BusBuddy.Tests
{
            public static class TestConfig
            {
            public static readonly string TestDbConnectionString = "Data Source=:memory:;Version=3;New=True;";

            public static readonly int TestTimeout = 5000; // milliseconds

            public static readonly List<string> ValidVehicleNumbers = new List<string>
            {
            "BUS001",
            "BUS002",
            "BUS003",
            "VAN001",
            "COACH01"        };

            public static readonly List<string?> InvalidVehicleNumbers = new List<string?>
            {
            "",
            "B",
            "B1",
            null
            };

            public static readonly List<string> ValidMakes = new List<string>
            {
            "Mercedes",
            "Ford",
            "Toyota",
            "Volvo",
            "MAN"
            };

            public static readonly Dictionary<string, string> TestUsers = new Dictionary<string, string>
            {
            { "admin", "password123" },
            { "user1", "password456" }
            };

            public static readonly string TestDataDirectory = Path.Combine(
            Directory.GetCurrentDirectory(), "TestData");

            public static readonly Dictionary<string, object> DefaultVehicle = new Dictionary<string, object>
            {
            { "Id", 1 },
            { "VehicleNumber", "BUS001" },
            { "Make", "Mercedes" },
            { "Model", "Sprinter" },
            { "Year", 2022 },
            { "Capacity", 25 },
            { "FuelType", "Diesel" },
            { "Status", "Active" }
            };
            }
}
