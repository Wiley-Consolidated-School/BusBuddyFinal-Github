# GitHub Actions Workflow Setup Guide

This guide explains how to set up and troubleshoot the BusBuddy GitHub Actions workflow.

## Fixed Issues

### ✅ Solution File Detection
- **Problem**: Intermittent "No solution file found" errors
- **Fix**: Enhanced solution file detection with fallback search
- **Details**: The workflow now checks the root directory first, then searches subdirectories

### ✅ Test Results Upload
- **Problem**: `fail-on-empty: true` causing failures when no TRX files found
- **Fix**: Changed to `fail-on-empty: false` and improved TRX file generation
- **Details**: Added comprehensive test result validation and directory creation

### ✅ Codecov Upload Configuration
- **Problem**: Token required for protected branches + malformed arguments
- **Fix**: Proper token handling with fallback and structured arguments
- **Details**: Added graceful degradation when token is not available

## Required Setup Steps

### 1. Codecov Token Configuration (Required for Coverage Reports)

To enable code coverage uploads to Codecov:

1. **Sign up at [Codecov.io](https://codecov.io)**
2. **Add your repository** to Codecov
3. **Get your upload token**:
   - Go to `https://codecov.io/gh/YOUR_ORG/YOUR_REPO/settings`
   - Copy the repository upload token
4. **Add the token to GitHub secrets**:
   - Go to your GitHub repository
   - Navigate to `Settings > Secrets and variables > Actions`
   - Click `New repository secret`
   - Name: `CODECOV_TOKEN`
   - Value: Your Codecov upload token

### 2. Repository Permissions

Ensure the workflow has proper permissions:
- ✅ Contents: read (for checkout)
- ✅ Actions: read (for artifacts)
- ✅ Checks: write (for test reporting)
- ✅ Pull-requests: write (for PR comments)

### 3. Branch Protection (Optional)

If using branch protection on `master`:
- The Codecov token is **required** for protected branches
- Without the token, coverage uploads will be skipped (non-blocking)

## Workflow Features

### Enhanced Codecov Integration
- **Coverage Flags**: Separate tracking for unit, integration, and UI tests
- **Stricter Thresholds**: 80% project target, 70% patch target
- **Smart Status Checks**: Different rules for PRs vs. main branch
- **Enhanced PR Comments**: Detailed coverage breakdown with file-level reporting
- **GitHub Checks**: Coverage annotations directly in PR reviews

### Build and Test Job
- **Environment**: Windows Latest
- **Framework**: .NET 8.0
- **Database**: SQLite (embedded for tests)
- **Coverage**: XPlat Code Coverage with Cobertura format + flags
- **Test Reports**: TRX format for detailed reporting
- **Error Recovery**: Graceful handling of missing tokens and files

### Test Result Artifacts
- Test results saved as artifacts (30-day retention)
- Coverage reports saved as artifacts (30-day retention)
- Available for download from workflow runs

### Deployment Check Job
- Runs only on `master` branch pushes
- Creates deployment package
- Validates release build
- Saves deployment artifacts (7-day retention)

### Dependabot Auto-Approval
- **Automated PR Management**: Auto-approves safe patch updates
- **Safety Checks**: Only patches, excludes critical packages
- **CI Integration**: Waits for all tests to pass before merging
- **Smart Commenting**: Explains decisions for transparency

### Security Scanning
- **Weekly Vulnerability Scans**: Automated dependency security checks
- **GitHub Integration**: Creates issues for critical findings
- **Comprehensive Reporting**: Detailed vulnerability and outdated package analysis
- **PR Integration**: Comments on dependency-related pull requests

## Troubleshooting

### No TRX Files Found
If test reporting fails:
1. Check the test execution logs
2. Verify `TestResults` directory creation
3. Ensure test projects are discovered by `dotnet test`

### Codecov Upload Fails
Common causes and solutions:
- **Missing token**: Add `CODECOV_TOKEN` secret (see setup steps above)
- **No coverage files**: Check test execution for coverage collection
- **Network issues**: The upload step is marked `continue-on-error: true`

### Solution File Not Found
If build fails to find solution:
1. Verify `BusBuddy.sln` exists in repository root
2. Check workflow logs for file search results
3. Ensure repository was checked out properly

## Local Testing

To test coverage generation locally:
```powershell
# Quick coverage check
.\run-coverage.bat

# Detailed coverage report
pwsh .\generate-coverage.ps1
```

## Workflow Files

- **Main workflow**: `.github/workflows/dotnet.yml`
- **Code quality**: `.github/workflows/code-quality.yml`
- **Release management**: `.github/workflows/release.yml`
- **Dependabot auto-approval**: `.github/workflows/dependabot-auto-approve.yml`
- **Security scanning**: `.github/workflows/dependency-security-scan.yml`
- **Coverage config**: `codecov.yml`
- **Test settings**: `coverlet.runsettings`
- **Dependency config**: `.github/dependabot.yml`

## Support

For issues with the workflow:
1. Check the workflow run logs
2. Verify all secrets are configured
3. Ensure repository structure matches expectations
4. Test locally using the provided scripts
