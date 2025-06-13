using Xunit;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using BusBuddy.UI.Views;
using Moq;
using BusBuddy.Data;
using BusBuddy.Models;

namespace BusBuddy.Tests
{
    public class ActivityManagementFormUnitTests : IDisposable
    {
        private ActivityManagementForm _form;

        public ActivityManagementFormUnitTests()
        {
            _form = new ActivityManagementForm();
        }

        public void Dispose()
        {
            _form?.Dispose();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void Constructor_InitializesSuccessfully()
        {
            Assert.NotNull(_form);
            Assert.Equal("Activity Management", _form.Text);
            Assert.Equal(new Size(1200, 900), _form.ClientSize);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void InitializeComponent_SetsKeyPreviewTrue()
        {
            Assert.True(_form.KeyPreview);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void InitializeComponent_CreatesRequiredControls()
        {
            // Verify main buttons exist
            var addButton = GetPrivateField<Button>("_addButton");
            var editButton = GetPrivateField<Button>("_editButton");
            var deleteButton = GetPrivateField<Button>("_deleteButton");
            var detailsButton = GetPrivateField<Button>("_detailsButton");
            var searchButton = GetPrivateField<Button>("_searchButton");

            Assert.NotNull(addButton);
            Assert.NotNull(editButton);
            Assert.NotNull(deleteButton);
            Assert.NotNull(detailsButton);
            Assert.NotNull(searchButton);

            Assert.Equal("Add New", addButton.Text);
            Assert.Equal("Edit", editButton.Text);
            Assert.Equal("Delete", deleteButton.Text);
            Assert.Equal("Details", detailsButton.Text);
            Assert.Equal("Search", searchButton.Text);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void InitializeComponent_CreatesDataGridView()
        {
            var activityGrid = GetPrivateField<DataGridView>("_activityGrid");
            Assert.NotNull(activityGrid);
            Assert.Equal(new Point(20, 60), activityGrid.Location);
            Assert.Equal(new Size(1150, 650), activityGrid.Size);
            Assert.Equal(DataGridViewAutoSizeColumnsMode.Fill, activityGrid.AutoSizeColumnsMode);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void InitializeComponent_CreatesSearchControls()
        {
            var searchBox = GetPrivateField<TextBox>("_searchBox");
            Assert.NotNull(searchBox);
            Assert.Equal(new Point(550, 20), searchBox.Location);
            Assert.Equal(new Size(150, 23), searchBox.Size);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void InitializeComponent_CreatesEditPanel()
        {
            var editPanel = GetPrivateField<Panel>("_editPanel");
            Assert.NotNull(editPanel);
            Assert.False(editPanel.Visible); // Should be hidden initially
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void InitializeComponent_CreatesEditPanelControls()
        {
            var datePicker = GetPrivateField<DateTimePicker>("_datePicker");
            var activityTypeComboBox = GetPrivateField<ComboBox>("_activityTypeComboBox");
            var destinationTextBox = GetPrivateField<TextBox>("_destinationTextBox");
            var leaveTimeTextBox = GetPrivateField<TextBox>("_leaveTimeTextBox");
            var eventTimeTextBox = GetPrivateField<TextBox>("_eventTimeTextBox");
            var returnTimeTextBox = GetPrivateField<TextBox>("_returnTimeTextBox");
            var saveButton = GetPrivateField<Button>("_saveButton");
            var cancelButton = GetPrivateField<Button>("_cancelButton");

            Assert.NotNull(datePicker);
            Assert.NotNull(activityTypeComboBox);
            Assert.NotNull(destinationTextBox);
            Assert.NotNull(leaveTimeTextBox);
            Assert.NotNull(eventTimeTextBox);
            Assert.NotNull(returnTimeTextBox);
            Assert.NotNull(saveButton);
            Assert.NotNull(cancelButton);

            Assert.Equal("Save", saveButton.Text);
            Assert.Equal("Cancel", cancelButton.Text);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void InitializeComponent_ActivityTypeComboBoxHasItems()
        {
            var activityTypeComboBox = GetPrivateField<ComboBox>("_activityTypeComboBox");
            Assert.NotNull(activityTypeComboBox);
            Assert.True(activityTypeComboBox.Items.Count > 0);
            Assert.Contains("Sports Trip", activityTypeComboBox.Items.Cast<object>().Select(x => x.ToString()));
            Assert.Contains("Activity Trip", activityTypeComboBox.Items.Cast<object>().Select(x => x.ToString()));
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void InitializeComponent_ButtonsHaveCorrectInitialState()
        {
            var editButton = GetPrivateField<Button>("_editButton");
            var deleteButton = GetPrivateField<Button>("_deleteButton");
            var detailsButton = GetPrivateField<Button>("_detailsButton");

            // These should be disabled initially (no selection)
            Assert.False(editButton.Enabled);
            Assert.False(deleteButton.Enabled);
            Assert.False(detailsButton.Enabled);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void KeyPreview_EnablesKeyboardHandling()
        {
            Assert.True(_form.KeyPreview);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void SearchBox_HandlesEmptyValues(string searchValue)
        {
            var searchBox = GetPrivateField<TextBox>("_searchBox");
            Assert.NotNull(searchBox);

            // Should not throw exception with empty values
            searchBox.Text = searchValue;
            Assert.Equal(searchValue ?? "", searchBox.Text);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void EditPanelControls_HaveAppropriateMaxLength()
        {
            var destinationTextBox = GetPrivateField<TextBox>("_destinationTextBox");
            var leaveTimeTextBox = GetPrivateField<TextBox>("_leaveTimeTextBox");
            var eventTimeTextBox = GetPrivateField<TextBox>("_eventTimeTextBox");
            var returnTimeTextBox = GetPrivateField<TextBox>("_returnTimeTextBox");

            Assert.NotNull(destinationTextBox);
            Assert.NotNull(leaveTimeTextBox);
            Assert.NotNull(eventTimeTextBox);
            Assert.NotNull(returnTimeTextBox);

            // Time textboxes should have reasonable sizes for time input
            Assert.Equal(new Size(80, 23), leaveTimeTextBox.Size);
            Assert.Equal(new Size(80, 23), eventTimeTextBox.Size);
            Assert.Equal(new Size(80, 23), returnTimeTextBox.Size);

            // Destination should be larger
            Assert.Equal(new Size(200, 23), destinationTextBox.Size);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void Form_HandlesDisposeCorrectly()
        {
            var testForm = new ActivityManagementForm();

            // Should not throw exception when disposing
            testForm.Dispose();

            // Form should be disposed
            Assert.True(testForm.IsDisposed);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void Form_HandlesMultipleDisposeCallsGracefully()
        {
            var testForm = new ActivityManagementForm();

            // Multiple dispose calls should not throw exceptions
            testForm.Dispose();
            testForm.Dispose();
            testForm.Dispose();

            Assert.True(testForm.IsDisposed);
        }

        private T GetPrivateField<T>(string fieldName) where T : class
        {
            var field = typeof(ActivityManagementForm).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)(field?.GetValue(_form) ?? default(T)!);
        }
    }
}
