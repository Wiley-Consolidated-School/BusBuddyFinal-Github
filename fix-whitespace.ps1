# Fix Trailing Whitespace Script
# This script removes trailing whitespace from source files

param(
    [switch]$WhatIf = $false,
    [string[]]$Include = @("*.cs", "*.md", "*.json", "*.xml", "*.config", "*.ps1")
)

Write-Host "🔧 Fixing Trailing Whitespace..." -ForegroundColor Green

if ($WhatIf) {
    Write-Host "Running in WhatIf mode - no files will be modified" -ForegroundColor Yellow
}

$fixedFiles = 0
$totalFiles = 0

foreach ($pattern in $Include) {
    Get-ChildItem -Recurse -File -Include $pattern | ForEach-Object {
        $totalFiles++
        $filePath = $_.FullName
        $originalContent = Get-Content $filePath -Raw -ErrorAction SilentlyContinue

        if ($originalContent) {
            # Remove trailing whitespace from lines
            $fixedContent = $originalContent -replace '[ \t]+(\r?\n)', '$1'

            # Ensure file ends with exactly one newline
            $fixedContent = $fixedContent.TrimEnd() + "`n"

            # Check if changes were made
            if ($originalContent -ne $fixedContent) {
                if ($WhatIf) {
                    Write-Host "  Would fix: $($_.Name)" -ForegroundColor Yellow
                } else {
                    Set-Content -Path $filePath -Value $fixedContent -NoNewline -Encoding UTF8
                    Write-Host "  Fixed: $($_.Name)" -ForegroundColor Green
                }
                $fixedFiles++
            }
        }
    }
}

if ($WhatIf) {
    Write-Host "`n📊 Summary (WhatIf mode):" -ForegroundColor Cyan
    Write-Host "  Files scanned: $totalFiles" -ForegroundColor White
    Write-Host "  Files that would be fixed: $fixedFiles" -ForegroundColor Yellow
    Write-Host "`n💡 Run without -WhatIf to apply changes" -ForegroundColor Blue
} else {
    Write-Host "`n📊 Summary:" -ForegroundColor Cyan
    Write-Host "  Files scanned: $totalFiles" -ForegroundColor White
    Write-Host "  Files fixed: $fixedFiles" -ForegroundColor Green

    if ($fixedFiles -gt 0) {
        Write-Host "`n✅ Trailing whitespace removed successfully!" -ForegroundColor Green
        Write-Host "💡 Consider committing these changes to git" -ForegroundColor Blue
    } else {
        Write-Host "`n✅ No trailing whitespace found!" -ForegroundColor Green
    }
}
