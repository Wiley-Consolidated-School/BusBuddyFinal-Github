# format-code.ps1 - Format code and fix whitespace issues
Write-Host "🔧 Formatting BusBuddy codebase..." -ForegroundColor Green

# Apply dotnet format to fix code style issues
Write-Host "📝 Running dotnet format..." -ForegroundColor Yellow
dotnet format BusBuddy.sln --verbosity normal

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Code formatting completed successfully!" -ForegroundColor Green
} else {
    Write-Host "⚠️ Code formatting completed with warnings" -ForegroundColor Yellow
}

# Fix trailing whitespace in C# files
Write-Host "🧹 Fixing trailing whitespace..." -ForegroundColor Yellow
$fixedFiles = 0

Get-ChildItem -Recurse -Include "*.cs" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    if ($content -and $content -match '\s+\r?\n') {
        $fixed = $content -replace '\s+\r?\n', "`r`n"
        $fixed = $fixed -replace '\s+$', ''  # Remove trailing whitespace at end of file
        Set-Content $_.FullName -Value $fixed -NoNewline
        Write-Host "  Fixed: $($_.Name)" -ForegroundColor Cyan
        $fixedFiles++
    }
}

if ($fixedFiles -eq 0) {
    Write-Host "✅ No trailing whitespace issues found!" -ForegroundColor Green
} else {
    Write-Host "✅ Fixed trailing whitespace in $fixedFiles file(s)" -ForegroundColor Green
}

Write-Host "🎯 Code formatting complete!" -ForegroundColor Green
Write-Host "💡 Tip: Consider running this script before committing changes" -ForegroundColor Blue
