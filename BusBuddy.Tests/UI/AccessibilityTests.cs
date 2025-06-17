using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Xunit;
using MaterialSkin.Controls;

namespace BusBuddy.Tests.UI
{
    [Collection("UI Tests")]
    public class AccessibilityTests : UITestBase
    {
        [Fact]
        public void Dashboard_ColorContrast_ShouldBeAccessible()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act & Assert - Check for good color contrast
            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            Assert.NotNull(headerPanel);

            // Verify background and foreground colors are different enough
            Assert.NotEqual(headerPanel.BackColor, headerPanel.ForeColor);

            // Check that we don't have white text on white background or similar issues
            var isLightBackground = IsLightColor(headerPanel.BackColor);
            var isLightForeground = IsLightColor(headerPanel.ForeColor);

            Assert.NotEqual(isLightBackground, isLightForeground);
        }

        [Fact]
        public void Dashboard_TextReadability_ShouldBeOptimal()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Find all text-containing controls
            var textControls = GetAllControlsOfType<Control>(_dashboard)
                .Where(c => !string.IsNullOrEmpty(c.Text) && c.Visible)
                .ToList();

            // Assert
            Assert.True(textControls.Count > 0, "Should have controls with text");

            foreach (var control in textControls)
            {
                // Text should not be empty or just whitespace
                Assert.False(string.IsNullOrWhiteSpace(control.Text),
                    $"Control '{control.Name}' should have meaningful text");

                // Font should be readable size (at least 8pt)
                if (control.Font != null)
                {
                    Assert.True(control.Font.Size >= 8,
                        $"Control '{control.Name}' font size should be at least 8pt for readability");
                }

                // Control should be large enough to contain its text
                if (control is Button || control is Label)
                {
                    Assert.True(control.Width > 20,
                        $"Text control '{control.Name}' should be wide enough for its content");
                    Assert.True(control.Height > 15,
                        $"Text control '{control.Name}' should be tall enough for its content");
                }
            }
        }

        [Fact]
        public void Dashboard_KeyboardNavigation_ShouldBeComplete()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Get all interactive controls
            var interactiveControls = GetAllControlsOfType<Control>(_dashboard)
                .Where(c => (c is Button || c is TextBox || c is ComboBox || c is CheckBox)
                           && c.Visible && c.Enabled)
                .ToList();

            // Assert
            foreach (var control in interactiveControls)
            {
                // Interactive controls should be keyboard accessible
                Assert.True(control.CanFocus,
                    $"Interactive control '{control.Name}' should be focusable");

                // Should have reasonable tab index
                Assert.True(control.TabIndex >= 0,
                    $"Control '{control.Name}' should have valid tab index");

                // Should be included in tab order (unless explicitly excluded)
                if (control is Button || control is TextBox)
                {
                    Assert.True(control.TabStop,
                        $"Primary control '{control.Name}' should be in tab order");
                }
            }
        }

        [Fact]
        public void Dashboard_ControlSizing_ShouldBeAppropriate()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Check button sizes for touch-friendliness
            var buttons = GetAllControlsOfType<Button>(_dashboard)
                .Where(b => b.Visible)
                .ToList();

            // Assert
            foreach (var button in buttons)
            {
                // Buttons should be large enough for easy clicking (minimum 32x32 for touch)
                Assert.True(button.Width >= 30,
                    $"Button '{button.Text}' should be at least 30px wide for accessibility");
                Assert.True(button.Height >= 25,
                    $"Button '{button.Text}' should be at least 25px tall for accessibility");

                // Buttons should not be excessively large (max reasonable size)
                Assert.True(button.Width <= 400,
                    $"Button '{button.Text}' should not be excessively wide");
                Assert.True(button.Height <= 100,
                    $"Button '{button.Text}' should not be excessively tall");
            }
        }

        [Fact]
        public void Dashboard_ControlSpacing_ShouldAllowEasyInteraction()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Find buttons in quick actions area
            var quickActionsPanel = FindControlByName(_dashboard, "QuickActionsFlowPanel");
            if (quickActionsPanel != null)
            {
                var buttons = GetAllControlsOfType<Button>(quickActionsPanel)
                    .Where(b => b.Visible)
                    .OrderBy(b => b.Left)
                    .ThenBy(b => b.Top)
                    .ToList();

                // Assert - Check spacing between adjacent buttons
                for (int i = 0; i < buttons.Count - 1; i++)
                {
                    var currentButton = buttons[i];
                    var nextButton = buttons[i + 1];

                    // Calculate distance between buttons
                    var horizontalDistance = Math.Abs(nextButton.Left - (currentButton.Left + currentButton.Width));
                    var verticalDistance = Math.Abs(nextButton.Top - (currentButton.Top + currentButton.Height));

                    // Buttons should have some spacing (at least 4px) to avoid accidental clicks
                    if (horizontalDistance < 50 && verticalDistance < 50) // If they're nearby
                    {
                        Assert.True(horizontalDistance >= 4 || verticalDistance >= 4,
                            $"Buttons '{currentButton.Text}' and '{nextButton.Text}' should have adequate spacing");
                    }
                }
            }
        }

        [Fact]
        public void Dashboard_VisualFeedback_ShouldBeProvided()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Check buttons for visual feedback properties
            var buttons = GetAllControlsOfType<Button>(_dashboard)
                .Where(b => b.Visible && b.Enabled)
                .ToList();

            // Assert
            foreach (var button in buttons)
            {
                // Buttons should change appearance when clicked/hovered
                // We can't test actual hover/click states in unit tests,
                // but we can verify the button has properties that support feedback

                Assert.True(button.Enabled, $"Button '{button.Text}' should be enabled to provide feedback");

                // Button should have visible border or styling to indicate it's clickable
                Assert.True(button.FlatStyle != FlatStyle.Flat || button.BackColor != SystemColors.Control,
                    $"Button '{button.Text}' should have visual styling to indicate it's interactive");
            }
        }

        [Fact]
        public void Dashboard_ErrorMessages_ShouldBeAccessible()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Look for any error display areas or labels
            var labels = GetAllControlsOfType<Label>(_dashboard)
                .Concat(GetAllControlsOfType<MaterialLabel>(_dashboard).Cast<Control>())
                .Where(l => l.Visible)
                .ToList();

            // Assert - If there are labels that might show errors
            foreach (var label in labels.Where(l => l.ForeColor == Color.Red ||
                                                   l.Text.ToLower().Contains("error") ||
                                                   l.Text.ToLower().Contains("warning")))
            {
                // Error messages should be clearly visible
                Assert.True(label.Font.Size >= 8, "Error messages should have readable font size");
                Assert.NotEqual(Color.White, label.ForeColor); // Should not be white (invisible on white background)
                Assert.True(label.Width > 50, "Error display should be wide enough to show messages");
            }
        }

        [Fact]
        public void Dashboard_HighDPI_ShouldScale()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act & Assert - Verify controls scale reasonably with DPI
            var currentDpi = _dashboard.DeviceDpi;

            // Standard DPI is 96, high DPI might be 120, 144, 192, etc.
            if (currentDpi > 96)
            {
                // Controls should scale proportionally
                var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
                if (headerPanel != null)
                {
                    var expectedMinHeight = (int)(80 * (currentDpi / 96.0)); // Scale expected height
                    Assert.True(headerPanel.Height >= expectedMinHeight * 0.8, // Allow some tolerance
                        $"Header panel should scale with DPI (current: {headerPanel.Height}, expected min: {expectedMinHeight})");
                }

                // Fonts should scale
                var buttons = GetAllControlsOfType<Button>(_dashboard);
                foreach (var button in buttons.Take(3)) // Check first few buttons
                {
                    if (button.Font != null)
                    {
                        Assert.True(button.Font.Size >= 7,
                            $"Button '{button.Text}' font should scale appropriately with high DPI");
                    }
                }
            }
        }

        [Fact]
        public void Dashboard_ScreenReader_ShouldHaveAccessibleNames()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Find controls that should have accessible names
            var importantControls = GetAllControlsOfType<Button>(_dashboard)
                .Cast<Control>()
                .Concat(GetAllControlsOfType<TextBox>(_dashboard))
                .Concat(GetAllControlsOfType<ComboBox>(_dashboard))
                .Where(c => c.Visible)
                .ToList();

            // Assert
            foreach (var control in importantControls)
            {
                // Controls should have meaningful text or accessible name
                var hasText = !string.IsNullOrWhiteSpace(control.Text);
                var hasAccessibleName = !string.IsNullOrWhiteSpace(control.AccessibleName);
                var hasName = !string.IsNullOrWhiteSpace(control.Name) && control.Name != control.GetType().Name;

                Assert.True(hasText || hasAccessibleName || hasName,
                    $"Control of type '{control.GetType().Name}' should have accessible text, name, or AccessibleName for screen readers");

                // AccessibleRole should be set appropriately
                if (control is Button)
                {
                    Assert.True(control.AccessibleRole == AccessibleRole.Default ||
                               control.AccessibleRole == AccessibleRole.PushButton,
                        $"Button '{control.Text}' should have appropriate AccessibleRole");
                }
            }
        }

        [Fact]
        public void Dashboard_LanguageSupport_ShouldHandleText()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Find all text-displaying controls
            var textControls = GetAllControlsOfType<Control>(_dashboard)
                .Where(c => !string.IsNullOrEmpty(c.Text))
                .ToList();

            // Assert
            foreach (var control in textControls)
            {
                // Text should not be hardcoded in a way that prevents localization
                // We check for some basic patterns that suggest good internationalization support

                // Text should not be excessively long (could indicate concatenated strings)
                Assert.True(control.Text.Length < 200,
                    $"Control '{control.Name}' text should be reasonable length for localization");

                // Controls should have enough space for text expansion (other languages might be longer)
                if (control is Button || control is Label)
                {
                    var textLength = control.Text.Length;
                    var estimatedNeededWidth = textLength * 8; // Rough estimate: 8 pixels per character

                    // Allow 50% extra space for text expansion in other languages
                    Assert.True(control.Width >= estimatedNeededWidth * 0.7,
                        $"Control '{control.Name}' should have adequate width for text localization");
                }
            }
        }

        private bool IsLightColor(Color color)
        {
            // Calculate perceived brightness using standard formula
            var brightness = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
            return brightness > 0.5;
        }
    }
}
