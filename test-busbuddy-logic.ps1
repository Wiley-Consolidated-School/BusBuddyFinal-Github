# test-busbuddy-logic.ps1
# Automated PowerShell script to test BusBuddy business logic outside the test suite
# No human input required; outputs PASS/FAIL for each scenario

$ErrorActionPreference = "Stop"

function Join-BBPath {
    param([string[]]$parts)
    return ($parts -join [System.IO.Path]::DirectorySeparatorChar)
}

# Try to auto-detect the build output directory for DLLs
$possiblePaths = @(
    Join-BBPath @($PSScriptRoot, 'bin', 'Debug', 'net8.0'),
    Join-BBPath @($PSScriptRoot, 'BusBuddy.Business', 'bin', 'Debug', 'net8.0'),
    Join-BBPath @($PSScriptRoot, 'BusBuddy.Data', 'bin', 'Debug', 'net8.0'),
    Join-BBPath @($PSScriptRoot, 'BusBuddy.Models', 'bin', 'Debug', 'net8.0')
)

$foundPath = $null
foreach ($path in $possiblePaths) {
    if (Test-Path (Join-BBPath @($path, 'BusBuddy.Business.dll'))) {
        $foundPath = $path
        break
    }
}

if (-not $foundPath) {
    Write-Host "ERROR: Could not find BusBuddy DLLs. Please build the solution first." -ForegroundColor Red
    exit 1
}

$businessDll = Join-BBPath @($foundPath, 'BusBuddy.Business.dll')
$dataDll = Join-BBPath @($foundPath, 'BusBuddy.Data.dll')
$modelsDll = Join-BBPath @($foundPath, 'BusBuddy.Models.dll')

# Load assemblies
Add-Type -Path $businessDll
Add-Type -Path $dataDll
Add-Type -Path $modelsDll

# Create repositories and services
$busRepo = New-Object BusBuddy.Data.BusRepository
$driverRepo = New-Object BusBuddy.Data.DriverRepository
$routeRepo = New-Object BusBuddy.Data.RouteRepository
$maintRepo = New-Object BusBuddy.Data.MaintenanceRepository
$fuelRepo = New-Object BusBuddy.Data.FuelRepository
$validation = New-Object BusBuddy.Business.ValidationService($busRepo, $driverRepo, $routeRepo, $maintRepo, $fuelRepo)

function Test-BusCRUD {
    $testBus = New-Object BusBuddy.Models.Bus
    $testBus.BusNumber = "POWERSHELLTEST"
    $testBus.Make = "TestMake"
    $testBus.Model = "TestModel"
    $testBus.Year = 2025
    $testBus.SeatingCapacity = 50
    $testBus.Status = "Active"
    $testBus.LicenseNumber = "PSH123"
    $testBus.DateLastInspection = [DateTime]::Today.AddDays(-10)

    $busId = $busRepo.AddBus($testBus)
    if ($busId -is [int] -and $busId -gt 0) {
        Write-Host "[PASS] AddBus: Bus added with ID $busId"
    } else {
        Write-Host "[FAIL] AddBus: Bus not added"
    }

    $result = $validation.ValidateBusAvailability($busId, [DateTime]::Today, "PowerShell test")
    if ($result.IsValid) {
        Write-Host "[PASS] ValidateBusAvailability: Bus is available"
    } else {
        Write-Host "[FAIL] ValidateBusAvailability: $($result.GetErrorMessage())"
    }

    $busRepo.DeleteBus($busId)
    Write-Host "[INFO] Deleted test bus with ID: $busId"
}

function Test-NullFuelRecord {
    $fuelResult = $validation.ValidateFuelRecord($null)
    if (-not $fuelResult.IsValid -and $fuelResult.GetErrorMessage() -like '*null*') {
        Write-Host "[PASS] Null fuel record validation: $($fuelResult.GetErrorMessage())"
    } else {
        Write-Host "[FAIL] Null fuel record validation: Should not be valid"
    }
}

function Test-NegativeFuelAmount {
    $fuelRecord = New-Object BusBuddy.Models.Fuel
    $fuelRecord.FuelDate = [DateTime]::Today.ToString("yyyy-MM-dd")
    $fuelRecord.FuelLocation = "Test Station"
    $fuelRecord.VehicleFueledID = 1
    $fuelRecord.FuelAmount = -5.0
    $fuelRecord.FuelCost = 87.98
    $result = $validation.ValidateFuelRecord($fuelRecord)
    if (-not $result.IsValid -and $result.GetErrorMessage().ToLower().Contains("negative")) {
        Write-Host "[PASS] Negative fuel amount validation: $($result.GetErrorMessage())"
    } else {
        Write-Host "[FAIL] Negative fuel amount validation: Should not be valid"
    }
}

function Test-MaintenanceRecord {
    $testBus = New-Object BusBuddy.Models.Bus
    $testBus.BusNumber = "POWERSHELLTEST2"
    $testBus.Make = "TestMake"
    $testBus.Model = "TestModel"
    $testBus.Year = 2025
    $testBus.SeatingCapacity = 50
    $testBus.Status = "Active"
    $testBus.LicenseNumber = "PSH124"
    $testBus.DateLastInspection = [DateTime]::Today.AddDays(-10)
    $busId = $busRepo.AddBus($testBus)

    $maintenanceRecord = New-Object BusBuddy.Models.Maintenance
    $maintenanceRecord.Date = [DateTime]::Today.ToString("yyyy-MM-dd")
    $maintenanceRecord.VehicleID = $busId
    $maintenanceRecord.MaintenanceCompleted = "Oil Change"
    $maintenanceRecord.RepairCost = 75.00
    $maintenanceRecord.OdometerReading = 125000
    $result = $validation.ValidateMaintenanceRecord($maintenanceRecord)
    if ($result.IsValid) {
        Write-Host "[PASS] Maintenance record validation: Valid record"
    } else {
        Write-Host "[FAIL] Maintenance record validation: $($result.GetErrorMessage())"
    }
    $busRepo.DeleteBus($busId)
}

function Test-BusNumberValidation {
    $cases = @(
        @{Number="BUS001"; Expected=$true},
        @{Number="V123"; Expected=$true},
        @{Number="AB"; Expected=$false},
        @{Number=""; Expected=$false},
        @{Number=$null; Expected=$false}
    )
    foreach ($case in $cases) {
        $result = $validation.IsValidBusNumber($case.Number)
        if ($result -eq $case.Expected) {
            Write-Host "[PASS] IsValidBusNumber('$($case.Number)') == $($case.Expected)"
        } else {
            Write-Host "[FAIL] IsValidBusNumber('$($case.Number)') != $($case.Expected)"
        }
    }
}

# Run all tests automatically
Test-BusCRUD
Test-NullFuelRecord
Test-NegativeFuelAmount
Test-MaintenanceRecord
Test-BusNumberValidation

Write-Host "[INFO] BusBuddy logic test script completed."
