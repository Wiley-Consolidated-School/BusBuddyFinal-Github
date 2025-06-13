using System;
using System.Linq;
using System.Windows.Forms;
using BusBuddy;
using BusBuddy.Models;
using Xunit;

namespace BusBuddy.Tests.Improved
{
    /// <summary>
    /// Improved MainForm tests following XUnit best practices
    /// Demonstrates proper AAA pattern, single responsibility, and clear assertions
    /// </summary>
    public class MainFormImprovedTests : IDisposable
    {
        private MainForm? _mainForm;

        #region Constructor Tests

        [Fact]
        public void MainForm_Constructor_CreatesInstanceSuccessfully()
        {
            // Arrange - No setup needed for constructor test

            // Act
            _mainForm = new MainForm();

            // Assert
            Assert.NotNull(_mainForm);
        }

        [Fact]
        public void MainForm_Constructor_SetsWindowTitle()
        {
            // Arrange
            const string expectedTitle = "BusBuddy - Bus Tracking Companion";

            // Act
            _mainForm = new MainForm();

            // Assert
            Assert.Equal(expectedTitle, _mainForm.Text);
        }

        [Fact]
        public void MainForm_Constructor_SetsWindowSize()
        {
            // Arrange
            const int expectedWidth = 1024;
            const int expectedHeight = 768;

            // Act
            _mainForm = new MainForm();

            // Assert
            Assert.Equal(expectedWidth, _mainForm.Size.Width);
            Assert.Equal(expectedHeight, _mainForm.Size.Height);
        }

        [Fact]
        public void MainForm_Constructor_SetsStartPosition()
        {
            // Arrange
            const FormStartPosition expectedPosition = FormStartPosition.CenterScreen;

            // Act
            _mainForm = new MainForm();

            // Assert
            Assert.Equal(expectedPosition, _mainForm.StartPosition);
        }

        #endregion

        #region Menu Tests

        [Fact]
        public void MainForm_Constructor_CreatesMenuStrip()
        {
            // Arrange - No setup needed

            // Act
            _mainForm = new MainForm();

            // Assert
            Assert.NotNull(_mainForm.MainMenuStrip);
        }

        [Fact]
        public void MainForm_Constructor_AddsMenuStripToControls()
        {
            // Arrange - No setup needed

            // Act
            _mainForm = new MainForm();

            // Assert
            Assert.Contains(_mainForm.MainMenuStrip, _mainForm.Controls.Cast<Control>());
        }

        [Fact]
        public void MainForm_Constructor_CreatesFileMenu()
        {
            // Arrange
            const string expectedMenuText = "File";

            // Act
            _mainForm = new MainForm();

            // Assert
            var fileMenu = _mainForm.MainMenuStrip?.Items.Cast<ToolStripMenuItem>()
                .FirstOrDefault(item => item.Text == expectedMenuText);
            Assert.NotNull(fileMenu);
        }

        [Fact]
        public void MainForm_Constructor_CreatesDataManagementMenu()
        {
            // Arrange
            const string expectedMenuText = "Data Management";

            // Act
            _mainForm = new MainForm();

            // Assert
            var dataMenu = _mainForm.MainMenuStrip?.Items.Cast<ToolStripMenuItem>()
                .FirstOrDefault(item => item.Text == expectedMenuText);
            Assert.NotNull(dataMenu);
        }

        [Fact]
        public void MainForm_Constructor_CreatesReportsMenu()
        {
            // Arrange
            const string expectedMenuText = "Reports";

            // Act
            _mainForm = new MainForm();

            // Assert
            var reportsMenu = _mainForm.MainMenuStrip?.Items.Cast<ToolStripMenuItem>()
                .FirstOrDefault(item => item.Text == expectedMenuText);
            Assert.NotNull(reportsMenu);
        }

        #endregion

        #region Dashboard Panel Tests

        [Fact]
        public void MainForm_Constructor_CreatesDashboardPanel()
        {
            // Arrange - No setup needed

            // Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanel = GetDashboardPanel(_mainForm);
            Assert.NotNull(dashboardPanel);
        }

        [Fact]
        public void MainForm_Constructor_SetsDashboardPanelBorder()
        {
            // Arrange
            const BorderStyle expectedBorderStyle = BorderStyle.FixedSingle;

            // Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanel = GetDashboardPanel(_mainForm);
            Assert.Equal(expectedBorderStyle, dashboardPanel?.BorderStyle);
        }

        [Fact]
        public void MainForm_Constructor_EnablesDashboardPanelAutoScroll()
        {
            // Arrange - No setup needed

            // Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanel = GetDashboardPanel(_mainForm);
            Assert.True(dashboardPanel?.AutoScroll);
        }

        [Fact]
        public void MainForm_Constructor_SetsDashboardPanelAnchors()
        {
            // Arrange
            const AnchorStyles expectedAnchors = AnchorStyles.Top | AnchorStyles.Left |
                                                 AnchorStyles.Right | AnchorStyles.Bottom;

            // Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanel = GetDashboardPanel(_mainForm);
            Assert.Equal(expectedAnchors, dashboardPanel?.Anchor);
        }

        #endregion

        #region Dashboard Title Tests

        [Fact]
        public void MainForm_Constructor_CreatesDashboardTitle()
        {
            // Arrange
            const string expectedTitleText = "Dashboard - All Views";

            // Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanel = GetDashboardPanel(_mainForm);
            var titleLabel = dashboardPanel?.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Text == expectedTitleText);
            Assert.NotNull(titleLabel);
        }

        [Fact]
        public void MainForm_Constructor_SetsDashboardTitleFont()
        {
            // Arrange
            const string expectedFontName = "Microsoft Sans Serif";
            const float expectedFontSize = 16F;

            // Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanel = GetDashboardPanel(_mainForm);
            var titleLabel = dashboardPanel?.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Text.Contains("Dashboard"));

            Assert.NotNull(titleLabel);
            Assert.Equal(expectedFontName, titleLabel.Font.Name);
            Assert.Equal(expectedFontSize, titleLabel.Font.Size);
            Assert.True(titleLabel.Font.Bold);
        }

        #endregion

        #region Category Label Tests

        [Fact]
        public void MainForm_Constructor_CreatesFleetManagementCategory()
        {
            // Arrange
            const string expectedCategoryName = "Fleet Management";

            // Act
            _mainForm = new MainForm();

            // Assert
            var categoryLabel = GetCategoryLabel(_mainForm, expectedCategoryName);
            Assert.NotNull(categoryLabel);
        }

        [Fact]
        public void MainForm_Constructor_CreatesPersonnelManagementCategory()
        {
            // Arrange
            const string expectedCategoryName = "Personnel Management";

            // Act
            _mainForm = new MainForm();

            // Assert
            var categoryLabel = GetCategoryLabel(_mainForm, expectedCategoryName);
            Assert.NotNull(categoryLabel);
        }

        [Fact]
        public void MainForm_Constructor_CreatesOperationsManagementCategory()
        {
            // Arrange
            const string expectedCategoryName = "Operations Management";

            // Act
            _mainForm = new MainForm();

            // Assert
            var categoryLabel = GetCategoryLabel(_mainForm, expectedCategoryName);
            Assert.NotNull(categoryLabel);
        }

        [Fact]
        public void MainForm_Constructor_CreatesAdministrativeCategory()
        {
            // Arrange
            const string expectedCategoryName = "Administrative";

            // Act
            _mainForm = new MainForm();

            // Assert
            var categoryLabel = GetCategoryLabel(_mainForm, expectedCategoryName);
            Assert.NotNull(categoryLabel);
        }

        #endregion

        #region View Button Tests

        [Theory]
        [InlineData("Vehicle Management")]
        [InlineData("Maintenance Management")]
        [InlineData("Fuel Management")]
        public void MainForm_Constructor_CreatesFleetManagementButtons(string buttonText)
        {
            // Arrange - buttonText provided by theory

            // Act
            _mainForm = new MainForm();

            // Assert
            var button = GetViewButton(_mainForm, buttonText);
            Assert.NotNull(button);
        }

        [Theory]
        [InlineData("Driver Management")]
        [InlineData("Time Card Management")]
        public void MainForm_Constructor_CreatesPersonnelManagementButtons(string buttonText)
        {
            // Arrange - buttonText provided by theory

            // Act
            _mainForm = new MainForm();

            // Assert
            var button = GetViewButton(_mainForm, buttonText);
            Assert.NotNull(button);
        }

        [Theory]
        [InlineData("Route Management")]
        [InlineData("Activity Management")]
        [InlineData("Activity Schedule Management")]
        public void MainForm_Constructor_CreatesOperationsManagementButtons(string buttonText)
        {
            // Arrange - buttonText provided by theory

            // Act
            _mainForm = new MainForm();

            // Assert
            var button = GetViewButton(_mainForm, buttonText);
            Assert.NotNull(button);
        }

        [Fact]
        public void MainForm_Constructor_CreatesSchoolCalendarButton()
        {
            // Arrange
            const string expectedButtonText = "School Calendar Management";

            // Act
            _mainForm = new MainForm();

            // Assert
            var button = GetViewButton(_mainForm, expectedButtonText);
            Assert.NotNull(button);
        }

        [Fact]
        public void MainForm_Constructor_CreatesCorrectNumberOfViewButtons()
        {
            // Arrange
            const int expectedButtonCount = 9;

            // Act
            _mainForm = new MainForm();

            // Assert
            var dashboardPanel = GetDashboardPanel(_mainForm);
            var viewButtons = dashboardPanel?.Controls.OfType<Button>()
                .Where(b => b.BackColor == System.Drawing.Color.LightSteelBlue)
                .ToList();

            Assert.Equal(expectedButtonCount, viewButtons?.Count);
        }

        #endregion

        #region Button Properties Tests

        [Fact]
        public void MainForm_Constructor_SetsViewButtonSize()
        {
            // Arrange
            const int expectedWidth = 180;
            const int expectedHeight = 50;

            // Act
            _mainForm = new MainForm();

            // Assert
            var button = GetFirstViewButton(_mainForm);
            Assert.NotNull(button);
            Assert.Equal(expectedWidth, button.Size.Width);
            Assert.Equal(expectedHeight, button.Size.Height);
        }

        [Fact]
        public void MainForm_Constructor_SetsViewButtonColors()
        {
            // Arrange
            var expectedBackColor = System.Drawing.Color.LightSteelBlue;
            var expectedForeColor = System.Drawing.Color.DarkBlue;

            // Act
            _mainForm = new MainForm();

            // Assert
            var button = GetFirstViewButton(_mainForm);
            Assert.NotNull(button);
            Assert.Equal(expectedBackColor, button.BackColor);
            Assert.Equal(expectedForeColor, button.ForeColor);
        }

        [Fact]
        public void MainForm_Constructor_SetsViewButtonFlatStyle()
        {
            // Arrange
            const FlatStyle expectedFlatStyle = FlatStyle.Flat;

            // Act
            _mainForm = new MainForm();

            // Assert
            var button = GetFirstViewButton(_mainForm);
            Assert.NotNull(button);
            Assert.Equal(expectedFlatStyle, button.FlatStyle);
        }

        #endregion

        #region Helper Methods

        private static Panel? GetDashboardPanel(Form form)
        {
            return form.Controls.OfType<Panel>()
                .FirstOrDefault(p => p.BorderStyle == BorderStyle.FixedSingle && p.AutoScroll);
        }

        private static Label? GetCategoryLabel(Form form, string categoryName)
        {
            var dashboardPanel = GetDashboardPanel(form);
            return dashboardPanel?.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Text == categoryName &&
                                   l.Font.Bold &&
                                   l.ForeColor == System.Drawing.Color.DarkBlue);
        }

        private static Button? GetViewButton(Form form, string buttonText)
        {
            var dashboardPanel = GetDashboardPanel(form);
            return dashboardPanel?.Controls.OfType<Button>()
                .FirstOrDefault(b => b.Text == buttonText &&
                                   b.BackColor == System.Drawing.Color.LightSteelBlue);
        }

        private static Button? GetFirstViewButton(Form form)
        {
            var dashboardPanel = GetDashboardPanel(form);
            return dashboardPanel?.Controls.OfType<Button>()
                .FirstOrDefault(b => b.BackColor == System.Drawing.Color.LightSteelBlue);
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            _mainForm?.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
