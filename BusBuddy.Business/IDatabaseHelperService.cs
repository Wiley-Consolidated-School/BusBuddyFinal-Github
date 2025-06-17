using System;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Business
{
    public interface IDatabaseHelperService
    {
        Route GetRouteWithDetails(int routeId);
        List<Route> GetRoutesWithDetailsByDate(DateTime date);
        List<Route> GetAllRoutesWithDetails();
        Activity GetActivityWithDetails(int activityId);
        Fuel GetFuelRecordWithDetails(int fuelId);
        Maintenance GetMaintenanceWithDetails(int maintenanceId);
        ActivitySchedule GetActivityScheduleWithDetails(int scheduleId);
        VehicleDetailsViewModel GetVehicleDetails(int vehicleId);
        DriverDetailsViewModel GetDriverDetails(int driverId);
    }
}
