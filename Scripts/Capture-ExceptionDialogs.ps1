#!/usr/bin/env pwsh
<#
.SYNOPSIS
Exception Dialog Capture System for BusBuddy
Captures unhandled exception popups, their content, stack traces, and source locations

.DESCRIPTION
Following BusBuddy guidelines:
- Concise responses with efficient debugging
- Single build approach - capture everything, analyze once
- PowerShell (pwsh) for all commands
- Comprehensive exception tracking and reporting

.EXAMPLE
.\Scripts\Capture-ExceptionDialogs.ps1
.\Scripts\Capture-ExceptionDialogs.ps1 -EnableUICapture
.\Scripts\Capture-ExceptionDialogs.ps1 -LogLevel Detailed
#>

param(
    [switch]$EnableUICapture,
    [ValidateSet("Basic", "Standard", "Detailed", "Verbose")]
    [string]$LogLevel = "Standard",
    [switch]$AutoFix
)

# BusBuddy Guidelines: Efficient debugging with minimal tool calls
$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

# Exception capture log file
$ExceptionLogPath = "logs\exception_capture_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
$ExceptionReportPath = "logs\exception_report_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"

# Ensure logs directory exists
if (-not (Test-Path "logs")) {
    New-Item -Path "logs" -ItemType Directory -Force | Out-Null
}

# Color coding for output
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Debug { param($Message) Write-Host "🔍 $Message" -ForegroundColor Gray }

function Write-ExceptionLog {
    param($Message, $Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"
    $logEntry = "[$timestamp] [$Level] $Message"
    Write-Host $logEntry
    Add-Content -Path $ExceptionLogPath -Value $logEntry -Encoding UTF8
}

Write-Info "BusBuddy Exception Dialog Capture System Starting..."
Write-ExceptionLog "Exception capture system initialized - Log Level: $LogLevel"

# Create exception capture class for injection into BusBuddy
$ExceptionCaptureClass = @"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;

namespace BusBuddy.Debug
{
    /// <summary>
    /// Exception capture system for BusBuddy application
    /// Captures unhandled exceptions, dialog content, and stack traces
    /// </summary>
    public static class ExceptionCapture
    {
        private static string _logPath = "logs\\exception_capture.log";
        private static List<ExceptionInfo> _capturedExceptions = new List<ExceptionInfo>();

        public class ExceptionInfo
        {
            public DateTime Timestamp { get; set; }
            public string ExceptionType { get; set; }
            public string Message { get; set; }
            public string StackTrace { get; set; }
            public string SourceMethod { get; set; }
            public string SourceFile { get; set; }
            public int? SourceLine { get; set; }
            public string ThreadInfo { get; set; }
            public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initialize exception capture system
        /// Call this early in Program.cs Main method
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Set up global exception handlers
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

                Application.ThreadException += OnThreadException;
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                Console.WriteLine("✅ ExceptionCapture: Global exception handlers registered");
                LogException("ExceptionCapture system initialized", "INFO");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ExceptionCapture initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle thread exceptions (UI thread)
        /// </summary>
        private static void OnThreadException(object eventSender, System.Threading.ThreadExceptionEventArgs e)
        {
            var exceptionInfo = CaptureExceptionDetails(e.Exception, "UI_THREAD");
            _capturedExceptions.Add(exceptionInfo);

            LogException($"UI THREAD EXCEPTION: {e.Exception.Message}", "ERROR");
            LogException($"Stack Trace: {e.Exception.StackTrace}", "ERROR");

            // Save detailed report
            SaveExceptionReport(exceptionInfo);

            // Show custom dialog instead of default
            ShowCustomExceptionDialog(exceptionInfo);
        }

        /// <summary>
        /// Handle unhandled domain exceptions
        /// </summary>
        private static void OnUnhandledException(object eventSender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                var exceptionInfo = CaptureExceptionDetails(ex, "APP_DOMAIN");
                _capturedExceptions.Add(exceptionInfo);

                LogException($"UNHANDLED DOMAIN EXCEPTION: {ex.Message}", "FATAL");
                LogException($"Stack Trace: {ex.StackTrace}", "FATAL");
                LogException($"Is Terminating: {e.IsTerminating}", "FATAL");

                // Save detailed report
                SaveExceptionReport(exceptionInfo);

                if (!e.IsTerminating)
                {
                    ShowCustomExceptionDialog(exceptionInfo);
                }
            }
        }

        /// <summary>
        /// Capture detailed exception information
        /// </summary>
        private static ExceptionInfo CaptureExceptionDetails(Exception ex, string context)
        {
            var info = new ExceptionInfo
            {
                Timestamp = DateTime.Now,
                ExceptionType = ex.GetType().FullName,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                ThreadInfo = $"Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}, " +
                           $"Is Background: {System.Threading.Thread.CurrentThread.IsBackground}"
            };

            // Extract source method and file info from stack trace
            try
            {
                var stackTrace = new StackTrace(ex, true);
                var frame = stackTrace.GetFrame(0);
                if (frame != null)
                {
                    info.SourceMethod = frame.GetMethod()?.Name;
                    info.SourceFile = frame.GetFileName();
                    info.SourceLine = frame.GetFileLineNumber();
                }
            }
            catch { /* Ignore stack trace parsing errors */ }

            // Add context information
            info.Context["CaptureContext"] = context;
            info.Context["ProcessId"] = Process.GetCurrentProcess().Id;
            info.Context["WorkingSet"] = Environment.WorkingSet;
            info.Context["TotalMemory"] = GC.GetTotalMemory(false);

            return info;
        }

        /// <summary>
        /// Save exception report to JSON file
        /// </summary>
        private static void SaveExceptionReport(ExceptionInfo info)
        {
            try
            {
                var reportPath = $"logs\\exception_report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(reportPath, json);

                Console.WriteLine($"📄 Exception report saved: {reportPath}");
            }
            catch (Exception saveEx)
            {
                Console.WriteLine($"❌ Failed to save exception report: {saveEx.Message}");
            }
        }

        /// <summary>
        /// Show custom exception dialog with detailed information
        /// </summary>
        private static void ShowCustomExceptionDialog(ExceptionInfo info)
        {
            try
            {
                var message = $"BusBuddy Exception Captured\\n\\n" +
                             $"Type: {info.ExceptionType}\\n" +
                             $"Message: {info.Message}\\n" +
                             $"Time: {info.Timestamp:yyyy-MM-dd HH:mm:ss}\\n\\n" +
                             $"Source: {info.SourceMethod} in {Path.GetFileName(info.SourceFile)}:{info.SourceLine}\\n\\n" +
                             $"This exception has been logged for analysis.\\n" +
                             $"Check logs\\exception_*.log for details.";

                var result = MessageBox.Show(
                    message,
                    "BusBuddy - Exception Captured",
                    MessageBoxButtons.AbortRetryIgnore,
                    MessageBoxIcon.Error);

                LogException($"User action: {result}", "INFO");

                if (result == DialogResult.Abort)
                {
                    Environment.Exit(1);
                }
            }
            catch (Exception dialogEx)
            {
                Console.WriteLine($"❌ Failed to show exception dialog: {dialogEx.Message}");
            }
        }

        /// <summary>
        /// Log exception message to file
        /// </summary>
        private static void LogException(string message, string level)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [{level}] {message}";

                Console.WriteLine(logEntry);

                Directory.CreateDirectory("logs");
                File.AppendAllText(_logPath, logEntry + Environment.NewLine);
            }
            catch { /* Ignore logging errors */ }
        }

        /// <summary>
        /// Get all captured exceptions for analysis
        /// </summary>
        public static List<ExceptionInfo> GetCapturedExceptions()
        {
            return new List<ExceptionInfo>(_capturedExceptions);
        }

        /// <summary>
        /// Clear captured exceptions
        /// </summary>
        public static void ClearCapturedExceptions()
        {
            _capturedExceptions.Clear();
            LogException("Captured exceptions cleared", "INFO");
        }
    }
}
"@

# Save the exception capture class
$ExceptionCaptureFile = "BusBuddy.UI\Helpers\ExceptionCapture.cs"
Write-Info "Creating ExceptionCapture class: $ExceptionCaptureFile"
$ExceptionCaptureClass | Out-File -FilePath $ExceptionCaptureFile -Encoding UTF8

Write-Success "ExceptionCapture class created"

# Create Program.cs modification to inject exception capture
Write-Info "Analyzing Program.cs for exception capture injection..."

$programContent = Get-Content "Program.cs" -Raw
if ($programContent -notmatch "ExceptionCapture\.Initialize") {
    Write-Warning "Program.cs does not have ExceptionCapture.Initialize() - needs manual addition"

    # Create injection instructions
    $injectionInstructions = @"
// Add this to Program.cs Main() method BEFORE any other initialization:

// EXCEPTION CAPTURE SYSTEM - Add after Syncfusion license registration
BusBuddy.Debug.ExceptionCapture.Initialize();
Console.WriteLine("🔍 Exception capture system activated");

"@

    $injectionInstructions | Out-File -FilePath "logs\exception_capture_injection.txt" -Encoding UTF8
    Write-Info "Exception capture injection instructions saved to: logs\exception_capture_injection.txt"
} else {
    Write-Success "Program.cs already has ExceptionCapture.Initialize()"
}

# Create UI Automation for dialog capture (if enabled)
if ($EnableUICapture) {
    Write-Info "Setting up UI automation for dialog capture..."

    $UIAutomationScript = @"
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

`$global:DialogCaptures = @()

function Start-DialogMonitoring {
    Write-Host "🔍 Starting dialog monitoring..."

    while (`$true) {
        try {
            # Find any error dialogs
            `$dialogs = Get-Process | Where-Object { `$_.ProcessName -eq "BusBuddy" -or `$_.MainWindowTitle -like "*Exception*" -or `$_.MainWindowTitle -like "*Error*" }

            foreach (`$dialog in `$dialogs) {
                if (`$dialog.MainWindowTitle -and `$dialog.MainWindowTitle -ne "") {
                    `$capture = @{
                        Timestamp = Get-Date
                        ProcessName = `$dialog.ProcessName
                        WindowTitle = `$dialog.MainWindowTitle
                        ProcessId = `$dialog.Id
                    }

                    `$global:DialogCaptures += `$capture
                    Write-Host "📋 Dialog captured: `$(`$dialog.MainWindowTitle)"

                    # Log to file
                    `$logEntry = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Dialog: `$(`$dialog.MainWindowTitle) (PID: `$(`$dialog.Id))"
                    Add-Content -Path "logs\dialog_capture.log" -Value `$logEntry
                }
            }
        } catch {
            Write-Warning "Dialog monitoring error: `$(`$_.Exception.Message)"
        }

        Start-Sleep -Milliseconds 500
    }
}

# Start monitoring in background
Start-Job -ScriptBlock { Start-DialogMonitoring } -Name "DialogMonitor"
Write-Host "✅ Dialog monitoring started in background job"
"@

    $UIAutomationScript | Out-File -FilePath "Scripts\Monitor-Dialogs.ps1" -Encoding UTF8
    Write-Success "UI dialog monitoring script created: Scripts\Monitor-Dialogs.ps1"
}

# Create test runner with exception capture
Write-Info "Creating test runner with exception capture..."

$TestRunnerWithCapture = @"
#!/usr/bin/env pwsh
<#
.SYNOPSIS
Test Runner with Exception Capture for BusBuddy
#>

param([switch]`$BuildFirst)

if (`$BuildFirst) {
    Write-Host "🔨 Building with exception capture..."
    dotnet build
    if (`$LASTEXITCODE -ne 0) { exit 1 }
}

Write-Host "🧪 Running tests with exception capture..."

# Clear previous exception logs
Remove-Item "logs\exception_*.log" -ErrorAction SilentlyContinue
Remove-Item "logs\exception_*.json" -ErrorAction SilentlyContinue

# Run application with exception capture
Write-Host "▶️ Starting BusBuddy with exception monitoring..."

`$job = Start-Job -ScriptBlock {
    Set-Location `$using:PWD
    dotnet run --no-build 2>&1
} -Name "BusBuddyWithCapture"

# Monitor for exception files
`$timeout = 30
`$elapsed = 0

while (`$elapsed -lt `$timeout -and `$job.State -eq "Running") {
    Start-Sleep -Seconds 1
    `$elapsed++

    # Check for new exception files
    `$exceptionFiles = Get-ChildItem "logs\exception_*.log" -ErrorAction SilentlyContinue
    if (`$exceptionFiles) {
        Write-Host "🚨 Exception detected! Files:"
        `$exceptionFiles | ForEach-Object { Write-Host "   📄 `$(`$_.Name)" }

        # Show recent content
        `$latestLog = `$exceptionFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        Write-Host "📋 Latest exception content:"
        Get-Content `$latestLog.FullName -Tail 10 | ForEach-Object { Write-Host "   `$_" }
        break
    }

    Write-Progress -Activity "Monitoring for exceptions" -SecondsRemaining (`$timeout - `$elapsed)
}

# Stop the job
Stop-Job `$job -ErrorAction SilentlyContinue
Remove-Job `$job -ErrorAction SilentlyContinue

# Summary
`$allExceptionFiles = Get-ChildItem "logs\exception_*" -ErrorAction SilentlyContinue
if (`$allExceptionFiles) {
    Write-Host "📊 Exception Summary:"
    Write-Host "   Total files: `$(`$allExceptionFiles.Count)"
    `$allExceptionFiles | ForEach-Object {
        Write-Host "   📄 `$(`$_.Name) (`$(`$_.Length) bytes)"
    }
} else {
    Write-Host "✅ No exceptions detected during test run"
}
"@

$TestRunnerWithCapture | Out-File -FilePath "Scripts\Test-WithExceptionCapture.ps1" -Encoding UTF8
Write-Success "Test runner with exception capture created: Scripts\Test-WithExceptionCapture.ps1"

# Modify the current test-complete-flow.ps1 to include exception capture
Write-Info "Modifying test-complete-flow.ps1 to include exception monitoring..."

# Add exception monitoring to the current test script
$ExceptionMonitoringAddition = @"

# Exception Capture Integration
Write-Test "Exception Monitoring Active"
if (Test-Path "BusBuddy.UI\Helpers\ExceptionCapture.cs") {
    Write-Success "ExceptionCapture class available - monitoring enabled"

    # Clear previous exception logs
    Remove-Item "logs\exception_*.log" -ErrorAction SilentlyContinue
    Remove-Item "logs\exception_*.json" -ErrorAction SilentlyContinue

    Write-Info "Exception logs cleared - fresh monitoring session"
} else {
    Write-Warning "ExceptionCapture class not found - limited exception monitoring"
}

# Function to check for new exceptions during testing
function Check-ForNewExceptions {
    $exceptionFiles = Get-ChildItem "logs\exception_*.log" -ErrorAction SilentlyContinue
    if ($exceptionFiles) {
        Write-Error "🚨 EXCEPTIONS DETECTED during testing!"
        foreach ($file in $exceptionFiles) {
            Write-Host "📄 Exception file: $($file.Name)" -ForegroundColor Red
            Write-Host "📋 Content preview:" -ForegroundColor Yellow
            Get-Content $file.FullName -Tail 5 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
        }
        return $true
    }
    return $false
}
"@

# Insert the exception monitoring code after the existing content
$currentContent = Get-Content "test-complete-flow.ps1" -Raw
if ($currentContent -notmatch "Exception Capture Integration") {
    # Find a good insertion point (after the initial setup)
    $insertionPoint = $currentContent.IndexOf('Write-Info "BusBuddy Complete Data Flow Testing - Layer: $Layer"')
    if ($insertionPoint -gt 0) {
        $beforeInsertion = $currentContent.Substring(0, $insertionPoint)
        $afterInsertion = $currentContent.Substring($insertionPoint)
        $newContent = $beforeInsertion + $ExceptionMonitoringAddition + "`n" + $afterInsertion

        $newContent | Out-File -FilePath "test-complete-flow.ps1" -Encoding UTF8
        Write-Success "Exception monitoring added to test-complete-flow.ps1"
    }
}

# Create summary report
Write-Info "Creating exception capture summary..."

$Summary = @{
    ExceptionCaptureClass = $ExceptionCaptureFile
    UIMonitoringScript = if ($EnableUICapture) { "Scripts\Monitor-Dialogs.ps1" } else { "Not enabled" }
    TestRunnerWithCapture = "Scripts\Test-WithExceptionCapture.ps1"
    LogDirectory = "logs\"
    InjectionInstructions = "logs\exception_capture_injection.txt"
    Status = "Ready for integration"
}

$Summary | ConvertTo-Json -Depth 3 | Out-File -FilePath $ExceptionReportPath -Encoding UTF8

Write-Success "Exception Capture System Setup Complete!"
Write-Info "📊 Summary saved to: $ExceptionReportPath"

Write-Info "🔧 Next Steps:"
Write-Info "1. Add ExceptionCapture.Initialize() to Program.cs Main() method"
Write-Info "2. Rebuild the application: dotnet build"
Write-Info "3. Run tests with monitoring: .\Scripts\Test-WithExceptionCapture.ps1"
Write-Info "4. Check logs\ directory for captured exceptions"

if ($EnableUICapture) {
    Write-Info "5. Start UI monitoring: .\Scripts\Monitor-Dialogs.ps1"
}

Write-ExceptionLog "Exception capture system setup completed successfully"
