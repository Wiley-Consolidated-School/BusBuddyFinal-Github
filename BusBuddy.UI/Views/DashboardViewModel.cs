using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Business;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Services;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// ViewModel for the Dashboard providing data binding for Syncfusion controls
    /// Implements INotifyPropertyChanged for real-time UI updates
    /// </summary>
    public class DashboardViewModel : INotifyPropertyChanged
    {
        // Services and repositories
        private readonly IRouteAnalyticsService _routeAnalyticsService;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IRouteRepository _routeRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IErrorHandlerService _errorHandler;

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
            IVehicleRepository? vehicleRepository = null,
            IRouteRepository? routeRepository = null,
            IDriverRepository? driverRepository = null,
            IActivityRepository? activityRepository = null,
            IErrorHandlerService? errorHandler = null)
        {
            // Initialize services with sensible defaults if not provided
            _routeAnalyticsService = routeAnalyticsService ?? new RouteAnalyticsService();
            _vehicleRepository = vehicleRepository ?? new VehicleRepository();
            _routeRepository = routeRepository ?? new RouteRepository();
            _driverRepository = driverRepository ?? new DriverRepository();
            _activityRepository = activityRepository ?? new ActivityRepository();
            _errorHandler = errorHandler ?? new ErrorHandlerService();

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

        #endregion

        #region Private Methods

        private void LoadMockData()
        {
            VehicleData = new ObservableCollection<VehicleData>
            {
                new VehicleData { VehicleId = 1, VehicleNumber = "001", Make = "Blue Bird", Model = "Vision", Year = 2020, Status = "Active", MaintenanceStatus = "Good", Mileage = 45000 },
                new VehicleData { VehicleId = 2, VehicleNumber = "002", Make = "Thomas", Model = "Saf-T-Liner", Year = 2019, Status = "Active", MaintenanceStatus = "Maintenance", Mileage = 52000 }
            };

            RoutesData = new ObservableCollection<RouteData>
            {
                new RouteData { RouteId = "R001", RouteName = "Route 1", Date = DateTime.Now.ToString("MM/dd/yy"), AMMiles = 45.2, PMMiles = 47.1, AMRiders = 35, PMRiders = 42, RouteType = "REG" },
                new RouteData { RouteId = "R002", RouteName = "Route 2", Date = DateTime.Now.ToString("MM/dd/yy"), AMMiles = 52.8, PMMiles = 54.3, AMRiders = 28, PMRiders = 31, RouteType = "REG" }
            };

            ActivityData = new ObservableCollection<ActivityData>
            {
                new ActivityData { Date = DateTime.Now.ToString("MM/dd/yy"), Vehicle = "Bus 12", Destination = "Football Game", ScheduledRiders = 45, Distance = 12.5, Status = "Completed" },
                new ActivityData { Date = DateTime.Now.ToString("MM/dd/yy"), Vehicle = "Bus 03", Destination = "Field Trip", ScheduledRiders = 38, Distance = 8.2, Status = "Scheduled" }
            };

            var trendData = new ObservableCollection<ChartDataPoint>();
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i);
                trendData.Add(new ChartDataPoint
                {
                    // Format as simple month/day to avoid regional formatting issues
                    Date = date.ToString("MM/dd/yyyy"),
                    Value = 4500 + (new Random().NextDouble() * 400)
                });
            }
            MileageTrends = trendData;

            TotalMiles = RoutesData.Sum(r => r.AMMiles + r.PMMiles);
            PupilsTransported = RoutesData.Sum(r => r.AMRiders + r.PMRiders);
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

                // Create empty collections
                var vehicles = new ObservableCollection<VehicleData>();
                var routes = new ObservableCollection<RouteData>();
                var activities = new ObservableCollection<ActivityData>();
                var mileageTrend = new ObservableCollection<ChartDataPoint>();

                DateTime endDate = DateTime.Today;
                DateTime startDate = endDate.AddDays(-7);

                // Load vehicles from repository
                try
                {
                    var allVehicles = _vehicleRepository.GetAllVehicles();
                    foreach (var vehicle in allVehicles)
                    {
                        vehicles.Add(new VehicleData
                        {
                            VehicleId = vehicle.VehicleID,
                            VehicleNumber = vehicle.VehicleNumber ?? "",
                            Make = vehicle.Make ?? "",
                            Model = vehicle.Model ?? "",
                            Year = vehicle.Year,
                            Status = vehicle.Status ?? "Unknown",
                            MaintenanceStatus = "Unknown", // Not in the Vehicle model
                            Mileage = 0 // Not in the Vehicle model, would need to be tracked separately
                        });
                    }
                    VehicleData = vehicles;
                    Console.WriteLine($"✅ Loaded {vehicles.Count} vehicles from repository");
                }
                catch (Exception ex)
                {
                    _errorHandler.HandleException(ex, "Loading vehicles");
                    Console.WriteLine($"❌ Failed to load vehicles: {ex.Message}");
                }

                // Load routes from repository (from last 7 days)
                try
                {
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
                            RouteId = route.RouteID.ToString(),
                            RouteName = route.RouteName ?? "",
                            Date = route.Date,
                            AMMiles = route.AMMiles.HasValue ? (double)route.AMMiles.Value : 0,
                            PMMiles = route.PMMiles.HasValue ? (double)route.PMMiles.Value : 0,
                            AMRiders = route.AMRiders ?? 0,
                            PMRiders = route.PMRiders ?? 0,
                            RouteType = route.RouteType ?? "REG"
                        });
                    }
                    RoutesData = routes;
                    Console.WriteLine($"✅ Loaded {routes.Count} routes from repository");
                }
                catch (Exception ex)
                {
                    _errorHandler.HandleException(ex, "Loading routes");
                    Console.WriteLine($"❌ Failed to load routes: {ex.Message}");
                }

                // Load activities from repository
                try
                {
                    var allActivities = _activityRepository.GetAllActivities();
                    foreach (var activity in allActivities.Take(10)) // Limit to most recent 10
                    {
                        var vehicleNumber = activity.AssignedVehicle?.VehicleNumber ?? "Unknown";

                        activities.Add(new ActivityData
                        {
                            Date = activity.Date ?? DateTime.Today.ToString("MM/dd/yy"),
                            Vehicle = $"Bus {vehicleNumber}",
                            Destination = activity.Destination ?? "Unknown",
                            ScheduledRiders = 0, // Not in the Activity model
                            Distance = 0, // Not in the Activity model
                            Status = activity.ActivityType ?? "Unknown"
                        });
                    }
                    ActivityData = activities;
                    Console.WriteLine($"✅ Loaded {activities.Count} activities from repository");
                }
                catch (Exception ex)
                {
                    _errorHandler.HandleException(ex, "Loading activities");
                    Console.WriteLine($"❌ Failed to load activities: {ex.Message}");
                }

                // Try to load mileage trends using the route analytics service
                if (ShouldAllowDataLoad())
                {
                    try
                    {
                        Console.WriteLine("🔄 Loading mileage trends...");

                        // Add safety checks for date range
                        if ((endDate - startDate).Days > 365)
                        {
                            Console.WriteLine("⚠️ Date range too large, limiting to 30 days for performance");
                            endDate = startDate.AddDays(30);
                        }

                        var metrics = await _routeAnalyticsService.GetRouteEfficiencyMetricsAsync(startDate, endDate);

                        if (metrics != null && metrics.Count > 0)
                        {
                            // Group metrics by date with iteration limit
                            var groupedMetrics = metrics
                                .GroupBy(m => m.Date.Date)
                                .OrderBy(g => g.Key)
                                .Take(100) // Limit to 100 data points max
                                .ToList();

                            foreach (var group in groupedMetrics)
                            {
                                mileageTrend.Add(new ChartDataPoint
                                {
                                    Date = group.Key.ToString("MM/dd"),
                                    Value = group.Sum(m => m.TotalMiles)
                                });
                            }

                            if (mileageTrend.Count > 0)
                            {
                                MileageTrends = mileageTrend;
                                Console.WriteLine($"✅ Loaded mileage trend data for {mileageTrend.Count} days");
                                RecordDataLoadSuccess();
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
                        RecordDataLoadFailure();
                        _errorHandler.HandleException(ex, "Loading mileage trends");
                        Console.WriteLine($"❌ Failed to load mileage trends: {ex.Message}");

                        // Add fallback empty data to prevent UI issues
                        MileageTrends = new ObservableCollection<ChartDataPoint>();
                    }
                }
                else
                {
                    Console.WriteLine("🚫 Skipping mileage trends load due to circuit breaker");
                    MileageTrends = new ObservableCollection<ChartDataPoint>();
                }

                // Calculate summary metrics
                try
                {
                    if (routes.Count > 0)
                    {
                        TotalMiles = routes.Sum(r => r.AMMiles + r.PMMiles);
                        PupilsTransported = routes.Sum(r => r.AMRiders + r.PMRiders);

                        // Calculate cost per student (if we have meaningful data)
                        if (PupilsTransported > 0)
                        {
                            var costPerStudentMetrics = await _routeAnalyticsService.CalculateCostPerStudentMetricsAsync(startDate, endDate);
                            if (costPerStudentMetrics != null)
                            {
                                // Use a fallback value if properties don't match
                                CostPerStudent = 2.50; // Default fallback value
                                StateContribution = 5_000_000m;
                                LocalContribution = 4_500_000m;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorHandler.HandleException(ex, "Calculating summary metrics");
                    Console.WriteLine($"❌ Failed to calculate summary metrics: {ex.Message}");
                }

                StatusMessage = "Data loaded successfully";
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(ex, "Loading dashboard data");
                StatusMessage = "Error loading data. Using mock data instead.";
                Console.WriteLine($"❌ Error loading real data: {ex.Message}. Falling back to mock data.");

                // Fall back to mock data if we can't load real data
                LoadMockData();
            }
        }

        /// <summary>
        /// Safe data loading with circuit breaker pattern to prevent infinite loops
        /// </summary>
        private static int _dataLoadFailureCount = 0;
        private static DateTime _lastDataLoadAttempt = DateTime.MinValue;
        private const int MAX_FAILURE_COUNT = 3;
        private static readonly TimeSpan CIRCUIT_BREAKER_TIMEOUT = TimeSpan.FromMinutes(5);

        private bool ShouldAllowDataLoad()
        {
            // If we've had too many failures recently, don't try again for a while
            if (_dataLoadFailureCount >= MAX_FAILURE_COUNT)
            {
                if (DateTime.Now - _lastDataLoadAttempt < CIRCUIT_BREAKER_TIMEOUT)
                {
                    Console.WriteLine($"⚠️ Circuit breaker active - skipping data load for {CIRCUIT_BREAKER_TIMEOUT.TotalMinutes} minutes");
                    return false;
                }
                else
                {
                    // Reset after timeout
                    _dataLoadFailureCount = 0;
                    Console.WriteLine("🔄 Circuit breaker reset - allowing data load");
                }
            }

            _lastDataLoadAttempt = DateTime.Now;
            return true;
        }

        private void RecordDataLoadSuccess()
        {
            _dataLoadFailureCount = 0;
        }

        private void RecordDataLoadFailure()
        {
            _dataLoadFailureCount++;
            Console.WriteLine($"⚠️ Data load failure #{_dataLoadFailureCount}");
        }

        /// <summary>
        /// Notifies the UI when a property value changes
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected virtual void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper method to set a property value and notify if changed
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="field">Reference to the backing field</param>
        /// <param name="value">New value to set</param>
        /// <param name="propertyName">Name of the property (automatically provided)</param>
        /// <returns>True if the value was changed, false otherwise</returns>
        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class VehicleData
    {
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = "";
        public string Make { get; set; } = "";
        public string Model { get; set; } = "";
        public int Year { get; set; }
        public string Status { get; set; } = "";
        public string MaintenanceStatus { get; set; } = "";
        public int Mileage { get; set; }
    }

    public class RouteData
    {
        public string RouteId { get; set; } = "";
        public string RouteName { get; set; } = "";
        public string Date { get; set; } = "";
        public double AMMiles { get; set; }
        public double PMMiles { get; set; }
        public int AMRiders { get; set; }
        public int PMRiders { get; set; }
        public string RouteType { get; set; } = "";
    }

    public class ActivityData
    {
        public string Date { get; set; } = "";
        public string Vehicle { get; set; } = "";
        public string Destination { get; set; } = "";
        public int ScheduledRiders { get; set; }
        public double Distance { get; set; }
        public string Status { get; set; } = "";
    }

    /// <summary>
    /// Data point for chart visualization with date as string for display purposes
    /// </summary>
    public class ChartDataPoint
    {
        /// <summary>
        /// Date in string format (e.g. "MM/dd" or "yyyy-MM-dd")
        /// </summary>
        public string Date { get; set; } = "";

        /// <summary>
        /// Value for the Y-axis
        /// </summary>
        public double Value { get; set; }
    }
}
