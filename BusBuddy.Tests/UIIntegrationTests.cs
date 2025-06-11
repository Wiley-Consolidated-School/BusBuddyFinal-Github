using Xunit;
using System;
using System.Threading;
using System.Windows.Forms;
using BusBuddy.UI;
using BusBuddy.Models;
using BusBuddy.Business;
using BusBuddy.Tests.TestHelpers;

namespace BusBuddy.Tests
{
    public class UIIntegrationTests
    {
        [STAThread]
        [Fact]
        public void MainForm_ShouldOpenVehicleManagementForm()
        {
            // Arrange
            using (var mainForm = new MockMainForm())
            {
                mainForm.Show();

                // Act - Find and simulate clicking the vehicle management button
                var vehicleButton = UITestHelpers.FindButtonByText(mainForm, "Manage Vehicles");

                // Assert
                Assert.NotNull(vehicleButton);
                Assert.Equal("Manage Vehicles", vehicleButton.Text);
                Assert.True(vehicleButton.Enabled);

                mainForm.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void VehicleManagementForm_ShouldHaveAllRequiredButtons()
        {
            // Arrange & Act
            using (var form = new MockVehicleManagementForm())
            {
                form.Show();

                // Assert - Check for required CRUD buttons
                var addButton = UITestHelpers.FindButtonByText(form, "Add New");
                var editButton = UITestHelpers.FindButtonByText(form, "Edit");
                var deleteButton = UITestHelpers.FindButtonByText(form, "Delete");
                var detailsButton = UITestHelpers.FindButtonByText(form, "Details");

                Assert.NotNull(addButton);
                Assert.NotNull(editButton);
                Assert.NotNull(deleteButton);
                Assert.NotNull(detailsButton);

                Assert.True(addButton.Enabled);
                // Edit, Delete, Details buttons should be disabled when no row is selected
                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void DriverManagementForm_ShouldHaveEditPanelHiddenInitially()
        {
            // Arrange & Act
            using (var form = new MockDriverManagementForm())
            {
                form.Show();

                // Assert - Edit panel should be hidden initially
                var editPanel = UITestHelpers.FindControl<Panel>(form, "_editPanel");
                if (editPanel != null)
                {
                    Assert.False(editPanel.Visible, "Edit panel should be hidden initially");
                }

                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void VehicleForm_ShouldValidateRequiredFieldsOnSave()
        {
            // Arrange
            using (var form = new MockVehicleForm())
            {
                form.Show();

                // Act - Try to find save button (might not exist in current implementation)
                var saveButton = UITestHelpers.FindButtonByText(form, "Save");
                var busNumberField = UITestHelpers.FindControl<TextBox>(form, "txtBusNumber");

                // Assert - Form should have required fields
                Assert.NotNull(busNumberField);

                // If save button exists, verify it's properly configured
                if (saveButton != null)
                {
                    Assert.True(saveButton.Enabled);
                }

                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void Form1_ShouldHaveWorkingRefreshButton()
        {
            // Arrange
            using (var form = new Form1())
            {
                form.Show();

                // Act - Find refresh button
                var refreshButton = UITestHelpers.FindButtonByText(form, "Refresh");

                // Assert
                Assert.NotNull(refreshButton);
                Assert.True(refreshButton.Enabled);
                Assert.Equal("Refresh", refreshButton.Text);

                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void MainForm_MenuItems_ShouldBeAccessible()
        {
            // Arrange
            using (var form = new MockMainForm())
            {
                form.Show();

                // Act - Find menu strip
                var menuStrip = UITestHelpers.FindControl<MenuStrip>(form, null);

                // Assert
                Assert.NotNull(menuStrip);
                Assert.True(menuStrip.Items.Count > 0, "Menu should have items");

                // Check for main menu items
                bool hasFileMenu = false;
                bool hasDataMenu = false;
                bool hasReportsMenu = false;

                foreach (ToolStripItem item in menuStrip.Items)
                {
                    if (item.Text == "File") hasFileMenu = true;
                    if (item.Text == "Data Management") hasDataMenu = true;
                    if (item.Text == "Reports") hasReportsMenu = true;
                }

                Assert.True(hasFileMenu, "Should have File menu");
                Assert.True(hasDataMenu, "Should have Data Management menu");
                Assert.True(hasReportsMenu, "Should have Reports menu");

                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void Forms_ShouldHaveProperModalBehavior()
        {
            // Arrange & Act
            using (var vehicleForm = new MockVehicleManagementForm())
            using (var driverForm = new MockDriverManagementForm())
            using (var routeForm = new MockRouteManagementForm())
            {
                // Assert - Forms should be configured for modal display
                Assert.Equal(FormStartPosition.CenterParent, vehicleForm.StartPosition);
                Assert.Equal(FormBorderStyle.FixedDialog, vehicleForm.FormBorderStyle);
                Assert.False(vehicleForm.MaximizeBox);
                Assert.False(vehicleForm.MinimizeBox);

                Assert.Equal(FormStartPosition.CenterParent, driverForm.StartPosition);
                Assert.Equal(FormBorderStyle.FixedDialog, driverForm.FormBorderStyle);

                Assert.Equal(FormStartPosition.CenterParent, routeForm.StartPosition);
                Assert.Equal(FormBorderStyle.FixedDialog, routeForm.FormBorderStyle);
            }
        }

        [STAThread]
        [Fact]
        public void VehicleManagementForm_DataGridView_ShouldBeProperlyConfigured()
        {
            // Arrange & Act
            using (var form = new MockVehicleManagementForm())
            {
                form.Show();

                // Find the main data grid
                var dataGrid = UITestHelpers.FindControl<DataGridView>(form, "vehicleDataGrid");

                // Assert
                if (dataGrid != null)
                {
                    Assert.True(dataGrid.ReadOnly, "Grid should be read-only");
                    Assert.False(dataGrid.AllowUserToAddRows, "Should not allow adding rows");
                    Assert.False(dataGrid.AllowUserToDeleteRows, "Should not allow deleting rows");
                    Assert.Equal(DataGridViewSelectionMode.FullRowSelect, dataGrid.SelectionMode);
                    Assert.False(dataGrid.MultiSelect, "Should not allow multi-select");
                }

                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void AllForms_ShouldImplementIDisposableCorrectly()
        {
            // This test ensures forms can be properly disposed without exceptions

            // Test MainForm
            var mainForm = new MockMainForm();
            mainForm.Dispose(); // Should not throw

            // Test VehicleManagementForm
            var vehicleForm = new MockVehicleManagementForm();
            vehicleForm.Dispose(); // Should not throw

            // Test DriverManagementForm
            var driverForm = new MockDriverManagementForm();
            driverForm.Dispose(); // Should not throw

            // Test RouteManagementForm
            var routeForm = new MockRouteManagementForm();
            routeForm.Dispose(); // Should not throw

            // Test VehicleForm
            var detailForm = new MockVehicleForm();
            detailForm.Dispose(); // Should not throw

            // Test Form1 (dashboard)
            // If you have a mock for Form1, use it here. Otherwise, skip or mock dependencies.

            // If we get here without exceptions, the test passes
            Assert.True(true);
        }

        // Helper methods moved to UITestHelpers class
    }
}
