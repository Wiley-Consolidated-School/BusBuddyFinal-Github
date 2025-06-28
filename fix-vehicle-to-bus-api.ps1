param()

Write-Host "Starting Vehicle -> Bus API migration in test files..." -ForegroundColor Green

$testFiles = Get-ChildItem -Path "BusBuddy.UI.Tests" -Filter "*.cs" -Recurse

$replacements = @{
    "VehicleRepository" = "BusRepository"
    "AddVehicle" = "AddBus"
    "DeleteVehicle" = "DeleteBus"
    "GetVehicleById" = "GetBusById"
    "GetAllVehicles" = "GetAllBuses"
    "UpdateVehicle" = "UpdateBus"
    "ValidateVehicleAvailability" = "ValidateBusAvailability"
    "IsValidVehicleNumber" = "IsValidBusNumber"
}

$totalReplacements = 0

foreach ($file in $testFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Cyan

    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    foreach ($old in $replacements.Keys) {
        $new = $replacements[$old]
        $before = $content
        $content = $content -replace "\b$old\b", $new

        if ($before -ne $content) {
            $matchCount = ([regex]::Matches($before, "\b$old\b")).Count
            $totalReplacements += $matchCount
            Write-Host "  Replaced $matchCount instances of '$old' -> '$new'" -ForegroundColor Yellow
        }
    }

    if ($originalContent -ne $content) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "  File updated successfully" -ForegroundColor Green
    }
}

Write-Host "`nMigration complete! Total replacements: $totalReplacements" -ForegroundColor Green
Write-Host "Next step: Build and fix any remaining type conversion errors" -ForegroundColor Cyan
