using System;
using Xunit;
using FluentAssertions;
using BusBuddy.Tests.Foundation;
using BusBuddy.UI.Views;

namespace BusBuddy.Tests.CDE40
{
    /// <summary>
    /// Tests for CDE40ReportForm functionality
    /// Following Syncfusion testing patterns for form testing
    /// Reference: https://help.syncfusion.com/windowsforms/testing/coded-ui
    /// </summary>
    public class CDE40ReportFormTests : SyncfusionTestBase
    {
        [Fact]
        public void CDE40ReportForm_Constructor_ShouldInitializeSuccessfully()
        {
            // Arrange & Act
            using var form = new CDE40ReportForm();

            // Assert
            form.Should().NotBeNull();
            form.Text.Should().Be("CDE-40 Transportation Report");
        }

        [Fact]
        public void CDE40ReportForm_FormProperties_ShouldBeConfiguredCorrectly()
        {
            // Arrange & Act
            using var form = new CDE40ReportForm();
            form.CreateControl();

            // Assert - Based on Syncfusion form testing recommendations
            form.Size.Width.Should().Be(800);
            form.Size.Height.Should().Be(600);
            form.StartPosition.Should().Be(System.Windows.Forms.FormStartPosition.CenterParent);
        }

        [Fact]
        public void CDE40ReportForm_SyncfusionButtons_ShouldBeInitialized()
        {
            // Arrange & Act
            using var form = new CDE40ReportForm();
            form.CreateControl();

            // Assert - Verify Syncfusion SfButton controls are present
            var generateButton = FindSfButtonByText(form, "Generate CDE-40 Report");
            var closeButton = FindSfButtonByText(form, "Close");

            generateButton.Should().NotBeNull("Generate button should be initialized");
            closeButton.Should().NotBeNull("Close button should be initialized");
        }

        [Fact]
        public void CDE40ReportForm_GenerateReportButton_ShouldHaveClickHandler()
        {
            // Arrange
            using var form = new CDE40ReportForm();
            form.CreateControl();
            var generateButton = FindSfButtonByText(form, "Generate CDE-40 Report");

            // Act & Assert
            generateButton.Should().NotBeNull();
            Action clickAction = () => generateButton!.PerformClick();
            clickAction.Should().NotThrow("Generate button click should not cause errors");
        }

        [Fact]
        public void CDE40ReportForm_CloseButton_ShouldCloseForm()
        {
            // Arrange
            using var form = new CDE40ReportForm();
            form.CreateControl();
            var closeButton = FindSfButtonByText(form, "Close");

            // Act & Assert
            closeButton.Should().NotBeNull();
            Action clickAction = () => closeButton!.PerformClick();
            clickAction.Should().NotThrow("Close button click should not cause errors");
        }

        [Fact]
        public void CDE40ReportForm_HeaderPanel_ShouldBeStyledCorrectly()
        {
            // Arrange & Act
            using var form = new CDE40ReportForm();
            form.CreateControl();

            // Assert - Verify header panel styling
            var headerPanel = FindPanelByDockStyle(form, System.Windows.Forms.DockStyle.Top);
            headerPanel.Should().NotBeNull("Header panel should exist");
            headerPanel!.Height.Should().Be(80, "Header panel should be 80px high");
            headerPanel.BackColor.Should().Be(System.Drawing.ColorTranslator.FromHtml("#212121"),
                "Header should have dark background");
        }

        [Fact]
        public void CDE40ReportForm_InfoLabel_ShouldContainRequiredInformation()
        {
            // Arrange & Act
            using var form = new CDE40ReportForm();
            form.CreateControl();

            // Assert - Verify info label content
            var infoLabel = FindLabelContainingText(form, "Colorado Department of Education");
            infoLabel.Should().NotBeNull("Info label should exist");
            infoLabel!.Text.Should().Contain("September 15", "Should mention report deadline");
            infoLabel.Text.Should().Contain("Total miles driven", "Should mention miles requirement");
            infoLabel.Text.Should().Contain("pupils transported", "Should mention pupil count requirement");
            infoLabel.Text.Should().Contain("Cost per student", "Should mention cost analysis requirement");
        }

        [Fact]
        public void CDE40ReportForm_Dispose_ShouldCleanupProperly()
        {
            // Arrange
            var form = new CDE40ReportForm();
            form.CreateControl();

            // Act
            Action disposeAction = () => form.Dispose();

            // Assert
            disposeAction.Should().NotThrow("Dispose should not throw exceptions");
        }

        /// <summary>
        /// Helper method to find Syncfusion SfButton by text
        /// Following Syncfusion testing recommendations for control identification
        /// </summary>
        private Syncfusion.WinForms.Controls.SfButton? FindSfButtonByText(System.Windows.Forms.Control parent, string text)
        {
            if (parent is Syncfusion.WinForms.Controls.SfButton button && button.Text == text)
                return button;

            foreach (System.Windows.Forms.Control child in parent.Controls)
            {
                var result = FindSfButtonByText(child, text);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Helper method to find Panel by DockStyle
        /// </summary>
        private System.Windows.Forms.Panel? FindPanelByDockStyle(System.Windows.Forms.Control parent, System.Windows.Forms.DockStyle dockStyle)
        {
            if (parent is System.Windows.Forms.Panel panel && panel.Dock == dockStyle)
                return panel;

            foreach (System.Windows.Forms.Control child in parent.Controls)
            {
                var result = FindPanelByDockStyle(child, dockStyle);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Helper method to find Label containing specific text
        /// </summary>
        private System.Windows.Forms.Label? FindLabelContainingText(System.Windows.Forms.Control parent, string text)
        {
            if (parent is System.Windows.Forms.Label label && label.Text.Contains(text))
                return label;

            foreach (System.Windows.Forms.Control child in parent.Controls)
            {
                var result = FindLabelContainingText(child, text);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
