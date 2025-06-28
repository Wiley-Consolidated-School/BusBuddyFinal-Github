param()

Write-Host "Starting variable name migration from Vehicle to Bus terminology..." -ForegroundColor Green

$testFiles = Get-ChildItem -Path "BusBuddy.UI.Tests" -Filter "*.cs" -Recurse

# Variable name replacements
$variableReplacements = @{
    "testVehicle" = "testBus"
    "vehicle" = "bus"
    "oldVehicle" = "oldBus"
    "originalVehicle" = "originalBus"
    "newVehicle" = "newBus"
    "retrievedVehicle" = "retrievedBus"
    "preRestoreVehicle" = "preRestoreBus"
    "corruptedVehicle" = "corruptedBus"
    "restoredVehicle" = "restoredBus"
    "finalVehicle" = "finalBus"
    "updatedVehicle" = "updatedBus"
    "modifiedVehicle" = "modifiedBus"
    "invalidVehicle" = "invalidBus"
    "nonExistentVehicle" = "nonExistentBus"
    "accessedVehicle" = "accessedBus"
    "vehicleData" = "busData"
}

$totalReplacements = 0

foreach ($file in $testFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Cyan

    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # Replace variable names
    foreach ($old in $variableReplacements.Keys) {
        $new = $variableReplacements[$old]
        $before = $content
        $content = $content -replace "\b$old\b", $new

        if ($before -ne $content) {
            $matchCount = ([regex]::Matches($before, "\b$old\b")).Count
            $totalReplacements += $matchCount
            Write-Host "  Replaced $matchCount instances of '$old' -> '$new'" -ForegroundColor Yellow
        }
    }

    # Fix VIN property assignments - change from .VIN = to constructor parameter or property setting
    $vinBefore = $content
    # Pattern: .VIN = "something"  ->  VIN = "something" (for constructor)
    $content = $content -replace '\.VIN\s*=\s*', 'VIN = '

    if ($vinBefore -ne $content) {
        $vinMatches = ([regex]::Matches($vinBefore, '\.VIN\s*=\s*')).Count
        $totalReplacements += $vinMatches
        Write-Host "  Fixed $vinMatches VIN property assignments" -ForegroundColor Magenta
    }

    if ($originalContent -ne $content) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "  File updated successfully" -ForegroundColor Green
    }
}

Write-Host "`nVariable migration complete! Total replacements: $totalReplacements" -ForegroundColor Green
Write-Host "Next step: Build and fix any remaining constructor issues" -ForegroundColor Cyan
