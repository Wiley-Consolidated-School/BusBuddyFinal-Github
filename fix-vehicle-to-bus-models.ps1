param()

Write-Host "Starting Vehicle -> Bus model migration in test files..." -ForegroundColor Green

$testFiles = Get-ChildItem -Path "BusBuddy.UI.Tests" -Filter "*.cs" -Recurse

$replacements = @{
    "VINNumber" = "VIN"
    "new Vehicle\(" = "new Bus("
    "Vehicle " = "Bus "
    "Vehicle\?" = "Bus?"
    "List<Vehicle>" = "List<Bus>"
    "IEnumerable<Vehicle>" = "IEnumerable<Bus>"
    "Collection<Vehicle>" = "Collection<Bus>"
}

$totalReplacements = 0

foreach ($file in $testFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Cyan

    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    foreach ($old in $replacements.Keys) {
        $new = $replacements[$old]
        $before = $content
        $content = $content -replace $old, $new

        if ($before -ne $content) {
            $matchCount = ([regex]::Matches($before, $old)).Count
            $totalReplacements += $matchCount
            Write-Host "  Replaced $matchCount instances of '$old' -> '$new'" -ForegroundColor Yellow
        }
    }

    if ($originalContent -ne $content) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "  File updated successfully" -ForegroundColor Green
    }
}

Write-Host "`nModel migration complete! Total replacements: $totalReplacements" -ForegroundColor Green
Write-Host "Next step: Build and fix any remaining type conversion errors" -ForegroundColor Cyan
