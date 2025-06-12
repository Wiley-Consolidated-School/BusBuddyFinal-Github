using System;
using System.Collections.Generic;
using System.Data;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{
    public class RouteRepository : BaseRepository, IRouteRepository
    {
        public RouteRepository() : base()
        {
        }

        public List<Route> GetAllRoutes()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var routes = connection.Query<Route>("SELECT * FROM Routes").AsList();
                return routes;
            }
        }

        public Route GetRouteById(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return connection.QuerySingleOrDefault<Route>(
                    "SELECT * FROM Routes WHERE RouteID = @RouteID",
                    new { RouteID = id });
            }
        }

        public List<Route> GetRoutesByDate(DateTime date)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var routes = connection.Query<Route>(
                    "SELECT * FROM Routes WHERE Date = @Date",
                    new { Date = date }).AsList();
                return routes;
            }
        }

        public List<Route> GetRoutesByDriver(int driverId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var routes = connection.Query<Route>(
                    "SELECT * FROM Routes WHERE AMDriverID = @DriverID OR PMDriverID = @DriverID",
                    new { DriverID = driverId }).AsList();
                return routes;
            }
        }

        public List<Route> GetRoutesByVehicle(int vehicleId)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var routes = connection.Query<Route>(
                    "SELECT * FROM Routes WHERE AMVehicleID = @VehicleID OR PMVehicleID = @VehicleID",
                    new { VehicleID = vehicleId }).AsList();
                return routes;
            }
        }

        public int AddRoute(Route route)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    INSERT INTO Routes (
                        Date, RouteName,
                        AMVehicleID, AMBeginMiles, AMEndMiles, AMRiders, AMDriverID,
                        PMVehicleID, PMBeginMiles, PMEndMiles, PMRiders, PMDriverID
                    )
                    VALUES (
                        @Date, @RouteName,
                        @AMVehicleID, @AMBeginMiles, @AMEndMiles, @AMRiders, @AMDriverID,
                        @PMVehicleID, @PMBeginMiles, @PMEndMiles, @PMRiders, @PMDriverID
                    );
                    SELECT last_insert_rowid()";

                return connection.QuerySingle<int>(sql, route);
            }
        }

        public bool UpdateRoute(Route route)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    UPDATE Routes
                    SET Date = @Date,
                        RouteName = @RouteName,
                        AMVehicleID = @AMVehicleID,
                        AMBeginMiles = @AMBeginMiles,
                        AMEndMiles = @AMEndMiles,
                        AMRiders = @AMRiders,
                        AMDriverID = @AMDriverID,
                        PMVehicleID = @PMVehicleID,
                        PMBeginMiles = @PMBeginMiles,
                        PMEndMiles = @PMEndMiles,
                        PMRiders = @PMRiders,
                        PMDriverID = @PMDriverID
                    WHERE RouteID = @RouteID";

                var rowsAffected = connection.Execute(sql, route);
                return rowsAffected > 0;
            }
        }

        public bool DeleteRoute(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = "DELETE FROM Routes WHERE RouteID = @RouteID";
                var rowsAffected = connection.Execute(sql, new { RouteID = id });
                return rowsAffected > 0;
            }
        }
    }
}
