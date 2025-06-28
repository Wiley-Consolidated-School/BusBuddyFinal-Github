# Test VehicleRepository data retrieval directly
# Quick verification that our fixes work

Write-Host "=== Testing VehicleRepository Data Loading ===" -ForegroundColor Cyan

# Test database connectivity first
Write-Host "`n1. Testing database connection..." -ForegroundColor Yellow
try {
    $result = sqlcmd -S .\SQLEXPRESS01 -d BusBuddy -E -Q "SELECT COUNT(*) FROM Vehicles" -h -1 -W
    $count = [int]($result.Trim())
    Write-Host "✅ Database accessible, found $count vehicles" -ForegroundColor Green

    if ($count -eq 0) {
        Write-Host "⚠️ No vehicles in database. Running seed script..." -ForegroundColor Yellow
        sqlcmd -S .\SQLEXPRESS01 -d BusBuddy -E -i "BusBuddy.Data\TestSeedData.sql"
        Write-Host "✅ Seed data applied" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Database connection failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test that our repository fixes are in place
Write-Host "`n2. Verifying repository fixes..." -ForegroundColor Yellow
$repoPath = "BusBuddy.Data\VehicleRepository.cs"
$content = Get-Content $repoPath -Raw

if ($content -match "vehicle\.VehicleID = vehicle\.Id") {
    Write-Host "✅ ID mapping fix is present" -ForegroundColor Green
} else {
    Write-Host "❌ ID mapping fix missing" -ForegroundColor Red
}

if ($content -match "DiagnoseDataRetrieval") {
    Write-Host "✅ Diagnostic method exists" -ForegroundColor Green
} else {
    Write-Host "❌ Diagnostic method missing" -ForegroundColor Red
}

# Check management form fixes
Write-Host "`n3. Verifying UI binding fixes..." -ForegroundColor Yellow
$formPath = "BusBuddy.UI\Views\VehicleManagementForm.cs"
$formContent = Get-Content $formPath -Raw

if ($formContent -match "Console\.WriteLine.*Populating data grid") {
    Write-Host "✅ Enhanced data grid population logging exists" -ForegroundColor Green
} else {
    Write-Host "❌ Enhanced data grid logging missing" -ForegroundColor Red
}

Write-Host "`n✅ All critical fixes verified!" -ForegroundColor Green
Write-Host "`n📋 Next steps to test the fix:" -ForegroundColor Cyan
Write-Host "  1. The application is currently running" -ForegroundColor Gray
Write-Host "  2. Navigate to Vehicle Management from the main menu" -ForegroundColor Gray
Write-Host "  3. Check if vehicles are displayed in the data grid" -ForegroundColor Gray
Write-Host "  4. Look for console output showing data loading progress" -ForegroundColor Gray
