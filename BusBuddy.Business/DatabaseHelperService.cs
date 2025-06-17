using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BusBuddy.Models;
using Dapper;
using BusBuddy.Data;

namespace BusBuddy.Business
{
    public class DatabaseHelperService : IDatabaseHelperService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IRouteRepository _routeRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IFuelRepository _fuelRepository;
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly ISchoolCalendarRepository _schoolCalendarRepository;
        private readonly IActivityScheduleRepository _activityScheduleRepository;

        public DatabaseHelperService()
        {
            _vehicleRepository = new VehicleRepository();
            _driverRepository = new DriverRepository();
            _routeRepository = new RouteRepository();
            _activityRepository = new ActivityRepository();
            _fuelRepository = new FuelRepository();
            _maintenanceRepository = new MaintenanceRepository();
            _schoolCalendarRepository = new SchoolCalendarRepository();
            _activityScheduleRepository = new ActivityScheduleRepository();
        }

        public Route GetRouteWithDetails(int routeId)
        {
            // Add comprehensive error logging
            Console.WriteLine($"GetRouteWithDetails called with routeId: {routeId}");

            try
            {
                var route = _routeRepository.GetRouteById(routeId);

                if (route == null)
                {
                    Console.WriteLine($"ERROR: Route with ID {routeId} not found in repository");
                    return null;
                }

                Console.WriteLine($"Route found: {route.RouteID}, {route.RouteName}");

                // Load AM Vehicle details
                if (route.AMVehicleID.HasValue)
                {
                    route.AMVehicle = _vehicleRepository.GetVehicleById(route.AMVehicleID.Value);
                    if (route.AMVehicle == null)
                    {
                        Console.WriteLine($"WARNING: AM Vehicle with ID {route.AMVehicleID.Value} not found");
                    }
                    else
                    {
                        Console.WriteLine($"AM Vehicle loaded: {route.AMVehicle.VehicleNumber}");
                    }
                }

                // Load AM Driver details
                if (route.AMDriverID.HasValue)
                {
                    route.AMDriver = _driverRepository.GetDriverById(route.AMDriverID.Value);
                    if (route.AMDriver == null)
                    {
                        Console.WriteLine($"WARNING: AM Driver with ID {route.AMDriverID.Value} not found");
                    }
                    else
                    {
                        Console.WriteLine($"AM Driver loaded: {route.AMDriver.Name}");
                    }
                }

                // Load PM Vehicle details
                if (route.PMVehicleID.HasValue)
                {
                    route.PMVehicle = _vehicleRepository.GetVehicleById(route.PMVehicleID.Value);
                    if (route.PMVehicle == null)
                    {
                        Console.WriteLine($"WARNING: PM Vehicle with ID {route.PMVehicleID.Value} not found");
                    }
                    else
                    {
                        Console.WriteLine($"PM Vehicle loaded: {route.PMVehicle.VehicleNumber}");
                    }
                }

                // Load PM Driver details
                if (route.PMDriverID.HasValue)
                {
                    route.PMDriver = _driverRepository.GetDriverById(route.PMDriverID.Value);
                    if (route.PMDriver == null)
                    {
                        Console.WriteLine($"WARNING: PM Driver with ID {route.PMDriverID.Value} not found");
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
                if (route.AMVehicleID.HasValue)
                {
                    route.AMVehicle = _vehicleRepository.GetVehicleById(route.AMVehicleID.Value);
                }

                if (route.AMDriverID.HasValue)
                {
                    route.AMDriver = _driverRepository.GetDriverById(route.AMDriverID.Value);
                }

                if (route.PMVehicleID.HasValue)
                {
                    route.PMVehicle = _vehicleRepository.GetVehicleById(route.PMVehicleID.Value);
                }

                if (route.PMDriverID.HasValue)
                {
                    route.PMDriver = _driverRepository.GetDriverById(route.PMDriverID.Value);
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
                    activity.AssignedVehicle = _vehicleRepository.GetVehicleById(activity.AssignedVehicleID.Value);
                }

                if (activity.DriverID.HasValue)
                {
                    activity.Driver = _driverRepository.GetDriverById(activity.DriverID.Value);
                }
            }

            return activity;
        }

        public Fuel GetFuelRecordWithDetails(int fuelId)
        {
            var fuelRecord = _fuelRepository.GetFuelRecordById(fuelId);
            if (fuelRecord != null && fuelRecord.VehicleFueledID.HasValue)
            {
                fuelRecord.VehicleFueled = _vehicleRepository.GetVehicleById(fuelRecord.VehicleFueledID.Value);
            }

            return fuelRecord;
        }

        public Maintenance GetMaintenanceWithDetails(int maintenanceId)
        {
            var maintenance = _maintenanceRepository.GetMaintenanceById(maintenanceId);
            if (maintenance != null && maintenance.VehicleID.HasValue)
            {
                maintenance.Vehicle = _vehicleRepository.GetVehicleById(maintenance.VehicleID.Value);
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
                    schedule.ScheduledVehicle = _vehicleRepository.GetVehicleById(schedule.ScheduledVehicleID.Value);
                }

                if (schedule.ScheduledDriverID.HasValue)
                {
                    schedule.ScheduledDriver = _driverRepository.GetDriverById(schedule.ScheduledDriverID.Value);
                }
            }

            return schedule;
        }

        // Get vehicle with all associated records
        public VehicleDetailsViewModel GetVehicleDetails(int vehicleId)
        {
            var vehicle = _vehicleRepository.GetVehicleById(vehicleId);
            if (vehicle == null)
            {
                return null;
            }

            var details = new VehicleDetailsViewModel
            {
                Vehicle = vehicle,
                AMRoutes = _routeRepository.GetRoutesByVehicle(vehicleId).Where(r => r.AMVehicleID == vehicleId).ToList(),
                PMRoutes = _routeRepository.GetRoutesByVehicle(vehicleId).Where(r => r.PMVehicleID == vehicleId).ToList(),
                Activities = _activityRepository.GetActivitiesByVehicle(vehicleId),
                FuelRecords = _fuelRepository.GetFuelRecordsByVehicle(vehicleId),
                MaintenanceRecords = _maintenanceRepository.GetMaintenanceByVehicle(vehicleId),
                ScheduledActivities = _activityScheduleRepository.GetScheduledActivitiesByVehicle(vehicleId)
            };

            return details;
        }

        // Get driver with all associated records
        public DriverDetailsViewModel GetDriverDetails(int driverId)
        {
            var driver = _driverRepository.GetDriverById(driverId);
            if (driver == null)
            {
                return null;
            }

            var details = new DriverDetailsViewModel
            {
                Driver = driver,
                AMRoutes = _routeRepository.GetRoutesByDriver(driverId).Where(r => r.AMDriverID == driverId).ToList(),
                PMRoutes = _routeRepository.GetRoutesByDriver(driverId).Where(r => r.PMDriverID == driverId).ToList(),
                Activities = _activityRepository.GetActivitiesByDriver(driverId),
                ScheduledActivities = _activityScheduleRepository.GetScheduledActivitiesByDriver(driverId)
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
                    if (route.AMVehicleID.HasValue)
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Loading AM Vehicle {route.AMVehicleID}");
                            route.AMVehicle = _vehicleRepository.GetVehicleById(route.AMVehicleID.Value);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Error loading AM Vehicle: {ex.Message}");
                            route.AMVehicle = null; // Gracefully handle missing vehicle
                        }
                    }

                    if (route.AMDriverID.HasValue)
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Loading AM Driver {route.AMDriverID}");
                            route.AMDriver = _driverRepository.GetDriverById(route.AMDriverID.Value);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Error loading AM Driver: {ex.Message}");
                            route.AMDriver = null; // Gracefully handle missing driver
                        }
                    }

                    if (route.PMVehicleID.HasValue)
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Loading PM Vehicle {route.PMVehicleID}");
                            route.PMVehicle = _vehicleRepository.GetVehicleById(route.PMVehicleID.Value);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Error loading PM Vehicle: {ex.Message}");
                            route.PMVehicle = null; // Gracefully handle missing vehicle
                        }
                    }

                    if (route.PMDriverID.HasValue)
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"DatabaseHelperService: Loading PM Driver {route.PMDriverID}");
                            route.PMDriver = _driverRepository.GetDriverById(route.PMDriverID.Value);
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
            }        }
    }

    public class VehicleDetailsViewModel
    {
        public Vehicle Vehicle { get; set; }
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
