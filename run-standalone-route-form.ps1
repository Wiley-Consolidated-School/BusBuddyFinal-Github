# PowerShell script to launch RouteFormSyncfusion directly, bypassing the UI project
# This script creates a standalone mini-project that references only what's needed

try {
    # Set the current directory to the script location
    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
    Set-Location $scriptPath

    $tempDir = Join-Path $scriptPath "standalone-route-form"
    if (-not (Test-Path $tempDir)) {
        New-Item -ItemType Directory -Path $tempDir | Out-Null
    }

    Write-Host "Creating standalone RouteForm launcher..." -ForegroundColor Cyan

    # Extract the RouteFormSyncfusion.cs file from the UI project
    $routeFormPath = Join-Path $scriptPath "BusBuddy.UI\Views\RouteFormSyncfusion.cs"

    if (-not (Test-Path $routeFormPath)) {
        Write-Host "Error: RouteFormSyncfusion.cs not found at expected path." -ForegroundColor Red
        exit 1
    }

    # Create a new project that references only the required dependencies
    $projectFile = Join-Path $tempDir "StandaloneRouteForm.csproj"
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Syncfusion.SfInput.WinForms" Version="23.1.43" />
    <PackageReference Include="Syncfusion.Core.WinForms" Version="23.1.43" />
    <PackageReference Include="Syncfusion.SfButton.WinForms" Version="23.1.43" />
    <PackageReference Include="Syncfusion.Tools.Windows" Version="23.1.43" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\BusBuddy.Models\BusBuddy.Models.csproj" />
    <ProjectReference Include="..\BusBuddy.Data\BusBuddy.Data.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Compile Include="..\BusBuddy.UI\Views\RouteFormSyncfusion.cs" Link="RouteFormSyncfusion.cs" />
    <Compile Include="..\BusBuddy.UI\Base\SyncfusionBaseForm.cs" Link="SyncfusionBaseForm.cs" />
    <Compile Include="..\BusBuddy.UI\Helpers\**\*.cs" Link="Helpers\%(RecursiveDir)%(Filename)%(Extension)" />
  </ItemGroup>
</Project>
"@ | Out-File -FilePath $projectFile -Encoding utf8

    # Create a simple Program.cs file
    $programFile = Join-Path $tempDir "Program.cs"
    @"
using System;
using System.Windows.Forms;
using BusBuddy.UI.Views;

namespace StandaloneRouteForm
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
"@ | Out-File -FilePath $programFile -Encoding utf8

    # Also try a simpler approach by just running the TestRouteForm.cs file directly
    Write-Host "Also preparing TestRouteForm.cs as a fallback option..." -ForegroundColor Yellow

    # Make sure TestRouteForm.cs has the required content
    $testFormPath = Join-Path $scriptPath "TestRouteForm.cs"
    $testFormContent = Get-Content $testFormPath -Raw

    if ($testFormContent -match "Application\.Run\(new RouteFormSyncfusion\(\)\);") {
        Write-Host "TestRouteForm.cs already has the correct code." -ForegroundColor Green
    } else {
        Write-Host "Updating TestRouteForm.cs..." -ForegroundColor Yellow
        $updatedContent = @"
using System;
using System.Windows.Forms;
using BusBuddy.UI.Views;

namespace BusBuddy.UI
{
    static class TestRouteForm
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
"@
        Set-Content -Path $testFormPath -Value $updatedContent
    }

    # Now try to compile just the TestRouteForm.cs directly
    Write-Host "Attempting to compile TestRouteForm.cs directly..." -ForegroundColor Cyan
    $testFormOutputDir = Join-Path $scriptPath "test-form-output"

    if (-not (Test-Path $testFormOutputDir)) {
        New-Item -ItemType Directory -Path $testFormOutputDir | Out-Null
    }

    $testFormProjectFile = Join-Path $testFormOutputDir "TestRouteForm.csproj"
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="..\TestRouteForm.cs" Link="TestRouteForm.cs" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\BusBuddy.Models\BusBuddy.Models.csproj" />
    <ProjectReference Include="..\BusBuddy.Data\BusBuddy.Data.csproj" />
    <ProjectReference Include="..\BusBuddy.UI\BusBuddy.UI.csproj" />
  </ItemGroup>
</Project>
"@ | Out-File -FilePath $testFormProjectFile -Encoding utf8

    # First try compiling the standalone form
    Write-Host "Building standalone RouteForm project..." -ForegroundColor Yellow
    Set-Location $tempDir
    $buildOutput = dotnet build 2>&1
    $buildSuccess = $LASTEXITCODE -eq 0

    if ($buildSuccess) {
        Write-Host "Standalone build successful! Running RouteForm..." -ForegroundColor Green
        Start-Process "dotnet" -ArgumentList "run" -WorkingDirectory $tempDir
    } else {
        Write-Host "Standalone build failed. Trying TestRouteForm approach..." -ForegroundColor Yellow

        # Try the TestRouteForm approach
        Set-Location $testFormOutputDir
        $testBuildOutput = dotnet build 2>&1
        $testBuildSuccess = $LASTEXITCODE -eq 0

        if ($testBuildSuccess) {
            Write-Host "TestRouteForm build successful! Running..." -ForegroundColor Green
            Start-Process "dotnet" -ArgumentList "run" -WorkingDirectory $testFormOutputDir
        } else {
            # If all else fails, create a minimal project that just includes the necessary files
            Write-Host "All build attempts failed. Let's try the most minimal approach..." -ForegroundColor Yellow

            $minimalDir = Join-Path $scriptPath "minimal-route-form"
            if (-not (Test-Path $minimalDir)) {
                New-Item -ItemType Directory -Path $minimalDir | Out-Null
            }

            # Copy the essential files from the UI project
            $uiViewsDir = Join-Path $scriptPath "BusBuddy.UI\Views"
            $minimalViewsDir = Join-Path $minimalDir "Views"

            if (-not (Test-Path $minimalViewsDir)) {
                New-Item -ItemType Directory -Path $minimalViewsDir | Out-Null
            }

            Copy-Item -Path $routeFormPath -Destination $minimalViewsDir

            # Create a simplified project
            $minimalProjectFile = Join-Path $minimalDir "MinimalRouteForm.csproj"
            @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Syncfusion.SfInput.WinForms" Version="23.1.43" />
    <PackageReference Include="Syncfusion.Core.WinForms" Version="23.1.43" />
    <PackageReference Include="Syncfusion.SfButton.WinForms" Version="23.1.43" />
    <PackageReference Include="Syncfusion.Tools.Windows" Version="23.1.43" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\BusBuddy.Models\BusBuddy.Models.csproj" />
    <ProjectReference Include="..\BusBuddy.Data\BusBuddy.Data.csproj" />
  </ItemGroup>
</Project>
"@ | Out-File -FilePath $minimalProjectFile -Encoding utf8

            # Create a simplified program
            $minimalProgramFile = Join-Path $minimalDir "Program.cs"
            @"
using System;
using System.Windows.Forms;

namespace MinimalRouteForm
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
                MessageBox.Show("This is the minimal version that would launch the RouteFormSyncfusion.",
                                "Minimal RouteForm", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // In a full implementation, we would run:
                // Application.Run(new BusBuddy.UI.Views.RouteFormSyncfusion());
            }
            catch (Exception ex)
            {
                MessageBox.Show(\$"Error: {ex.Message}\n\n{ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
"@ | Out-File -FilePath $minimalProgramFile -Encoding utf8

            Set-Location $minimalDir
            $minimalBuildOutput = dotnet build 2>&1
            $minimalBuildSuccess = $LASTEXITCODE -eq 0

            if ($minimalBuildSuccess) {
                Write-Host "Minimal build successful! Running minimal RouteForm demo..." -ForegroundColor Green
                Start-Process "dotnet" -ArgumentList "run" -WorkingDirectory $minimalDir
            } else {
                Write-Host "Trying the absolute simplest Syncfusion form as a last resort..." -ForegroundColor Yellow

                # Create an ultra-minimal project with just a basic Syncfusion form
                $ultraMinimalDir = Join-Path $scriptPath "ultra-minimal-form"
                if (-not (Test-Path $ultraMinimalDir)) {
                    New-Item -ItemType Directory -Path $ultraMinimalDir | Out-Null
                }

                # Create a super simple project file
                $ultraMinimalProjectFile = Join-Path $ultraMinimalDir "UltraMinimalForm.csproj"
                @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Syncfusion.Core.WinForms" Version="23.1.43" />
    <PackageReference Include="Syncfusion.SfButton.WinForms" Version="23.1.43" />
  </ItemGroup>
</Project>
"@ | Out-File -FilePath $ultraMinimalProjectFile -Encoding utf8

                # Create a very simple form that just shows a Syncfusion button
                $ultraMinimalFormFile = Join-Path $ultraMinimalDir "UltraMinimalForm.cs"
                @"
using System;
using System.Windows.Forms;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Themes;
using Syncfusion.WinForms.Buttons;

namespace UltraMinimalForm
{
    public class SimpleForm : Form
    {
        private SfButton _button;

        public SimpleForm()
        {
            Text = "Ultra Minimal Syncfusion Form";
            Size = new System.Drawing.Size(400, 300);

            _button = new SfButton();
            _button.Text = "Syncfusion Button";
            _button.Location = new System.Drawing.Point(150, 120);
            _button.Click += (s, e) => MessageBox.Show("This simple form works! Now fix the UI project errors to run the actual RouteFormSyncfusion.");

            Controls.Add(_button);
        }
    }

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
                Application.Run(new SimpleForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(\$"Error: {ex.Message}\n\n{ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
"@ | Out-File -FilePath $ultraMinimalFormFile -Encoding utf8

                Set-Location $ultraMinimalDir
                $ultraMinimalBuildOutput = dotnet build 2>&1
                $ultraMinimalBuildSuccess = $LASTEXITCODE -eq 0

                if ($ultraMinimalBuildSuccess) {
                    Write-Host "Ultra-minimal build successful! Running simple Syncfusion demo..." -ForegroundColor Green
                    Start-Process "dotnet" -ArgumentList "run" -WorkingDirectory $ultraMinimalDir
                } else {
                    Write-Host "All build attempts failed. The UI project has too many dependencies or errors to build or run separately." -ForegroundColor Red
                    Write-Host "You may need to fix the errors in the UI project first before running the RouteForm." -ForegroundColor Red
                }
            }
        }
    }

    # Return to original directory
    Set-Location $scriptPath
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
