# PowerShell script to compile and run just the RouteFormSyncfusion
# This is a simplified approach that compiles only the necessary components

try {
    # Set the current directory to the script location
    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
    Set-Location $scriptPath

    Write-Host "Compiling RouteFormSyncfusion test..." -ForegroundColor Cyan

    # Create a temporary directory for the build
    $tempDir = Join-Path $scriptPath "temp-route-form"
    if (-not (Test-Path $tempDir)) {
        New-Item -ItemType Directory -Path $tempDir | Out-Null
    }

    # Compile just the TestRouteForm.cs file with references to required assemblies
    $csc = "csc.exe"
    $references = @(
        "/reference:BusBuddy.Models\bin\Debug\net8.0-windows\BusBuddy.Models.dll",
        "/reference:BusBuddy.Data\bin\Debug\net8.0-windows\BusBuddy.Data.dll",
        "/reference:BusBuddy.Business\bin\Debug\net8.0-windows\BusBuddy.Business.dll",
        "/reference:BusBuddy.UI\bin\Debug\net8.0-windows\BusBuddy.UI.dll",
        "/reference:C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Windows.Forms\v4.0_4.0.0.0__b77a5c561934e089\System.Windows.Forms.dll",
        "/reference:C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System\v4.0_4.0.0.0__b77a5c561934e089\System.dll"
    )

    $compileCommand = "$csc /target:winexe /out:$tempDir\RouteFormTest.exe TestRouteForm.cs $($references -join ' ')"

    Write-Host "Running: $compileCommand" -ForegroundColor Yellow
    Invoke-Expression $compileCommand

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Compilation failed with exit code $LASTEXITCODE" -ForegroundColor Red

        # Alternative approach - try using dotnet to compile
        Write-Host "Attempting alternative compilation with dotnet..." -ForegroundColor Yellow

        # Create a temporary project file
        $tempProjectFile = Join-Path $tempDir "TempRouteFormProject.csproj"
        @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="..\TestRouteForm.cs" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\BusBuddy.Models\BusBuddy.Models.csproj" />
    <ProjectReference Include="..\BusBuddy.Data\BusBuddy.Data.csproj" />
    <ProjectReference Include="..\BusBuddy.Business\BusBuddy.Business.csproj" />
    <ProjectReference Include="..\BusBuddy.UI\BusBuddy.UI.csproj" />
  </ItemGroup>
</Project>
"@ | Out-File -FilePath $tempProjectFile -Encoding utf8

        # Build with dotnet
        Set-Location $tempDir
        dotnet build
        Set-Location $scriptPath

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Alternative compilation failed with exit code $LASTEXITCODE" -ForegroundColor Red
            exit $LASTEXITCODE
        }
    }

    # Run the compiled executable
    Write-Host "Running RouteFormSyncfusion test..." -ForegroundColor Green
    $exePath = Join-Path $tempDir "RouteFormTest.exe"
    if (Test-Path $exePath) {
        Start-Process $exePath
    } else {
        # Try the dotnet version
        $dllPath = Join-Path $tempDir "bin\Debug\net8.0-windows\TempRouteFormProject.dll"
        if (Test-Path $dllPath) {
            dotnet $dllPath
        } else {
            Write-Host "Executable not found. Build may have failed." -ForegroundColor Red
        }
    }
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
finally {
    # Clean up temporary files
    # Uncomment the line below when you're sure everything works
    # Remove-Item -Path (Join-Path $scriptPath "temp-route-form") -Recurse -Force -ErrorAction SilentlyContinue
}
