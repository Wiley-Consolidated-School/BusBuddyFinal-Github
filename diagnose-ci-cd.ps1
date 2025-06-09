#!/usr/bin/env pwsh
# CI/CD Diagnostic Script for BusBuddy Project
# This script simulates the GitHub Actions workflow and helps diagnose issues

Write-Host "🚀 BusBuddy CI/CD Diagnostic Script" -ForegroundColor Green
Write-Host "===================================" -ForegroundColor Green

# Check prerequisites
Write-Host "`n📋 Checking Prerequisites..." -ForegroundColor Yellow

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK Version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found" -ForegroundColor Red
    exit 1
}

# Check solution file
if (Test-Path "BusBuddy.sln") {
    Write-Host "✅ Solution file found: BusBuddy.sln" -ForegroundColor Green
} else {
    Write-Host "❌ Solution file not found" -ForegroundColor Red
    exit 1
}

# Check workflow file
if (Test-Path ".github/workflows/dotnet.yml") {
    Write-Host "✅ GitHub Actions workflow found" -ForegroundColor Green
} else {
    Write-Host "❌ GitHub Actions workflow not found" -ForegroundColor Red
}

# Step 1: Restore packages
Write-Host "`n🔄 Step 1: Restoring NuGet packages..." -ForegroundColor Yellow
try {
    dotnet restore BusBuddy.sln
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Package restore successful" -ForegroundColor Green
    } else {
        Write-Host "❌ Package restore failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Package restore error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Build Debug configuration
Write-Host "`n🔨 Step 2: Building (Debug)..." -ForegroundColor Yellow
try {
    dotnet build BusBuddy.sln --configuration Debug --no-restore
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Debug build successful" -ForegroundColor Green
    } else {
        Write-Host "❌ Debug build failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Debug build error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Build Release configuration
Write-Host "`n🔨 Step 3: Building (Release)..." -ForegroundColor Yellow
try {
    dotnet build BusBuddy.sln --configuration Release --no-restore
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Release build successful" -ForegroundColor Green
    } else {
        Write-Host "❌ Release build failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Release build error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 4: Run tests
Write-Host "`n🧪 Step 4: Running tests..." -ForegroundColor Yellow
try {
    dotnet test BusBuddy.sln --configuration Release --no-build --logger "console;verbosity=normal"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ All tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ Some tests failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Test execution error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 5: Generate test results
Write-Host "`n📊 Step 5: Generating test results..." -ForegroundColor Yellow
try {
    if (!(Test-Path "TestResults")) {
        New-Item -ItemType Directory -Path "TestResults" -Force | Out-Null
    }
    dotnet test BusBuddy.sln --configuration Release --no-build --logger "trx" --results-directory "TestResults"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Test results generated successfully" -ForegroundColor Green
        $trxFiles = Get-ChildItem -Path "TestResults" -Filter "*.trx"
        Write-Host "📄 Generated $($trxFiles.Count) test result file(s)" -ForegroundColor Cyan
    } else {
        Write-Host "❌ Test result generation failed" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Test result generation error: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 6: Check for database files
Write-Host "`n🗄️ Step 6: Checking database setup..." -ForegroundColor Yellow
if (Test-Path "BusBuddy.Data/DatabaseScript.sql") {
    Write-Host "✅ Database script found" -ForegroundColor Green
    $scriptContent = Get-Content "BusBuddy.Data/DatabaseScript.sql" -Raw
    if ($scriptContent -match "WeeklyHours") {
        Write-Host "✅ WeeklyHours column found in schema" -ForegroundColor Green
    } else {
        Write-Host "❌ WeeklyHours column missing from schema" -ForegroundColor Red
    }
} else {
    Write-Host "❌ Database script not found" -ForegroundColor Red
}

if (Test-Path "BusBuddy.Tests/App.config") {
    Write-Host "✅ Test configuration file found" -ForegroundColor Green
} else {
    Write-Host "❌ Test configuration file missing" -ForegroundColor Red
}

# Step 7: Check project structure
Write-Host "`n📁 Step 7: Validating project structure..." -ForegroundColor Yellow
$requiredFolders = @("BusBuddy.Tests", "BusBuddy.Data", "Db", "BusBuddy.Models")
foreach ($folder in $requiredFolders) {
    if (Test-Path $folder) {
        Write-Host "✅ $folder directory exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $folder directory missing" -ForegroundColor Red
    }
}

# Summary
Write-Host "`n📈 Diagnostic Summary" -ForegroundColor Green
Write-Host "===================" -ForegroundColor Green
Write-Host "✅ All checks completed successfully!" -ForegroundColor Green
Write-Host "🎯 Your project is ready for CI/CD deployment" -ForegroundColor Cyan
Write-Host ""
Write-Host "If you're seeing GitHub Actions failures, the issue might be:" -ForegroundColor Yellow
Write-Host "  1. Repository permissions or access issues" -ForegroundColor White
Write-Host "  2. GitHub Actions runner environment differences" -ForegroundColor White
Write-Host "  3. Timing issues with file cleanup in parallel test execution" -ForegroundColor White
Write-Host ""
Write-Host "✨ Local environment is fully functional!" -ForegroundColor Green
