using System;
using System.Linq;
using System.Windows.Forms;
using BusBuddy;
using BusBuddy.Models;
using Xunit;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Unit tests to validate the proper startup sequence and initialization of BusBuddy MainForm
    /// </summary>
    public class MainFormConstructorTests : IDisposable
    {
        private MainForm? _mainForm;

        [Fact]
        [Trait("Category", "Constructor")]
        public void MainForm_Constructor_ShouldInitializeWithoutExceptions()
        {
            // Arrange & Act
            Exception? caughtException = null;
            try
            {
                _mainForm = new MainForm();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.Null(caughtException);
            Assert.NotNull(_mainForm);
        }

        [Fact]
        [Trait("Category", "Constructor")]
        public void MainForm_Constructor_ShouldHaveValidWindowProperties()
        {
            // Arrange & Act
            _mainForm = new MainForm();

            // Assert
            Assert.Equal("BusBuddy - Bus Tracking Companion", _mainForm.Text);
            Assert.Equal(1024, _mainForm.Size.Width);
            Assert.Equal(768, _mainForm.Size.Height);
            Assert.Equal(FormStartPosition.CenterScreen, _mainForm.StartPosition);
        }

        [Fact]
        [Trait("Category", "Constructor")]
        public void MainForm_Constructor_ShouldHaveMenuStrip()
        {
            // Arrange & Act
            _mainForm = new MainForm();

            // Assert
            Assert.NotNull(_mainForm.MainMenuStrip);
            Assert.True(_mainForm.Controls.Contains(_mainForm.MainMenuStrip));
        }

        [Fact]
        [Trait("Category", "Constructor")]
        public void MainForm_Constructor_ShouldHaveRequiredMenuItems()
        {
            // Arrange & Act
            _mainForm = new MainForm();

            // Assert
            Assert.NotNull(_mainForm.MainMenuStrip);

            var menuItems = _mainForm.MainMenuStrip.Items.Cast<ToolStripMenuItem>().ToList();
            Assert.Contains(menuItems, item => item.Text == "File");
            Assert.Contains(menuItems, item => item.Text == "Data Management");
            Assert.Contains(menuItems, item => item.Text == "Reports");
        }

        [Fact]
        [Trait("Category", "Constructor")]
        public void MainForm_Constructor_ShouldHaveDashboardPanel()
        {
            // Arrange & Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanels = _mainForm.Controls.OfType<Panel>()
                .Where(p => p.BorderStyle == BorderStyle.FixedSingle && p.AutoScroll == true)
                .ToList();

            Assert.Single(dashboardPanels);

            var dashboardPanel = dashboardPanels.First();
            Assert.True(dashboardPanel.Anchor.HasFlag(AnchorStyles.Top));
            Assert.True(dashboardPanel.Anchor.HasFlag(AnchorStyles.Left));
            Assert.True(dashboardPanel.Anchor.HasFlag(AnchorStyles.Right));
            Assert.True(dashboardPanel.Anchor.HasFlag(AnchorStyles.Bottom));
        }

        [Fact]
        [Trait("Category", "Constructor")]
        public void MainForm_Constructor_ShouldHaveDashboardTitle()
        {
            // Arrange & Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanel = _mainForm.Controls.OfType<Panel>()
                .FirstOrDefault(p => p.BorderStyle == BorderStyle.FixedSingle && p.AutoScroll == true);

            Assert.NotNull(dashboardPanel);

            var titleLabel = dashboardPanel.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Text.Contains("Dashboard"));

            Assert.NotNull(titleLabel);
            Assert.Equal("Dashboard - All Views", titleLabel.Text);
        }

        [Fact]
        [Trait("Category", "Dashboard")]
        public void MainForm_Constructor_ShouldHaveCategoryLabels()
        {
            // Arrange & Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanel = _mainForm.Controls.OfType<Panel>()
                .FirstOrDefault(p => p.BorderStyle == BorderStyle.FixedSingle && p.AutoScroll == true);

            Assert.NotNull(dashboardPanel);

            var categoryLabels = dashboardPanel.Controls.OfType<Label>()
                .Where(l => l.Font.Bold && l.ForeColor == System.Drawing.Color.DarkBlue)
                .Select(l => l.Text)
                .ToList();

            Assert.Contains("Fleet Management", categoryLabels);
            Assert.Contains("Personnel Management", categoryLabels);
            Assert.Contains("Operations Management", categoryLabels);
            Assert.Contains("Administrative", categoryLabels);
        }

        [Fact]
        [Trait("Category", "Dashboard")]
        public void MainForm_Constructor_ShouldHaveViewButtons()
        {
            // Arrange & Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanel = _mainForm.Controls.OfType<Panel>()
                .FirstOrDefault(p => p.BorderStyle == BorderStyle.FixedSingle && p.AutoScroll == true);

            Assert.NotNull(dashboardPanel);

            var viewButtons = dashboardPanel.Controls.OfType<Button>()
                .Where(b => b.BackColor == System.Drawing.Color.LightSteelBlue)
                .Select(b => b.Text)
                .ToList();

            // Fleet Management buttons
            Assert.Contains("Vehicle Management", viewButtons);
            Assert.Contains("Maintenance Management", viewButtons);
            Assert.Contains("Fuel Management", viewButtons);

            // Personnel Management buttons
            Assert.Contains("Driver Management", viewButtons);
            Assert.Contains("Time Card Management", viewButtons);

            // Operations Management buttons
            Assert.Contains("Route Management", viewButtons);
            Assert.Contains("Activity Management", viewButtons);
            Assert.Contains("Activity Schedule Management", viewButtons);

            // Administrative buttons
            Assert.Contains("School Calendar Management", viewButtons);

            // Verify we have all 9 expected buttons
            Assert.Equal(9, viewButtons.Count);
        }

        [Fact]
        [Trait("Category", "Dashboard")]
        public void MainForm_Constructor_ShouldHaveButtonsWithCorrectProperties()
        {
            // Arrange & Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanel = _mainForm.Controls.OfType<Panel>()
                .FirstOrDefault(p => p.BorderStyle == BorderStyle.FixedSingle && p.AutoScroll == true);

            Assert.NotNull(dashboardPanel);

            var viewButtons = dashboardPanel.Controls.OfType<Button>()
                .Where(b => b.BackColor == System.Drawing.Color.LightSteelBlue)
                .ToList();

            foreach (var button in viewButtons)
            {
                Assert.Equal(180, button.Size.Width);
                Assert.Equal(50, button.Size.Height);
                Assert.Equal(System.Drawing.Color.LightSteelBlue, button.BackColor);
                Assert.Equal(System.Drawing.Color.DarkBlue, button.ForeColor);
                Assert.Equal(FlatStyle.Flat, button.FlatStyle);
                Assert.Equal(System.Drawing.Color.SteelBlue, button.FlatAppearance.BorderColor);
            }
        }

        [Fact]
        [Trait("Category", "Dashboard")]
        public void MainForm_Constructor_ShouldHaveButtonsWithClickHandlers()
        {
            // Arrange & Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanel = _mainForm.Controls.OfType<Panel>()
                .FirstOrDefault(p => p.BorderStyle == BorderStyle.FixedSingle && p.AutoScroll == true);

            Assert.NotNull(dashboardPanel);

            var viewButtons = dashboardPanel.Controls.OfType<Button>()
                .Where(b => b.BackColor == System.Drawing.Color.LightSteelBlue)
                .ToList();

            foreach (var button in viewButtons)
            {
                // Check that each button has at least one click event handler
                var clickEvent = typeof(Button).GetEvent("Click");
                var field = typeof(Button).GetField("EVENT_CLICK",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

                if (field != null)
                {
                    var eventKey = field.GetValue(null);
                    var events = button.GetType()
                        .GetProperty("Events", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (events != null)
                    {
                        var eventHandlerList = events.GetValue(button);
                        var hasHandlers = eventHandlerList?.GetType()
                            .GetMethod("get_Item", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.Invoke(eventHandlerList, new[] { eventKey }) != null;

                        Assert.True(hasHandlers, $"Button '{button.Text}' should have a click event handler");
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Initialization")]
        public void MainForm_Constructor_ShouldFollowCorrectInitializationOrder()
        {
            // This test verifies that the constructor doesn't throw null reference exceptions
            // which would indicate improper initialization order

            // Arrange & Act
            Exception? constructorException = null;
            MainForm? testForm = null;

            try
            {
                testForm = new MainForm();

                // Force the form to create its handle and render
                var handle = testForm.Handle; // This triggers full initialization

            }
            catch (Exception ex)
            {
                constructorException = ex;
            }
            finally
            {
                testForm?.Dispose();
            }

            // Assert
            Assert.Null(constructorException);
        }

        [Fact]
        [Trait("Category", "Performance")]
        public void MainForm_Constructor_ShouldCompleteWithinReasonableTime()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            _mainForm = new MainForm();
            stopwatch.Stop();

            // Assert - Constructor should complete within 5 seconds
            Assert.True(stopwatch.ElapsedMilliseconds < 5000,
                $"MainForm constructor took {stopwatch.ElapsedMilliseconds}ms, which is too slow");
        }

        public void Dispose()
        {
            _mainForm?.Dispose();
        }
    }
}
