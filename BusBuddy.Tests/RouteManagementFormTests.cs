using Xunit;
using System;
using BusBuddy.UI.Views;
using BusBuddy.Data;

namespace BusBuddy.Tests
{
    [Collection("Database collection")]
    public class RouteManagementFormTests
    {
        private readonly DatabaseFixture _fixture;

        public RouteManagementFormTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void RouteManagementForm_InitializesAndLoads()
        {
            using var context = _fixture.CreateContext();

            // Create form - this should not throw exceptions
            var form = new RouteManagementForm();

            // Verify form was created successfully
            Assert.NotNull(form);
            Assert.Equal("Route Management", form.Text);
        }

        [Fact]
        public void RouteManagementForm_HandlesEmptyData()
        {
            using var context = _fixture.CreateContext();

            var form = new RouteManagementForm();

            // Should handle empty data gracefully
            Assert.NotNull(form);
        }
    }
}
