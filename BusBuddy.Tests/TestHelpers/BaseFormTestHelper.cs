using Xunit;
using System;
using System.Windows.Forms;

namespace BusBuddy.Tests.TestHelpers
{
    /// <summary>
    /// Base test helper class for common form testing patterns.
    /// Reduces duplication across form unit test files.
    /// </summary>
    public static class BaseFormTestHelper
    {
        /// <summary>
        /// Tests that a form constructor initializes without throwing exceptions
        /// and sets basic properties correctly.
        /// </summary>
        /// <typeparam name="T">Form type that inherits from Form</typeparam>
        /// <param name="createForm">Function to create the form instance</param>
        /// <param name="expectedTitle">Expected window title</param>
        /// <param name="expectedMinSize">Expected minimum size (optional)</param>
        public static void AssertFormInitializesSuccessfully<T>(
            Func<T> createForm,
            string expectedTitle,
            System.Drawing.Size? expectedMinSize = null) where T : Form
        {
            // Arrange & Act
            using var form = createForm();

            // Assert - Basic form properties
            Assert.NotNull(form);
            Assert.Equal(expectedTitle, form.Text);

            // Assert minimum size if specified
            if (expectedMinSize.HasValue)
            {
                Assert.True(form.Size.Width >= expectedMinSize.Value.Width,
                    $"Form width {form.Size.Width} should be >= {expectedMinSize.Value.Width}");
                Assert.True(form.Size.Height >= expectedMinSize.Value.Height,
                    $"Form height {form.Size.Height} should be >= {expectedMinSize.Value.Height}");
            }

            // Assert that form is properly configured
            Assert.True(form.Visible == false, "Form should not be visible during test");
        }

        /// <summary>
        /// Tests that a form constructor throws ArgumentNullException when given null dependencies.
        /// </summary>
        /// <param name="createFormWithNull">Action that should throw ArgumentNullException</param>
        public static void AssertFormThrowsOnNullDependency(Action createFormWithNull)
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(createFormWithNull);
        }

        /// <summary>
        /// Tests that a form has essential CRUD buttons.
        /// </summary>
        /// <param name="form">Form to test</param>
        /// <param name="expectedButtons">Array of expected button texts</param>
        public static void AssertFormHasCrudButtons(Form form, string[] expectedButtons)
        {
            foreach (var buttonText in expectedButtons)
            {
                var button = UITestHelpers.FindButtonByText(form, buttonText);
                Assert.NotNull(button);
                Assert.Equal(buttonText, button.Text);
                Assert.True(button.Enabled, $"Button '{buttonText}' should be enabled");
            }
        }

        /// <summary>
        /// Tests that a form properly handles disposal without throwing exceptions.
        /// </summary>
        /// <typeparam name="T">Form type</typeparam>
        /// <param name="createForm">Function to create form instance</param>
        public static void AssertFormHandlesDisposeCorrectly<T>(Func<T> createForm) where T : Form
        {
            // Arrange
            var form = createForm();

            // Act & Assert - Should not throw exception when disposing
            form.Dispose();

            // Additional dispose calls should also not throw
            form.Dispose();
        }

        /// <summary>
        /// Tests that a form has proper keyboard accessibility (tab order, shortcuts).
        /// </summary>
        /// <param name="form">Form to test</param>
        public static void AssertFormHasKeyboardAccessibility(Form form)
        {
            // Assert that form handles keyboard shortcuts
            Assert.True(form.KeyPreview, "Form should handle keyboard shortcuts");

            // Additional accessibility checks could be added here
            // such as tab order validation, mnemonic key checks, etc.
        }

        /// <summary>
        /// Standard CRUD button names for management forms.
        /// </summary>
        public static readonly string[] StandardCrudButtons = { "Add", "Edit", "Delete", "Save", "Cancel" };
    }
}
