# Verify the data loading fixes in BusBuddy
# This script checks key components of the data loading chain

Write-Host "=== BusBuddy Data Loading Verification ===" -ForegroundColor Cyan

# 1. Check database connectivity
Write-Host "`n1. Testing database connectivity..." -ForegroundColor Yellow
try {
    $result = sqlcmd -S .\SQLEXPRESS01 -d BusBuddy -E -Q "SELECT COUNT(*) as Count FROM Vehicles" -h -1
    if ($result -match '\d+') {
        Write-Host "✅ Database accessible, found $($matches[0]) vehicles" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Database connection failed: $($_.Exception.Message)" -ForegroundColor Red
}

# 2. Check if build artifacts exist
Write-Host "`n2. Checking build artifacts..." -ForegroundColor Yellow
$buildPath = ".\bin\Debug\net8.0-windows"
if (Test-Path "$buildPath\BusBuddy.exe") {
    Write-Host "✅ BusBuddy.exe exists" -ForegroundColor Green
} else {
    Write-Host "❌ BusBuddy.exe not found" -ForegroundColor Red
}

if (Test-Path "$buildPath\BusBuddy.Data.dll") {
    Write-Host "✅ BusBuddy.Data.dll exists" -ForegroundColor Green
} else {
    Write-Host "❌ BusBuddy.Data.dll not found" -ForegroundColor Red
}

# 3. Check key fixes applied
Write-Host "`n3. Verifying fixes applied..." -ForegroundColor Yellow

# Check VehicleRepository diagnostic method
$repoFile = "BusBuddy.Data\VehicleRepository.cs"
if (Test-Path $repoFile) {
    $content = Get-Content $repoFile -Raw
    if ($content -match "DiagnoseDataRetrieval") {
        Write-Host "✅ VehicleRepository diagnostic method exists" -ForegroundColor Green
    }
    if ($content -match "Id, VehicleNumber FROM Vehicles") {
        Write-Host "✅ VehicleRepository uses correct column names" -ForegroundColor Green
    }
    if ($content -match "vehicle\.VehicleID = vehicle\.Id") {
        Write-Host "✅ VehicleRepository has ID mapping fix" -ForegroundColor Green
    }
}

# Check VehicleManagementForm data binding
$formFile = "BusBuddy.UI\Views\VehicleManagementForm.cs"
if (Test-Path $formFile) {
    $content = Get-Content $formFile -Raw
    if ($content -match "PopulateDataGrid.*Console\.WriteLine") {
        Write-Host "✅ VehicleManagementForm has enhanced data grid population" -ForegroundColor Green
    }
}

Write-Host "`n=== Verification Summary ===" -ForegroundColor Cyan
Write-Host "Key fixes applied to resolve data loading issues:" -ForegroundColor White
Write-Host "  • Fixed VehicleRepository column mapping (Id vs VehicleID)" -ForegroundColor Gray
Write-Host "  • Added diagnostic logging to repository methods" -ForegroundColor Gray
Write-Host "  • Enhanced PopulateDataGrid with better error handling" -ForegroundColor Gray
Write-Host "  • Added ID property mapping for database compatibility" -ForegroundColor Gray

Write-Host "`n📋 To test the fix:" -ForegroundColor Yellow
Write-Host "  1. Run the application: & '.\bin\Debug\net8.0-windows\BusBuddy.exe'" -ForegroundColor Gray
Write-Host "  2. Navigate to Vehicle Management from the dashboard" -ForegroundColor Gray
Write-Host "  3. Check console output for diagnostic messages" -ForegroundColor Gray
Write-Host "  4. Verify that vehicles are displayed in the data grid" -ForegroundColor Gray
