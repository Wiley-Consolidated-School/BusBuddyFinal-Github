# Code Coverage Setup and Usage Guide

## Prerequisites Verification Checklist

✅ **Extensions Installed:**
- Coverage Gutters (ryanluker.vscode-coverage-gutters)
- C# (ms-dotnettools.csharp)
- .NET Core Test Explorer (formulahendry.dotnet-test-explorer)

✅ **Packages Installed:**
- coverlet.collector (for test coverage collection)
- coverlet.msbuild (for MSBuild integration)
- MSTest.TestFramework and MSTest.TestAdapter

## Step 1: Generate Coverage Data

### Method 1: Using dotnet test (Recommended)
```bash
# Navigate to solution root
cd "C:\Users\steve.mckitrick\Desktop\BusBuddy"

# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Alternative with specific format
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./TestResults/

# For LCOV format (preferred by Coverage Gutters)
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./TestResults/coverage.info
```

### Method 2: Using MSBuild properties
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover,lcov /p:CoverletOutput=./TestResults/coverage
```

## Step 2: Verify Coverage Files

After running tests, check for these files:
- `./TestResults/coverage.info` (LCOV format)
- `./TestResults/coverage.cobertura.xml` (Cobertura format)
- `./TestResults/{guid}/coverage.cobertura.xml` (XPlat Code Coverage)

## Step 3: Configure Coverage Gutters

### Enable Coverage Display:
1. Press `Ctrl+Shift+P` (Command Palette)
2. Type "Coverage Gutters: Display Coverage"
3. Select and run the command

### Watch for Changes:
1. Press `Ctrl+Shift+P`
2. Type "Coverage Gutters: Watch"
3. This will auto-update coverage when files change

### Status Bar:
- Look for coverage percentage in the status bar
- Click to toggle coverage display

## Step 4: Troubleshooting

### If coverage isn't showing:

1. **Check file paths:**
   ```bash
   # List TestResults contents
   dir TestResults /S
   ```

2. **Verify coverage file format:**
   ```bash
   # Check if coverage files exist
   Get-ChildItem -Recurse -Filter "*.info"
   Get-ChildItem -Recurse -Filter "*coverage*"
   ```

3. **Manual file selection:**
   - Press `Ctrl+Shift+P`
   - Type "Coverage Gutters: Load Coverage File"
   - Browse to your coverage file

4. **Reset Coverage Gutters:**
   - Press `Ctrl+Shift+P`
   - Type "Coverage Gutters: Remove Coverage"
   - Then reload with "Display Coverage"

## Step 5: Understanding Coverage Indicators

- 🟢 **Green line numbers**: Code is covered by tests
- 🔴 **Red line numbers**: Code is not covered by tests
- 🟡 **Yellow line numbers**: Partial coverage (branches)
- ⚪ **No highlighting**: Not executable code

## Automated Script for Easy Coverage

Create a PowerShell script to automate the process:

```powershell
# run-coverage.ps1
Write-Host "Cleaning previous results..." -ForegroundColor Yellow
Remove-Item -Path "./TestResults" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build

Write-Host "Running tests with coverage..." -ForegroundColor Yellow
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

Write-Host "Generating LCOV format..." -ForegroundColor Yellow
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./TestResults/coverage.info

Write-Host "Coverage files generated:" -ForegroundColor Green
Get-ChildItem -Path "./TestResults" -Recurse -Filter "*coverage*" | Select-Object FullName

Write-Host "Now open VS Code and use 'Coverage Gutters: Display Coverage'" -ForegroundColor Cyan
```

## Expected Results

When properly configured, you should see:
1. Line numbers highlighted in green/red in the editor
2. Coverage percentage in the status bar
3. Branch coverage information on hover
4. Coverage gutters showing hit counts

## Common Issues and Solutions

### Issue: "No coverage files found"
**Solution:** Ensure the correct path in settings.json and verify coverage files exist

### Issue: Coverage shows but is outdated
**Solution:** Use "Coverage Gutters: Watch" to auto-refresh

### Issue: Only seeing partial coverage
**Solution:** Ensure all test projects have coverage collection enabled

### Issue: Red/green colors not visible
**Solution:** Check VS Code theme compatibility or adjust Coverage Gutters color settings
