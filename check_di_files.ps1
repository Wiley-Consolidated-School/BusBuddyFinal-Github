# Check specific files that might have been moved and contain test content
$suspiciousFiles = @(
    "ServiceRegistration.cs",
    "DIContainer.cs",
    "DependencyInjection.cs",
    "ContainerSetup.cs"
)

Write-Host "Checking for potentially misplaced DI files:" -ForegroundColor Yellow

foreach ($fileName in $suspiciousFiles) {
    $filePath = Join-Path "c:\Users\steve.mckitrick\Desktop\BusBuddy" $fileName
    if (Test-Path $filePath) {
        Write-Host "Found: $fileName" -ForegroundColor Green
        $content = Get-Content $filePath -Raw

        # Check for test indicators
        if ($content -match "using Xunit|using Moq|\[Fact\]|\[Theory\]|TestMethod|TestClass") {
            Write-Host "  🚨 CONTAINS TEST CONTENT - NEEDS TO BE MOVED!" -ForegroundColor Red

            # Show the problematic lines
            $lines = Get-Content $filePath
            for ($i = 0; $i -lt [Math]::Min(20, $lines.Length); $i++) {
                if ($lines[$i] -match "using Xunit|using Moq|\[Fact\]|\[Theory\]") {
                    Write-Host "    Line $($i+1): $($lines[$i])" -ForegroundColor Red
                }
            }
        } else {
            Write-Host "  ✓ Appears to be legitimate service file" -ForegroundColor Green
        }
    }
}

# Also check for any .cs files that start with "Test" or end with "Test" or "Tests"
Write-Host "`nChecking for obvious test files in main directory:" -ForegroundColor Yellow
Get-ChildItem -Path "c:\Users\steve.mckitrick\Desktop\BusBuddy" -Filter "*.cs" | Where-Object {
    $_.Name -match "^Test|Test\.cs$|Tests\.cs$"
} | ForEach-Object {
    Write-Host "🚨 Found test file in main directory: $($_.Name)" -ForegroundColor Red
}
