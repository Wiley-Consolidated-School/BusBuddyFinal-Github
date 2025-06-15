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
        VehicleDetailsViewModel GetVehicleDetails(int vehicleId);
        DriverDetailsViewModel GetDriverDetails(int driverId);
    }
}
