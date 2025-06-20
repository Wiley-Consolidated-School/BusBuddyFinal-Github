# BusBuddy Diagnostic Script
# Tests the complete flow from dotnet run to dashboard display

param(
    [switch]$SkipBuild = $false,
    [switch]$Verbose = $false
)

Write-Host "🔍 BusBuddy Diagnostic Script" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan

$rootPath = Get-Location
$logFile = "logs/diagnostics.log"

# Ensure logs directory exists
if (!(Test-Path "logs")) {
    New-Item -ItemType Directory -Path "logs" -Force | Out-Null
}

function Write-Log {
    param($Message, $Level = "INFO", $Color = "White")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    Write-Host $logEntry -ForegroundColor $Color
    Add-Content -Path $logFile -Value $logEntry
}

function Test-FileExists {
    param($Path, $Description)

    if (Test-Path $Path) {
        Write-Log "✅ $Description found: $Path" "CHECK" Green
        return $true
    } else {
        Write-Log "❌ $Description missing: $Path" "ERROR" Red
        return $false
    }
}

function Test-ProjectStructure {
    Write-Log "🏗️ Testing project structure..." "INFO" Cyan

    $requiredDirs = @(
        @{ Path = "BusBuddy.UI"; Desc = "UI Project Directory" },
        @{ Path = "BusBuddy.Business"; Desc = "Business Logic Directory" },
        @{ Path = "BusBuddy.Data"; Desc = "Data Layer Directory" },
        @{ Path = "BusBuddy.Models"; Desc = "Models Directory" },
        @{ Path = "BusBuddy.Tests"; Desc = "Tests Directory" }
    )

    $requiredFiles = @(
        @{ Path = "BusBuddy.csproj"; Desc = "Main Project File" },
        @{ Path = "Program.cs"; Desc = "Application Entry Point" },
        @{ Path = "App.config"; Desc = "Application Configuration" },
        @{ Path = "BusBuddy.UI\Views\BusBuddyDashboardSyncfusion.cs"; Desc = "Main Dashboard Form" }
    )

    $structureOk = $true

    foreach ($dir in $requiredDirs) {
        if (!(Test-FileExists $dir.Path $dir.Desc)) {
            $structureOk = $false
        }
    }

    foreach ($file in $requiredFiles) {
        if (!(Test-FileExists $file.Path $file.Desc)) {
            $structureOk = $false
        }
    }

    return $structureOk
}

function Test-Dependencies {
    Write-Log "📦 Testing dependencies..." "INFO" Cyan

    # Check .csproj for required packages
    $csprojPath = "BusBuddy.csproj"
    if (Test-Path $csprojPath) {
        $content = Get-Content $csprojPath -Raw

        $requiredPackages = @(
            "Syncfusion.SfForm.WinForms",
            "Syncfusion.Themes.MaterialDesign.WinForms",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.EntityFrameworkCore"
        )

        $missingPackages = @()
        foreach ($package in $requiredPackages) {
            if ($content -notmatch $package) {
                $missingPackages += $package
                Write-Log "❌ Missing package reference: $package" "ERROR" Red
            } else {
                Write-Log "✅ Package found: $package" "CHECK" Green
            }
        }

        return $missingPackages.Count -eq 0
    } else {
        Write-Log "❌ Cannot read BusBuddy.csproj" "ERROR" Red
        return $false
    }
}

function Test-Configuration {
    Write-Log "⚙️ Testing configuration..." "INFO" Cyan

    $configOk = $true

    # Check App.config
    $configPath = "App.config"
    if (Test-Path $configPath) {
        $content = Get-Content $configPath -Raw

        # Check for connection string
        if ($content -match "connectionStrings") {
            Write-Log "✅ Connection strings section found" "CHECK" Green
        } else {
            Write-Log "⚠️ No connection strings found in App.config" "WARNING" Yellow
            $configOk = $false
        }

        # Check for Syncfusion license key
        if ($content -match "SyncfusionLicenseKey") {
            Write-Log "✅ Syncfusion license key configured" "CHECK" Green
        } else {
            Write-Log "⚠️ Syncfusion license key not found in App.config" "WARNING" Yellow
        }
    } else {
        Write-Log "❌ App.config not found" "ERROR" Red
        $configOk = $false
    }

    return $configOk
}

function Test-CodeIntegrity {
    Write-Log "🔧 Testing code integrity..." "INFO" Cyan

    $codeOk = $true

    # Check Program.cs
    $programPath = "Program.cs"
    if (Test-Path $programPath) {
        $content = Get-Content $programPath -Raw

        $requiredElements = @(
            @{ Pattern = "SyncfusionLicenseHelper"; Desc = "Syncfusion license initialization" },
            @{ Pattern = "SyncfusionThemeHelper"; Desc = "Syncfusion theme initialization" },
            @{ Pattern = "BusBuddyDashboardSyncfusion"; Desc = "Main dashboard creation" },
            @{ Pattern = "ServiceContainerInstance"; Desc = "Service container usage" }
        )

        foreach ($element in $requiredElements) {
            if ($content -match $element.Pattern) {
                Write-Log "✅ Found: $($element.Desc)" "CHECK" Green
            } else {
                Write-Log "❌ Missing: $($element.Desc)" "ERROR" Red
                $codeOk = $false
            }
        }
    } else {
        Write-Log "❌ Program.cs not found" "ERROR" Red
        $codeOk = $false
    }

    # Check Dashboard form
    $dashboardPath = "BusBuddy.UI\Views\BusBuddyDashboardSyncfusion.cs"
    if (Test-Path $dashboardPath) {
        $content = Get-Content $dashboardPath -Raw

        $requiredMethods = @(
            "InitializeDashboard",
            "CreateBasicLayout",
            "PopulateFormButtons",
            "LoadAnalyticsDataAsync"
        )

        foreach ($method in $requiredMethods) {
            if ($content -match $method) {
                Write-Log "✅ Dashboard method found: $method" "CHECK" Green
            } else {
                Write-Log "❌ Dashboard method missing: $method" "ERROR" Red
                $codeOk = $false
            }
        }
    } else {
        Write-Log "❌ Dashboard form not found" "ERROR" Red
        $codeOk = $false
    }

    return $codeOk
}

function Test-Build {
    Write-Log "🔨 Testing build..." "INFO" Cyan

    try {
        $buildResult = dotnet build --configuration Release --verbosity quiet 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Log "✅ Build successful" "CHECK" Green
            return $true
        } else {
            Write-Log "❌ Build failed" "ERROR" Red
            Write-Log "Build output: $buildResult" "ERROR" Red
            return $false
        }
    }
    catch {
        Write-Log "❌ Build error: $($_.Exception.Message)" "ERROR" Red
        return $false
    }
}

function Test-RuntimeFlow {
    Write-Log "🚀 Testing runtime flow simulation..." "INFO" Cyan

    # This simulates the startup flow without actually running the GUI
    $flowOk = $true

    # Check if helper classes exist
    $helperClasses = @(
        "BusBuddy.UI.Helpers.SyncfusionLicenseHelper",
        "BusBuddy.UI.Helpers.SyncfusionThemeHelper",
        "BusBuddy.DependencyInjection.ServiceContainerInstance"
    )

    foreach ($class in $helperClasses) {
        $parts = $class.Split('.')
        $namespace = $parts[0..($parts.Length-2)] -join '\'
        $className = $parts[-1]

        $searchPath = "$namespace\*$className*.cs"
        $found = Get-ChildItem -Path $searchPath -Recurse -ErrorAction SilentlyContinue

        if ($found) {
            Write-Log "✅ Helper class found: $class" "CHECK" Green
        } else {
            Write-Log "❌ Helper class missing: $class" "ERROR" Red
            $flowOk = $false
        }
    }

    return $flowOk
}

function Show-Recommendations {
    Write-Log "💡 Generating recommendations..." "INFO" Cyan

    Write-Host "`n📋 RECOMMENDATIONS:" -ForegroundColor Magenta
    Write-Host "==================" -ForegroundColor Magenta

    # Check for common issues
    if (!(Test-Path "App.config")) {
        Write-Host "🔧 Create App.config with Syncfusion license key and connection string" -ForegroundColor Yellow
    }

    if (!(Test-Path "logs")) {
        Write-Host "📁 Create logs directory for error logging" -ForegroundColor Yellow
    }

    # Check for Syncfusion packages
    $csproj = Get-Content "BusBuddy.csproj" -Raw -ErrorAction SilentlyContinue
    if ($csproj -and $csproj -notmatch "Syncfusion") {
        Write-Host "📦 Install Syncfusion NuGet packages:" -ForegroundColor Yellow
        Write-Host "   dotnet add package Syncfusion.SfForm.WinForms" -ForegroundColor Gray
        Write-Host "   dotnet add package Syncfusion.Themes.MaterialDesign.WinForms" -ForegroundColor Gray
    }

    Write-Host "`n🚀 TO RUN THE APPLICATION:" -ForegroundColor Green
    Write-Host "==========================" -ForegroundColor Green
    Write-Host "1. Ensure all dependencies are installed: dotnet restore" -ForegroundColor White
    Write-Host "2. Build the project: dotnet build --configuration Release" -ForegroundColor White
    Write-Host "3. Run the application: dotnet run --configuration Release" -ForegroundColor White
    Write-Host "4. Monitor console output for any startup issues" -ForegroundColor White

    Write-Host "`n🔍 TROUBLESHOOTING:" -ForegroundColor Blue
    Write-Host "==================" -ForegroundColor Blue
    Write-Host "• Check logs/error.log for detailed error information" -ForegroundColor White
    Write-Host "• Verify Syncfusion license is properly configured" -ForegroundColor White
    Write-Host "• Ensure SQL Server Express is running (if using SQL Server)" -ForegroundColor White
    Write-Host "• Run cleanup script if MaterialSkin forms are found" -ForegroundColor White
}

# Main execution
Write-Log "Starting BusBuddy diagnostics..." "INFO" Cyan
Write-Log "Root path: $rootPath" "INFO"

$allChecks = @()

# Run all tests
$allChecks += @{ Name = "Project Structure"; Result = Test-ProjectStructure }
$allChecks += @{ Name = "Dependencies"; Result = Test-Dependencies }
$allChecks += @{ Name = "Configuration"; Result = Test-Configuration }
$allChecks += @{ Name = "Code Integrity"; Result = Test-CodeIntegrity }

if (!$SkipBuild) {
    $allChecks += @{ Name = "Build"; Result = Test-Build }
}

$allChecks += @{ Name = "Runtime Flow"; Result = Test-RuntimeFlow }

# Summary
Write-Host "`n📊 DIAGNOSTIC SUMMARY:" -ForegroundColor Yellow
Write-Host "======================" -ForegroundColor Yellow

$passedChecks = 0
$totalChecks = $allChecks.Count

foreach ($check in $allChecks) {
    if ($check.Result) {
        Write-Host "✅ $($check.Name)" -ForegroundColor Green
        $passedChecks++
    } else {
        Write-Host "❌ $($check.Name)" -ForegroundColor Red
    }
}

$successRate = [math]::Round(($passedChecks / $totalChecks) * 100, 1)
Write-Host "`nOverall Health: $passedChecks/$totalChecks ($successRate%)" -ForegroundColor $(if ($successRate -ge 80) { "Green" } elseif ($successRate -ge 60) { "Yellow" } else { "Red" })

if ($successRate -eq 100) {
    Write-Host "🎉 All checks passed! The application should run successfully." -ForegroundColor Green
} elseif ($successRate -ge 80) {
    Write-Host "⚠️ Most checks passed. Address the failed items before running." -ForegroundColor Yellow
} else {
    Write-Host "🚨 Multiple issues detected. Review and fix the problems before running." -ForegroundColor Red
}

Show-Recommendations

Write-Host "`n📝 Detailed log saved to: $logFile" -ForegroundColor Cyan
Write-Host "🏁 Diagnostics complete." -ForegroundColor Cyan
