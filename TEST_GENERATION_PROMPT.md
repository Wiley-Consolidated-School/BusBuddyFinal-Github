# Test Generation Instructions for BusBuddy

When generating tests for the BusBuddy project, ALWAYS follow these rules:

## Framework and Location

- **Use xUnit EXCLUSIVELY** - No other test frameworks (MSTest, NUnit) are allowed
- **Place ALL tests in the BusBuddy.UI.Tests project**
- **Reference DynamicLayoutManagerTest.cs as the standard pattern**

## Test Structure

1. **Follow this structure for all test files:**
   ```csharp
   using System;
   using Xunit;
   using BusBuddy.[RelevantNamespace];
   
   namespace BusBuddy.UI.Tests
   {
       public class ClassNameTest : IDisposable
       {
           // Setup fields
           
           public ClassNameTest()
           {
               // Setup code
           }
           
           public void Dispose()
           {
               // Cleanup code
           }
           
           #region MethodName Tests
           
           [Fact]
           public void MethodName_Scenario_ExpectedBehavior()
           {
               // Arrange
               
               // Act
               
               // Assert
           }
           
           #endregion
       }
   }
   ```

2. **Organization:**
   - Group tests by method in #region blocks
   - Test both normal functionality and edge cases
   - Test null/invalid arguments
   - Use descriptive method names (MethodName_Scenario_ExpectedBehavior)

3. **Naming:**
   - Test class should be named `{ClassName}Test`
   - Test methods should be named `{MethodName}_{Scenario}_{ExpectedBehavior}`

4. **Pattern:**
   - Always use the Arrange-Act-Assert pattern
   - Add comments for each section

## Test Method Requirements

1. **Basic tests:**
   - Test with null parameters (expect ArgumentNullException)
   - Test with valid parameters (verify correct operation)
   - Test edge cases

2. **UI components:**
   - Create and dispose of UI components properly using IDisposable
   - Test visual properties and behaviors
   - Use Form/Panel for parent controls in tests

3. **Syncfusion components:**
   - Test with Syncfusion-specific assertions
   - Test layout properties specific to Syncfusion controls

## Example

```csharp
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
```
