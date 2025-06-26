# Generate-XUnitTest.ps1
# This script creates an xUnit test file for a given source file

param (
    [Parameter(Mandatory=$true)]
    [string]$SourceFile,

    [Parameter(Mandatory=$false)]
    [switch]$Force
)

# Ensure source file exists
if (-not (Test-Path $SourceFile)) {
    Write-Host "Source file not found: $SourceFile" -ForegroundColor Red
    exit 1
}

# Extract class name from file name
$className = [System.IO.Path]::GetFileNameWithoutExtension($SourceFile)

# Extract namespace from file content
$content = Get-Content $SourceFile -Raw
$namespaceMatch = [regex]::Match($content, 'namespace\s+([^\s{;]+)')
if (-not $namespaceMatch.Success) {
    Write-Host "Could not determine namespace in source file." -ForegroundColor Red
    exit 1
}
$namespace = $namespaceMatch.Groups[1].Value

# Determine the target test file path
$testDirectory = Join-Path $PSScriptRoot "BusBuddy.UI.Tests"
$testFilePath = Join-Path $testDirectory "$className`Test.cs"

# Check if test file already exists
if ((Test-Path $testFilePath) -and (-not $Force)) {
    Write-Host "Test file already exists at: $testFilePath" -ForegroundColor Yellow
    Write-Host "Use -Force to overwrite." -ForegroundColor Yellow
    exit 0
}

# Create test directory if it doesn't exist
if (-not (Test-Path $testDirectory)) {
    New-Item -ItemType Directory -Path $testDirectory -Force | Out-Null
    Write-Host "Created test directory: $testDirectory" -ForegroundColor Green
}

# Extract public methods from source file
$methodMatches = [regex]::Matches($content, 'public\s+(?:static\s+)?([\w<>[\],\s]+)\s+(\w+)\s*\(([^)]*)\)')
$methods = @()
foreach ($match in $methodMatches) {
    $returnType = $match.Groups[1].Value.Trim()
    $methodName = $match.Groups[2].Value.Trim()
    $parameters = $match.Groups[3].Value.Trim()

    # Skip constructors, properties, and event handlers
    if ($methodName -ne $className -and -not $methodName.StartsWith("set_") -and -not $methodName.StartsWith("get_") -and -not $methodName.EndsWith("Handler")) {
        $methods += @{
            Name = $methodName
            ReturnType = $returnType
            Parameters = $parameters
        }
    }
}

# Create test file content
$testContent = @"
using System;
using System.Drawing;
using System.Windows.Forms;
using Xunit;
using $namespace;

namespace BusBuddy.UI.Tests
{
    public class ${className}Test : IDisposable
    {
        private readonly Form _testForm;
        private readonly Panel _parentPanel;

        public ${className}Test()
        {
            _testForm = new Form();
            _parentPanel = new Panel();
            _testForm.Controls.Add(_parentPanel);
        }

        public void Dispose()
        {
            _testForm?.Dispose();
            _parentPanel?.Dispose();
        }

"@

# Add test regions for each method
foreach ($method in $methods) {
    $testContent += @"

        #region $($method.Name) Tests

        [Fact]
        public void $($method.Name)_WithValidParameters_ReturnsExpectedResult()
        {
            // Arrange

            // Act

            // Assert

        }

        [Fact]
        public void $($method.Name)_WithNullParameter_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => $className.$($method.Name)(null));
        }

        #endregion
"@
}

# Close the class and namespace
$testContent += @"

    }
}
"@

# Write the test file
Set-Content -Path $testFilePath -Value $testContent -Encoding UTF8

Write-Host "Created test file at: $testFilePath" -ForegroundColor Green
Write-Host "Generated test stubs for $($methods.Count) methods" -ForegroundColor Cyan

# Open the file in VS Code if requested
if ($env:OPEN_IN_EDITOR -eq "true") {
    code $testFilePath
}
