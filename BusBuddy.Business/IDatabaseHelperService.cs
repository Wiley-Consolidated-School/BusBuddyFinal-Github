using System;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Business
{
    public interface IDatabaseHelperService
    {
        Route GetRouteWithDetails(int RouteId);
        List<Route> GetRoutesWithDetailsByDate(DateTime date);
        List<Route> GetAllRoutesWithDetails();
        Activity GetActivityWithDetails(int activityId);
        Fuel GetFuelRecordWithDetails(int fuelId);
        Maintenance GetMaintenanceWithDetails(int maintenanceId);
        ActivitySchedule GetActivityScheduleWithDetails(int scheduleId);
        BusDetailsViewModel GetBusDetails(int busId);
        DriverDetailsViewModel GetDriverDetails(int DriverId);
        
        // Bus methods
        Bus GetBusById(int id);
        List<Bus> GetAllBuses();
        
        // Route method  
        Route GetRouteById(int id);
    }
}

