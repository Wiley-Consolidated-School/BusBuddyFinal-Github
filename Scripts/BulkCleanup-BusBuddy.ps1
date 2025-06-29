# BulkCleanup-BusBuddy.ps1
# Standardizes C# file formatting for BusBuddy project
# - Removes trailing whitespace
# - Ensures single newline at end of file
# - Converts line endings to CRLF (Windows)

Get-ChildItem -Path . -Recurse -Include *.cs | ForEach-Object {
    $file = $_.FullName
    # Remove trailing whitespace
    (Get-Content $file) | ForEach-Object { $_.TrimEnd() } |
    Set-Content $file -NoNewline
    # Add a single newline at the end
    Add-Content $file "`n"
    # Convert line endings to CRLF (Windows)
    (Get-Content $file) -replace "`n", "`r`n" | Set-Content $file
}

Write-Host "Bulk cleanup complete: trailing whitespace, line endings, and file endings standardized." -ForegroundColor Green
