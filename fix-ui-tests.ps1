# Fix UI Tests for Headless Environments
# Based on Syncfusion testing best practices

Write-Host "🔧 Updating UI tests to support headless environments..." -ForegroundColor Green

# Define the test files to update
$testFiles = @(
    "BusBuddy.Tests\UI\DashboardIntegrationTests.cs",
    "BusBuddy.Tests\UI\EdgeCaseTests.cs",
    "BusBuddy.Tests\UI\DashboardAdvancedTests.cs",
    "BusBuddy.Tests\UI\ComponentTests.cs",
    "BusBuddy.Tests\UI\AdvancedInteractionTests.cs",
    "BusBuddy.Tests\UI\AccessibilityTests.cs",
    "BusBuddy.Tests\UI\ValidationAndRobustnessTests.cs",
    "BusBuddy.Tests\UI\UserInteractionTests.cs",
    "BusBuddy.Tests\UI\PerformanceTests.cs",
    "BusBuddy.Tests\UI\NavigationTests.cs",
    "BusBuddy.Tests\UI\LayoutAndThemeTests.cs",
    "BusBuddy.Tests\UI\IntegrationScenarioTests.cs"
)

$updated = 0
$skipped = 0

foreach ($file in $testFiles) {
    if (Test-Path $file) {
        Write-Host "  📝 Processing: $file" -ForegroundColor Yellow

        try {
            $content = Get-Content $file -Raw

            # Replace [Fact] with [UITestFact]
            $content = $content -replace '\[Fact\]', '[UITestFact]'

            # Replace [Theory] with [UITestTheory]
            $content = $content -replace '\[Theory\]', '[UITestTheory]'

            # Save the updated content
            Set-Content $file $content -NoNewline

            Write-Host "    ✅ Updated successfully" -ForegroundColor Green
            $updated++
        }
        catch {
            Write-Host "    ❌ Error updating: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "  ⚠️ File not found: $file" -ForegroundColor Orange
        $skipped++
    }
}

Write-Host ""
Write-Host "📊 Summary:" -ForegroundColor Cyan
Write-Host "  ✅ Updated: $updated files" -ForegroundColor Green
Write-Host "  ⚠️ Skipped: $skipped files" -ForegroundColor Yellow

Write-Host ""
Write-Host "🎯 Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Build the solution to apply changes"
Write-Host "  2. Run tests - UI tests will be skipped in headless environments"
Write-Host "  3. UI tests will run normally when display is available"

Write-Host ""
Write-Host "✅ UI test updates completed!" -ForegroundColor Green
