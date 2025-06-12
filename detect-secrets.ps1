#!/usr/bin/env pwsh
# detect-secrets.ps1 - Basic script to detect potential secrets in code

Write-Host "🔐 Checking for secrets in code..."

# Define patterns to look for
$patterns = @(
    '(?i)password\s*=\s*["\047][^\047"]+["\047]',
    '(?i)pwd\s*=\s*["\047][^\047"]+["\047]',
    '(?i)connectionstring\s*=\s*["\047][^\047"]+["\047]',
    '(?i)username\s*=\s*["\047][^\047"]+["\047]',
    '(?i)api[_\-]?key\s*=\s*["\047][^\047"]+["\047]',
    '(?i)secret\s*=\s*["\047][^\047"]+["\047]',
    '(?i)token\s*=\s*["\047][^\047"]+["\047]'
)

# Check baseline for known allowed secrets
$baselineExists = Test-Path ".secrets.baseline"
$baseline = @()
if ($baselineExists) {
    $baseline = Get-Content ".secrets.baseline"
}

# Find files to scan (C#, config, JSON, XML files)
$files = Get-ChildItem -Path . -Recurse -Include *.cs,*.config,*.json,*.xml -File | Where-Object {
    $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\TestResults\*"
}

$secretsFound = $false
$foundSecrets = @()

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue

    if (-not [string]::IsNullOrWhiteSpace($content)) { # Add this check
        foreach ($pattern in $patterns) {
            $matches = [regex]::Matches($content, $pattern)

            foreach ($match in $matches) {
                $lineNumber = ($content.Substring(0, $match.Index).Split("`n")).Length
                $line = $content.Split("`n")[$lineNumber - 1]
                $secretLine = "$($file.FullName):$lineNumber - $line"

                # Check if this secret is in the baseline (allowed)
                if ($baseline -notcontains $secretLine) {
                    $foundSecrets += $secretLine
                    $secretsFound = $true
                }
            }
        }
    } # End of check for non-empty content
}

if ($secretsFound) {
    Write-Host "❌ Potential secrets found in files:" -ForegroundColor Red
    foreach ($secret in $foundSecrets) {
        Write-Host $secret -ForegroundColor Yellow
    }
    Write-Host "Consider moving sensitive values to environment variables or secure storage."
    Write-Host "If these are false positives, add them to .secrets.baseline file."
    exit 1
} else {
    Write-Host "✅ No secrets detected in code" -ForegroundColor Green
    exit 0
}
