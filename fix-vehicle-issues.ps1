#!/usr/bin/env pwsh
# Auto-generated Vehicle Issue Fix Script
# Generated on 06/28/2025 12:53:12

Write-Host "🔧 Applying targeted fixes for vehicle issues..." -ForegroundColor Cyan

$testDir = "BusBuddy.UI.Tests"
$filesFixed = 0
$issuesFixed = 0

# API Method Replacements (Critical Priority)
$apiReplacements = @{
    'VehicleRepository' = 'BusRepository'
}

# Apply fixes to each file
Get-ChildItem -Path $testDir -Filter "*.cs" -Recurse | ForEach-Object {
    $file = $_
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    $fileChanged = $false
    
    foreach ($find in $apiReplacements.Keys) {
        $replace = $apiReplacements[$find]
        if ($content -match $find) {
            $content = $content -replace $find, $replace
            $fileChanged = $true
            $issuesFixed++
            Write-Host "  Fixed: $find -> $replace in $($file.Name)" -ForegroundColor Green
        }
    }
    
    if ($fileChanged) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        $filesFixed++
        Write-Host "✅ Updated: $($file.Name)" -ForegroundColor Cyan
    }
}

Write-Host "
🎉 Fix Complete!" -ForegroundColor Green
Write-Host "  Files Updated: $filesFixed" -ForegroundColor White
Write-Host "  Issues Fixed: $issuesFixed" -ForegroundColor White
Write-Host "
💡 Run 'dotnet build BusBuddy.sln' to verify fixes" -ForegroundColor Yellow
