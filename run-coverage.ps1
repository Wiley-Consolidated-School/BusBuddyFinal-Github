#!/usr/bin/env pwsh
# run-coverage.ps1 - Automated Code Coverage Script for BusBuddy

param(
    [string]$Format = "lcov,cobertura",
    [switch]$OpenResults,
    [switch]$Clean
)

# Colors for console output
$Green = "Green"
$Yellow = "Yellow"
$Red = "Red"
$Cyan = "Cyan"

function Write-Step {
    param([string]$Message)
    Write-Host "🔄 $Message" -ForegroundColor $Yellow
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor $Cyan
}

# Main execution
try {
    Write-Host "🚌 BusBuddy Code Coverage Runner" -ForegroundColor $Cyan
    Write-Host "================================" -ForegroundColor $Cyan

    # Step 1: Clean previous results if requested
    if ($Clean -or $PSBoundParameters.ContainsKey('Clean')) {
        Write-Step "Cleaning previous test results..."
        if (Test-Path "./TestResults") {
            Remove-Item -Path "./TestResults" -Recurse -Force
            Write-Success "Previous results cleaned"
        } else {
            Write-Info "No previous results to clean"
        }
    }

    # Step 2: Verify solution exists
    if (-not (Test-Path "BusBuddy.sln")) {
        Write-Error "Solution file not found. Please run from solution root directory."
        exit 1
    }

    # Step 3: Build solution
    Write-Step "Building solution..."
    dotnet build --configuration Debug --no-restore | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed. Please fix build errors before running coverage."
        exit 1
    }
    Write-Success "Build completed successfully"

    # Step 4: Run tests with XPlat Code Coverage
    Write-Step "Running tests with XPlat Code Coverage..."
    dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults --logger trx --no-build

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Tests failed. Coverage may be incomplete."
    } else {
        Write-Success "Tests completed"
    }

    # Step 5: Generate additional coverage formats using Coverlet
    Write-Step "Generating coverage in $Format format(s)..."

    # Create TestResults directory if it doesn't exist
    if (-not (Test-Path "./TestResults")) {
        New-Item -ItemType Directory -Path "./TestResults" -Force | Out-Null
    }

    # Run with Coverlet for additional formats
    $coverletCmd = "dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=$Format /p:CoverletOutput=./TestResults/coverage /p:ExcludeByFile=`"**/*Designer.cs;**/*.g.cs;**/*.g.i.cs`" --no-build"
    Invoke-Expression $coverletCmd

    # Step 6: List generated coverage files
    Write-Step "Locating coverage files..."
    $coverageFiles = @()

    # Find all coverage files
    $patterns = @("*coverage*", "*.info", "*.xml", "*.json")
    foreach ($pattern in $patterns) {
        $files = Get-ChildItem -Path "./TestResults" -Recurse -Filter $pattern -ErrorAction SilentlyContinue
        $coverageFiles += $files
    }

    if ($coverageFiles.Count -eq 0) {
        Write-Error "No coverage files found!"
        Write-Info "Expected locations:"
        Write-Info "  - ./TestResults/coverage.info (LCOV)"
        Write-Info "  - ./TestResults/coverage.cobertura.xml"
        Write-Info "  - ./TestResults/{guid}/coverage.cobertura.xml"
    } else {
        Write-Success "Found $($coverageFiles.Count) coverage file(s):"
        foreach ($file in $coverageFiles) {
            $relativePath = $file.FullName.Replace((Get-Location).Path, ".")
            $size = [math]::Round($file.Length / 1KB, 2)
            Write-Host "  📄 $relativePath ($size KB)" -ForegroundColor $Green
        }
    }

    # Step 7: Show coverage summary if available
    Write-Step "Generating coverage summary..."

    # Try to find and parse Cobertura XML for summary
    $coberturaFile = $coverageFiles | Where-Object { $_.Name -like "*cobertura*" } | Select-Object -First 1
    if ($coberturaFile) {
        try {
            [xml]$xml = Get-Content $coberturaFile.FullName
            $lineRate = [math]::Round([double]$xml.coverage.'line-rate' * 100, 2)
            $branchRate = [math]::Round([double]$xml.coverage.'branch-rate' * 100, 2)

            Write-Success "Coverage Summary:"
            Write-Host "  📊 Line Coverage: $lineRate%" -ForegroundColor $Green
            Write-Host "  🌿 Branch Coverage: $branchRate%" -ForegroundColor $Green
        } catch {
            Write-Info "Could not parse coverage summary from XML"
        }
    }

    # Step 8: VS Code integration instructions
    Write-Host ""
    Write-Host "🎯 Next Steps for VS Code Coverage Gutters:" -ForegroundColor $Cyan
    Write-Host "1. Open VS Code in this directory" -ForegroundColor $Yellow
    Write-Host "2. Press Ctrl+Shift+P and run 'Coverage Gutters: Display Coverage'" -ForegroundColor $Yellow
    Write-Host "3. Press Ctrl+Shift+P and run 'Coverage Gutters: Watch' for auto-refresh" -ForegroundColor $Yellow
    Write-Host ""
    Write-Host "📁 Coverage files are ready in ./TestResults/" -ForegroundColor $Green

    # Step 9: Open results if requested
    if ($OpenResults) {
        Write-Step "Opening TestResults folder..."
        Invoke-Item "./TestResults"
    }

    Write-Success "Coverage generation completed successfully! 🎉"

} catch {
    Write-Error "An error occurred: $($_.Exception.Message)"
    exit 1
}
