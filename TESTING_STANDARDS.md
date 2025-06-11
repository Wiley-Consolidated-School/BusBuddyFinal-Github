# BusBuddy Testing Standards

This document enforces GitHub Copilot best practices for unit testing in the BusBuddy project.

## 1. Test Naming Conventions (ENFORCED)

### Pattern: `MethodName_Scenario_ExpectedBehavior`

✅ **Good Examples:**
```csharp
[Fact]
public void MockMainForm_WhenInitialized_ShouldHaveCorrectTitle()

[Fact]
public void VehicleManagementForm_WhenCRUDButtonsCreated_ShouldBeProperlyConfigured()

[Fact]
public void UITestHelpers_FindControl_WhenControlExists_ShouldReturnCorrectControl()
```

❌ **Bad Examples:**
```csharp
[Fact]
public void TestMainForm() // Too vague

[Fact] 
public void MainFormTest() // Doesn't describe scenario or expectation

[Fact]
public void Test1() // Meaningless name
```

## 2. Test Structure - AAA Pattern (ENFORCED)

Every test MUST follow the Arrange-Act-Assert pattern with clear comments:

```csharp
[STAThread]
[Fact]
public void MockVehicleForm_WhenFieldsAreEmpty_ShouldPreventSave()
{
    // Arrange - Set up test data and dependencies
    using var form = new MockVehicleForm();
    var busNumberField = UITestHelpers.FindControl<TextBox>(form, "txtBusNumber");
    var saveButton = UITestHelpers.FindButtonByText(form, "Save");
    
    // Act - Perform the action being tested
    busNumberField!.Text = "";
    var canSave = ValidateForm(form);
    
    // Assert - Verify the expected outcome
    Assert.False(canSave);
    Assert.NotNull(saveButton);
}
```

## 3. Context and Documentation (ENFORCED)

### Required Comments:
- **Class-level summary** explaining what is being tested
- **Method-level summary** for complex test scenarios
- **Inline comments** for non-obvious setup or assertions

```csharp
/// <summary>
/// Tests UI component initialization, layout, and interaction behavior.
/// Uses mock forms to avoid database dependencies in UI testing.
/// </summary>
public class UIComponentTests
{
    /// <summary>
    /// Verifies that the main form initializes with correct window properties
    /// and essential navigation controls for the bus management system.
    /// </summary>
    [STAThread]
    [Fact]
    public void MockMainForm_WhenInitialized_ShouldHaveCorrectWindowProperties()
    {
        // Test implementation...
    }
}
```

## 4. Mock and Dependency Management (ENFORCED)

### Use Mocks to Isolate Units Under Test:
```csharp
// ✅ Good - No database dependencies
using var form = new MockVehicleManagementForm();

// ❌ Bad - Real form with database dependencies  
using var form = new VehicleManagementForm(); // This will fail in test environment
```

### Proper Resource Disposal:
```csharp
// ✅ Good - Using statements for automatic disposal
using var form = new MockMainForm();
using var bitmap = UITestHelpers.CaptureFormScreenshot(form);

// ❌ Bad - Manual disposal (error-prone)
var form = new MockMainForm();
// ... test code ...
form.Dispose(); // Might not execute if test fails
```

## 5. Test Coverage Requirements (ENFORCED)

### Minimum Coverage:
- **UI Components:** Form initialization, control presence, layout validation
- **Business Logic:** All public methods with happy path and edge cases
- **Data Access:** CRUD operations with mock data
- **Integration:** End-to-end workflows with mocked dependencies

### Edge Cases to Test:
- Null inputs
- Empty collections
- Boundary values
- Exception scenarios
- Thread safety (for UI tests)

## 6. Platform-Specific Testing (ENFORCED)

### Windows Forms UI Tests:
```csharp
[STAThread] // Required for Windows Forms
[Fact]
public void Form_WhenInitialized_ShouldUseSTAThread()
{
    // UI tests must run on STA thread
}
```

### Graphics and Visual Testing:
```csharp
[Fact]
public void Chart_WhenDataLoaded_ShouldRenderCorrectly()
{
    // Arrange - Create form with chart
    using var form = new Form1();
    form.Show();
    
    // Act - Allow rendering time
    Application.DoEvents();
    Thread.Sleep(100);
    
    // Assert - Verify visual elements
    var chart = UITestHelpers.FindControl<Chart>(form, "chartTopProducts");
    Assert.NotNull(chart);
    Assert.True(chart.Series.Count > 0);
}
```

## 7. Build Integration (AUTOMATED ENFORCEMENT)

The following rules are automatically enforced during build:

### Compiler Warnings as Errors:
- Missing test attributes
- Improper test naming
- Undisposed resources
- Missing assertions

### Static Analysis:
- xUnit analyzer rules
- Code quality rules
- Naming convention enforcement

## 8. Pre-commit Hooks (RECOMMENDED)

Create `.githooks/pre-commit`:
```bash
#!/bin/sh
# Run tests before commit
dotnet test --configuration Release --logger "console;verbosity=minimal"
if [ $? -ne 0 ]; then
    echo "Tests failed. Commit aborted."
    exit 1
fi

# Run code formatting
dotnet format --verify-no-changes
if [ $? -ne 0 ]; then
    echo "Code formatting issues found. Run 'dotnet format' and try again."
    exit 1
fi
```

## 9. Continuous Integration (CI/CD)

### GitHub Actions Workflow:
- Build verification
- Test execution with coverage
- Code quality gates
- Automated formatting checks

## 10. Tool Configuration

### EditorConfig enforces:
- Consistent indentation
- Line endings
- File encoding
- Test file naming patterns

### Ruleset enforces:
- Test method structure
- Documentation requirements
- Resource management
- Assertion patterns

---

## Enforcement Mechanisms

| Rule Type | Enforcement Method | Failure Action |
|-----------|-------------------|----------------|
| Naming Conventions | .editorconfig + Analyzers | Build Error |
| AAA Pattern | Code Review + Analyzers | Build Warning → Error |
| Resource Disposal | Static Analysis | Build Error |
| Test Coverage | CI/CD Pipeline | Block Merge |
| Documentation | Analyzers | Build Warning |

## Getting Started

1. **Install Analyzers**: Already configured in `BusBuddy.Tests.csproj`
2. **Follow Templates**: Use existing test files as templates
3. **Run Locally**: `dotnet test` to verify compliance
4. **Check Coverage**: `dotnet test --collect:"XPlat Code Coverage"`

## Questions?

Refer to:
- [GitHub Copilot Testing Guide](https://github.blog/ai-and-ml/github-copilot/how-to-generate-unit-tests-with-github-copilot-tips-and-examples/)
- [xUnit Documentation](https://xunit.net/)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/best-practices)
