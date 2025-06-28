# PowerShell script to compile and run the TestRouteForm
# This script compiles and runs just the necessary components for the RouteFormSyncfusion

try {
    # Set the current directory to the script location
    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
    Set-Location $scriptPath

    Write-Host "Compiling TestRouteForm..." -ForegroundColor Cyan

    # Compile the TestRouteForm.cs file
    $compileCommand = 'dotnet publish BusBuddy.sln -c Debug -o ./publish'

    Write-Host "Running: $compileCommand" -ForegroundColor Yellow
    Invoke-Expression $compileCommand

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Compilation failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }

    Write-Host "Running RouteFormSyncfusion..." -ForegroundColor Green

    # Run the published application
    $exePath = Join-Path $scriptPath "publish\BusBuddy.exe"
    if (Test-Path $exePath) {
        Start-Process $exePath
    } else {
        Write-Host "Executable not found at: $exePath" -ForegroundColor Red

        # Alternative: Try running with dotnet
        $dllPath = Join-Path $scriptPath "publish\BusBuddy.dll"
        if (Test-Path $dllPath) {
            Write-Host "Attempting to run with dotnet command..." -ForegroundColor Yellow
            dotnet $dllPath
        } else {
            Write-Host "Application not found. Please check the build output." -ForegroundColor Red
        }
    }
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
