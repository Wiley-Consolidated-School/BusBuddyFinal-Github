using Xunit;
using System;
using BusBuddy.UI.Views;
using BusBuddy.Data;

namespace BusBuddy.Tests
{
    [Collection("Database collection")]
    public class TimeCardManagementFormTests
    {
        private readonly DatabaseFixture _fixture;

        public TimeCardManagementFormTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TimeCardManagementForm_InitializesAndLoads()
        {
            using var context = _fixture.CreateContext();

            // Create form - this should not throw exceptions
            var form = new TimeCardManagementForm();

            // Verify form was created successfully
            Assert.NotNull(form);
            Assert.Equal("Time Card Management", form.Text);
        }

        [Fact]
        public void TimeCardManagementForm_HandlesEmptyData()
        {
            using var context = _fixture.CreateContext();

            var form = new TimeCardManagementForm();

            // Should handle empty data gracefully
            Assert.NotNull(form);
        }
    }
}
