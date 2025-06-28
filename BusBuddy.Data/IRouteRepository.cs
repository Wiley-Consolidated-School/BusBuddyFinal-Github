using System;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Data
{
    public interface IRouteRepository
    {
        List<Route> GetAllRoutes();
        Route GetRouteById(int id);
        List<Route> GetRoutesByDate(DateTime date);
        List<Route> GetRoutesByDriver(int DriverId);
        List<Route> GetRoutesByBus(int busId);
        int AddRoute(Route route);
        bool UpdateRoute(Route route);
        bool DeleteRoute(int id);
    }
}

