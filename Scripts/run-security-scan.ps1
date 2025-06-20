# Security Scan Script for BusBuddy
# Performs automated security scans using various tools

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("All", "StaticAnalysis", "Dependencies", "Secrets", "CodeQL")]
    [string]$ScanType = "All",

    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "./SecurityResults",

    [Parameter(Mandatory=$false)]
    [switch]$Verbose,

    [Parameter(Mandatory=$false)]
    [switch]$ContinueOnFindings
)

# Set error handling
$ErrorActionPreference = "Continue"
if ($Verbose) { $VerbosePreference = "Continue" }

# Colors for output
$Green = "Green"
$Red = "Red"
$Yellow = "Yellow"
$Cyan = "Cyan"
$Blue = "Blue"

function Write-SecurityHeader {
    param([string]$Message)
    Write-Host "`n🔒 === $Message ===" -ForegroundColor $Cyan
    Write-Host "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
}

function Write-SecuritySuccess {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $Green
}

function Write-SecurityWarning {
    param([string]$Message)
    Write-Host "⚠️ $Message" -ForegroundColor $Yellow
}

function Write-SecurityError {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $Red
}

function Write-SecurityInfo {
    param([string]$Message)
    Write-Host "ℹ️ $Message" -ForegroundColor $Blue
}

function Write-SecurityFinding {
    param([string]$Severity, [string]$Message)
    $color = switch ($Severity) {
        "High" { $Red }
        "Medium" { $Yellow }
        "Low" { $Blue }
        default { $Blue }
    }
    Write-Host "🔍 [$Severity] $Message" -ForegroundColor $color
}

# Initialize security scan
Write-SecurityHeader "BusBuddy Security Scan Suite"
Write-SecurityInfo "Scan Type: $ScanType"
Write-SecurityInfo "Output Path: $OutputPath"

# Create output directory
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    Write-SecurityInfo "Created output directory: $OutputPath"
}

# Security scan results tracking
$SecurityResults = @{
    StaticAnalysis = @{ Findings = 0; Severity = @{ High = 0; Medium = 0; Low = 0 } }
    Dependencies = @{ Vulnerabilities = 0; Packages = 0 }
    Secrets = @{ LeakedSecrets = 0; Files = 0 }
    CodeQL = @{ Issues = 0; Rules = 0 }
}

# Function to run static code analysis
function Invoke-StaticAnalysis {
    Write-SecurityHeader "Static Code Analysis"

    try {
        # Run SecurityCodeScan analysis
        Write-SecurityInfo "Running SecurityCodeScan analysis..."

        # Build with security analyzers enabled
        $analysisStart = Get-Date

        $buildOutput = dotnet build BusBuddy.sln --configuration Release --verbosity normal 2>&1
        $analysisDuration = (Get-Date) - $analysisStart

        # Parse security warnings from build output
        $securityWarnings = $buildOutput | Where-Object { $_ -match "warning\s+(SCS\d+|CA\d+)" }

        Write-SecurityInfo "Analysis completed in $($analysisDuration.TotalSeconds.ToString('F2')) seconds"

        if ($securityWarnings.Count -eq 0) {
            Write-SecuritySuccess "No security warnings found in static analysis"
        } else {
            Write-SecurityWarning "Found $($securityWarnings.Count) security warnings"

            foreach ($warning in $securityWarnings) {
                if ($warning -match "(SCS\d+|CA\d+)") {
                    $ruleId = $matches[1]
                    $severity = Get-SecurityRuleSeverity $ruleId
                    Write-SecurityFinding $severity $warning.Trim()
                    $SecurityResults.StaticAnalysis.Severity[$severity]++
                }
            }

            $SecurityResults.StaticAnalysis.Findings = $securityWarnings.Count
        }

        # Save detailed results
        $analysisReport = @{
            timestamp = Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ"
            duration = $analysisDuration.TotalSeconds
            findings = $securityWarnings
            summary = $SecurityResults.StaticAnalysis
        } | ConvertTo-Json -Depth 3

        $analysisReport | Out-File -FilePath (Join-Path $OutputPath "static-analysis.json") -Encoding utf8

    }
    catch {
        Write-SecurityError "Static analysis failed: $($_.Exception.Message)"
    }
}

# Function to scan dependencies for vulnerabilities
function Invoke-DependencyScanning {
    Write-SecurityHeader "Dependency Vulnerability Scanning"

    try {
        # Install dotnet-audit if not available
        if (!(Get-Command dotnet-audit -ErrorAction SilentlyContinue)) {
            Write-SecurityInfo "Installing dotnet-audit tool..."
            dotnet tool install --global dotnet-audit
        }

        Write-SecurityInfo "Scanning NuGet packages for known vulnerabilities..."

        $auditStart = Get-Date
        $auditOutput = dotnet audit BusBuddy.sln --output json 2>&1
        $auditDuration = (Get-Date) - $auditStart

        Write-SecurityInfo "Dependency scan completed in $($auditDuration.TotalSeconds.ToString('F2')) seconds"

        # Parse audit results
        try {
            $auditResults = $auditOutput | ConvertFrom-Json -ErrorAction SilentlyContinue

            if ($auditResults -and $auditResults.vulnerabilities) {
                $vulnCount = $auditResults.vulnerabilities.Count
                Write-SecurityWarning "Found $vulnCount vulnerabilities in dependencies"

                foreach ($vuln in $auditResults.vulnerabilities) {
                    $severity = $vuln.severity
                    Write-SecurityFinding $severity "Package: $($vuln.package), Version: $($vuln.version), CVE: $($vuln.cve)"
                }

                $SecurityResults.Dependencies.Vulnerabilities = $vulnCount
            } else {
                Write-SecuritySuccess "No known vulnerabilities found in dependencies"
            }

            # Count total packages
            $projectFiles = Get-ChildItem -Recurse -Filter "*.csproj"
            $totalPackages = 0
            foreach ($proj in $projectFiles) {
                $content = Get-Content $proj.FullName -Raw
                $packages = [regex]::Matches($content, '<PackageReference\s+Include="[^"]+')
                $totalPackages += $packages.Count
            }

            $SecurityResults.Dependencies.Packages = $totalPackages
            Write-SecurityInfo "Scanned $totalPackages packages across $($projectFiles.Count) projects"

        }
        catch {
            Write-SecurityWarning "Could not parse audit output as JSON, analyzing text output..."

            if ($auditOutput -match "found (\d+) vulnerabilities") {
                $vulnCount = [int]$matches[1]
                $SecurityResults.Dependencies.Vulnerabilities = $vulnCount
                Write-SecurityWarning "Found $vulnCount vulnerabilities (text parsing)"
            } else {
                Write-SecuritySuccess "No vulnerabilities detected (text analysis)"
            }
        }

        # Save dependency scan results
        $dependencyReport = @{
            timestamp = Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ"
            duration = $auditDuration.TotalSeconds
            output = $auditOutput
            summary = $SecurityResults.Dependencies
        } | ConvertTo-Json -Depth 3

        $dependencyReport | Out-File -FilePath (Join-Path $OutputPath "dependency-scan.json") -Encoding utf8

    }
    catch {
        Write-SecurityError "Dependency scanning failed: $($_.Exception.Message)"
    }
}

# Function to scan for secrets and sensitive data
function Invoke-SecretsScanning {
    Write-SecurityHeader "Secrets and Sensitive Data Scanning"

    try {
        Write-SecurityInfo "Scanning for potential secrets and sensitive data..."

        $secretsStart = Get-Date
        $secretsFound = @()
        $filesScanned = 0

        # Define patterns for common secrets
        $secretPatterns = @{
            "API Key" = "(?i)(api[_-]?key|apikey)\s*[:=]\s*['""]?[a-zA-Z0-9]{20,}['""]?"
            "Password" = "(?i)(password|pwd)\s*[:=]\s*['""]?(?!['""]?\s*$)[^'""\\r\\n]{8,}['""]?"
            "Connection String" = "(?i)(connectionstring|connstr)\s*[:=]\s*['""]?[^'""\\r\\n]*password[^'""\\r\\n]*['""]?"
            "Private Key" = "-----BEGIN\s+(RSA\s+)?PRIVATE\s+KEY-----"
            "JWT Token" = "ey[A-Za-z0-9_-]{10,}\.[A-Za-z0-9._-]{10,}\.[A-Za-z0-9._-]{10,}"
            "Email" = "\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b"
            "Phone" = "\b\d{3}[-.]?\d{3}[-.]?\d{4}\b"
        }

        # Scan source files
        $sourceExtensions = @("*.cs", "*.config", "*.json", "*.xml", "*.txt", "*.md")
        foreach ($extension in $sourceExtensions) {
            $files = Get-ChildItem -Recurse -Filter $extension | Where-Object {
                $_.FullName -notmatch "(\\bin\\|\\obj\\|\\TestResults\\|\\packages\\|\\\.git\\)"
            }

            foreach ($file in $files) {
                $filesScanned++
                $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue

                if ($content) {                    foreach ($patternName in $secretPatterns.Keys) {
                        $pattern = $secretPatterns[$patternName]
                        $regexMatches = [regex]::Matches($content, $pattern)

                        foreach ($match in $regexMatches) {
                            # Skip obvious test/example data
                            if ($match.Value -notmatch "(?i)(test|example|sample|demo|placeholder|YOUR_KEY_HERE)") {
                                $secretsFound += @{
                                    File = $file.FullName
                                    Type = $patternName
                                    Match = $match.Value.Substring(0, [Math]::Min(50, $match.Value.Length)) + "..."
                                    Line = ($content.Substring(0, $match.Index) -split "`n").Count
                                }
                            }
                        }
                    }
                }
            }
        }

        $secretsDuration = (Get-Date) - $secretsStart
        Write-SecurityInfo "Secrets scan completed in $($secretsDuration.TotalSeconds.ToString('F2')) seconds"
        Write-SecurityInfo "Scanned $filesScanned files"

        if ($secretsFound.Count -eq 0) {
            Write-SecuritySuccess "No exposed secrets or sensitive data found"
        } else {
            Write-SecurityWarning "Found $($secretsFound.Count) potential secrets/sensitive data exposures"

            foreach ($secret in $secretsFound) {
                Write-SecurityFinding "Medium" "File: $($secret.File), Line: $($secret.Line), Type: $($secret.Type)"
                if ($Verbose) {
                    Write-SecurityInfo "  Match: $($secret.Match)"
                }
            }
        }

        $SecurityResults.Secrets.LeakedSecrets = $secretsFound.Count
        $SecurityResults.Secrets.Files = $filesScanned

        # Save secrets scan results
        $secretsReport = @{
            timestamp = Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ"
            duration = $secretsDuration.TotalSeconds
            filesScanned = $filesScanned
            findings = $secretsFound
            summary = $SecurityResults.Secrets
        } | ConvertTo-Json -Depth 3

        $secretsReport | Out-File -FilePath (Join-Path $OutputPath "secrets-scan.json") -Encoding utf8

    }
    catch {
        Write-SecurityError "Secrets scanning failed: $($_.Exception.Message)"
    }
}

# Function to run CodeQL analysis (if available)
function Invoke-CodeQLAnalysis {
    Write-SecurityHeader "CodeQL Security Analysis"

    try {
        # Check if CodeQL is available
        if (Get-Command codeql -ErrorAction SilentlyContinue) {
            Write-SecurityInfo "Running CodeQL analysis..."

            $codeqlStart = Get-Date

            # Create CodeQL database
            $dbPath = Join-Path $OutputPath "codeql-db"
            codeql database create $dbPath --language=csharp --source-root=. --command="dotnet build BusBuddy.sln"

            # Run security queries
            $resultsPath = Join-Path $OutputPath "codeql-results.sarif"
            codeql database analyze $dbPath csharp-security-and-quality.qls --format=sarif-latest --output=$resultsPath

            $codeqlDuration = (Get-Date) - $codeqlStart
            Write-SecurityInfo "CodeQL analysis completed in $($codeqlDuration.TotalSeconds.ToString('F2')) seconds"

            # Parse results if available
            if (Test-Path $resultsPath) {
                $results = Get-Content $resultsPath | ConvertFrom-Json
                $issueCount = 0

                if ($results.runs -and $results.runs[0].results) {
                    $issueCount = $results.runs[0].results.Count
                      foreach ($result in $results.runs[0].results) {
                        $severity = if ($result.level) { $result.level } else { "Medium" }
                        $ruleId = $result.ruleId
                        $message = $result.message.text
                        Write-SecurityFinding $severity "CodeQL: $ruleId - $message"
                    }
                }

                $SecurityResults.CodeQL.Issues = $issueCount
                Write-SecurityInfo "Found $issueCount CodeQL security issues"
            }

        } else {
            Write-SecurityWarning "CodeQL not available - skipping CodeQL analysis"
            Write-SecurityInfo "Install CodeQL CLI from: https://github.com/github/codeql-cli-binaries"
        }
    }
    catch {
        Write-SecurityError "CodeQL analysis failed: $($_.Exception.Message)"
    }
}

# Function to get security rule severity
function Get-SecurityRuleSeverity {
    param([string]$RuleId)

    $highSeverityRules = @("SCS0002", "SCS0014", "SCS0017", "SCS0018", "SCS0019", "CA2100", "CA3001", "CA3003")
    $mediumSeverityRules = @("SCS0001", "SCS0005", "SCS0016", "CA2109", "CA2119", "CA2153")

    if ($highSeverityRules -contains $RuleId) { return "High" }
    if ($mediumSeverityRules -contains $RuleId) { return "Medium" }
    return "Low"
}

# Run security scans based on type
switch ($ScanType) {
    "All" {
        Invoke-StaticAnalysis
        Invoke-DependencyScanning
        Invoke-SecretsScanning
        Invoke-CodeQLAnalysis
    }
    "StaticAnalysis" {
        Invoke-StaticAnalysis
    }
    "Dependencies" {
        Invoke-DependencyScanning
    }
    "Secrets" {
        Invoke-SecretsScanning
    }
    "CodeQL" {
        Invoke-CodeQLAnalysis
    }
}

# Generate security summary report
Write-SecurityHeader "Security Scan Summary"

$totalFindings = $SecurityResults.StaticAnalysis.Findings +
                $SecurityResults.Dependencies.Vulnerabilities +
                $SecurityResults.Secrets.LeakedSecrets +
                $SecurityResults.CodeQL.Issues

Write-Host "`nSecurity Scan Results:" -ForegroundColor $Cyan
Write-Host $("=" * 70) -ForegroundColor Gray
Write-Host ("{0,-20} {1,15} {2,15}" -f "Scan Type", "Findings", "Status") -ForegroundColor $Blue

# Static Analysis
$staticStatus = if ($SecurityResults.StaticAnalysis.Findings -eq 0) { "PASS" } else { "REVIEW" }
$staticColor = if ($staticStatus -eq "PASS") { $Green } else { $Yellow }
Write-Host ("{0,-20} {1,15} {2,15}" -f "Static Analysis", $SecurityResults.StaticAnalysis.Findings, $staticStatus) -ForegroundColor $staticColor

# Dependencies
$depStatus = if ($SecurityResults.Dependencies.Vulnerabilities -eq 0) { "PASS" } else { "FAIL" }
$depColor = if ($depStatus -eq "PASS") { $Green } else { $Red }
Write-Host ("{0,-20} {1,15} {2,15}" -f "Dependencies", $SecurityResults.Dependencies.Vulnerabilities, $depStatus) -ForegroundColor $depColor

# Secrets
$secretStatus = if ($SecurityResults.Secrets.LeakedSecrets -eq 0) { "PASS" } else { "REVIEW" }
$secretColor = if ($secretStatus -eq "PASS") { $Green } else { $Yellow }
Write-Host ("{0,-20} {1,15} {2,15}" -f "Secrets", $SecurityResults.Secrets.LeakedSecrets, $secretStatus) -ForegroundColor $secretColor

# CodeQL
$codeqlStatus = if ($SecurityResults.CodeQL.Issues -eq 0) { "PASS" } else { "REVIEW" }
$codeqlColor = if ($codeqlStatus -eq "PASS") { $Green } else { $Yellow }
Write-Host ("{0,-20} {1,15} {2,15}" -f "CodeQL", $SecurityResults.CodeQL.Issues, $codeqlStatus) -ForegroundColor $codeqlColor

Write-Host $("=" * 70) -ForegroundColor Gray
Write-Host ("{0,-20} {1,15} {2,15}" -f "TOTAL", $totalFindings, $(if ($totalFindings -eq 0) { "PASS" } else { "REVIEW" })) -ForegroundColor $(if ($totalFindings -eq 0) { $Green } else { $Yellow })

# Security recommendations
Write-Host "`nSecurity Recommendations:" -ForegroundColor $Cyan
Write-Host "- Review all medium and high severity findings"
Write-Host "- Update vulnerable dependencies immediately"
Write-Host "- Implement proper secrets management (Azure Key Vault, etc.)"
Write-Host "- Regular security scans in CI/CD pipeline"
Write-Host "- Security training for development team"

# Generate comprehensive security report
$securityReport = @{
    timestamp = Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ"
    scanType = $ScanType
    summary = @{
        totalFindings = $totalFindings
        riskLevel = if ($totalFindings -eq 0) { "Low" } elseif ($totalFindings -le 5) { "Medium" } else { "High" }
    }
    results = $SecurityResults
    recommendations = @(
        "Review and remediate all high severity findings",
        "Update vulnerable dependencies",
        "Implement secrets management solution",
        "Enable security scanning in CI/CD",
        "Regular security training"
    )
} | ConvertTo-Json -Depth 4

$reportPath = Join-Path $OutputPath "security-summary.json"
$securityReport | Out-File -FilePath $reportPath -Encoding utf8

# Final status
Write-SecurityHeader "Security Scan Complete"

if ($totalFindings -eq 0) {
    Write-SecuritySuccess "No security issues found!"
    $exitCode = 0
} elseif ($totalFindings -le 5 -and $SecurityResults.Dependencies.Vulnerabilities -eq 0) {
    Write-SecurityWarning "$totalFindings security findings require review"
    $exitCode = if ($ContinueOnFindings) { 0 } else { 1 }
} else {
    Write-SecurityError "$totalFindings security issues found, including critical vulnerabilities"
    $exitCode = if ($ContinueOnFindings) { 0 } else { 2 }
}

Write-SecurityInfo "Detailed results saved to: $OutputPath"
Write-SecurityInfo "Security summary: $reportPath"

exit $exitCode
