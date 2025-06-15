# PowerShell script to fix constructor calls in test files
# This will temporarily comment out constructor arguments to get a clean build

Write-Host "Fixing constructor calls in test files..."

# Fix FuelManagementForm calls (3 args -> parameterless)
$files = Get-ChildItem -Path "BusBuddy.Tests" -Filter "*.cs" -Recurse
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw

    # Fix FuelManagementForm calls
    $content = $content -replace 'new FuelManagementForm\([^)]+\)', 'new FuelManagementForm()'

    # Fix MaintenanceManagementForm calls
    $content = $content -replace 'new MaintenanceManagementForm\([^)]+\)', 'new MaintenanceManagementForm()'

    # Fix RouteManagementForm calls
    $content = $content -replace 'new RouteManagementForm\([^)]+\)', 'new RouteManagementForm()'

    # Fix VehicleRepository calls
    $content = $content -replace 'new VehicleRepository\([^)]+\)', 'new VehicleRepository()'

    # Fix DatabaseHelperService calls (9 args)
    $content = $content -replace 'new DatabaseHelperService\([^)]+\)', 'new DatabaseHelperService()'

    Set-Content $file.FullName $content
}

Write-Host "Constructor calls fixed. Please build solution to verify."
