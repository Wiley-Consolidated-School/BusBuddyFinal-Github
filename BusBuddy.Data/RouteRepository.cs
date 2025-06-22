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

                // Convert the date to string in yyyy-MM-dd format for comparison
                var dateString = date.Date.ToString("yyyy-MM-dd");

                // Compare only the date part (first 10 chars) of the NVARCHAR column
                var routes = connection.Query<Route>(
                    "SELECT * FROM Routes WHERE LEFT(Date, 10) = @DateString",
                    new { DateString = dateString }).AsList();
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
        }        public int AddRoute(Route route)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Validate foreign key constraints
                        ValidateRouteReferences(connection, route, transaction);                        var sql = @"
                            INSERT INTO Routes (
                                Date, RouteName,
                                AMVehicleID, AMBeginMiles, AMEndMiles, AMRiders, AMDriverID,
                                PMVehicleID, PMBeginMiles, PMEndMiles, PMRiders, PMDriverID,
                                Notes, RouteType
                            )
                            VALUES (
                                @Date, @RouteName,
                                @AMVehicleID, @AMBeginMiles, @AMEndMiles, @AMRiders, @AMDriverID,
                                @PMVehicleID, @PMBeginMiles, @PMEndMiles, @PMRiders, @PMDriverID,
                                @Notes, @RouteType
                            );
                            SELECT SCOPE_IDENTITY()";

                        var routeId = connection.QuerySingle<int>(sql, route, transaction);
                        transaction.Commit();
                        return routeId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }        public bool UpdateRoute(Route route)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Validate foreign key constraints
                        ValidateRouteReferences(connection, route, transaction);                        var sql = @"
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
                                PMDriverID = @PMDriverID,
                                Notes = @Notes,
                                RouteType = @RouteType
                            WHERE RouteID = @RouteID";

                        var rowsAffected = connection.Execute(sql, route, transaction);
                        transaction.Commit();
                        return rowsAffected > 0;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }        public bool DeleteRoute(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var sql = "DELETE FROM Routes WHERE RouteID = @RouteID";
                        var rowsAffected = connection.Execute(sql, new { RouteID = id }, transaction);

                        transaction.Commit();
                        return rowsAffected > 0;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void ValidateRouteReferences(IDbConnection connection, Route route, IDbTransaction transaction)
        {
            // Validate AM Vehicle
            if (route.AMVehicleID.HasValue)
            {
                var vehicleExists = connection.QuerySingleOrDefault<int>(
                    "SELECT COUNT(*) FROM Vehicles WHERE Id = @VehicleID",
                    new { VehicleID = route.AMVehicleID }, transaction);

                if (vehicleExists == 0)
                {
                    throw new InvalidOperationException($"AM Vehicle with ID {route.AMVehicleID} does not exist.");
                }
            }

            // Validate PM Vehicle
            if (route.PMVehicleID.HasValue)
            {
                var vehicleExists = connection.QuerySingleOrDefault<int>(
                    "SELECT COUNT(*) FROM Vehicles WHERE Id = @VehicleID",
                    new { VehicleID = route.PMVehicleID }, transaction);

                if (vehicleExists == 0)
                {
                    throw new InvalidOperationException($"PM Vehicle with ID {route.PMVehicleID} does not exist.");
                }
            }

            // Validate AM Driver
            if (route.AMDriverID.HasValue)
            {
                var driverExists = connection.QuerySingleOrDefault<int>(
                    "SELECT COUNT(*) FROM Drivers WHERE DriverID = @DriverID",
                    new { DriverID = route.AMDriverID }, transaction);

                if (driverExists == 0)
                {
                    throw new InvalidOperationException($"AM Driver with ID {route.AMDriverID} does not exist.");
                }
            }

            // Validate PM Driver
            if (route.PMDriverID.HasValue)
            {
                var driverExists = connection.QuerySingleOrDefault<int>(
                    "SELECT COUNT(*) FROM Drivers WHERE DriverID = @DriverID",
                    new { DriverID = route.PMDriverID }, transaction);

                if (driverExists == 0)
                {
                    throw new InvalidOperationException($"PM Driver with ID {route.PMDriverID} does not exist.");
                }
            }
        }
    }
}
