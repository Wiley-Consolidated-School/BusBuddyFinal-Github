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
            var route = _routeRepository.GetRouteById(routeId);
            if (route != null)
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

            return route;
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
