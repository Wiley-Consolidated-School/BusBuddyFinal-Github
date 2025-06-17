# 🧪 Safe UI Test Runner
# This script runs only the safe UI tests that don't create actual forms

param(
    [string]$Category = "All",
    [switch]$Verbose
)

Write-Host "🧪 Running Safe UI Tests..." -ForegroundColor Cyan

# Change to the project directory
Set-Location "C:\Users\steve.mckitrick\Desktop\BusBuddy"

# Define safe test categories
$safeCategories = @(
    "Navigation",
    "TabControl",
    "StatusPanel",
    "Theme",
    "Typography",
    "Layout",
    "ControlState",
    "Toolbar",
    "DataGrid",
    "EditPanel",
    "MaterialDesign",
    "ResponsiveDesign",
    "FormStructure",
    "InputFields",
    "FieldLabels",
    "Accessibility",
    "DialogBehavior",
    "MaterialEditPanel",
    "MaterialMessageBox",
    "CustomControls",
    "ComponentLayout",
    "ComponentTheming",
    "ComponentAccessibility",
    "ComponentPerformance",
    "DataGridConfig",
    "DataGridTheming",
    "DataGridLayout",
    "DataGridInteraction",
    "DataGridData",
    "DataGridTypography",
    "DataGridPerformance"
)

$loggerLevel = if ($Verbose) { "normal" } else { "minimal" }

if ($Category -eq "All") {
    # Run all safe categories
    foreach ($cat in $safeCategories) {
        Write-Host "Running $cat tests..." -ForegroundColor Yellow
        dotnet test --filter "TestCategory=$cat" --logger "console;verbosity=$loggerLevel"

        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ $cat tests failed." -ForegroundColor Red
        }
    }
} else {
    # Run specific category
    if ($safeCategories -contains $Category) {
        Write-Host "Running $Category tests..." -ForegroundColor Yellow
        dotnet test --filter "TestCategory=$Category" --logger "console;verbosity=$loggerLevel"
    } else {
        Write-Host "❌ Unknown category: $Category" -ForegroundColor Red
        Write-Host "Available categories: $($safeCategories -join ', ')" -ForegroundColor Gray
        exit 1
    }
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Safe UI tests completed successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Some safe UI tests failed." -ForegroundColor Red
}

Write-Host "`n🛡️ Safe tests completed. To run potentially risky form tests, use:" -ForegroundColor Cyan
Write-Host "   dotnet test --filter `"TestCategory=FormLifecycle`"" -ForegroundColor Gray
Write-Host "   (Warning: These may open actual UI windows)" -ForegroundColor Yellow

Write-Host "`n📋 Usage examples:" -ForegroundColor Cyan
Write-Host "   .\run-safe-ui-tests.ps1                    # Run all safe tests" -ForegroundColor Gray
Write-Host "   .\run-safe-ui-tests.ps1 -Category Theme    # Run only theme tests" -ForegroundColor Gray
Write-Host "   .\run-safe-ui-tests.ps1 -Verbose           # Run with verbose output" -ForegroundColor Gray
