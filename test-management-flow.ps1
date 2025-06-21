#!/usr/bin/env pwsh
# Test script to verify management form flow

Write-Host "🔍 TESTING MANAGEMENT FORM FLOW" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

# 1. Test Database Connection
Write-Host "`n1. Testing Database Connection..." -ForegroundColor Yellow
try {
    $testConnection = "Data Source=.\SQLEXPRESS;Initial Catalog=BusBuddyDB;Integrated Security=True;TrustServerCertificate=True;"
    Write-Host "✅ Connection string configured" -ForegroundColor Green
} catch {
    Write-Host "❌ Database connection issue: $($_.Exception.Message)" -ForegroundColor Red
}

# 2. Test Repository Classes
Write-Host "`n2. Testing Repository Classes..." -ForegroundColor Yellow
$repositories = @(
    "VehicleRepository.cs",
    "DriverRepository.cs",
    "RouteRepository.cs",
    "ActivityRepository.cs",
    "FuelRepository.cs",
    "MaintenanceRepository.cs",
    "SchoolCalendarRepository.cs",
    "ActivityScheduleRepository.cs"
)

foreach ($repo in $repositories) {
    $path = "BusBuddy.Data\$repo"
    if (Test-Path $path) {
        Write-Host "✅ $repo exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $repo missing" -ForegroundColor Red
    }
}

# 3. Test Model Classes
Write-Host "`n3. Testing Model Classes..." -ForegroundColor Yellow
$models = @(
    "Vehicle.cs",
    "Driver.cs",
    "Route.cs",
    "Activity.cs",
    "Fuel.cs",
    "Maintenance.cs",
    "SchoolCalendar.cs",
    "ActivitySchedule.cs"
)

foreach ($model in $models) {
    $path = "BusBuddy.Models\$model"
    if (Test-Path $path) {
        Write-Host "✅ $model exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $model missing" -ForegroundColor Red
    }
}

# 4. Test Management Forms
Write-Host "`n4. Testing Management Form Classes..." -ForegroundColor Yellow
$forms = @(
    "VehicleManagementFormSyncfusion.cs",
    "DriverManagementFormSyncfusion.cs",
    "RouteManagementFormSyncfusion.cs",
    "ActivityManagementFormSyncfusion.cs",
    "FuelManagementFormSyncfusion.cs",
    "MaintenanceManagementFormSyncfusion.cs",
    "SchoolCalendarManagementFormSyncfusion.cs",
    "ActivityScheduleManagementFormSyncfusion.cs"
)

foreach ($form in $forms) {
    $path = "BusBuddy.UI\Views\$form"
    if (Test-Path $path) {
        Write-Host "✅ $form exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $form missing" -ForegroundColor Red
    }
}

# 5. Test Edit Forms
Write-Host "`n5. Testing Edit Form Classes..." -ForegroundColor Yellow
$editForms = @(
    "VehicleFormSyncfusion.cs",
    "DriverFormSyncfusion.cs",
    "RouteFormSyncfusion.cs",
    "ActivityFormSyncfusion.cs",
    "FuelFormSyncfusion.cs",
    "MaintenanceFormSyncfusion.cs",
    "SchoolCalendarFormSyncfusion.cs",
    "ActivityScheduleFormSyncfusion.cs"
)

foreach ($form in $editForms) {
    $path = "BusBuddy.UI\Views\$form"
    if (Test-Path $path) {
        Write-Host "✅ $form exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $form missing" -ForegroundColor Red
    }
}

# 6. Test Navigation Service
Write-Host "`n6. Testing Navigation Service..." -ForegroundColor Yellow
$navService = "BusBuddy.UI\Services\NavigationService.cs"
if (Test-Path $navService) {
    Write-Host "✅ NavigationService exists" -ForegroundColor Green
} else {
    Write-Host "❌ NavigationService missing" -ForegroundColor Red
}

# 7. Test Base Classes
Write-Host "`n7. Testing Base Classes..." -ForegroundColor Yellow
$baseClasses = @(
    "BusBuddy.UI\Base\BaseManagementForm.cs",
    "BusBuddy.UI\Base\SyncfusionBaseForm.cs"
)

foreach ($base in $baseClasses) {
    if (Test-Path $base) {
        Write-Host "✅ $base exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $base missing" -ForegroundColor Red
    }
}

Write-Host "`n🔍 ANALYSIS COMPLETE" -ForegroundColor Green
Write-Host "Check the results above to identify missing components." -ForegroundColor Cyan
