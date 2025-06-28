using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BusBuddy.Data;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Business
{
    /// <summary>
    /// Business layer database helper service with complex data operations
    /// 🔧 FIXED: Now uses dependency injection instead of direct repository instantiation
    /// </summary>
    public class DatabaseHelperService : IDatabaseHelperService
    {
        private readonly IBusRepository _busRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IRouteRepository _routeRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IFuelRepository _fuelRepository;
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly ISchoolCalendarRepository _schoolCalendarRepository;
        private readonly IActivityScheduleRepository _activityScheduleRepository;

        /// <summary>
        /// 🔧 DEPENDENCY INJECTION CONSTRUCTOR - Updated to use IBusRepository interface
        /// All repositories are injected via DI container
        /// </summary>
        public DatabaseHelperService(
            IBusRepository busRepository,
            IDriverRepository driverRepository,
            IRouteRepository routeRepository,
            IActivityRepository activityRepository,
            IFuelRepository fuelRepository,
            IMaintenanceRepository maintenanceRepository,
            ISchoolCalendarRepository schoolCalendarRepository,
            IActivityScheduleRepository activityScheduleRepository)
        {
            _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            _routeRepository = routeRepository ?? throw new ArgumentNullException(nameof(routeRepository));
            _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
            _fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
            _maintenanceRepository = maintenanceRepository ?? throw new ArgumentNullException(nameof(maintenanceRepository));
            _schoolCalendarRepository = schoolCalendarRepository ?? throw new ArgumentNullException(nameof(schoolCalendarRepository));
            _activityScheduleRepository = activityScheduleRepository ?? throw new ArgumentNullException(nameof(activityScheduleRepository));
        }

        public Route GetRouteWithDetails(int RouteId)
        {
            // Add comprehensive error logging
            Console.WriteLine($"GetRouteWithDetails called with RouteId: {RouteId}");

            try
            {
                var route = _routeRepository.GetRouteById(RouteId);

                if (route == null)
                {
                    Console.WriteLine($"ERROR: Route with ID {RouteId} not found in repository");
                    return null;
                }

                Console.WriteLine($"Route found: {route.RouteId}, {route.RouteName}");

                // Load AM Vehicle details
                if (route.AMBusId.HasValue)
                {
                    route.AMBus = _busRepository.GetBusById(route.AMBusId.Value);
                    if (route.AMBus == null)
                    {
                        Console.WriteLine($"WARNING: AM Vehicle with ID {route.AMBusId.Value} not found");
                    }
                    else
                    {
                        Console.WriteLine($"AM Vehicle loaded: {route.AMBus.BusNumber}");
                    }
                }

                // Load AM Driver details
                if (route.AMDriverId.HasValue)
                {
                    route.AMDriver = _driverRepository.GetDriverById(route.AMDriverId.Value);
                    if (route.AMDriver == null)
                    {
                        Console.WriteLine($"WARNING: AM Driver with ID {route.AMDriverId.Value} not found");
                    }
                    else
                    {
                        Console.WriteLine($"AM Driver loaded: {route.AMDriver.Name}");
                    }
                }

                // Load PM Bus details
                if (route.PMBusId.HasValue)
                {
                    route.PMBus = _busRepository.GetBusById(route.PMBusId.Value);
                    if (route.PMBus == null)
                    {
                        Console.WriteLine($"WARNING: PM Bus with ID {route.PMBusId.Value} not found");
                    }
                    else
                    {
                        Console.WriteLine($"PM Bus loaded: {route.PMBus.BusNumber}");
                    }
                }

                // Load PM Driver details
                if (route.PMDriverId.HasValue)
                {
                    route.PMDriver = _driverRepository.GetDriverById(route.PMDriverId.Value);
                    if (route.PMDriver == null)
                    {
                        Console.WriteLine($"WARNING: PM Driver with ID {route.PMDriverId.Value} not found");
                    }
                    else
                    {
                        Console.WriteLine($"PM Driver loaded: {route.PMDriver.Name}");
                    }
                }

                Console.WriteLine("Route with details loaded successfully");
                return route;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetRouteWithDetails: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public List<Route> GetRoutesWithDetailsByDate(DateTime date)
        {
            var routes = _routeRepository.GetRoutesByDate(date);

            foreach (var route in routes)
            {
                if (route.AMBusId.HasValue)
                {
                    route.AMBus = _busRepository.GetBusById(route.AMBusId.Value);
                }

                if (route.AMDriverId.HasValue)
                {
                    route.AMDriver = _driverRepository.GetDriverById(route.AMDriverId.Value);
                }

                if (route.PMBusId.HasValue)
                {
                    route.PMBus = _busRepository.GetBusById(route.PMBusId.Value);
                }

                if (route.PMDriverId.HasValue)
                {
                    route.PMDriver = _driverRepository.GetDriverById(route.PMDriverId.Value);
                }
            }

            return routes;
        }

        public Activity GetActivityWithDetails(int activityId)
        {
            var activity = _activityRepository.GetActivityById(activityId);
            if (activity != null)
            {
                if (activity.AssignedVehicleID.HasValue)
                {
                    activity.AssignedVehicle = _busRepository.GetBusById(activity.AssignedVehicleID.Value);
                }

                if (activity.DriverId.HasValue)
                {
                    activity.Driver = _driverRepository.GetDriverById(activity.DriverId.Value);
                }
            }

            return activity;
        }

        public Fuel GetFuelRecordWithDetails(int fuelId)
        {
            var fuelRecord = _fuelRepository.GetFuelRecordById(fuelId);
            if (fuelRecord != null && fuelRecord.VehicleFueledID.HasValue)
            {
                fuelRecord.VehicleFueled = _busRepository.GetBusById(fuelRecord.VehicleFueledID.Value);
            }

            return fuelRecord;
        }

        public Maintenance GetMaintenanceWithDetails(int maintenanceId)
        {
            var maintenance = _maintenanceRepository.GetMaintenanceById(maintenanceId);
            if (maintenance != null && maintenance.BusId.HasValue)
            {
                maintenance.Vehicle = _busRepository.GetBusById(maintenance.BusId.Value);
            }

            return maintenance;
        }

        public ActivitySchedule GetActivityScheduleWithDetails(int scheduleId)
        {
            var schedule = _activityScheduleRepository.GetScheduledActivityById(scheduleId);
            if (schedule != null)
            {
                if (schedule.ScheduledVehicleID.HasValue)
                {
                    schedule.ScheduledVehicle = _busRepository.GetBusById(schedule.ScheduledVehicleID.Value);
                }

                if (schedule.ScheduledDriverID.HasValue)
                {
                    schedule.ScheduledDriver = _driverRepository.GetDriverById(schedule.ScheduledDriverID.Value);
                }
            }

            return schedule;
        }

        // Get bus with all associated records
        public BusDetailsViewModel GetBusDetails(int busId)
        {
            var bus = _busRepository.GetBusById(busId);
            if (bus == null)
            {
                return null;
            }

            var details = new BusDetailsViewModel
            {
                Bus = bus,
                AMRoutes = _routeRepository.GetRoutesByBus(busId).Where(r => r.AMBusId == busId).ToList(),
                PMRoutes = _routeRepository.GetRoutesByBus(busId).Where(r => r.PMBusId == busId).ToList(),
                Activities = _activityRepository.GetActivitiesByBus(busId),
                FuelRecords = _fuelRepository.GetFuelRecordsByBus(busId),
                MaintenanceRecords = _maintenanceRepository.GetMaintenanceByBus(busId),
                ScheduledActivities = _activityScheduleRepository.GetScheduledActivitiesByBus(busId)
            };

            return details;
        }

        /// <summary>
        /// Gets a bus by its ID
        /// </summary>
        public Bus GetBusById(int id)
        {
            return _busRepository.GetBusById(id);
        }

        /// <summary>
        /// Gets all buses from the repository
        /// </summary>
        public List<Bus> GetAllBuses()
        {
            return _busRepository.GetAllBuses().ToList();
        }

        /// <summary>
        /// Gets a route by its ID
        /// </summary>
        public Route GetRouteById(int id)
        {
            return _routeRepository.GetRouteById(id);
        }

        // Get driver with all associated records
        public DriverDetailsViewModel GetDriverDetails(int DriverId)
        {
            var driver = _driverRepository.GetDriverById(DriverId);
            if (driver == null)
            {
                return null;
            }

            var details = new DriverDetailsViewModel
            {
                Driver = driver,
                AMRoutes = _routeRepository.GetRoutesByDriver(DriverId).Where(r => r.AMDriverId == DriverId).ToList(),
                PMRoutes = _routeRepository.GetRoutesByDriver(DriverId).Where(r => r.PMDriverId == DriverId).ToList(),
                Activities = _activityRepository.GetActivitiesByDriver(DriverId),
                ScheduledActivities = _activityScheduleRepository.GetScheduledActivitiesByDriver(DriverId)
            };

            return details;
        }

        public List<Route> GetAllRoutesWithDetails()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DatabaseHelperService: Getting all routes...");
                var routes = _routeRepository.GetAllRoutes() ?? new List<Route>();
                System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Found {routes.Count} routes");

                foreach (var route in routes)
                {
                    if (route.AMBusId.HasValue)
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Loading AM Vehicle {route.AMBusId}");
                            route.AMBus = _busRepository.GetBusById(route.AMBusId.Value);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Error loading AM Vehicle: {ex.Message}");
                            route.AMBus = null; // Gracefully handle missing vehicle
                        }
                    }

                    if (route.AMDriverId.HasValue)
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Loading AM Driver {route.AMDriverId}");
                            route.AMDriver = _driverRepository.GetDriverById(route.AMDriverId.Value);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Error loading AM Driver: {ex.Message}");
                            route.AMDriver = null; // Gracefully handle missing driver
                        }
                    }

                    if (route.PMBusId.HasValue)
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Loading PM Vehicle {route.PMBusId}");
                            route.PMBus = _busRepository.GetBusById(route.PMBusId.Value);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Error loading PM Vehicle: {ex.Message}");
                            route.PMBus = null; // Gracefully handle missing vehicle
                        }
                    }

                    if (route.PMDriverId.HasValue)
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Loading PM Driver {route.PMDriverId}");
                            route.PMDriver = _driverRepository.GetDriverById(route.PMDriverId.Value);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Error loading PM Driver: {ex.Message}");
                            route.PMDriver = null; // Gracefully handle missing driver
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("DatabaseHelperService: Completed loading route details");
                return routes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Exception in GetAllRoutesWithDetails: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }

    public class BusDetailsViewModel
    {
        public Bus Bus { get; set; }
        public List<Route> AMRoutes { get; set; }
        public List<Route> PMRoutes { get; set; }
        public List<Activity> Activities { get; set; }
        public List<Fuel> FuelRecords { get; set; }
        public List<Maintenance> MaintenanceRecords { get; set; }
        public List<ActivitySchedule> ScheduledActivities { get; set; }
    }

    public class DriverDetailsViewModel
    {
        public Driver Driver { get; set; }
        public List<Route> AMRoutes { get; set; }
        public List<Route> PMRoutes { get; set; }
        public List<Activity> Activities { get; set; }
        public List<ActivitySchedule> ScheduledActivities { get; set; }
    }
}

