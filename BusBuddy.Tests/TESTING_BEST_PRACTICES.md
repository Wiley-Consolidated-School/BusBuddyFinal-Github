# BusBuddy Unit Testing Best Practices

Based on the XUnit best practices article and analysis of our current tests, this document outlines the standards for writing reliable unit tests in the BusBuddy project.

## 📋 Core Principles

### 1. Triple A Pattern (Arrange, Act, Assert)
**Every test must clearly separate these three phases:**

```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange - Set up test data and conditions
    var vehicle = new Vehicle();
    const string expectedValue = "BUS123";

    // Act - Execute the method/action being tested
    vehicle.VehicleNumber = expectedValue;

    // Assert - Verify the result
    Assert.Equal(expectedValue, vehicle.VehicleNumber);
}
```

### 2. Naming Convention
**Use descriptive names that follow this pattern:**
- `MethodName_Scenario_ExpectedResult`
- `ComponentName_Action_ExpectedOutcome`

**Examples:**
- ✅ `ValidateVehicleNumber_NullValue_ReturnsFalse`
- ✅ `MainForm_Constructor_SetsWindowTitle`
- ❌ `TestVehicle`
- ❌ `CheckValidation`

### 3. Single Responsibility
**Each test should verify ONE specific behavior:**

```csharp
// ✅ Good - Tests one specific property
[Fact]
public void Vehicle_SetMake_SetsCorrectly()
{
    // Arrange
    var vehicle = new Vehicle();
    const string expectedMake = "Mercedes";

    // Act
    vehicle.Make = expectedMake;

    // Assert
    Assert.Equal(expectedMake, vehicle.Make);
}

// ❌ Bad - Tests multiple unrelated things
[Fact]
public void Vehicle_SetAllProperties_SetsEverything()
{
    var vehicle = new Vehicle();
    vehicle.Make = "Mercedes";
    vehicle.Model = "Sprinter";
    vehicle.Year = 2022;
    // ... testing too many things at once
}
```

## 🎯 Test Categories

### Constructor Tests
- Test default values
- Test object initialization
- Test parameter validation

### Property Tests
- Test valid values
- Test null/empty values
- Test boundary conditions
- Test edge cases

### Validation Tests
- Test each validation rule separately
- Test boundary values (min, max, min-1, max+1)
- Test null, empty, whitespace scenarios
- Test valid cases

### UI Component Tests
- Test component creation
- Test property settings
- Test event handlers
- Test layout and positioning

## 🔍 Testing Patterns

### Theory Tests for Multiple Scenarios
```csharp
[Theory]
[InlineData("Diesel")]
[InlineData("Gasoline")]
[InlineData("Electric")]
public void FuelType_ValidTypes_SetsCorrectly(string fuelType)
{
    // Arrange
    var vehicle = new Vehicle();

    // Act
    vehicle.FuelType = fuelType;

    // Assert
    Assert.Equal(fuelType, vehicle.FuelType);
}
```

### Boundary Testing Pattern
```csharp
[Fact]
public void ValidateYear_1899_ReturnsFalse() // Below minimum
{
    // Test just below boundary
}

[Fact]
public void ValidateYear_1900_ReturnsTrue() // At minimum boundary
{
    // Test at exact boundary
}

[Fact]
public void ValidateYear_2025_ReturnsTrue() // Valid value
{
    // Test valid case
}
```

### Null/Empty Testing Pattern
```csharp
[Fact]
public void Property_NullValue_AcceptsNull()
{
    // Test null handling
}

[Fact]
public void Property_EmptyString_AcceptsEmpty()
{
    // Test empty string handling
}

[Fact]
public void Property_WhitespaceOnly_AcceptsWhitespace()
{
    // Test whitespace handling
}
```

## 🏗️ Test Organization

### File Structure
```
BusBuddy.Tests/
├── Models/
│   ├── VehicleModelTests.cs
│   ├── DriverModelTests.cs
│   └── ValidationTests.cs
├── UI/
│   ├── MainFormTests.cs
│   ├── FormDisplayTests.cs
│   └── DataGridTests.cs
├── Business/
│   ├── ServiceTests.cs
│   └── RepositoryTests.cs
└── Integration/
    ├── DatabaseTests.cs
    └── EndToEndTests.cs
```

### Regions for Organization
```csharp
public class VehicleModelTests
{
    #region Constructor Tests
    // Constructor-related tests
    #endregion

    #region Property Tests
    // Property-related tests
    #endregion

    #region Validation Tests
    // Validation-related tests
    #endregion

    #region Helper Methods
    // Private helper methods
    #endregion
}
```

## ✅ Assertion Best Practices

### Use Specific Assertions
```csharp
// ✅ Good - Specific assertion
Assert.Equal(expectedValue, actualValue);

// ❌ Bad - Generic assertion
Assert.True(actualValue == expectedValue);
```

### Use Appropriate Assertion Methods
```csharp
// For null checks
Assert.Null(value);
Assert.NotNull(value);

// For collections
Assert.Empty(collection);
Assert.Single(collection);
Assert.Contains(expectedItem, collection);

// For exceptions
Assert.Throws<ArgumentException>(() => methodCall);

// For boolean values
Assert.True(condition);
Assert.False(condition);
```

## 🧪 Test Data Management

### Use Constants for Expected Values
```csharp
[Fact]
public void Vehicle_SetVehicleNumber_SetsCorrectly()
{
    // Arrange
    var vehicle = new Vehicle();
    const string expectedVehicleNumber = "BUS123"; // Clear constant

    // Act
    vehicle.VehicleNumber = expectedVehicleNumber;

    // Assert
    Assert.Equal(expectedVehicleNumber, vehicle.VehicleNumber);
}
```

### Use Helper Methods for Setup
```csharp
private static Vehicle CreateValidVehicle()
{
    return new Vehicle
    {
        VehicleNumber = "BUS123",
        Make = "Mercedes",
        Model = "Sprinter",
        Year = 2022
    };
}
```

## 🎭 Mock and Isolation Patterns

### Use Mocks for Dependencies
```csharp
[Fact]
public void Service_WithMockRepository_CallsRepositoryCorrectly()
{
    // Arrange
    var mockRepository = new MockVehicleRepository();
    var service = new VehicleService(mockRepository);

    // Act
    service.AddVehicle(CreateValidVehicle());

    // Assert
    Assert.True(mockRepository.AddWasCalled);
}
```

## 📊 Test Coverage Guidelines

### Ensure Coverage of:
- All public methods and properties
- All validation rules and edge cases
- All error conditions and exception paths
- All UI component initialization
- All business logic branches

### Edge Cases to Test:
- Null values
- Empty collections
- Boundary values (min, max, min-1, max+1)
- Very large values
- Invalid formats
- Concurrent access scenarios

## 🏃‍♂️ Running Tests

### Use Traits for Organization
```csharp
[Fact]
[Trait("Category", "Model")]
[Trait("Priority", "High")]
public void Vehicle_CoreProperty_Test()
{
    // Test implementation
}
```

### Run Specific Test Categories
```bash
# Run all model tests
dotnet test --filter "Category=Model"

# Run high priority tests
dotnet test --filter "Priority=High"

# Run specific class tests
dotnet test --filter "FullyQualifiedName~VehicleModelTests"
```

## ❌ Common Anti-Patterns to Avoid

### Don't Test Multiple Things in One Test
```csharp
// ❌ Bad - Tests too many things
[Fact]
public void Vehicle_SetProperties_SetsAllPropertiesCorrectly()
{
    var vehicle = new Vehicle();
    vehicle.Make = "Mercedes";        // Testing Make
    vehicle.Model = "Sprinter";       // Testing Model
    vehicle.Year = 2022;              // Testing Year
    // ... testing everything at once
}
```

### Don't Use Magic Numbers/Strings
```csharp
// ❌ Bad - Magic values
Assert.Equal("Mercedes", vehicle.Make);

// ✅ Good - Clear constants
const string expectedMake = "Mercedes";
Assert.Equal(expectedMake, vehicle.Make);
```

### Don't Ignore the Arrange Phase
```csharp
// ❌ Bad - Arrange and Act combined
[Fact]
public void Test()
{
    Assert.Equal("BUS123", new Vehicle { VehicleNumber = "BUS123" }.VehicleNumber);
}

// ✅ Good - Clear separation
[Fact]
public void Vehicle_SetVehicleNumber_SetsCorrectly()
{
    // Arrange
    var vehicle = new Vehicle();
    const string expectedNumber = "BUS123";

    // Act
    vehicle.VehicleNumber = expectedNumber;

    // Assert
    Assert.Equal(expectedNumber, vehicle.VehicleNumber);
}
```

## 📈 Quality Metrics

### Test Quality Indicators:
- Each test has a clear AAA structure
- Test names are descriptive and specific
- Tests are independent and can run in any order
- Tests are fast (< 100ms for unit tests)
- Tests have single responsibility
- All edge cases are covered
- No commented-out test code
- Proper cleanup in IDisposable tests

### Code Coverage Targets:
- Models: 100% (simple property tests)
- Business Logic: 95%+ (critical paths)
- UI Components: 80%+ (focus on initialization and key interactions)
- Integration: 70%+ (focus on main scenarios)

This document should be updated as we learn more about effective testing patterns specific to the BusBuddy application.
