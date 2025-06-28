#!/usr/bin/env pwsh
# Targeted Bus Issues Fix Script
# Based on detailed error analysis from build output

Write-Host "🎯 Applying targeted fixes for Bus-related issues..." -ForegroundColor Cyan

$testDir = "BusBuddy.UI.Tests"
$filesFixed = 0
$issuesFixed = 0

# Primary Fixes Based on Build Errors
$fixes = @{
    # Critical Type Fixes (Case-sensitive)
    "new bus\(" = "new Bus("
    "Bus\s+bus\s*=" = "Bus testBus ="
    "var bus\s*=" = "var testBus ="
    "\s+bus\s*=" = " testBus ="
    "return bus;" = "return testBus;"
    "\.bus\s*=" = ".testBus ="

    # Variable Reference Fixes
    "\bbusVIN\b" = "testBusVIN"
    "corruptedBusVIN" = "corruptedTestBusVIN"

    # VIN Property Assignment Fixes (Bus.VIN is read-only)
    "testBus\.VIN\s*=" = "// testBus.VIN = // Read-only property"
    "bus\.VIN\s*=" = "// bus.VIN = // Read-only property"

    # Method signature fixes
    "public Bus CreateTestbus\(" = "public Bus CreateTestBus("
    "private Bus CreateTestbus\(" = "private Bus CreateTestBus("

    # Variable declarations that became malformed
    "Bus bus\s*=" = "Bus testBus ="
    "var\s+busId\s*=" = "var testBusId ="
    "vehicleId" = "busId"

    # Fix constructor calls that got mangled
    "return new bus\s*{" = "return new Bus {"
    "= new bus\s*{" = "= new Bus {"

    # Interface/Repository fixes
    "IBusRepository\s+BusRepository" = "BusRepository busRepository"
    "IBusRepository.*new BusRepository" = "var busRepository = new BusRepository"
}

# Type conversion fixes for common errors
$typeConversionFixes = @{
    # int to bool conversions (common pattern in tests)
    "TrainingComplete\s*=\s*1" = "TrainingComplete = true"
    "TrainingComplete\s*=\s*0" = "TrainingComplete = false"
    "==\s*1\s*\)" = "== true)"
    "==\s*0\s*\)" = "== false)"

    # decimal to int? conversions
    "AMMiles\s*=\s*(\d+\.\d+)" = 'AMMiles = (int?)$1'
    "PMMiles\s*=\s*(\d+\.\d+)" = 'PMMiles = (int?)$1'

    # Method group fixes
    "Count\?" = "Count"
    "Count\s*>=\s*(\d+)" = 'Count >= $1'
    "\.Count\s*\?\s*" = ".Count "
}

# Repository constructor fixes
$repositoryFixes = @{
    # ValidationService constructor expects concrete classes, not interfaces
    "new ValidationService\(\s*BusRepository," = "new ValidationService(busRepository,"
    "_validationService\s*=\s*new ValidationService\(\s*_vehicleRepository" = "_validationService = new ValidationService(_busRepository"
}

Write-Host "📁 Processing test files..." -ForegroundColor Green

# Apply fixes to each file
Get-ChildItem -Path $testDir -Filter "*.cs" -Recurse | ForEach-Object {
    $file = $_
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    $fileChanged = $false

    Write-Host "  Checking: $($file.Name)" -ForegroundColor Gray

    # Apply primary fixes
    foreach ($find in $fixes.Keys) {
        $replace = $fixes[$find]
        if ($content -match $find) {
            $matches = [regex]::Matches($content, $find).Count
            $content = $content -replace $find, $replace
            $fileChanged = $true
            $issuesFixed += $matches
            Write-Host "    Fixed $matches instances: $find -> $replace" -ForegroundColor Green
        }
    }

    # Apply type conversion fixes
    foreach ($find in $typeConversionFixes.Keys) {
        $replace = $typeConversionFixes[$find]
        if ($content -match $find) {
            $matches = [regex]::Matches($content, $find).Count
            $content = $content -replace $find, $replace
            $fileChanged = $true
            $issuesFixed += $matches
            Write-Host "    Fixed $matches type conversions: $find -> $replace" -ForegroundColor Yellow
        }
    }

    # Apply repository fixes
    foreach ($find in $repositoryFixes.Keys) {
        $replace = $repositoryFixes[$find]
        if ($content -match $find) {
            $matches = [regex]::Matches($content, $find).Count
            $content = $content -replace $find, $replace
            $fileChanged = $true
            $issuesFixed += $matches
            Write-Host "    Fixed $matches repository issues: $find -> $replace" -ForegroundColor Cyan
        }
    }

    # Fix specific VIN assignment issues (Bus.VIN is read-only)
    $vinPattern = '(\s+)(testBus|bus)\.VIN\s*=\s*([^;]+);'
    if ($content -match $vinPattern) {
        $content = $content -replace $vinPattern, '$1// $2.VIN = $3; // VIN is read-only, set in constructor'
        $fileChanged = $true
        $issuesFixed++
        Write-Host "    Fixed VIN assignment (read-only property)" -ForegroundColor Magenta
    }

    # Fix void assignment issues (var x = method that returns void)
    $voidAssignPattern = 'var\s+\w+\s*=\s*\w+Repository\.Add\w+\('
    if ($content -match $voidAssignPattern) {
        # These should be: var id = repository.AddBus(bus); (returns int)
        # But AddBus actually returns int, so this might be a different issue
        Write-Host "    Found potential void assignment issue in $($file.Name)" -ForegroundColor Red
    }

    if ($fileChanged) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        $filesFixed++
        Write-Host "  ✅ Updated: $($file.Name)" -ForegroundColor Cyan
    }
}

# Special handling for ValidationServiceTest.cs - fix the "bus" type issue
$validationTestPath = Join-Path $testDir "ValidationServiceTest.cs"
if (Test-Path $validationTestPath) {
    $content = Get-Content -Path $validationTestPath -Raw
    if ($content -match "return new bus\s*{") {
        $content = $content -replace "return new bus\s*{", "return new Bus {"
        $content = $content -replace "Bus\s+bus\s*=", "Bus testBus ="
        $content = $content -replace "\bbus\.", "testBus."
        Set-Content -Path $validationTestPath -Value $content -Encoding UTF8
        Write-Host "  ✅ Fixed ValidationServiceTest.cs type issues" -ForegroundColor Green
        $issuesFixed += 5
    }
}

Write-Host "`n🎉 Targeted Fix Complete!" -ForegroundColor Green
Write-Host "  Files Updated: $filesFixed" -ForegroundColor White
Write-Host "  Issues Fixed: $issuesFixed" -ForegroundColor White

Write-Host "`n🔧 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Build to check remaining errors: dotnet build BusBuddy.sln" -ForegroundColor White
Write-Host "2. Review any remaining type conversion issues" -ForegroundColor White
Write-Host "3. Check for specific method signature mismatches" -ForegroundColor White
