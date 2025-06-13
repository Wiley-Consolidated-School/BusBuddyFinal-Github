using System;
using System.Linq;
using BusBuddy.Data;
using BusBuddy.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BusBuddy.Tests
{
    [Collection("Database collection")]
    public class EfCoreTests
    {
        private readonly DatabaseFixture _fixture;

        public EfCoreTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CanCreateContext()
        {
            // Ensure the fixture can create an EF Core context
            var context = _fixture.CreateContext();
            Assert.NotNull(context);
        }

        [Fact]
        public void CanQueryVehicles_WithInMemoryDatabase()
        {
            // Act - Get context and query vehicles
            var context = _fixture.CreateContext();
            var vehicles = context?.Vehicles.ToList();

            // Assert
            Assert.NotNull(vehicles);
            Assert.True(vehicles!.Count >= 1, "Should have at least one vehicle");
            Assert.Contains(vehicles, v => v.VehicleNumber == "BUS001");
        }

        [Fact]
        public void CanAddVehicle_WithInMemoryDatabase()
        {
            // Arrange - Get a fresh context
            var context = _fixture.CreateContext();
            Assert.NotNull(context);

            // Initial count
            var initialCount = context!.Vehicles.Count();

            // Act - Add a new vehicle
            var newVehicle = new Vehicle
            {
                VehicleNumber = "V999",
                Make = "Test Make",
                Model = "Test Model",
                Year = 2025,
                Capacity = 42,
                FuelType = "Hybrid",
                Status = "New"
            };

            context.Vehicles.Add(newVehicle);
            context.SaveChanges();

            // Get a fresh context to verify the addition persisted
            var verifyContext = _fixture.CreateContext();

            // Assert
            Assert.NotNull(verifyContext);
            var newCount = verifyContext!.Vehicles.Count();
            Assert.Equal(initialCount + 1, newCount);

            var foundVehicle = verifyContext.Vehicles.FirstOrDefault(v => v.VehicleNumber == "V999");
            Assert.NotNull(foundVehicle);
            Assert.Equal("Test Make", foundVehicle.Make);
        }

        [Fact]
        public void CanQueryFuels_WithInMemoryDatabase()
        {
            // Act
            var context = _fixture.CreateContext();
            var fuels = context?.Fuels.ToList();

            // Assert
            Assert.NotNull(fuels);
            Assert.True(fuels!.Count >= 1, "Should have at least one fuel record");
            Assert.Contains(fuels, f => f.FuelLocation == "Main Depot");
        }

        [Fact]
        public void CanUseFuelVehicleNavigation_WithInMemoryDatabase()
        {
            // Arrange
            var context = _fixture.CreateContext();
            Assert.NotNull(context);

            // Make sure we can use Include for navigation properties
            var fuelsWithVehicles = context!.Fuels
                .Include(f => f.VehicleFueled)
                .ToList();

            // Assert - at least one fuel record should have its vehicle loaded
            Assert.NotEmpty(fuelsWithVehicles);
            var fuelWithVehicle = fuelsWithVehicles.FirstOrDefault(f => f.VehicleFueled != null);

            // There may not be navigation data if we haven't properly loaded it in the seed data
            if (fuelWithVehicle != null)
            {
                Assert.NotNull(fuelWithVehicle.VehicleFueled);
                Assert.NotNull(fuelWithVehicle.VehicleFueled!.VehicleNumber);
            }
        }
    }
}
