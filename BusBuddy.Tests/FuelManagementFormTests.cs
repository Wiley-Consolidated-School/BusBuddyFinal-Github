using Xunit;
using System;
using BusBuddy.UI.Views;
using BusBuddy.Data;

namespace BusBuddy.Tests
{
    [Collection("Database collection")]
    public class FuelManagementFormTests
    {
        private readonly DatabaseFixture _fixture;

        public FuelManagementFormTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void FuelManagementForm_InitializesAndLoads()
        {
            using var context = _fixture.CreateContext();

            // Create form - this should not throw exceptions even if Vehicles table doesn't exist
            var form = new FuelManagementForm();

            // Verify form was created successfully
            Assert.NotNull(form);
            Assert.Equal("Fuel Management", form.Text);
        }

        [Fact]
        public void FuelManagementForm_HandlesNoVehicles()
        {
            using var context = _fixture.CreateContext();

            var form = new FuelManagementForm();

            // Should handle missing vehicles gracefully
            Assert.NotNull(form);
        }
    }
}
