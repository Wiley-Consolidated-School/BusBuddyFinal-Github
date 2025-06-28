using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Business;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Models; // Add this for the data models
using BusBuddy.UI.Services;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// ViewModel for the Dashboard providing data binding for Syncfusion controls
    /// Implements INotifyPropertyChanged for real-time UI updates
    /// </summary>
    public class DashboardViewModel : INotifyPropertyChanged, IDisposable
    {
        // Services and repositories
        private readonly IRouteAnalyticsService _routeAnalyticsService;
        private readonly IBusService _busService;
        private readonly IRouteRepository _routeRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IErrorHandlerService _errorHandler;
        private readonly BusRepository _busRepository;

        // Basic metrics for dashboard display
        private double _costPerStudent = 2.70;
        private double _totalMiles = 45200;
        private int _pupilsTransported = 1850;
        private decimal _stateContribution = 5_100_000_000m;
        private decimal _localContribution = 4_541_700_000m;

        // Data collections for grids with ObservableCollection for real-time updates
        private ObservableCollection<VehicleData> _vehicleData = new();
        private ObservableCollection<RouteData> _routesData = new();
        private ObservableCollection<ActivityData> _activityData = new();
        private ObservableCollection<ChartDataPoint> _mileageTrends = new();
        private DateTime _selectedDate = DateTime.Today;

        private bool _isLoading = false;
        private string _statusMessage = "Ready";

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the DashboardViewModel class with service dependencies
        /// </summary>
        public DashboardViewModel(
            IRouteAnalyticsService? routeAnalyticsService = null,
            IBusService? busService = null,
            IRouteRepository? routeRepository = null,
            IDriverRepository? driverRepository = null,
            IActivityRepository? activityRepository = null,
            IErrorHandlerService? errorHandler = null,
            BusRepository? busRepository = null)
        {
            // Initialize services with sensible defaults if not provided
            _routeAnalyticsService = routeAnalyticsService ?? new RouteAnalyticsService();
            _busService = busService ?? new BusService();
            _routeRepository = routeRepository ?? new RouteRepository();
            _driverRepository = driverRepository ?? new DriverRepository();
            _activityRepository = activityRepository ?? new ActivityRepository();
            _errorHandler = errorHandler ?? new ErrorHandlerService();
            _busRepository = busRepository ?? new BusRepository();

            Console.WriteLine("🔄 Initializing DashboardViewModel with proper services");
        }

        #region Properties

        public double CostPerStudent
        {
            get => _costPerStudent;
            set
            {
                if (Math.Abs(_costPerStudent - value) > 0.01)
                {
                    _costPerStudent = value;
                    OnPropertyChanged(nameof(CostPerStudent));
                }
            }
        }

        public double TotalMiles
        {
            get => _totalMiles;
            set
            {
                if (Math.Abs(_totalMiles - value) > 0.1)
                {
                    _totalMiles = value;
                    OnPropertyChanged(nameof(TotalMiles));
                }
            }
        }

        public int PupilsTransported
        {
            get => _pupilsTransported;
            set
            {
                if (_pupilsTransported != value)
                {
                    _pupilsTransported = value;
                    OnPropertyChanged(nameof(PupilsTransported));
                }
            }
        }

        public ObservableCollection<VehicleData> VehicleData
        {
            get => _vehicleData;
            set
            {
                _vehicleData = value ?? new ObservableCollection<VehicleData>();
                OnPropertyChanged(nameof(VehicleData));
            }
        }

        public ObservableCollection<RouteData> RoutesData
        {
            get => _routesData;
            set
            {
                _routesData = value ?? new ObservableCollection<RouteData>();
                OnPropertyChanged(nameof(RoutesData));
            }
        }

        public ObservableCollection<ActivityData> ActivityData
        {
            get => _activityData;
            set
            {
                _activityData = value ?? new ObservableCollection<ActivityData>();
                OnPropertyChanged(nameof(ActivityData));
            }
        }

        public ObservableCollection<ChartDataPoint> MileageTrends
        {
            get => _mileageTrends;
            set
            {
                _mileageTrends = value ?? new ObservableCollection<ChartDataPoint>();
                OnPropertyChanged(nameof(MileageTrends));
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    // Input validation: not in the future, not more than 2 years in the past
                    var today = DateTime.Today;
                    var minDate = today.AddYears(-2);
                    if (value > today)
                        throw new ArgumentOutOfRangeException(nameof(SelectedDate), "SelectedDate cannot be in the future.");
                    if (value < minDate)
                        throw new ArgumentOutOfRangeException(nameof(SelectedDate), $"SelectedDate cannot be more than 2 years in the past. Minimum allowed: {minDate:yyyy-MM-dd}");
                    _selectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value ?? "Ready";
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        public decimal StateContribution
        {
            get => _stateContribution;
            set
            {
                if (_stateContribution != value)
                {
                    _stateContribution = value;
                    OnPropertyChanged(nameof(StateContribution));
                }
            }
        }

        public decimal LocalContribution
        {
            get => _localContribution;
            set
            {
                if (_localContribution != value)
                {
                    _localContribution = value;
                    OnPropertyChanged(nameof(LocalContribution));
                }
            }
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading dashboard data...";

                Console.WriteLine("🔄 Initializing dashboard data...");

                // Try to load real data first, with fallback to mock data
                await LoadRealDataAsync();

                StatusMessage = "Dashboard data loaded successfully";
                Console.WriteLine("✅ Dashboard data initialized successfully");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
                Console.WriteLine($"❌ Error initializing dashboard data: {ex.Message}");

                // Final fallback to mock data if everything else fails
                LoadMockData();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshDataAsync()
        {
            await InitializeAsync();
        }

        /// <summary>
        /// Verifies SQL Server connection by attempting to connect and querying database information
        /// </summary>
        private async Task<bool> VerifySqlConnection(string connectionString)
        {
            try
            {
                Console.WriteLine("🔍 Testing SQL Server connection...");

                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Check database version
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT @@VERSION, DB_NAME(), SUSER_SNAME()";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string version = reader.GetString(0).Split('\n')[0].Trim();
                            string database = reader.GetString(1);
                            string username = reader.GetString(2);

                            Console.WriteLine($"✅ Connected to: {version}");
                            Console.WriteLine($"✅ Database: {database}");
                            Console.WriteLine($"✅ User: {username}");
                        }
                    }

                    // Check table existence and counts
                    var tableCommand = connection.CreateCommand();
                    tableCommand.CommandText = @"
SELECT
    t.name AS TableName,
    p.rows AS RowCount
FROM
    sys.tables t
INNER JOIN
    sys.partitions p ON t.object_id = p.object_id
WHERE
    t.name IN ('Vehicles', 'Routes', 'Activities')
    AND p.index_id IN (0, 1)";

                    using (var reader = await tableCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string tableName = reader.GetString(0);
                            int rowCount = Convert.ToInt32(reader.GetValue(1));

                            Console.WriteLine($"✅ Table: {tableName}, Rows: {rowCount}");
                        }
                    }

                    // Check AUTO_CLOSE setting on database
                    var autoCloseCommand = connection.CreateCommand();
                    autoCloseCommand.CommandText = "SELECT is_auto_close_on FROM sys.databases WHERE name = DB_NAME()";
                    var isAutoCloseOn = Convert.ToBoolean(await autoCloseCommand.ExecuteScalarAsync());

                    if (isAutoCloseOn)
                    {
                        Console.WriteLine("⚠️ AUTO_CLOSE is enabled on the database, which may cause connection issues");
                        Console.WriteLine("   Run this SQL to fix: ALTER DATABASE BusBuddy SET AUTO_CLOSE OFF");
                    }
                    else
                    {
                        Console.WriteLine("✅ AUTO_CLOSE is disabled (recommended setting)");
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SQL Server connection test failed: {ex.Message}");
                Console.WriteLine($"   Details: {ex.GetType().Name}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
                }

                return false;
            }
        }

        /// <summary>
        /// Extracts connection information from the app's configuration
        /// </summary>
        private (string ConnectionString, string Server, string Database, bool TrustedConnection) GetConnectionInfo()
        {
            string connectionString = "";
            string server = "";
            string database = "";
            bool trustedConnection = false;

            try
            {
                // Try to get connection string from configuration
                var conn = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"];
                if (conn != null)
                {
                    connectionString = conn.ConnectionString;

                    // Parse connection string parts
                    var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                    server = builder.DataSource;
                    database = builder.InitialCatalog;
                    trustedConnection = builder.IntegratedSecurity;
                }
                else
                {
                    // Fallback to default SQL Server Express
                    connectionString = "Server=.\\SQLEXPRESS01;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;";
                    server = ".\\SQLEXPRESS01";
                    database = "BusBuddy";
                    trustedConnection = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error parsing connection info: {ex.Message}");

                // Last resort fallback
                connectionString = "Server=.\\SQLEXPRESS01;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;";
                server = ".\\SQLEXPRESS01";
                database = "BusBuddy";
                trustedConnection = true;
            }

            return (connectionString, server, database, trustedConnection);
        }

        /// <summary>
        /// Diagnoses database access issues by testing each repository directly
        /// </summary>
        /// <returns>A diagnostic report string</returns>
        public async Task<string> DiagnoseDataAccessAsync()
        {
            var diagnosticLog = new System.Text.StringBuilder();
            diagnosticLog.AppendLine($"=== BusBuddy Data Access Diagnostics ({DateTime.Now}) ===");

            try
            {
                // 1. Verify connection string
                var connectionInfo = GetConnectionInfo();
                diagnosticLog.AppendLine($"Connection info: Server={connectionInfo.Server}, Database={connectionInfo.Database}, Trusted={connectionInfo.TrustedConnection}");

                // 2. Test direct SQL connection
                try
                {
                    using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionInfo.ConnectionString))
                    {
                        await connection.OpenAsync();
                        diagnosticLog.AppendLine("✅ Direct SQL connection: SUCCESS");

                        // Check database version
                        var command = connection.CreateCommand();
                        command.CommandText = "SELECT @@VERSION";
                        var version = (await command.ExecuteScalarAsync())?.ToString();
                        diagnosticLog.AppendLine($"SQL Server Version: {version}");
                    }
                }
                catch (Exception ex)
                {
                    diagnosticLog.AppendLine($"❌ Direct SQL connection: FAILED - {ex.Message}");
                }

                // 3. Test BusRepository
                try
                {
                    var buses = _busRepository.GetAllBuses();
                    diagnosticLog.AppendLine($"✅ BusRepository.GetAllBuses: SUCCESS - {buses.Count()} buses found");
                }
                catch (Exception ex)
                {
                    diagnosticLog.AppendLine($"❌ BusRepository.GetAllBuses: FAILED - {ex.Message}");
                }

                // 4. Test RouteRepository
                try
                {
                    var routes = _routeRepository.GetRoutesByDate(DateTime.Today);
                    diagnosticLog.AppendLine($"✅ RouteRepository.GetRoutesByDate: SUCCESS - {routes.Count} routes found");
                }
                catch (Exception ex)
                {
                    diagnosticLog.AppendLine($"❌ RouteRepository.GetRoutesByDate: FAILED - {ex.Message}");
                }

                // 5. Test ActivityRepository
                try
                {
                    var activities = _activityRepository.GetAllActivities();
                    diagnosticLog.AppendLine($"✅ ActivityRepository.GetAllActivities: SUCCESS - {activities.Count} activities found");
                }
                catch (Exception ex)
                {
                    diagnosticLog.AppendLine($"❌ ActivityRepository.GetAllActivities: FAILED - {ex.Message}");
                }

                // 6. Test BusService
                try
                {
                    var buses = await _busService.GetAllBusesAsync();
                    diagnosticLog.AppendLine($"✅ BusService.GetAllBusesAsync: SUCCESS - {buses.Count} buses found");
                }
                catch (Exception ex)
                {
                    diagnosticLog.AppendLine($"❌ BusService.GetAllBusesAsync: FAILED - {ex.Message}");
                }

                // 7. Test RouteAnalyticsService
                try
                {
                    var metrics = await _routeAnalyticsService.GetRouteEfficiencyMetricsAsync(DateTime.Today.AddDays(-7), DateTime.Today);
                    diagnosticLog.AppendLine($"✅ RouteAnalyticsService.GetRouteEfficiencyMetricsAsync: SUCCESS - {metrics.Count} metrics found");
                }
                catch (Exception ex)
                {
                    diagnosticLog.AppendLine($"❌ RouteAnalyticsService.GetRouteEfficiencyMetricsAsync: FAILED - {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                diagnosticLog.AppendLine($"❌ Diagnostic process error: {ex.Message}");
            }

            var results = diagnosticLog.ToString();
            Console.WriteLine(results);
            return results;
        }

        #endregion

        #region Private Methods

        private void LoadMockData()
        {
            VehicleData = new ObservableCollection<VehicleData>
            {
                new VehicleData { BusId = 1, BusNumber = "001", Make = "Blue Bird", Model = "Vision", Year = 2020, Status = "Active", MaintenanceStatus = "Good" },
                new VehicleData { BusId = 2, BusNumber = "002", Make = "Thomas", Model = "Saf-T-Liner", Year = 2019, Status = "Active", MaintenanceStatus = "Maintenance" }
            };

            RoutesData = new ObservableCollection<RouteData>
            {
                new RouteData { RouteId = 1, RouteName = "Route 1", Date = DateTime.Now, AMMiles = 45.2m, PMMiles = 47.1m, AMRiders = 35, PMRiders = 42, RouteType = "REG" },
                new RouteData { RouteId = 2, RouteName = "Route 2", Date = DateTime.Now, AMMiles = 52.8m, PMMiles = 54.3m, AMRiders = 28, PMRiders = 31, RouteType = "REG" }
            };

            ActivityData = new ObservableCollection<ActivityData>
            {
                new ActivityData { ActivityID = 1, Date = DateTime.Now, bus = "Bus 12", Destination = "Football Game", ScheduledRiders = 45, Distance = 12.5, Status = "Completed" },
                new ActivityData { ActivityID = 2, Date = DateTime.Now, bus = "Bus 03", Destination = "Field Trip", ScheduledRiders = 38, Distance = 8.2, Status = "Scheduled" }
            };

            var trendData = new ObservableCollection<ChartDataPoint>();
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i);
                trendData.Add(new ChartDataPoint
                {
                    // Format as simple month/day to avoid regional formatting issues
                    Date = date,
                    Value = 4500 + (new Random().NextDouble() * 400)
                });
            }
            MileageTrends = trendData;

            TotalMiles = RoutesData.Sum(r => (double)(r.AMMiles ?? 0) + (double)(r.PMMiles ?? 0));
            PupilsTransported = RoutesData.Sum(r => (r.AMRiders ?? 0) + (r.PMRiders ?? 0));
        }

        /// <summary>
        /// Attempts to load real data from repositories
        /// Falls back to mock data if the real data cannot be loaded
        /// </summary>
        private async Task LoadRealDataAsync()
        {
            try
            {
                StatusMessage = "Loading real data from repositories...";
                Console.WriteLine("🔄 Attempting to load real data from repositories");

                // Log connection string being used to help diagnose connectivity issues
                var connectionInfo = GetConnectionInfo();
                Console.WriteLine($"🔍 Connection info: Server={connectionInfo.Server}, Database={connectionInfo.Database}, Trusted={connectionInfo.TrustedConnection}");

                // Check SQL connection before attempting to load data
                var connectionSuccessful = await VerifySqlConnection(connectionInfo.ConnectionString);
                if (!connectionSuccessful)
                {
                    Console.WriteLine("⚠️ SQL Server connection test failed, likely to see repository errors");
                }

                var buses = new ObservableCollection<VehicleData>();
                var routes = new ObservableCollection<RouteData>();
                var activities = new ObservableCollection<ActivityData>();
                var mileageTrend = new ObservableCollection<ChartDataPoint>();

                DateTime endDate = DateTime.Today;
                DateTime startDate = endDate.AddDays(-7);

                // Use tasks for parallel loading, but capture detailed errors
                var vehicleTask = Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine("🔍 Starting bus data load...");
                        var allBuses = await _busService.GetAllBusesAsync();
                        foreach (var bus in allBuses)
                        {
                            buses.Add(new VehicleData
                            {
                                BusId = bus.BusId,
                                BusNumber = bus.BusNumber ?? "",
                                Make = bus.Make ?? "",
                                Model = bus.Model ?? "",
                                Year = bus.Year ?? 0,
                                Status = bus.Status ?? "Unknown",
                                MaintenanceStatus = "Unknown"
                            });
                        }
                        Console.WriteLine($"✅ Loaded {buses.Count} buses from BusService");
                    }
                    catch (Exception ex)
                    {
                        _errorHandler.HandleException(ex, "Loading buses from BusService");
                        Console.WriteLine($"❌ Failed to load buses from BusService: {ex.Message} | Inner: {ex.InnerException?.Message} | Stack: {ex.StackTrace}");
                        try
                        {
                            Console.WriteLine("🔍 Falling back to BusRepository...");
                            var allBuses = _busRepository.GetAllBuses();
                            foreach (var bus in allBuses)
                            {
                                buses.Add(new VehicleData
                                {
                                    BusId = bus.BusId,
                                    BusNumber = bus.BusNumber ?? bus.BusNumber ?? "",
                                    Make = bus.Make ?? "",
                                    Model = bus.Model ?? "",
                                    Year = bus.Year ?? 0,
                                    Status = bus.Status ?? "Unknown",
                                    MaintenanceStatus = "Unknown"
                                });
                            }
                            Console.WriteLine($"✅ Loaded {buses.Count} buses from BusRepository");
                        }
                        catch (Exception busEx)
                        {
                            _errorHandler.HandleException(busEx, "Loading buses from BusRepository");
                            Console.WriteLine($"❌ Failed to load buses from BusRepository: {busEx.Message} | Inner: {busEx.InnerException?.Message} | Stack: {busEx.StackTrace}");
                        }
                    }
                });

                var routeTask = Task.Run(() =>
                {
                    try
                    {
                        Console.WriteLine("🔍 Starting route data load...");
                        var allRoutes = new List<Route>();
                        for (var date = startDate; date <= endDate; date = date.AddDays(1))
                        {
                            var routesForDay = _routeRepository.GetRoutesByDate(date);
                            allRoutes.AddRange(routesForDay);
                        }

                        foreach (var route in allRoutes)
                        {
                            routes.Add(new RouteData
                            {
                                RouteId = route.RouteId,
                                RouteName = route.RouteName ?? "",
                                Date = route.DateAsDateTime,
                                AMMiles = route.AMMiles,
                                PMMiles = route.PMMiles,
                                AMRiders = route.AMRiders,
                                PMRiders = route.PMRiders,
                                RouteType = route.RouteType ?? "REG"
                            });
                        }
                        Console.WriteLine($"✅ Loaded {routes.Count} routes from repository");
                    }
                    catch (Exception ex)
                    {
                        _errorHandler.HandleException(ex, "Loading routes");
                        Console.WriteLine($"❌ Failed to load routes: {ex.Message} | Inner: {ex.InnerException?.Message} | Stack: {ex.StackTrace}");
                    }
                });

                var activityTask = Task.Run(() =>
                {
                    try
                    {
                        Console.WriteLine("🔍 Starting activity data load...");
                        var allActivities = _activityRepository.GetAllActivities();
                        foreach (var activity in allActivities.Take(10))
                        {
                            var BusNumber = activity.AssignedVehicle?.BusNumber ?? "Unknown";
                            activities.Add(new ActivityData
                            {
                                Date = activity.DateAsDateTime ?? DateTime.Today,
                                bus = $"Bus {BusNumber}",
                                Destination = activity.Destination ?? "Unknown",
                                ScheduledRiders = 0,
                                Distance = 0,
                                Status = activity.ActivityType ?? "Unknown"
                            });
                        }
                        Console.WriteLine($"✅ Loaded {activities.Count} activities from repository");
                    }
                    catch (Exception ex)
                    {
                        _errorHandler.HandleException(ex, "Loading activities");
                        Console.WriteLine($"❌ Failed to load activities: {ex.Message} | Inner: {ex.InnerException?.Message} | Stack: {ex.StackTrace}");
                    }
                });

                var mileageTask = ShouldAllowDataLoad() ? Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine("🔍 Starting mileage trend data load...");
                        if ((endDate - startDate).Days > 365)
                        {
                            Console.WriteLine("⚠️ Date range too large, limiting to 30 days for performance");
                            endDate = startDate.AddDays(30);
                        }

                        var metrics = await _routeAnalyticsService.GetRouteEfficiencyMetricsAsync(startDate, endDate);
                        if (metrics != null && metrics.Count > 0)
                        {
                            var groupedMetrics = metrics
                                .GroupBy(m => m.Date.Date)
                                .OrderBy(g => g.Key)
                                .Take(100)
                                .ToList();

                            foreach (var group in groupedMetrics)
                            {
                                mileageTrend.Add(new ChartDataPoint
                                {
                                    Date = group.Key,
                                    Value = group.Sum(m => m.TotalMiles)
                                });
                            }

                            if (mileageTrend.Count > 0)
                            {
                                Console.WriteLine($"✅ Loaded mileage trend data for {mileageTrend.Count} days");
                                RecordDataLoadSuccess("Mileage trend data");
                            }
                            else
                            {
                                Console.WriteLine("⚠️ No mileage trend data available");
                            }
                        }
                        else
                        {
                            Console.WriteLine("⚠️ No route efficiency metrics returned");
                        }
                    }
                    catch (Exception ex)
                    {
                        RecordDataLoadFailure("Mileage trend data", ex);
                        _errorHandler.HandleException(ex, "Loading mileage trends");
                        Console.WriteLine($"❌ Failed to load mileage trends: {ex.Message} | Inner: {ex.InnerException?.Message} | Stack: {ex.StackTrace}");
                    }
                }) : Task.CompletedTask;

                await Task.WhenAll(vehicleTask, routeTask, activityTask, mileageTask);

                VehicleData = buses;
                RoutesData = routes;
                ActivityData = activities;
                MileageTrends = mileageTrend;

                try
                {
                    if (routes.Count > 0)
                    {
                        TotalMiles = (double)(routes.Sum(r => (r.AMMiles ?? 0) + (r.PMMiles ?? 0)));
                        PupilsTransported = routes.Sum(r => (r.AMRiders ?? 0) + (r.PMRiders ?? 0));

                        if (PupilsTransported > 0)
                        {
                            Console.WriteLine("🔍 Calculating cost per student metrics...");
                            var costPerStudentMetrics = await _routeAnalyticsService.CalculateCostPerStudentMetricsAsync(startDate, endDate);
                            if (costPerStudentMetrics != null && costPerStudentMetrics.RouteCostPerStudentPerDay > 0)
                            {
                                CostPerStudent = (double)costPerStudentMetrics.RouteCostPerStudentPerDay;
                                // Set default values for state and local contributions
                                StateContribution = 5_000_000m;
                                LocalContribution = 4_500_000m;
                                Console.WriteLine($"✅ Cost per student calculated: ${CostPerStudent:F2}");
                            }
                            else
                            {
                                Console.WriteLine("⚠️ Cost per student metrics unavailable, using fallback values");
                                CostPerStudent = 2.50;
                                StateContribution = 5_000_000m;
                                LocalContribution = 4_500_000m;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ No routes loaded, skipping summary metrics calculation");
                    }
                }
                catch (Exception ex)
                {
                    _errorHandler.HandleException(ex, "Calculating summary metrics");
                    Console.WriteLine($"❌ Failed to calculate summary metrics: {ex.Message} | Inner: {ex.InnerException?.Message} | Stack: {ex.StackTrace}");
                }

                StatusMessage = routes.Count > 0 || buses.Count > 0 || activities.Count > 0
                    ? "Data loaded successfully"
                    : "No real data loaded, falling back to mock data";

                if (routes.Count == 0 && buses.Count == 0 && activities.Count == 0)
                {
                    Console.WriteLine("❌ No real data loaded from any repository, falling back to mock data");
                    throw new InvalidOperationException("All repository calls failed to return data");
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(ex, "Loading dashboard data");
                StatusMessage = "Error loading data. Using mock data instead.";
                Console.WriteLine($"❌ Error loading real data: {ex.Message} | Inner: {ex.InnerException?.Message} | Stack: {ex.StackTrace}. Falling back to mock data.");
                LoadMockData();
            }
        }

        /// <summary>
        /// Determines if data loading should be allowed based on current state
        /// </summary>
        /// <returns>True if data loading is allowed, false otherwise</returns>
        private bool ShouldAllowDataLoad()
        {
            if (_isLoading)
            {
                Console.WriteLine("⚠️ Data load already in progress");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Records successful data load operation
        /// </summary>
        /// <param name="operation">Name of the operation that succeeded</param>
        private void RecordDataLoadSuccess(string operation)
        {
            Console.WriteLine($"✅ {operation} loaded successfully");
            StatusMessage = $"{operation} loaded successfully";
        }

        /// <summary>
        /// Records data load failure with error details
        /// </summary>
        /// <param name="operation">Name of the operation that failed</param>
        /// <param name="ex">Exception that occurred</param>
        private void RecordDataLoadFailure(string operation, Exception ex)
        {
            Console.WriteLine($"❌ Failed to load {operation}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }

            StatusMessage = $"Failed to load {operation}";
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Clean up managed resources
            _vehicleData.Clear();
            _routesData.Clear();
            _activityData.Clear();
            _mileageTrends.Clear();

            // Suppress finalization
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Raises the PropertyChanged event when a property value changes
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

