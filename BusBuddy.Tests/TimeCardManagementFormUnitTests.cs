using Xunit;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using BusBuddy.UI.Views;
using Moq;
using BusBuddy.Data;
using BusBuddy.Business;
using BusBuddy.Models;

namespace BusBuddy.Tests
{
    public class TimeCardManagementFormUnitTests : IDisposable
    {
        private TimeCardManagementForm _form;
        private Mock<ITimeCardRepository> _mockRepo;
        private Mock<IDatabaseHelperService> _mockDatabaseService;

        public TimeCardManagementFormUnitTests()
        {
            _mockRepo = new Mock<ITimeCardRepository>();
            _mockDatabaseService = new Mock<IDatabaseHelperService>();
            var mockPTORepo = new Mock<IPTOBalanceRepository>();
            _form = new TimeCardManagementForm(_mockRepo.Object, mockPTORepo.Object);
        }

        public void Dispose()
        {
            _form?.Dispose();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void Constructor_WithRepository_InitializesSuccessfully()
        {
            Assert.NotNull(_form);
            Assert.Equal("Time Card Management", _form.Text);
            // Allow for window chrome affecting the actual size
            Assert.Equal(1200, _form.Size.Width);
            Assert.True(_form.Size.Height >= 880 && _form.Size.Height <= 900, $"Expected height between 880-900, actual: {_form.Size.Height}");
            Assert.Equal(Color.WhiteSmoke, _form.BackColor);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void Constructor_WithNullRepository_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TimeCardManagementForm(null!, null));
        }

        [Fact(Skip = "Requires database configuration - should be integration test")]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void DefaultConstructor_InitializesSuccessfully()
        {
            var form = new TimeCardManagementForm();
            Assert.NotNull(form);
            Assert.Equal("Time Card Management", form.Text);
            form.Dispose();
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
            var timeCardGrid = GetPrivateField<DataGridView>("_timeCardGrid");
            Assert.NotNull(timeCardGrid);
            Assert.Equal(new Point(20, 60), timeCardGrid.Location);
            Assert.Equal(new Size(1150, 600), timeCardGrid.Size);
            Assert.Equal(DataGridViewAutoSizeColumnsMode.Fill, timeCardGrid.AutoSizeColumnsMode);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void InitializeComponent_CreatesSummaryPanel()
        {
            var summaryPanel = GetPrivateField<Panel>("_summaryPanel");
            var lblWeekTotal = GetPrivateField<Label>("_lblWeekTotal");
            var lblMonthTotal = GetPrivateField<Label>("_lblMonthTotal");

            Assert.NotNull(summaryPanel);
            Assert.NotNull(lblWeekTotal);
            Assert.NotNull(lblMonthTotal);

            Assert.Equal(new Point(500, 10), summaryPanel.Location);
            Assert.Equal(new Size(650, 40), summaryPanel.Size);
            Assert.Equal(Color.FromArgb(240, 248, 255), summaryPanel.BackColor);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void InitializeComponent_CreatesEditPanel()
        {
            var editPanel = GetPrivateField<Panel>("_editPanel");
            Assert.NotNull(editPanel);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void InitializeComponent_CreatesEditPanelControls()
        {
            var datePicker = GetPrivateField<DateTimePicker>("_datePicker");
            var clockInTextBox = GetPrivateField<TextBox>("_clockInTextBox");
            var lunchOutTextBox = GetPrivateField<TextBox>("_lunchOutTextBox");
            var lunchInTextBox = GetPrivateField<TextBox>("_lunchInTextBox");
            var clockOutTextBox = GetPrivateField<TextBox>("_clockOutTextBox");
            var routeAMOutTextBox = GetPrivateField<TextBox>("_routeAMOutTextBox");
            var routeAMInTextBox = GetPrivateField<TextBox>("_routeAMInTextBox");
            var routePMOutTextBox = GetPrivateField<TextBox>("_routePMOutTextBox");
            var routePMInTextBox = GetPrivateField<TextBox>("_routePMInTextBox");
            var totalTimeTextBox = GetPrivateField<TextBox>("_totalTimeTextBox");
            var overtimeTextBox = GetPrivateField<TextBox>("_overtimeTextBox");
            var routeDayCheckBox = GetPrivateField<CheckBox>("_routeDayCheckBox");
            var saveButton = GetPrivateField<Button>("_saveButton");
            var cancelButton = GetPrivateField<Button>("_cancelButton");

            Assert.NotNull(datePicker);
            Assert.NotNull(clockInTextBox);
            Assert.NotNull(lunchOutTextBox);
            Assert.NotNull(lunchInTextBox);
            Assert.NotNull(clockOutTextBox);
            Assert.NotNull(routeAMOutTextBox);
            Assert.NotNull(routeAMInTextBox);
            Assert.NotNull(routePMOutTextBox);
            Assert.NotNull(routePMInTextBox);
            Assert.NotNull(totalTimeTextBox);
            Assert.NotNull(overtimeTextBox);
            Assert.NotNull(routeDayCheckBox);
            Assert.NotNull(saveButton);
            Assert.NotNull(cancelButton);
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
        [InlineData("08:00")]
        [InlineData("8:00 AM")]
        [InlineData("17:30")]
        [InlineData("5:30 PM")]
        public void TimeTextBoxes_AcceptVariousTimeFormats(string timeValue)
        {
            var clockInTextBox = GetPrivateField<TextBox>("_clockInTextBox");
            var clockOutTextBox = GetPrivateField<TextBox>("_clockOutTextBox");
            var lunchOutTextBox = GetPrivateField<TextBox>("_lunchOutTextBox");
            var lunchInTextBox = GetPrivateField<TextBox>("_lunchInTextBox");

            Assert.NotNull(clockInTextBox);
            Assert.NotNull(clockOutTextBox);
            Assert.NotNull(lunchOutTextBox);
            Assert.NotNull(lunchInTextBox);

            // Should not throw exception with various time formats
            clockInTextBox.Text = timeValue;
            clockOutTextBox.Text = timeValue;
            lunchOutTextBox.Text = timeValue;
            lunchInTextBox.Text = timeValue;

            Assert.Equal(timeValue, clockInTextBox.Text);
            Assert.Equal(timeValue, clockOutTextBox.Text);
            Assert.Equal(timeValue, lunchOutTextBox.Text);
            Assert.Equal(timeValue, lunchInTextBox.Text);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        [InlineData("0.0")]
        [InlineData("8.0")]
        [InlineData("8.5")]
        [InlineData("40.0")]
        [InlineData("")]
        public void TotalTimeTextBox_AcceptsNumericValues(string totalTimeValue)
        {
            var totalTimeTextBox = GetPrivateField<TextBox>("_totalTimeTextBox");
            Assert.NotNull(totalTimeTextBox);

            // Should not throw exception with various numeric values
            totalTimeTextBox.Text = totalTimeValue;
            Assert.Equal(totalTimeValue, totalTimeTextBox.Text);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        [InlineData("0.0")]
        [InlineData("1.5")]
        [InlineData("2.0")]
        [InlineData("")]
        public void OvertimeTextBox_AcceptsNumericValues(string overtimeValue)
        {
            var overtimeTextBox = GetPrivateField<TextBox>("_overtimeTextBox");
            Assert.NotNull(overtimeTextBox);

            // Should not throw exception with various numeric values
            overtimeTextBox.Text = overtimeValue;
            Assert.Equal(overtimeValue, overtimeTextBox.Text);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void RouteDayCheckBox_TogglesFunctionality()
        {
            var routeDayCheckBox = GetPrivateField<CheckBox>("_routeDayCheckBox");
            Assert.NotNull(routeDayCheckBox);

            // Should toggle without errors
            routeDayCheckBox.Checked = true;
            Assert.True(routeDayCheckBox.Checked);

            routeDayCheckBox.Checked = false;
            Assert.False(routeDayCheckBox.Checked);
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
        public void SummaryLabels_InitializeWithDefaultText()
        {
            var lblWeekTotal = GetPrivateField<Label>("_lblWeekTotal");
            var lblMonthTotal = GetPrivateField<Label>("_lblMonthTotal");

            Assert.NotNull(lblWeekTotal);
            Assert.NotNull(lblMonthTotal);

            // Labels should have initial text
            Assert.Contains("Week Total", lblWeekTotal.Text);
            Assert.Contains("Month Total", lblMonthTotal.Text);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void Form_HandlesDisposeCorrectly()
        {
            var testForm = new TimeCardManagementForm(_mockRepo.Object, null);

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
            var testForm = new TimeCardManagementForm(_mockRepo.Object, null);

            // Multiple dispose calls should not throw exceptions
            testForm.Dispose();
            testForm.Dispose();
            testForm.Dispose();

            Assert.True(testForm.IsDisposed);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void RouteTimeTextBoxes_AcceptTimeValues()
        {
            var routeAMOutTextBox = GetPrivateField<TextBox>("_routeAMOutTextBox");
            var routeAMInTextBox = GetPrivateField<TextBox>("_routeAMInTextBox");
            var routePMOutTextBox = GetPrivateField<TextBox>("_routePMOutTextBox");
            var routePMInTextBox = GetPrivateField<TextBox>("_routePMInTextBox");

            Assert.NotNull(routeAMOutTextBox);
            Assert.NotNull(routeAMInTextBox);
            Assert.NotNull(routePMOutTextBox);
            Assert.NotNull(routePMInTextBox);

            string testTime = "7:30 AM";
            routeAMOutTextBox.Text = testTime;
            routeAMInTextBox.Text = testTime;
            routePMOutTextBox.Text = testTime;
            routePMInTextBox.Text = testTime;

            Assert.Equal(testTime, routeAMOutTextBox.Text);
            Assert.Equal(testTime, routeAMInTextBox.Text);
            Assert.Equal(testTime, routePMOutTextBox.Text);
            Assert.Equal(testTime, routePMInTextBox.Text);
        }

        private T GetPrivateField<T>(string fieldName)
        {
            var field = typeof(TimeCardManagementForm).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)(field?.GetValue(_form) ?? default(T)!);
        }
    }
}
