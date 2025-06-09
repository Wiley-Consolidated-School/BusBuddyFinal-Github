# GitHub Actions Setup Guide

This document explains the GitHub Actions workflows set up for this project and how to make the most of them.

## Available Workflows

### 1. .NET Build and Test (.github/workflows/dotnet.yml)

This workflow runs whenever you push to the `master` branch or create a pull request against it. It:

- Builds the entire solution
- Runs all tests in the BusBuddy.Tests project
- Creates a test report
- Publishes the application build artifacts

You can also trigger this workflow manually from the Actions tab on GitHub.

### 2. Code Quality (.github/workflows/code-quality.yml)

This workflow focuses on code quality and runs:

- .NET code style checks
- Static code analysis
- Test coverage analysis

The workflow generates a code coverage report that you can download from the workflow run.

### 3. Release Creation (.github/workflows/release.yml)

This workflow is triggered when you push a tag with a version number (e.g., `v1.0.0`):

```bash
git tag v1.0.0
git push origin v1.0.0
```

It automatically:
- Builds the application in Release mode
- Creates a ZIP archive with all necessary files
- Creates a GitHub release with the generated artifacts

## Setting Up a New Developer Environment

1. Clone the repository
2. Install Visual Studio 2022 or later
3. Open the solution file (BusBuddy.sln)
4. Restore NuGet packages
5. Build and run the application

## Adding New Tests

When adding new functionality, please also add corresponding tests to ensure the GitHub Actions workflows can verify your changes work correctly.

## Future Enhancements

Here are some planned enhancements for our CI/CD pipeline:

1. Add SonarCloud integration for deeper code quality analysis
2. Set up automated deployment to test environments
3. Add performance testing benchmarks

## Troubleshooting

If a workflow fails, you can view the detailed logs in the GitHub Actions tab of the repository. Common issues include:

- Failed tests
- Code style violations
- Missing dependencies

For questions or issues with the CI/CD setup, please contact the repository maintainers.
