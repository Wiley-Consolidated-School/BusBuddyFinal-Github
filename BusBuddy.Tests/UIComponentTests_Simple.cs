using Xunit;
using System;
using System.Windows.Forms;
using System.Drawing;
using BusBuddy.Tests.TestHelpers;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Simple UI Component Tests to verify namespace access
    /// </summary>
    public class UIComponentTests_Simple
    {
        [STAThread]
        [Fact]
        public void MockMainForm_WhenInitialized_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            using var form = new MockMainForm(null, new Size(1024, 768));

            // Assert
            Assert.NotNull(form);
            Assert.Equal("BusBuddy - Bus Tracking Companion", form.Text);
            Assert.Equal(new Size(1024, 768), form.ClientSize);
        }

        [STAThread]
        [Fact]
        public void UITestHelpers_FindButtonByText_ShouldWork()
        {
            // Arrange
            using var form = new MockMainForm();

            // Act
            var button = UITestHelpers.FindButtonByText(form, "Manage Vehicles");

            // Assert
            Assert.NotNull(button);
            Assert.Equal("Manage Vehicles", button.Text);
        }
    }
}
