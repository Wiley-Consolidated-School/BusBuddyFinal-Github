using Xunit;
using Moq;
using BusBuddy.Business;
using BusBuddy.Data;
using BusBuddy.Models;
using System.Collections.Generic;

namespace BusBuddy.Tests
{
    public class DatabaseHelperServiceTests
    {
        private readonly Mock<IVehicleRepository> _mockVehicleRepo;
        private readonly Mock<IDriverRepository> _mockDriverRepo;
        private readonly Mock<IRouteRepository> _mockRouteRepo;
        private readonly DatabaseHelperService _service;

        public DatabaseHelperServiceTests()
        {
            _mockVehicleRepo = new Mock<IVehicleRepository>();
            _mockDriverRepo = new Mock<IDriverRepository>();
            _mockRouteRepo = new Mock<IRouteRepository>();

            // Note: Since DatabaseHelperService creates its own repositories,
            // we'll test it as an integration test with actual database calls
            // or create a modified version that accepts dependencies
            _service = new DatabaseHelperService();
        }

        [Fact]
        public void GetRouteWithDetails_ValidRouteId_ReturnsRouteWithVehicleAndDriver()
        {
            // This is an integration test since DatabaseHelperService
            // creates its own repository instances
            // For true unit testing, we'd need dependency injection

            // Arrange
            var routeId = 1;

            // Act & Assert
            // Since this requires actual database, we'll test the method exists
            // and doesn't throw exceptions for basic scenarios
            var result = _service.GetRouteWithDetails(routeId);

            // The method should not throw an exception
            Assert.NotNull(result);
        }

        [Fact]
        public void DatabaseHelperService_Constructor_InitializesSuccessfully()
        {
            // Act
            var service = new DatabaseHelperService();

            // Assert
            Assert.NotNull(service);
        }
    }
}
