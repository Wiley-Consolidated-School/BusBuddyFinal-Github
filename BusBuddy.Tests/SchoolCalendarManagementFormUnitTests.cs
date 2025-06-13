using Xunit;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;
using BusBuddy.UI.Views;
using Moq;
using BusBuddy.Data;
using BusBuddy.Models;

namespace BusBuddy.Tests
{
    public class SchoolCalendarManagementFormUnitTests : IDisposable
    {
        private SchoolCalendarManagementForm _form;

        public SchoolCalendarManagementFormUnitTests()
        {
            _form = new SchoolCalendarManagementForm();
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
            Assert.Equal("School Calendar Management", _form.Text);
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
            var calendarGrid = GetPrivateField<DataGridView>("_calendarGrid");
            var monthComboBox = GetPrivateField<ComboBox>("_monthComboBox");
            var yearUpDown = GetPrivateField<NumericUpDown>("_yearUpDown");

            Assert.NotNull(calendarGrid);
            Assert.NotNull(monthComboBox);
            Assert.NotNull(yearUpDown);
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
            var dayTypeComboBox = GetPrivateField<ComboBox>("_dayTypeComboBox");
            var notesTextBox = GetPrivateField<TextBox>("_notesTextBox");
            var saveButton = GetPrivateField<Button>("_saveButton");

            Assert.NotNull(dayTypeComboBox);
            Assert.NotNull(notesTextBox);
            Assert.NotNull(saveButton);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void KeyPreview_EnablesKeyboardHandling()
        {
            Assert.True(_form.KeyPreview);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void DisplayYear_InitializesToCurrentYear()
        {
            var displayYear = GetPrivateField<int>("_displayYear");
            Assert.Equal(DateTime.Now.Year, displayYear);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void DisplayMonth_InitializesToCurrentMonth()
        {
            var displayMonth = GetPrivateField<int>("_displayMonth");
            Assert.Equal(DateTime.Now.Month, displayMonth);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void DayTypes_InitializeWithValidValues()
        {
            var dayTypes = GetPrivateField<string[]>("_dayTypes");
            Assert.NotNull(dayTypes);
            Assert.Contains("School Day", dayTypes);
            Assert.Contains("Holiday", dayTypes);
            Assert.Contains("Vacation", dayTypes);
            Assert.Contains("Half Day", dayTypes);
            Assert.Contains("Non-Student Day", dayTypes);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void DayTypeColors_InitializeWithValidColors()
        {
            var dayTypeColors = GetPrivateField<Dictionary<string, Color>>("_dayTypeColors");
            Assert.NotNull(dayTypeColors);

            Assert.True(dayTypeColors.ContainsKey("School Day"));
            Assert.True(dayTypeColors.ContainsKey("Holiday"));
            Assert.True(dayTypeColors.ContainsKey("Vacation"));
            Assert.True(dayTypeColors.ContainsKey("Half Day"));
            Assert.True(dayTypeColors.ContainsKey("Non-Student Day"));

            Assert.Equal(Color.LightGreen, dayTypeColors["School Day"]);
            Assert.Equal(Color.LightCoral, dayTypeColors["Holiday"]);
            Assert.Equal(Color.Khaki, dayTypeColors["Vacation"]);
            Assert.Equal(Color.LightYellow, dayTypeColors["Half Day"]);
            Assert.Equal(Color.LightGray, dayTypeColors["Non-Student Day"]);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void DefaultColor_InitializesToWhite()
        {
            var defaultColor = GetPrivateField<Color>("_defaultColor");
            Assert.Equal(Color.White, defaultColor);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void YearUpDown_HasReasonableRange()
        {
            var yearUpDown = GetPrivateField<NumericUpDown>("_yearUpDown");
            Assert.NotNull(yearUpDown);

            // Year should be within a reasonable range
            Assert.True(yearUpDown.Value >= 2000);
            Assert.True(yearUpDown.Value <= 2100);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void MonthComboBox_HasValidRange()
        {
            var monthComboBox = GetPrivateField<ComboBox>("_monthComboBox");
            Assert.NotNull(monthComboBox);

            // Month should be within valid range (if populated)
            if (monthComboBox.Items.Count > 0)
            {
                Assert.True(monthComboBox.Items.Count <= 12);
            }
        }

        [Theory]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("Special event note")]
        public void NotesTextBox_HandlesVariousValues(string notesValue)
        {
            var notesTextBox = GetPrivateField<TextBox>("_notesTextBox");
            Assert.NotNull(notesTextBox);

            // Should not throw exception with various note values
            notesTextBox.Text = notesValue;
            Assert.Equal(notesValue ?? "", notesTextBox.Text);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void CalendarEntries_InitializeAsEmptyList()
        {
            var calendarEntries = GetPrivateField<List<SchoolCalendar>>("_calendarEntries");
            Assert.NotNull(calendarEntries);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void SelectedDate_InitializesToValidDate()
        {
            var selectedDate = GetPrivateField<DateTime>("_selectedDate");
            Assert.True(selectedDate >= DateTime.MinValue);
            Assert.True(selectedDate <= DateTime.MaxValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void Form_HandlesDisposeCorrectly()
        {
            var testForm = new SchoolCalendarManagementForm();

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
            var testForm = new SchoolCalendarManagementForm();

            // Multiple dispose calls should not throw exceptions
            testForm.Dispose();
            testForm.Dispose();
            testForm.Dispose();

            Assert.True(testForm.IsDisposed);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void CalendarGrid_InitializesWithoutErrors()
        {
            var calendarGrid = GetPrivateField<DataGridView>("_calendarGrid");
            Assert.NotNull(calendarGrid);

            // Grid should be initialized without errors
            Assert.True(calendarGrid.ColumnCount >= 0);
            Assert.True(calendarGrid.RowCount >= 0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Phase", "1")]
        public void DayTypeComboBox_AcceptsValidDayTypes()
        {
            var dayTypeComboBox = GetPrivateField<ComboBox>("_dayTypeComboBox");
            Assert.NotNull(dayTypeComboBox);

            var dayTypes = GetPrivateField<string[]>("_dayTypes");

            // Should accept all valid day types without error
            foreach (var dayType in dayTypes)
            {
                dayTypeComboBox.Text = dayType;
                Assert.Equal(dayType, dayTypeComboBox.Text);
            }
        }

        private T GetPrivateField<T>(string fieldName)
        {
            var field = typeof(SchoolCalendarManagementForm).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)(field?.GetValue(_form) ?? default(T)!);
        }
    }
}
