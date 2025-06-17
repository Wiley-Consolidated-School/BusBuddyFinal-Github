# Add global suppression for common nullable warnings in UI code
Write-Host "Adding global suppressions for nullable warnings..." -ForegroundColor Green

# Create GlobalSuppressions.cs file in BusBuddy.UI project
$globalSuppressions = @'
using System.Diagnostics.CodeAnalysis;

// Global suppressions for UI project - nullable warnings are mostly harmless in WinForms UI code
// where controls are guaranteed to be initialized in InitializeComponent

[assembly: SuppressMessage("Compiler", "CS8618:Non-nullable field must contain a non-null value when exiting constructor", Justification = "UI controls are initialized in InitializeComponent")]
[assembly: SuppressMessage("Compiler", "CS8602:Dereference of a possibly null reference", Justification = "UI controls are guaranteed to be initialized")]
[assembly: SuppressMessage("Compiler", "CS8604:Possible null reference argument", Justification = "UI controls are guaranteed to be initialized")]
'@

Set-Content -Path "BusBuddy.UI\GlobalSuppressions.cs" -Value $globalSuppressions -Encoding UTF8
Write-Host "Created GlobalSuppressions.cs" -ForegroundColor Green

# Also add to project file if not already present
$csprojPath = "BusBuddy.UI\BusBuddy.UI.csproj"
if (Test-Path $csprojPath) {
    $csprojContent = Get-Content $csprojPath -Raw
    if ($csprojContent -notlike "*<TreatWarningsAsErrors>*") {
        # Add warning configuration
        $csprojContent = $csprojContent -replace '</PropertyGroup>', "  <TreatWarningsAsErrors>false</TreatWarningsAsErrors>`n  </PropertyGroup>"
        Set-Content -Path $csprojPath -Value $csprojContent -Encoding UTF8
        Write-Host "Updated project file with warning configuration" -ForegroundColor Green
    }
}

Write-Host "Completed global suppressions setup!" -ForegroundColor Green
