# Complete Theme Helper Migration - Handle ALL references
param([switch]$DryRun)

$root = "c:\Users\steve.mckitrick\Desktop\BusBuddy"

Write-Host "🔧 Complete Theme Helper Migration" -ForegroundColor Cyan

# Get all CS files that might have theme helper references
$files = Get-ChildItem -Path $root -Recurse -Include "*.cs" | Where-Object {
    $_.FullName -notmatch "\\bin\\|\\obj\\|\\TestResults\\"
}

$totalChanges = 0
$processedFiles = 0

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
    if (-not $content) { continue }

    $originalContent = $content

    # Replace ALL theme helper patterns with proper equivalents
    $content = $content `
        -replace 'BusBuddyThemeHelper\.MaterialColors\.Primary', 'BusBuddyThemeManager.ThemeColors.GetPrimaryColor(BusBuddyThemeManager.CurrentTheme)' `
        -replace 'BusBuddyThemeHelper\.MaterialColors\.Background', 'BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme)' `
        -replace 'BusBuddyThemeHelper\.MaterialColors\.TextPrimary', 'BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme)' `
        -replace 'BusBuddyThemeHelper\.MaterialColors\.Text\b', 'BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme)' `
        -replace 'BusBuddyThemeHelper\.MaterialColors\.TextSecondary', 'BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme)' `
        -replace 'BusBuddyThemeHelper\.MaterialColors\.Surface', 'BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme)' `
        -replace 'BusBuddyThemeHelper\.MaterialColors\.Border', 'BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme)' `
        -replace 'BusBuddyThemeHelper\.ControlTheming\.ApplyFormTheme\(([^)]+)\)', 'BusBuddyThemeManager.ApplyTheme($1, BusBuddyThemeManager.SupportedThemes.Office2016White)' `
        -replace 'BusBuddyThemeHelper\.Typography\.GetDefaultFont\(\)', 'new Font("Segoe UI", 9, FontStyle.Regular)' `
        -replace 'BusBuddyThemeHelper\.Typography\.GetLabelFont\(\)', 'new Font("Segoe UI", 9, FontStyle.Regular)'

    if ($content -ne $originalContent) {
        $relativePath = $file.FullName.Replace("$root\", "")
        Write-Host "✅ Fixed: $relativePath" -ForegroundColor Green

        if (-not $DryRun) {
            Set-Content -Path $file.FullName -Value $content -NoNewline
        }

        $processedFiles++
        # Count approximate changes (rough estimate)
        $changes = ($originalContent.Length - $content.Length) / 10
        $totalChanges += [Math]::Max(1, [Math]::Abs($changes))
    }
}

Write-Host "📊 Processed $processedFiles files" -ForegroundColor Cyan
if ($DryRun) { Write-Host "🔍 DRY RUN - No changes made" -ForegroundColor Yellow }
