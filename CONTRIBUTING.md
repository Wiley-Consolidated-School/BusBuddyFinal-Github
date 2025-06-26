# Contributing to BusBuddy

Thank you for your interest in contributing to BusBuddy! This document outlines the standards and guidelines that all contributors should follow.

## Testing Standards

### Test Framework and Location

1. **xUnit Only**: All tests must be written using xUnit. MSTest, NUnit, or any other testing frameworks are not permitted.

2. **Test Location**: All test files must be placed in the `BusBuddy.UI.Tests` project only. Do not create test files in any other project or location.

3. **Naming Convention**: Test methods should follow the convention `MethodName_Scenario_ExpectedBehavior`.

4. **UI Tests**: UI tests must include the `[STAThread]` attribute for proper thread apartment state.

### Creating New Tests

1. Create your test class in the `BusBuddy.UI.Tests` project
2. Ensure you're using xUnit attributes (`[Fact]`, `[Theory]`, etc.)
3. Follow the naming conventions
4. Run the tests locally before submitting a PR

### Testing Helper/Utility Methods

If you need to create helper methods for testing:

1. Place them within the `BusBuddy.UI.Tests` project in an appropriate helper/utility class
2. Do not create utility test methods in production code

## Code Style

1. Follow the existing code style in the project
2. Use meaningful variable and method names
3. Add comments for complex logic

## Pull Request Process

1. Ensure your code builds without errors
2. Run all tests and make sure they pass
3. Update documentation if necessary
4. Create a pull request with a clear description of the changes

## CI/CD Pipeline

Our CI pipeline automatically checks:
- Build success
- Test pass/fail status
- Code formatting
- Test naming conventions
- Test location (must be in BusBuddy.UI.Tests)
- Test framework (must be xUnit)

Pull requests failing these checks will not be merged.

## Questions?

If you have any questions about contributing, please contact the project maintainers.
