# Search for test files outside BusBuddy.Tests directory
Write-Host "Searching for test files outside BusBuddy.Tests..." -ForegroundColor Yellow

# Find all .cs files with "Test" in the name outside BusBuddy.Tests
$testFiles = Get-ChildItem -Path "." -Recurse -Filter "*.cs" | Where-Object {
    $_.Name -match "Test" -and $_.FullName -notmatch "BusBuddy\.Tests"
}

Write-Host "`nTest files found outside BusBuddy.Tests:" -ForegroundColor Green
foreach ($file in $testFiles) {
    Write-Host "  $($file.FullName)" -ForegroundColor Cyan
}

# Also check for files that contain Xunit or test attributes
Write-Host "`nSearching for files with test content..." -ForegroundColor Yellow
$allCsFiles = Get-ChildItem -Path "." -Recurse -Filter "*.cs" | Where-Object {
    $_.FullName -notmatch "BusBuddy\.Tests" -and $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\"
}

$filesWithTestContent = @()
foreach ($file in $allCsFiles) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    if ($content -match "using Xunit|using Moq|\[Fact\]|\[Theory\]|\[Test\]|TestMethod|TestClass") {
        $filesWithTestContent += $file
    }
}

Write-Host "`nFiles with test content outside BusBuddy.Tests:" -ForegroundColor Green
foreach ($file in $filesWithTestContent) {
    Write-Host "  $($file.FullName)" -ForegroundColor Cyan
}

Write-Host "`nTotal misplaced test files: $($testFiles.Count + $filesWithTestContent.Count)" -ForegroundColor Red
