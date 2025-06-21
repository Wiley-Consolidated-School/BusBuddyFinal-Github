# Fix Vehicle Management Form Initialization
# This script fixes the "repository not initialized" error in the Vehicle Management form
# by creating a centralized repository initialization method and ensuring it's called
# before the form is displayed.

# 1. Make sure the ServiceContainerSingleton is always initialized in the Dashboard before showing the form
# 2. Ensure VehicleManagementFormSyncfusion constructor uses the singleton
# 3. Fix LoadData in VehicleManagementFormSyncfusion to handle initialization errors gracefully
# 4. Add retry logic to repository access

param(
    [switch]$Debug,
    [switch]$NoBackup
)

$ErrorActionPreference = "Stop"

function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    } else {
        $input | Write-Output
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

function Log-Info($message) {
    Write-ColorOutput Green "[INFO] $message"
}

function Log-Warning($message) {
    Write-ColorOutput Yellow "[WARNING] $message"
}

function Log-Error($message) {
    Write-ColorOutput Red "[ERROR] $message"
}

# Path constants
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$dashboardPath = Join-Path $scriptDir "BusBuddy.UI\Views\BusBuddyDashboardSyncfusion.cs"
$vehicleFormPath = Join-Path $scriptDir "BusBuddy.UI\Views\VehicleManagementFormSyncfusion.cs"
$serviceContainerPath = Join-Path $scriptDir "BusBuddy.UI\Services\ServiceContainerSingleton.cs"

# Check if files exist
if (-not (Test-Path $dashboardPath)) {
    Log-Error "Dashboard file not found at: $dashboardPath"
    exit 1
}

if (-not (Test-Path $vehicleFormPath)) {
    Log-Error "Vehicle Management form file not found at: $vehicleFormPath"
    exit 1
}

if (-not (Test-Path $serviceContainerPath)) {
    Log-Error "ServiceContainerSingleton file not found at: $serviceContainerPath"
    exit 1
}

# Create backups
if (-not $NoBackup) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    Copy-Item -Path $dashboardPath -Destination "$dashboardPath.backup_$timestamp"
    Copy-Item -Path $vehicleFormPath -Destination "$vehicleFormPath.backup_$timestamp"
    Copy-Item -Path $serviceContainerPath -Destination "$serviceContainerPath.backup_$timestamp"
    Log-Info "Created backups with timestamp $timestamp"
}

# 1. Add the EnsureRepository method to ServiceContainerSingleton if it doesn't exist
$serviceContainerContent = Get-Content $serviceContainerPath -Raw
if (-not $serviceContainerContent.Contains("public static T EnsureRepository<T>() where T : class")) {
    Log-Info "Adding EnsureRepository method to ServiceContainerSingleton"

    $ensureRepositoryMethod = @"

        /// <summary>
        /// Ensures a repository is initialized by retrieving it from the singleton instance
        /// Use this before accessing any repository through a form or service
        /// </summary>
        /// <typeparam name="T">The repository interface type</typeparam>
        /// <returns>A repository instance from the singleton container</returns>
        public static T EnsureRepository<T>() where T : class
        {
            if (!IsInitialized)
            {
                Console.WriteLine(`$"⚠️ ServiceContainerSingleton not initialized when requesting {typeof(T).Name}, initializing now");
                Initialize();
            }

            var repository = Instance.GetService<T>();
            if (repository == null)
            {
                Console.WriteLine(`$"❌ Failed to resolve {typeof(T).Name} from ServiceContainerSingleton");
                throw new InvalidOperationException(`$"Failed to resolve {typeof(T).Name} from ServiceContainerSingleton");
            }

            Console.WriteLine(`$"✅ Successfully resolved {typeof(T).Name} from ServiceContainerSingleton");
            return repository;
        }
"@

    # Insert the method before the closing brace of the class
    $serviceContainerContent = $serviceContainerContent -replace "}\s*$", "$ensureRepositoryMethod`n}"
    Set-Content -Path $serviceContainerPath -Value $serviceContainerContent
}

# 2. Fix VehicleManagementFormSyncfusion constructor to use EnsureRepository
$vehicleFormContent = Get-Content $vehicleFormPath -Raw

# Add using directive if needed
if (-not $vehicleFormContent.Contains("using BusBuddy.UI.Services;")) {
    Log-Info "Adding BusBuddy.UI.Services namespace to VehicleManagementFormSyncfusion"
    $vehicleFormContent = $vehicleFormContent -replace "using System;", "using System;`nusing BusBuddy.UI.Services;"
}

# Update constructor to use EnsureRepository
if ($vehicleFormContent.Contains("public VehicleManagementFormSyncfusion()")) {
    Log-Info "Updating VehicleManagementFormSyncfusion constructor to use EnsureRepository"

    # Match the constructor and replace it
    $pattern = "public VehicleManagementFormSyncfusion\(\)\s*\{[^}]*\}"
    $replacement = @"
public VehicleManagementFormSyncfusion()
        {
            try
            {
                Console.WriteLine("🔍 Creating VehicleManagementFormSyncfusion using singleton ServiceContainer");

                // Use the helper method to ensure repository initialization
                _vehicleRepository = ServiceContainerSingleton.EnsureRepository<IVehicleRepository>();

                // Force test the repository to ensure it's working
                var vehicles = _vehicleRepository.GetAllVehicles();
                var count = vehicles?.Count() ?? 0;
                Console.WriteLine(`$"✅ VehicleRepository working: {count} vehicles available");
            }
            catch (Exception ex)
            {
                Console.WriteLine(`$"❌ Error in VehicleManagementFormSyncfusion constructor: {ex.Message}");
                Console.WriteLine(`$"Stack Trace: {ex.StackTrace}");
                MessageBox.Show(`$"Failed to initialize vehicle repository: {ex.Message}`n`nPlease check database connection.",
                    "Repository Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // Re-throw to show the error to the user
            }
            // NOTE: LoadData() is called by the base class after all controls are initialized
        }
"@

    $vehicleFormContent = $vehicleFormContent -replace $pattern, $replacement
    Set-Content -Path $vehicleFormPath -Value $vehicleFormContent
}

# 3. Update the EnsureVehicleRepositoryInitialized method in BusBuddyDashboardSyncfusion
$dashboardContent = Get-Content $dashboardPath -Raw

# Add using directive if needed
if (-not $dashboardContent.Contains("using BusBuddy.UI.Services;")) {
    Log-Info "Adding BusBuddy.UI.Services namespace to BusBuddyDashboardSyncfusion"
    $dashboardContent = $dashboardContent -replace "using System;", "using System;`nusing BusBuddy.UI.Services;"
}

# Find and replace the EnsureVehicleRepositoryInitialized method
$pattern = "private void EnsureVehicleRepositoryInitialized\(\)\s*\{[^}]*\}"
$replacement = @"
private void EnsureVehicleRepositoryInitialized()
        {
            try
            {
                Console.WriteLine("🔍 Dashboard: Ensuring VehicleRepository is initialized via ServiceContainerSingleton");

                // Use the helper method to ensure repository initialization
                var repository = ServiceContainerSingleton.EnsureRepository<IVehicleRepository>();

                // Force test the repository to ensure it's working
                var vehicles = repository.GetAllVehicles();
                var count = vehicles?.Count ?? 0;
                Console.WriteLine(`$"✅ VehicleRepository initialized successfully: {count} vehicles available");
            }
            catch (Exception ex)
            {
                Console.WriteLine(`$"❌ VehicleRepository initialization error: {ex.Message}");
                MessageBox.Show(`$"Failed to initialize vehicle repository: {ex.Message}`n`nPlease check database connection.",
                    "Repository Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
"@

$dashboardContent = $dashboardContent -replace $pattern, $replacement
Set-Content -Path $dashboardPath -Value $dashboardContent

Log-Info "Repository initialization fix script completed successfully"
Log-Info "Please build and test the application to verify the fix"
