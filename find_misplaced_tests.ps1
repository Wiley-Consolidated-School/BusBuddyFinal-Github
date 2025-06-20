# Check for any files that might have been moved from BusBuddy.DependencyInjection
Write-Host "Checking main BusBuddy directory for recently moved files:" -ForegroundColor Yellow

# Check all .cs files in main directory
$mainFiles = Get-ChildItem -Path "." -Filter "*.cs" -File

Write-Host "`nC# files in main BusBuddy directory:" -ForegroundColor Green
foreach ($file in $mainFiles) {
    Write-Host "  📄 $($file.Name)" -ForegroundColor White

    # Check if file contains test-related content
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    $hasTestContent = $false

    if ($content -match "using Xunit") { Write-Host "    ✓ Uses Xunit" -ForegroundColor Red; $hasTestContent = $true }
    if ($content -match "using Moq") { Write-Host "    ✓ Uses Moq" -ForegroundColor Red; $hasTestContent = $true }
    if ($content -match "\[Fact\]") { Write-Host "    ✓ Has [Fact] attributes" -ForegroundColor Red; $hasTestContent = $true }
    if ($content -match "\[Theory\]") { Write-Host "    ✓ Has [Theory] attributes" -ForegroundColor Red; $hasTestContent = $true }
    if ($content -match "TestClass|TestMethod") { Write-Host "    ✓ Has test attributes" -ForegroundColor Red; $hasTestContent = $true }

    if ($hasTestContent) {
        Write-Host "    🚨 THIS IS A TEST FILE - NEEDS TO BE MOVED!" -ForegroundColor Red -BackgroundColor Yellow
    }
}

Write-Host "`nChecking if BusBuddy.Tests directory exists and contents:" -ForegroundColor Yellow
if (Test-Path "BusBuddy.Tests") {
    Write-Host "✓ BusBuddy.Tests directory exists" -ForegroundColor Green
    Write-Host "Contents of BusBuddy.Tests:" -ForegroundColor Cyan
    Get-ChildItem -Path "BusBuddy.Tests" -Recurse | Select-Object FullName | ForEach-Object {
        Write-Host "  $($_.FullName)" -ForegroundColor White
    }
} else {
    Write-Host "❌ BusBuddy.Tests directory not found!" -ForegroundColor Red
}
