# Setup Test Database for BusBuddy
Write-Host "Setting up BusBuddy_Test database..." -ForegroundColor Cyan

try {
    # Try to detect SQL Server instance
    $sqlInstances = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL' -ErrorAction SilentlyContinue

    if ($sqlInstances -eq $null) {
        Write-Host "No SQL Server instances detected on this machine." -ForegroundColor Yellow
        Write-Host "You may need to install SQL Server Express." -ForegroundColor Yellow
    }
    else {
        Write-Host "SQL Server instances found:" -ForegroundColor Green
        $sqlInstances.PSObject.Properties | Where-Object { $_.Name -ne "PSPath" -and $_.Name -ne "PSParentPath" -and $_.Name -ne "PSChildName" } | ForEach-Object {
            Write-Host "  - $($_.Name)" -ForegroundColor Green
        }
    }

    # Run the database creation script
    & "$PSScriptRoot\create-test-database.ps1"

    # Now update the App.config file if needed
    $appConfigPath = "$PSScriptRoot\App.config"
    $appConfig = Get-Content $appConfigPath -Raw

    # Update connection string if needed
    if ($appConfig -match "Server=ST-LPTP9-23\\SQLEXPRESS01") {
        Write-Host "Updating connection string in App.config..." -ForegroundColor Yellow
        $appConfig = $appConfig -replace "Server=ST-LPTP9-23\\SQLEXPRESS01", "Server=.\SQLEXPRESS01"
        Set-Content -Path $appConfigPath -Value $appConfig
        Write-Host "App.config updated successfully." -ForegroundColor Green
    }

    Write-Host "Setup complete. You should now be able to run BusBuddy with the test database." -ForegroundColor Green
}
catch {
    Write-Host "Error during setup: $_" -ForegroundColor Red
}

Write-Host "Press any key to continue..."
$null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
