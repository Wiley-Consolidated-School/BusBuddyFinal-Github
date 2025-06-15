using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.UI.Views;
using BusBuddy.Business;
using BusBuddy.UI.Services;
using BusBuddy.Data;
using BusBuddy.Services;
using DI = BusBuddy.DependencyInjection;

namespace BusBuddy
{
    internal static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Allocate console for debugging
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                AllocConsole();
            }

            Console.WriteLine("🚌 BusBuddy Application Starting...");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Configure dependency injection
            DI.ServiceContainer.ConfigureServices();

            // Initialize database based on configuration
            try
            {
                if (DatabaseConfiguration.DatabaseProvider == "SqlServer")
                {
                    Console.WriteLine("🔍 Attempting SQL Server initialization...");
                    var connectionString = DatabaseConfiguration.GetConnectionString();
                    var initializer = new SqlServerDatabaseInitializer(connectionString);
                    initializer.Initialize();
                }
                else
                {
                    Console.WriteLine("❌ SQL Server connection string not found. Please configure SQL Server connection.");
                    MessageBox.Show("SQL Server connection string not found. Please configure SQL Server connection in App.config.", "Database Configuration Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // Don't continue if no valid connection string
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database initialization completely failed: {ex.Message}");
                MessageBox.Show($"Failed to initialize database: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Don't continue if database fails
            }

            // Verify database is accessible before launching UI
            try
            {
                Console.WriteLine("🔍 Verifying database connectivity before launching UI...");
                var testRepository = new VehicleRepository();
                var vehicleCount = testRepository.GetAllVehicles().Count;
                Console.WriteLine($"✅ Database verification successful! Found {vehicleCount} vehicles.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database verification failed: {ex.Message}");
                MessageBox.Show($"Database is not accessible: {ex.Message}\n\nThe application will start but forms may not work correctly.",
                    "Database Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            try
            {
                // Use the EnhancedMainForm with dependency injection
                var databaseService = DI.ServiceContainer.GetService<IDatabaseHelperService>();

                // Create instance-based container for UI services
                var uiServiceContainer = new ServiceContainer();
                var uiNavigationService = uiServiceContainer.GetService<INavigationService>();

                // Wrap the UI navigation service with our main navigation service that handles TimeCard
                var navigationService = new MainNavigationService(uiNavigationService);

                var mainForm = new EnhancedMainForm(databaseService, navigationService);
                mainForm.WindowState = FormWindowState.Maximized;
                Application.Run(mainForm);
            }
            finally
            {
                // Clean up DI container
                DI.ServiceContainer.Dispose();
            }
        }
    }
}
