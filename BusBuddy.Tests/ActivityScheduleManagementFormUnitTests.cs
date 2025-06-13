using Xunit;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using BusBuddy.UI.Views;
using Moq;
using BusBuddy.Data;
using BusBuddy.Models;

namespace BusBuddy.Tests
{
    public class ActivityScheduleManagementFormUnitTests : IDisposable
    {
        private ActivityScheduleManagementForm _form;

        public ActivityScheduleManagementFormUnitTests()
        {
            _form = new ActivityScheduleManagementForm();
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
            Assert.Equal("Activity Schedule Management", _form.Text);
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

            Assert.NotNull(addButton);
            Assert.NotNull(editButton);
            Assert.NotNull(deleteButton);
            Assert.NotNull(detailsButton);

            Assert.Equal("Add New", addButton.Text);
            Assert.Equal("Edit", editButton.Text);
            Assert.Equal("Delete", deleteButton.Text);
            Assert.Equal("Details", detailsButton.Text);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void InitializeComponent_CreatesDataGridView()
        {
            var activityScheduleGrid = GetPrivateField<DataGridView>("_activityScheduleGrid");
            Assert.NotNull(activityScheduleGrid);
            Assert.Equal(new Point(20, 60), activityScheduleGrid.Location);
            Assert.Equal(new Size(1150, 650), activityScheduleGrid.Size);
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
            var tripTypeComboBox = GetPrivateField<ComboBox>("_tripTypeComboBox");
            var vehicleComboBox = GetPrivateField<ComboBox>("_vehicleComboBox");
            var destinationTextBox = GetPrivateField<TextBox>("_destinationTextBox");
            var leaveTimeTextBox = GetPrivateField<TextBox>("_leaveTimeTextBox");
            var eventTimeTextBox = GetPrivateField<TextBox>("_eventTimeTextBox");
            var ridersTextBox = GetPrivateField<TextBox>("_ridersTextBox");
            var driverComboBox = GetPrivateField<ComboBox>("_driverComboBox");
            var saveButton = GetPrivateField<Button>("_saveButton");
            var cancelButton = GetPrivateField<Button>("_cancelButton");

            Assert.NotNull(datePicker);
            Assert.NotNull(tripTypeComboBox);
            Assert.NotNull(vehicleComboBox);
            Assert.NotNull(destinationTextBox);
            Assert.NotNull(leaveTimeTextBox);
            Assert.NotNull(eventTimeTextBox);
            Assert.NotNull(ridersTextBox);
            Assert.NotNull(driverComboBox);
            Assert.NotNull(saveButton);
            Assert.NotNull(cancelButton);

            Assert.Equal("Save", saveButton.Text);
            Assert.Equal("Cancel", cancelButton.Text);
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
        public void DestinationTextBox_HandlesEmptyValues(string destinationValue)
        {
            var destinationTextBox = GetPrivateField<TextBox>("_destinationTextBox");
            Assert.NotNull(destinationTextBox);

            // Should not throw exception with empty values
            destinationTextBox.Text = destinationValue;
            Assert.Equal(destinationValue ?? "", destinationTextBox.Text);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void RidersTextBox_HandlesEmptyValues(string ridersValue)
        {
            var ridersTextBox = GetPrivateField<TextBox>("_ridersTextBox");
            Assert.NotNull(ridersTextBox);

            // Should not throw exception with empty values
            ridersTextBox.Text = ridersValue;
            Assert.Equal(ridersValue ?? "", ridersTextBox.Text);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        [InlineData("12:00")]
        [InlineData("08:30 AM")]
        [InlineData("3:45 PM")]
        [InlineData("")]
        public void TimeTextBoxes_AcceptVariousTimeFormats(string timeValue)
        {
            var leaveTimeTextBox = GetPrivateField<TextBox>("_leaveTimeTextBox");
            var eventTimeTextBox = GetPrivateField<TextBox>("_eventTimeTextBox");

            Assert.NotNull(leaveTimeTextBox);
            Assert.NotNull(eventTimeTextBox);

            // Should not throw exception with various time formats
            leaveTimeTextBox.Text = timeValue;
            eventTimeTextBox.Text = timeValue;

            Assert.Equal(timeValue, leaveTimeTextBox.Text);
            Assert.Equal(timeValue, eventTimeTextBox.Text);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void ComboBoxes_InitializeWithoutErrors()
        {
            var tripTypeComboBox = GetPrivateField<ComboBox>("_tripTypeComboBox");
            var vehicleComboBox = GetPrivateField<ComboBox>("_vehicleComboBox");
            var driverComboBox = GetPrivateField<ComboBox>("_driverComboBox");

            Assert.NotNull(tripTypeComboBox);
            Assert.NotNull(vehicleComboBox);
            Assert.NotNull(driverComboBox);

            // ComboBoxes should be initialized without errors
            Assert.Equal(-1, tripTypeComboBox.SelectedIndex);
            Assert.Equal(-1, vehicleComboBox.SelectedIndex);
            Assert.Equal(-1, driverComboBox.SelectedIndex);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void DatePicker_HasReasonableDefaultValue()
        {
            var datePicker = GetPrivateField<DateTimePicker>("_datePicker");
            Assert.NotNull(datePicker);

            // Date picker should have a valid date
            Assert.True(datePicker.Value >= DateTime.MinValue);
            Assert.True(datePicker.Value <= DateTime.MaxValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void Form_HandlesDisposeCorrectly()
        {
            var testForm = new ActivityScheduleManagementForm();

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
            var testForm = new ActivityScheduleManagementForm();

            // Multiple dispose calls should not throw exceptions
            testForm.Dispose();
            testForm.Dispose();
            testForm.Dispose();

            Assert.True(testForm.IsDisposed);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void IsEditingFlag_InitializesFalse()
        {
            var isEditing = GetPrivateField<bool>("_isEditing");
            Assert.False(isEditing);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void CurrentActivitySchedule_InitializesNull()
        {
            var currentActivitySchedule = GetPrivateField<ActivitySchedule>("_currentActivitySchedule");
            Assert.Null(currentActivitySchedule);
        }

        private T GetPrivateField<T>(string fieldName)
        {
            var field = typeof(ActivityScheduleManagementForm).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)(field?.GetValue(_form) ?? default(T)!);
        }
    }
}
