# PowerShell script to fix whitespace formatting issues in C# test files

# Function to fix whitespace issues in a file
function Fix-WhitespaceInFile {
    param (
        [string]$FilePath
    )

    Write-Host "Fixing whitespace in: $FilePath"

    $content = Get-Content $FilePath -Raw
    if (-not $content) {
        Write-Host "File is empty or could not be read: $FilePath"
        return
    }

    # Fix common whitespace issues
    # Replace sequences of spaces at start of lines with proper indentation
    $content = $content -replace '(\r?\n)[ ]{1,19}(\S)', '$1            $2'

    # Fix method spacing - ensure blank line before [Fact] attributes
    $content = $content -replace '(\r?\n)(\s*)(\[Fact\])', '$1$2$3'

    # Fix class spacing - ensure blank line before private class definitions
    $content = $content -replace '(\r?\n)(\s*)(private class)', '$1$2$3'

    # Ensure consistent line endings
    $content = $content -replace '\r\n', "`r`n"

    # Write back to file
    [System.IO.File]::WriteAllText($FilePath, $content)
    Write-Host "Fixed whitespace in: $FilePath"
}

# List of files to fix
$filesToFix = @(
    "BusBuddy.Tests\DatabaseInitializerTests.cs",
    "BusBuddy.Tests\ReadmeTest.cs",
    "BusBuddy.Tests\TestConfig.cs",
    "BusBuddy.Tests\VehicleRepositoryTests.cs",
    "BusBuddy.Tests\VehicleServiceTests.cs"
)

Write-Host "Starting whitespace fixes..."

foreach ($file in $filesToFix) {
    $fullPath = Join-Path $PWD $file
    if (Test-Path $fullPath) {
        Fix-WhitespaceInFile -FilePath $fullPath
    } else {
        Write-Host "File not found: $fullPath"
    }
}

Write-Host "Whitespace fixes completed!"
