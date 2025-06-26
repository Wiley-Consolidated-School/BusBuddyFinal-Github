# Copilot Test Generation Examples

This file contains examples of how to generate tests in the BusBuddy project using GitHub Copilot.

## Example 1: Creating a Test Class

When creating a new test class, follow this pattern:

```csharp
using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using BusBuddy.UI.Layout;
using Xunit;

namespace BusBuddy.UI.Tests
{
    public class DynamicLayoutManagerTest : IDisposable
    {
        private readonly Form _testForm;
        private readonly Panel _parentPanel;

        public DynamicLayoutManagerTest()
        {
            _testForm = new Form();
            _parentPanel = new Panel();
            _testForm.Controls.Add(_parentPanel);
        }

        public void Dispose()
        {
            _testForm?.Dispose();
            _parentPanel?.Dispose();
        }

        #region CreateFlowLayoutContainer Tests

        [Fact]
        public void CreateFlowLayoutContainer_WithNullParent_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => DynamicLayoutManager.CreateFlowLayoutContainer(null));
        }

        [Fact]
        public void CreateFlowLayoutContainer_CreatesContainerWithCorrectProperties()
        {
            // Act
            var container = DynamicLayoutManager.CreateFlowLayoutContainer(_parentPanel);

            // Assert
            Assert.NotNull(container);
            Assert.Equal(DockStyle.Fill, container.Dock);
            Assert.Equal(Color.Transparent, container.BackColor);
            Assert.Contains(container, _parentPanel.Controls.Cast<Control>());
        }

        #endregion
    }
}
```

## Example 2: Creating Tests with Various Assertions

```csharp
// Test for correct property values
[Fact]
public void CreateTableLayoutContainer_CreatesTableWithCorrectRowsAndColumns()
{
    // Arrange
    const int rows = 3;
    const int columns = 4;
    const int padding = 8;

    // Act
    var tablePanel = DynamicLayoutManager.CreateTableLayoutContainer(_parentPanel, rows, columns, padding);

    // Assert
    Assert.NotNull(tablePanel);
    Assert.Equal(rows, tablePanel.RowCount);
    Assert.Equal(columns, tablePanel.ColumnCount);
    Assert.Equal(new Padding(padding), tablePanel.Padding);
    Assert.Equal(DockStyle.Fill, tablePanel.Dock);
    Assert.Equal(Color.Transparent, tablePanel.BackColor);
}

// Test for collections
[Fact]
public void CreateTableLayoutContainer_SetsCorrectRowAndColumnStyles()
{
    // Arrange
    const int rows = 2;
    const int columns = 3;

    // Act
    var tablePanel = DynamicLayoutManager.CreateTableLayoutContainer(_parentPanel, rows, columns);

    // Assert
    Assert.Equal(rows, tablePanel.RowStyles.Count);
    Assert.Equal(columns, tablePanel.ColumnStyles.Count);

    // Check row styles
    Assert.Equal(50f, tablePanel.RowStyles[0].Height);
    Assert.Equal(50f, tablePanel.RowStyles[1].Height);
    Assert.Equal(SizeType.Percent, tablePanel.RowStyles[0].SizeType);
}

// Test for precise floating point values
[Fact]
public void CreateResponsiveGridLayout_CreatesCorrectTableStructure()
{
    // Arrange
    var config = new LayoutConfiguration
    {
        Rows = 3,
        Columns = 2,
        RowSizes = new System.Collections.Generic.List<float> { 25f, 25f, 50f },
        ColumnSizes = new System.Collections.Generic.List<float> { 30f, 70f }
    };

    // Act
    var container = DynamicLayoutManager.CreateResponsiveGridLayout(_parentPanel, config);

    // Assert
    Assert.NotNull(container);
    Assert.Equal(1, container.Controls.Count);

    var tablePanel = container.Controls[0] as TableLayoutPanel;
    Assert.NotNull(tablePanel);
    Assert.Equal(config.Rows, tablePanel.RowCount);
    Assert.Equal(config.Columns, tablePanel.ColumnCount);

    // Check row heights with precise values
    Assert.Equal(config.RowSizes[0], tablePanel.RowStyles[0].Height);
    Assert.Equal(config.RowSizes[1], tablePanel.RowStyles[1].Height);
    Assert.Equal(config.RowSizes[2], tablePanel.RowStyles[2].Height);
}
```

## Commands for Building and Testing

To run tests:
- Use "Run Tests" from right-click menu on test method
- Or run from the Testing panel in VS Code
- Or use the VS Code task: "test BusBuddy"
