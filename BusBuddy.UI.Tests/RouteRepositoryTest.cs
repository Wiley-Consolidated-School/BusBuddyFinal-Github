using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using BusBuddy.Data;
using BusBuddy.Models;

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// Tests for RouteRepository ensuring proper CRUD operations and data integrity.
    /// Tests cover database operations, validation, and error handling.
    /// </summary>
    [Trait("Category", "Repository")]
    public class RouteRepositoryTest : IDisposable
    {
        private readonly RouteRepository _repository;
        private readonly List<int> _testRouteIds;

        public RouteRepositoryTest()
        {
            _repository = new RouteRepository();
            _testRouteIds = new List<int>();
        }

        public void Dispose()
        {
            // Cleanup test routes
            foreach (var id in _testRouteIds)
            {
                try
                {
                    _repository.DeleteRoute(id);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Fact]
        public void GetAllRoutes_ShouldReturnListOfRoutes()
        {
            // Act
            var routes = _repository.GetAllRoutes();

            // Assert
            Assert.NotNull(routes);
            Assert.IsType<List<Route>>(routes);
        }

        [Fact]
        public void AddRoute_WithValidData_ShouldReturnRouteId()
        {
            // Arrange
            var route = new Route
            {
                RouteName = "Test Route",
                Notes = "Test route description",
                AMBeginMiles = 0,
                AMEndMiles = 15.5m
            };

            // Act
            var routeId = _repository.AddRoute(route);
            _testRouteIds.Add(routeId);

            // Assert
            Assert.True(routeId > 0);
        }

        [Fact]
        public void GetRouteById_WithValidId_ShouldReturnRoute()
        {
            // Arrange
            var route = new Route
            {
                RouteName = "Test Route 2",
                Notes = "Another test route",
                AMBeginMiles = 0,
                AMEndMiles = 12.3m
            };
            var routeId = _repository.AddRoute(route);
            _testRouteIds.Add(routeId);

            // Act
            var retrievedRoute = _repository.GetRouteById(routeId);

            // Assert
            Assert.NotNull(retrievedRoute);
            Assert.Equal(routeId, retrievedRoute.RouteID);
            Assert.Equal("Test Route 2", retrievedRoute.RouteName);
        }
    }
}
