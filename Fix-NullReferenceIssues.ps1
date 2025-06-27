# Bulk fix script for null reference issues found by NullReferenceAnalysisTest
param(
    [switch]$DryRun = $false,
    [string]$Severity = "Critical",  # Critical, High, Medium, All
    [string]$Pattern = "All"         # ControlsAdd, MethodCall, Collection, All
)

$workspaceRoot = $PSScriptRoot

Write-Host "🔧 BusBuddy Null Reference Bulk Fix Tool" -ForegroundColor Cyan
Write-Host "Workspace: $workspaceRoot" -ForegroundColor Gray
Write-Host "Mode: $(if($DryRun){'DRY RUN - No changes will be made'}else{'LIVE - Files will be modified'})" -ForegroundColor $(if($DryRun){'Yellow'}else{'Green'})

# Get all C# files
$sourceFiles = Get-ChildItem -Path $workspaceRoot -Recurse -Filter "*.cs" |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|TestResults)\\' }

$fixesApplied = 0
$filesModified = 0

foreach ($file in $sourceFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    Write-Host "`n📁 Processing: $($file.Name)" -ForegroundColor White

    # Fix 1: Controls.Add() without null checks (CRITICAL)
    if ($Severity -in @("Critical", "All") -and $Pattern -in @("ControlsAdd", "All")) {
        $controlsAddPattern = '(\s*)([a-zA-Z_][a-zA-Z0-9_]*\.Controls\.Add\([^)]+\);)'
        $content = [regex]::Replace($content, $controlsAddPattern, {
            param($match)
            $indent = $match.Groups[1].Value
            $addCall = $match.Groups[2].Value
            $objectName = ($addCall -split '\.')[0]

            return @"
$indent if ($objectName?.Controls != null)
$indent {
$indent     $addCall
$indent }
"@
        })
    }

    # Fix 2: Repository method calls without null checks (HIGH)
    if ($Severity -in @("Critical", "High", "All") -and $Pattern -in @("MethodCall", "All")) {
        $repositoryPattern = '(\s*)([a-zA-Z_][a-zA-Z0-9_]*Repository\.[a-zA-Z][a-zA-Z0-9_]*\([^)]*\))'
        $content = [regex]::Replace($content, $repositoryPattern, {
            param($match)
            $indent = $match.Groups[1].Value
            $methodCall = $match.Groups[2].Value
            $repoName = ($methodCall -split '\.')[0]

            # Check if already has null check in previous lines
            $lines = $content -split "`n"
            for ($i = 0; $i -lt $lines.Length; $i++) {
                if ($lines[$i].Contains($methodCall)) {
                    # Check previous 5 lines for existing null check
                    $hasNullCheck = $false
                    for ($j = [Math]::Max(0, $i-5); $j -lt $i; $j++) {
                        if ($lines[$j] -match "$repoName\s*(!=|is not)\s*null") {
                            $hasNullCheck = $true
                            break
                        }
                    }
                    if (-not $hasNullCheck) {
                        return @"
$indent if ($repoName != null)
$indent {
$indent     $methodCall
$indent }
"@
                    }
                    break
                }
            }
            return $match.Value
        })
    }

    # Fix 3: Service method calls without null checks (HIGH)
    if ($Severity -in @("Critical", "High", "All") -and $Pattern -in @("MethodCall", "All")) {
        $servicePattern = '(\s*)(_[a-zA-Z][a-zA-Z0-9_]*Service\.[a-zA-Z][a-zA-Z0-9_]*\([^)]*\))'
        $content = [regex]::Replace($content, $servicePattern, {
            param($match)
            $indent = $match.Groups[1].Value
            $methodCall = $match.Groups[2].Value
            $serviceName = ($methodCall -split '\.')[0]

            return @"
$indent if ($serviceName != null)
$indent {
$indent     $methodCall
$indent }
"@
        })
    }

    # Fix 4: Collection operations without null checks (HIGH)
    if ($Severity -in @("Critical", "High", "All") -and $Pattern -in @("Collection", "All")) {
        $collectionPattern = '(\s*)([a-zA-Z_][a-zA-Z0-9_]*\.(Count|Length|Clear\(\)|Add\([^)]+\)|Remove\([^)]+\)))'
        $content = [regex]::Replace($content, $collectionPattern, {
            param($match)
            $indent = $match.Groups[1].Value
            $operation = $match.Groups[2].Value
            $objectName = ($operation -split '\.')[0]

            if ($operation.Contains("Count") -or $operation.Contains("Length")) {
                return "$indent ($objectName?.$($operation.Substring($objectName.Length + 1)) ?? 0)"
            } else {
                return @"
$indent if ($objectName != null)
$indent {
$indent     $operation;
$indent }
"@
            }
        })
    }

    # Check if any changes were made
    if ($content -ne $originalContent) {
        $changes = ($content -split "`n").Count - ($originalContent -split "`n").Count
        Write-Host "  ✅ Found fixable issues (+$changes lines)" -ForegroundColor Green

        if (-not $DryRun) {
            Set-Content -Path $file.FullName -Value $content -Encoding UTF8
            $filesModified++
        }
        $fixesApplied++
    } else {
        Write-Host "  ⚪ No fixable issues found" -ForegroundColor Gray
    }
}

Write-Host "`n📊 SUMMARY" -ForegroundColor Cyan
Write-Host "Files processed: $($sourceFiles.Count)" -ForegroundColor White
Write-Host "Files with fixes: $fixesApplied" -ForegroundColor Yellow
if (-not $DryRun) {
    Write-Host "Files modified: $filesModified" -ForegroundColor Green
    Write-Host "`n✅ Bulk fixes applied! Run tests to verify." -ForegroundColor Green
} else {
    Write-Host "DRY RUN COMPLETE - No files were modified" -ForegroundColor Yellow
    Write-Host "Run without -DryRun to apply fixes" -ForegroundColor Gray
}

Write-Host "`n🔍 Next steps:" -ForegroundColor Cyan
Write-Host "1. Run: dotnet test --filter NullReferenceAnalysisTest" -ForegroundColor White
Write-Host "2. Review remaining issues manually" -ForegroundColor White
Write-Host "3. Test application functionality" -ForegroundColor White
