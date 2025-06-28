# PowerShell script to build and launch just the RouteFormSyncfusion
# This is the simplest approach for testing just the RouteFormSyncfusion

try {
    # Set the current directory to the script location
    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
    Set-Location $scriptPath

    Write-Host "Building and launching RouteFormSyncfusion..." -ForegroundColor Cyan

    # First try to build just the BusBuddy.UI project
    Write-Host "Building BusBuddy.UI project..." -ForegroundColor Yellow
    dotnet build BusBuddy.UI/BusBuddy.UI.csproj

    if ($LASTEXITCODE -ne 0) {
        Write-Host "UI project build failed. Attempting to build complete solution..." -ForegroundColor Yellow
        dotnet build BusBuddy.sln

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Build failed. Cannot run RouteFormSyncfusion." -ForegroundColor Red
            exit $LASTEXITCODE
        }
    }

    # Create a minimal program that just launches the RouteFormSyncfusion
    $tempFolder = Join-Path $scriptPath "temp-route-launcher"
    if (-not (Test-Path $tempFolder)) {
        New-Item -ItemType Directory -Path $tempFolder | Out-Null
    }

    $launcherProjectFile = Join-Path $tempFolder "RouteLauncher.csproj"
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\BusBuddy.UI\BusBuddy.UI.csproj" />
  </ItemGroup>
</Project>
"@ | Out-File -FilePath $launcherProjectFile -Encoding utf8

    $launcherProgramFile = Join-Path $tempFolder "Program.cs"
    @"
using System;
using System.Windows.Forms;
using BusBuddy.UI.Views;

namespace RouteLauncher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXlcdHRdRGNcWENxXkZWYUA=");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new RouteFormSyncfusion());
            }
            catch (Exception ex)
            {
                MessageBox.Show(\$"Error starting form: {ex.Message}\n\n{ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
"@ | Out-File -FilePath $launcherProgramFile -Encoding utf8

    # Build and run the launcher
    Write-Host "Building the Route Form launcher..." -ForegroundColor Yellow
    Set-Location $tempFolder
    dotnet build

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Launching RouteFormSyncfusion..." -ForegroundColor Green
        dotnet run
    } else {
        Write-Host "Failed to build the launcher. Cannot run RouteFormSyncfusion." -ForegroundColor Red
    }

    # Return to original directory
    Set-Location $scriptPath
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
finally {
    # Clean up temporary files (uncomment when everything works)
    # Remove-Item -Path (Join-Path $scriptPath "temp-route-launcher") -Recurse -Force -ErrorAction SilentlyContinue
}
