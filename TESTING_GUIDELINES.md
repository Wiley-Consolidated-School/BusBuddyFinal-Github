# BusBuddy Testing Guidelines

## Test Structure and Standards

### Testing Framework

- **xUnit ONLY**: All tests must use xUnit as the testing framework
- **NO MSTest, NUnit, or SimpleTest**: Other testing frameworks are not permitted

### Syncfusion License Handling

- **DO NOT create license manager classes or helpers** for Syncfusion licensing
- **DO NOT mock license registration** in tests - this should be handled directly in Program.cs
- **For test environment issues**, use environment variables or test-specific configurations rather than creating licensing helpers

### Test Location

- **All tests must be placed in `BusBuddy.UI.Tests` project**
- **No test files should exist in other project directories**
- **No standalone test projects outside of `BusBuddy.UI.Tests`**

### Naming Conventions

- Test class names should end with `Test` (e.g., `DynamicLayoutManagerTest`)
- Test methods should follow the pattern: `MethodName_Scenario_ExpectedResult`
- Group related tests in `#region` blocks for better organization

### Test Structure

- Each test class should implement `IDisposable` if it creates disposable resources
- Use the constructor for test setup and `Dispose()` method for cleanup
- Test methods should follow the Arrange-Act-Assert pattern
- Use descriptive assertion messages for clarity

### Example Test Class Structure

```csharp
using System;
using Xunit;
using BusBuddy.UI.YourNamespace;

namespace BusBuddy.UI.Tests
{
    public class YourClassTest : IDisposable
    {
        // Setup fields
        private readonly YourClass _sut;
        
        public YourClassTest()
        {
            // Setup code
            _sut = new YourClass();
        }
        
        public void Dispose()
        {
            // Cleanup code
        }
        
        #region MethodName Tests
        
        [Fact]
        public void MethodName_Scenario_ExpectedResult()
        {
            // Arrange
            var input = "test";
            
            // Act
            var result = _sut.MethodName(input);
            
            // Assert
            Assert.Equal("expected", result);
        }
        
        #endregion
    }
}
```

## For AI Assistants

When asked to create or modify tests:

1. **ALWAYS use xUnit** for all test development
2. **ALWAYS place tests in `BusBuddy.UI.Tests` project**
3. **NEVER create tests using MSTest, NUnit, or SimpleTest frameworks**
4. **NEVER place tests in source code projects** (e.g., BusBuddy.UI, BusBuddy.Business)
5. **Follow the existing test patterns** in `DynamicLayoutManagerTest.cs` as a reference
6. **Use regions** to organize related test methods
7. **Ensure test method names are descriptive** and follow the convention

## Project Structure

```
BusBuddy/
├── BusBuddy.UI/
│   └── [Source code - NO TESTS HERE]
├── BusBuddy.Business/
│   └── [Source code - NO TESTS HERE]
├── BusBuddy.Data/
│   └── [Source code - NO TESTS HERE]
├── BusBuddy.Models/
│   └── [Source code - NO TESTS HERE]
└── BusBuddy.UI.Tests/
    └── [ALL TESTS GO HERE]
        ├── DynamicLayoutManagerTest.cs
        └── [Other test files]
```

## Running Tests

- Use the VS Code task "test BusBuddy" to run all tests
- To discover tests, use the "Discover Tests" task
- For test coverage, use the "Generate Code Coverage" task
