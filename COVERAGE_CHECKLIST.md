# 🚌 BusBuddy Code Coverage Verification Checklist

## Pre-Setup Verification

### ✅ **Step 1: Verify Extensions**
- [ ] Coverage Gutters extension installed (`ryanluker.vscode-coverage-gutters`)
- [ ] C# extension installed (`ms-dotnettools.csharp`)
- [ ] .NET Core Test Explorer installed (optional but helpful)

### ✅ **Step 2: Verify Project Structure**
- [ ] Solution builds without errors: `dotnet build`
- [ ] Tests run successfully: `dotnet test`
- [ ] BusBuddy.Tests project contains test files
- [ ] No test files in BusBuddy.UI project (they should be in Tests project)

## Core Setup Process

### ✅ **Step 3: Generate Coverage**
Run one of these methods:

**Method A: Using the automated script (Recommended)**
```bash
.\run-coverage.ps1 -Clean
```

**Method B: Manual dotnet command**
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov,cobertura /p:CoverletOutput=./TestResults/coverage
```

**Method C: Using VS Code Tasks**
- Press `Ctrl+Shift+P`
- Type "Tasks: Run Task"
- Select "Generate Code Coverage"

### ✅ **Step 4: Verify Coverage Files**
Check that these files exist:
- [ ] `./TestResults/coverage.info` (LCOV format)
- [ ] `./TestResults/coverage.cobertura.xml` (Cobertura format)
- [ ] Files are not empty (should be > 1KB)

### ✅ **Step 5: Display Coverage in VS Code**

**Method A: Using keyboard shortcut**
- [ ] Press `Ctrl+Shift+C` to display coverage

**Method B: Using Command Palette**
- [ ] Press `Ctrl+Shift+P`
- [ ] Type "Coverage Gutters: Display Coverage"
- [ ] Execute the command

**Method C: Using Status Bar**
- [ ] Look for coverage percentage in status bar
- [ ] Click to toggle coverage display

### ✅ **Step 6: Enable Auto-Watch**
- [ ] Press `Ctrl+Shift+W` OR
- [ ] Command Palette → "Coverage Gutters: Watch"

## Visual Verification

### ✅ **Step 7: Confirm Coverage Display**
You should see:
- [ ] 🟢 Green line numbers = Covered code
- [ ] 🔴 Red line numbers = Uncovered code
- [ ] 🟡 Yellow line numbers = Partially covered (branches)
- [ ] Coverage percentage in status bar
- [ ] Gutters showing hit counts (small numbers next to line numbers)

### ✅ **Step 8: Test Coverage Accuracy**
- [ ] Open a file with tests (should show green lines)
- [ ] Open a file without tests (should show red lines)
- [ ] Hover over yellow lines to see branch coverage details

## Troubleshooting

### ❌ **If coverage isn't showing:**

1. **Check file existence:**
   ```bash
   Get-ChildItem -Recurse -Filter "*coverage*"
   ```

2. **Manually load coverage file:**
   - Command Palette → "Coverage Gutters: Load Coverage File"
   - Browse to `./TestResults/coverage.info`

3. **Reset and retry:**
   - Command Palette → "Coverage Gutters: Remove Coverage"
   - Wait 2 seconds
   - Command Palette → "Coverage Gutters: Display Coverage"

4. **Check VS Code settings:**
   - Verify `.vscode/settings.json` has correct coverage file paths
   - Check that file associations are correct

### ❌ **If tests fail:**
- Fix build errors first: `dotnet build`
- Ensure all test dependencies are installed
- Check that test files are in the correct project

### ❌ **If coverage is incomplete:**
- Ensure all projects have `coverlet.collector` package
- Check that test project references the main projects
- Verify exclude patterns aren't too aggressive

## Success Criteria

### 🎯 **Coverage is working correctly when:**
- [ ] Status bar shows coverage percentage (e.g., "Coverage: 75%")
- [ ] Line numbers are colored (green/red/yellow)
- [ ] Coverage updates when you run tests
- [ ] Hit counts appear in gutters
- [ ] Hovering shows coverage details

### 🎯 **Common Coverage Targets:**
- [ ] Line coverage > 70%
- [ ] Branch coverage > 60%
- [ ] Critical business logic > 90%

## Advanced Features

### 📊 **Additional Coverage Commands:**
- `Ctrl+Shift+R` - Remove coverage display
- Command Palette → "Coverage Gutters: Preview Coverage Report" - HTML report
- Command Palette → "Coverage Gutters: Toggle Coverage" - Quick on/off

### 📈 **Coverage Reports:**
- LCOV info files can be uploaded to services like Codecov
- Cobertura XML works with Azure DevOps and GitHub Actions
- HTML reports can be generated for detailed analysis

---

## Quick Reference Card

| Action | Keyboard Shortcut | Command Palette |
|--------|------------------|-----------------|
| Generate Coverage | `Ctrl+Shift+T` | Tasks: Run Task → Generate Code Coverage |
| Display Coverage | `Ctrl+Shift+C` | Coverage Gutters: Display Coverage |
| Watch Coverage | `Ctrl+Shift+W` | Coverage Gutters: Watch |
| Remove Coverage | `Ctrl+Shift+R` | Coverage Gutters: Remove Coverage |

---

✅ **Mark this checklist complete when you can see colored line numbers in your C# files!**
