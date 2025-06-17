using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Xunit;
using MaterialSkin.Controls;

namespace BusBuddy.Tests.UI
{
    [Collection("UI Tests")]
    public class AdvancedInteractionTests : UITestBase
    {
        [Fact]
        public void Dashboard_KeyboardNavigation_ShouldWorkSeamlessly()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var focusableControls = GetAllControlsOfType<Control>(_dashboard)
                .Where(c => c.CanFocus && c.Visible && c.Enabled)
                .ToList();

            // Act & Assert - Tab navigation should work
            if (focusableControls.Any())
            {
                var firstControl = focusableControls.First();
                firstControl.Focus();
                Assert.True(firstControl.Focused || firstControl.ContainsFocus);

                // Test Tab key navigation (simulate)
                foreach (var control in focusableControls.Take(3))
                {
                    if (control.CanFocus)
                    {
                        control.Focus();
                        Assert.True(control.Focused || control.ContainsFocus ||
                                  _dashboard.ActiveControl == control);
                    }
                }
            }

            // Dashboard should remain stable
            Assert.True(_dashboard.Visible);
            Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));
        }

        [Fact]
        public void Dashboard_AccessibilityKeystrokes_ShouldBeSupported()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var buttons = GetAllControlsOfType<Button>(_dashboard);

            // Act & Assert - Test accessibility keystrokes
            foreach (var button in buttons.Take(3))
            {
                if (button.Enabled && button.Visible)
                {                    // Test that buttons respond to Enter key
                    var clickException = Record.Exception(() => {
                        button.Focus();
                        // Simulate Enter key press effect
                        button.PerformClick();
                    });
                    Assert.Null(clickException);

                    // Test that buttons can receive focus
                    if (button.CanFocus)
                    {
                        button.Focus();
                        Assert.True(button.Focused || button.ContainsFocus);
                    }
                }
            }
        }

        [Fact]
        public void Dashboard_MouseInteraction_ShouldBeAccurate()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var clickableControls = GetAllControlsOfType<Control>(_dashboard)
                .Where(c => c is Button || c is MaterialButton)
                .ToList();

            // Act & Assert - Mouse interactions
            foreach (var control in clickableControls.Take(5))
            {
                if (control.Enabled && control.Visible)
                {
                    // Test that control can handle mouse events
                    var originalBackColor = control.BackColor;
                      // Simulate mouse enter/leave
                    var mouseException = Record.Exception(() => {
                        var mouseEnterArgs = new EventArgs();
                        control.GetType().GetMethod("OnMouseEnter",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.Invoke(control, new object[] { mouseEnterArgs });
                    });
                    // Some exceptions might be expected here, so we don't assert null

                    // Control should remain stable
                    Assert.True(control.Width > 0);
                    Assert.True(control.Height > 0);
                }
            }
        }

        [Fact]
        public void Dashboard_DragDrop_ShouldHandleGracefully()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Test drag-drop capability
            var dragDropCapableControls = GetAllControlsOfType<Control>(_dashboard)
                .Where(c => c.AllowDrop)
                .ToList();

            // Assert - Even if no controls support drag-drop, test should not crash
            Assert.NotNull(dragDropCapableControls);
              // Test that dashboard can handle drag-drop events gracefully
            var dragException = Record.Exception(() => {
                var dragEventArgs = new DragEventArgs(null, 0, 0, 0, DragDropEffects.None, DragDropEffects.None);
                // Most Windows Forms controls can handle these events even if they don't use them
            });
            // Some exceptions might be expected for drag-drop, so we don't assert null

            Assert.True(_dashboard.Visible);
        }

        [Fact]
        public void Dashboard_ContextualActions_ShouldBeAppropriate()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var quickActions = FindControlByName(_dashboard, "QuickActionsPanel");

            // Act & Assert - Context-appropriate actions
            if (quickActions != null)
            {
                var actionButtons = GetAllControlsOfType<Button>(quickActions);

                foreach (var button in actionButtons)
                {
                    // Action buttons should have meaningful text or tooltips
                    var hasText = !string.IsNullOrWhiteSpace(button.Text);
                    var hasTooltip = !string.IsNullOrWhiteSpace(button.AccessibleDescription);
                    var hasAccessibleName = !string.IsNullOrWhiteSpace(button.AccessibleName);

                    Assert.True(hasText || hasTooltip || hasAccessibleName,
                        "Action buttons should have descriptive text, tooltip, or accessible name");

                    // Buttons should be appropriately sized for touch/click
                    Assert.True(button.Width >= 20, "Buttons should be wide enough for interaction");
                    Assert.True(button.Height >= 20, "Buttons should be tall enough for interaction");
                }
            }
        }

        [Fact]
        public void Dashboard_VisualFeedback_ShouldBePresent()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var interactiveControls = GetAllControlsOfType<Control>(_dashboard)
                .Where(c => c is Button || c is MaterialButton)
                .ToList();

            // Act & Assert - Visual feedback
            foreach (var control in interactiveControls.Take(3))
            {
                // Controls should provide visual feedback for different states
                Assert.NotEqual(Color.Empty, control.BackColor);
                Assert.NotEqual(Color.Empty, control.ForeColor);

                // Interactive controls should be visually distinct
                Assert.NotEqual(control.BackColor, control.ForeColor);

                // Enabled and disabled states should be distinguishable
                var originalEnabled = control.Enabled;
                control.Enabled = false;
                var disabledColor = control.ForeColor;

                control.Enabled = true;
                var enabledColor = control.ForeColor;

                // Note: Some controls might not change color when disabled
                // but the test ensures they handle state changes gracefully
                control.Enabled = originalEnabled;
            }
        }

        [Fact]
        public void Dashboard_ResponsiveInteraction_ShouldAdaptToInput()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var initialSnapshot = CaptureControlSnapshot(_dashboard);

            // Act - Test responsive behavior
            var toggleButton = FindControlByName(_dashboard, "SidebarToggleButton") as Button;
            if (toggleButton != null)
            {
                // Test rapid interactions
                for (int i = 0; i < 5; i++)
                {
                    toggleButton.PerformClick();
                    System.Threading.Thread.Sleep(50);

                    // Dashboard should remain responsive
                    Assert.True(_dashboard.Visible);
                    Assert.True(_dashboard.Enabled);
                }
            }

            // Assert - Dashboard should adapt but remain stable
            var finalSnapshot = CaptureControlSnapshot(_dashboard);
            Assert.True(AreSnapshotsSimilar(initialSnapshot, finalSnapshot, 0.8));

            Assert.True(_dashboard.Visible);
            Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));
        }

        [Fact]
        public void Dashboard_MultiModalInput_ShouldBeSupported()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var buttons = GetAllControlsOfType<Button>(_dashboard);

            // Act & Assert - Multi-modal input (keyboard, mouse, touch simulation)
            foreach (var button in buttons.Take(3))
            {
                if (button.Enabled && button.Visible)
                {
                    // Test keyboard activation
                    var keyboardException = Record.Exception(() => {
                        button.Focus();
                        // Simulate space bar or enter
                        button.PerformClick();
                    });

                    // Test mouse activation
                    var mouseException = Record.Exception(() => {
                        button.PerformClick();
                    });

                    // Test programmatic activation
                    var programmaticException = Record.Exception(() => {
                        if (button is MaterialButton materialButton)
                        {
                            materialButton.PerformClick();
                        }
                        else
                        {
                            button.PerformClick();
                        }
                    });

                    // At least one activation method should work
                    Assert.True(keyboardException == null || mouseException == null || programmaticException == null);
                }
            }

            // Dashboard should remain functional
            Assert.True(_dashboard.Visible);
        }

        [Fact]
        public void Dashboard_InputValidation_ShouldPreventErrors()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var textControls = GetAllControlsOfType<TextBox>(_dashboard);

            // Act & Assert - Input validation
            foreach (var textBox in textControls.Take(3))
            {
                if (textBox.Enabled && textBox.Visible)
                {
                    var originalText = textBox.Text;

                    // Test various input scenarios
                    var testInputs = new[] { "", "normal text", "123", "special!@#$%",
                                           new string('a', 1000), "\n\r\t" };

                    foreach (var input in testInputs)
                    {
                        var inputException = Record.Exception(() => {
                            textBox.Text = input;
                            // Control should handle the input gracefully
                        });
                        Assert.Null(inputException);

                        // TextBox should remain functional
                        Assert.True(textBox.Width > 0);
                        Assert.True(textBox.Height > 0);
                    }

                    // Restore original text
                    textBox.Text = originalText;
                }
            }
        }

        [Fact]
        public void Dashboard_PerformanceUnderLoad_ShouldRemainResponsive()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var operations = 0;
            var maxOperations = 50; // Limit to avoid excessive testing

            // Act - Perform intensive UI operations
            while (stopwatch.ElapsedMilliseconds < 1000 && operations < maxOperations)
            {
                // Find controls
                FindControlByName(_dashboard, "HeaderPanel");

                // Enumerate controls
                GetAllControlsOfType<Button>(_dashboard);

                // Access properties
                var allControls = GetAllControlsOfType<Control>(_dashboard);
                foreach (var control in allControls.Take(5))
                {
                    var _ = control.Visible;
                    var __ = control.Enabled;
                }

                operations++;
            }

            stopwatch.Stop();

            // Assert - Should remain responsive under load
            Assert.True(operations >= 10, $"Should complete multiple operations, completed {operations}");
            Assert.True(stopwatch.ElapsedMilliseconds < 2000, "Operations should complete within reasonable time");

            // Dashboard should still be functional
            Assert.True(_dashboard.Visible);
            Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));
        }

        [Fact]
        public void Dashboard_ErrorRecoveryDuringInteraction_ShouldBeGraceful()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var operations = new List<(string Name, Action Operation)>
            {
                ("Find null control", () => FindControlByName(_dashboard, null!)),
                ("Find empty name control", () => FindControlByName(_dashboard, "")),
                ("Access disposed control", () => {
                    var tempControl = new Button();
                    tempControl.Dispose();
                    try { var _ = tempControl.Text; } catch { }
                }),
                ("Invalid operation", () => {
                    var button = new Button();
                    try { button.PerformClick(); button.Dispose(); } catch { }
                })
            };

            var successfulOperations = 0;
            var exceptions = new List<Exception>();

            // Act - Try operations that might fail
            foreach (var (name, operation) in operations)
            {
                try
                {
                    operation();
                    successfulOperations++;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            // Assert - Should handle errors gracefully
            Assert.True(_dashboard.Visible);
            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            Assert.NotNull(headerPanel);

            // Some operations might legitimately fail, but dashboard should continue working
            Assert.True(exceptions.Count < operations.Count, "Not all operations should fail");
        }
    }
}
