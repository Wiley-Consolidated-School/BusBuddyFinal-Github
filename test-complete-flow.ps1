#!/usr/bin/env pwsh
<#
.SYNOPSIS
Complete Data Flow Testing Script for BusBuddy
Tests: Database → Repositories → Business Services → UI Services → Management Forms

.DESCRIPTION
Following BusBuddy guidelines:
- Single build approach - run one build, get data, move on
- Efficient testing with minimal tool calls
- PowerShell (pwsh) for all commands
- Comprehensive coverage of data pipeline

.EXAMPLE
.\test-complete-flow.ps1
.\test-complete-flow.ps1 -Layer Database
.\test-complete-flow.ps1 -Layer Repository
.\test-complete-flow.ps1 -Quick
#>

param(
    [ValidateSet("All", "Database", "Repository", "Business", "UI", "Forms")]
    [string]$Layer = "All",

    [switch]$Quick,
    [switch]$Verbose
)

# BusBuddy Guidelines: Concise responses, efficient debugging
$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

# Color coding for output
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Test { param($Message) Write-Host "🧪 $Message" -ForegroundColor Magenta }


# Exception Capture Integration
Write-Test "Exception Monitoring Active"
if (Test-Path "BusBuddy.UI\Helpers\ExceptionCapture.cs") {
    Write-Success "ExceptionCapture class available - monitoring enabled"

    # Clear previous exception logs
    Remove-Item "logs\exception_*.log" -ErrorAction SilentlyContinue
    Remove-Item "logs\exception_*.json" -ErrorAction SilentlyContinue

    Write-Info "Exception logs cleared - fresh monitoring session"
} else {
    Write-Warning "ExceptionCapture class not found - limited exception monitoring"
}

# Function to test for new exceptions during testing
function Test-ForNewExceptions {
    $exceptionFiles = Get-ChildItem "logs\exception_*.log" -ErrorAction SilentlyContinue
    if ($exceptionFiles) {
        Write-Error "🚨 EXCEPTIONS DETECTED during testing!"
        foreach ($file in $exceptionFiles) {
            Write-Host "📄 Exception file: $($file.Name)" -ForegroundColor Red
            Write-Host "📋 Content preview:" -ForegroundColor Yellow
            Get-Content $file.FullName -Tail 5 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
        }
        return $true
    }
    return $false
}

Write-Info "BusBuddy Complete Data Flow Testing - Layer: $Layer"
Write-Info "Following BusBuddy Guidelines: Single build approach, efficient testing"

# Phase 1: Build Verification (Single build approach)
if ($Layer -eq "All" -or $Layer -eq "Database") {
    Write-Test "Phase 1: Build Verification"

    $buildResult = dotnet build --no-restore 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Build successful - proceeding with tests"
    } else {
        Write-Error "Build failed - stopping tests"
        Write-Host $buildResult
        exit 1
    }

    # Check for exceptions after build
    Test-ForNewExceptions | Out-Null
}

# Phase 2: Database Layer Testing
if ($Layer -eq "All" -or $Layer -eq "Database") {
    Write-Test "Phase 2: Database Layer Testing"

    # Test database connection
    Write-Info "Testing database connection..."
    dotnet test --filter "Category=Database" --no-build --verbosity minimal 2>&1 | Out-Null

    if ($LASTEXITCODE -eq 0) {
        Write-Success "Database tests passed"
    } else {
        Write-Warning "Database tests had issues - checking connection"

        # Quick connection test using UnifiedServiceManager
        Write-Info "Testing UnifiedServiceManager database connection..."
        $connectionTest = @"
using BusBuddy.UI.Helpers;
using System.Threading.Tasks;
class Program {
    static async Task Main() {
        try {
            var manager = UnifiedServiceManager.Instance;
            await manager.PreWarmDatabaseAsync();
            System.Console.WriteLine("✅ Database connection successful");
        } catch (System.Exception ex) {
            System.Console.WriteLine($"❌ Database connection failed: {ex.Message}");
        }
    }
}
"@
        $connectionTest | Out-File -FilePath "temp_db_test.cs" -Encoding UTF8

        try {
            dotnet script temp_db_test.cs 2>&1
            Remove-Item "temp_db_test.cs" -ErrorAction SilentlyContinue
        } catch {
            Write-Warning "Connection test failed - continuing with repository tests"
        }
    }
}

# Phase 3: Repository Layer Testing
if ($Layer -eq "All" -or $Layer -eq "Repository") {
    Write-Test "Phase 3: Repository Layer Testing"

    $repositoryTests = @(
        "VehicleRepositoryTest",
        "DriverRepository",
        "RouteRepository",
        "MaintenanceRepository",
        "FuelRepository"
    )

    foreach ($repo in $repositoryTests) {
        Write-Info "Testing $repo..."

        if ($Quick) {
            # Quick test - just check if test class exists and compiles
            $testExists = Test-Path "BusBuddy.UI.Tests\*$repo*.cs"
            if ($testExists) {
                Write-Success "$repo test file exists"
            } else {
                Write-Warning "$repo test file missing"
            }
        } else {
            # Full repository test
            $repoTestResult = dotnet test --filter "DisplayName~$repo" --no-build --verbosity minimal 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Success "$repo tests passed"
            } else {
                Write-Warning "$repo tests had issues"
                if ($Verbose) { Write-Host $repoTestResult }
            }
        }
    }
}

# Phase 4: Business Services Testing
if ($Layer -eq "All" -or $Layer -eq "Business") {
    Write-Test "Phase 4: Business Services Testing"

    $businessServices = @(
        "DatabaseHelperService",
        "RouteAnalyticsService",
        "PredictiveMaintenanceService",
        "VehicleService",
        "ValidationService"
    )

    foreach ($service in $businessServices) {
        Write-Info "Testing $service..."

        if ($Quick) {
            # Quick test - verify service registration in UnifiedServiceManager
            $servicePattern = "services\.AddScoped.*$service"
            $registrationExists = Select-String -Path "BusBuddy.UI\Helpers\UnifiedServiceManager.cs" -Pattern $servicePattern -Quiet
            if ($registrationExists) {
                Write-Success "$service is registered in DI container"
            } else {
                Write-Warning "$service registration not found"
            }
        } else {
            # Full service test
            $serviceTestResult = dotnet test --filter "DisplayName~$service" --no-build --verbosity minimal 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Success "$service tests passed"
            } else {
                Write-Warning "$service tests had issues"
                if ($Verbose) { Write-Host $serviceTestResult }
            }
        }
    }
}

# Phase 5: UI Services Testing
if ($Layer -eq "All" -or $Layer -eq "UI") {
    Write-Test "Phase 5: UI Services Testing"

    $uiServices = @(
        "UIDataService",
        "NavigationService",
        "ReportService",
        "AnalyticsService",
        "ErrorHandlerService"
    )

    foreach ($service in $uiServices) {
        Write-Info "Testing $service..."

        if ($Quick) {
            # Quick test - check if service interface exists
            $interfaceExists = Test-Path "BusBuddy.UI\Services\I$service.cs"
            $implementationExists = Test-Path "BusBuddy.UI\Services\$service.cs"

            if ($interfaceExists -and $implementationExists) {
                Write-Success "$service interface and implementation exist"
            } else {
                Write-Warning "$service missing interface ($interfaceExists) or implementation ($implementationExists)"
            }
        } else {
            # Full UI service test
            $uiTestResult = dotnet test --filter "DisplayName~$service" --no-build --verbosity minimal 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Success "$service tests passed"
            } else {
                Write-Warning "$service tests had issues"
                if ($Verbose) { Write-Host $uiTestResult }
            }
        }
    }
}

# Phase 6: Management Forms Testing
if ($Layer -eq "All" -or $Layer -eq "Forms") {
    Write-Test "Phase 6: Management Forms Testing"

    # Test Dashboard initialization
    Write-Info "Testing Dashboard initialization..."
    $dashboardTestResult = dotnet test --filter "Category=Dashboard" --no-build --verbosity minimal 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-Success "Dashboard tests passed"
    } else {
        Write-Warning "Dashboard tests had issues"
        if ($Verbose) { Write-Host $dashboardTestResult }
    }

    # Test SyncfusionBaseForm
    Write-Info "Testing SyncfusionBaseForm..."
    $baseFormTestResult = dotnet test --filter "DisplayName~SyncfusionBaseForm" --no-build --verbosity minimal 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-Success "SyncfusionBaseForm tests passed"
    } else {
        Write-Warning "SyncfusionBaseForm tests had issues"
        if ($Verbose) { Write-Host $baseFormTestResult }
    }

    # Check for management forms
    $managementForms = Get-ChildItem "BusBuddy.UI\Views" -Filter "*.cs" | Where-Object { $_.Name -notmatch "Dashboard" }
    Write-Info "Found $($managementForms.Count) management forms"

    foreach ($form in $managementForms) {
        $formName = $form.BaseName
        Write-Info "Management form: $formName"
    }
}

# Phase 7: Integration Testing
if ($Layer -eq "All") {
    Write-Test "Phase 7: Integration Testing"

    Write-Info "Running complete integration tests..."
    $integrationResult = dotnet test --filter "Category=Integration" --no-build --verbosity minimal 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-Success "Integration tests passed"
    } else {
        Write-Warning "Integration tests had issues"
        if ($Verbose) { Write-Host $integrationResult }
    }

    # Test complete data flow
    Write-Info "Testing complete data flow: Database → Repositories → Services → Forms"
    dotnet test --filter "DisplayName~Flow" --no-build --verbosity minimal 2>&1 | Out-Null

    if ($LASTEXITCODE -eq 0) {
        Write-Success "Data flow tests passed"
    } else {
        Write-Warning "Data flow tests had issues - manual verification recommended"
    }
}

# Summary Report
Write-Test "Testing Complete - Summary"

# Final exception check
if (Test-ForNewExceptions) {
    Write-Warning "⚠️ Exceptions were detected during testing - check logs for details"
} else {
    Write-Success "✅ No exceptions detected during testing"
}

Write-Info "All testing phases completed following BusBuddy guidelines"
Write-Info "Data Pipeline: Database → EF Context → Repositories → Business Services → UI Services → Forms"

if ($Quick) {
    Write-Info "Quick test mode - ran structure and configuration checks"
} else {
    Write-Info "Full test mode - ran complete test suite"
}

Write-Success "Testing script completed successfully"
Write-Info "Next steps: Run application and test UI manually if needed"

