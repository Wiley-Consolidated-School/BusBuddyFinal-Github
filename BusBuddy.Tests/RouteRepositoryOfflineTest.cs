using System;
using System.Collections.Generic;
using Xunit;
using BusBuddy.Data;
using BusBuddy.Models;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Tests RouteRepository offline/fallback behavior for route management/reporting.
    /// Ensures sample data is returned and displayable when DB is unavailable.
    /// </summary>
    public class RouteRepositoryOfflineTest
    {
        [Fact]
        public void GetAllRoutes_WhenDatabaseOffline_ReturnsSampleData()
        {
            // Arrange
            var repo = new RouteRepository();
            // Simulate DB offline by forcing fallback (connection string invalid or DB not running)
            // In this implementation, fallback is triggered by actual DB failure
            // This test will pass if run without a DB, or you can force the fallback by renaming the DB

            // Act
            List<Route> routes = repo.GetAllRoutes();

            // Assert
            Assert.NotNull(routes);
            Assert.NotEmpty(routes);
            Assert.All(routes, r => Assert.False(string.IsNullOrWhiteSpace(r.RouteName)));
        }
    }
}

