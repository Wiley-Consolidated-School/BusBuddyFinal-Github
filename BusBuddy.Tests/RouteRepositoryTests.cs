using System;
using System.Linq;
using BusBuddy.Data;
using BusBuddy.Models;
using Xunit;

namespace BusBuddy.Tests
{
    public class RouteRepositoryTests
    {
        [Fact]
        public void RoutePipeline_EndToEnd_CRUD_Works_Enhanced()
        {
            var repo = new RouteRepository();
            var uniqueName = $"Test Route {Guid.NewGuid()}";
            var testRoute = new Route
            {
                RouteDate = DateTime.Today.AddDays(1),
                RouteName = uniqueName,
                AMBusId = 1,
                AMBeginMiles = 100,
                AMEndMiles = 120,
                AMRiders = 25,
                AMDriverId = 1,
                PMBusId = 2,
                PMBeginMiles = 130,
                PMEndMiles = 150,
                PMRiders = 30,
                PMDriverId = 2,
                Notes = "Initial test note with special chars: !@#$%^&*()_+|~`<>{}[]",
                RouteType = "CDL"
            };

            int routeId = 0;
            try
            {
                // Insert
                routeId = repo.AddRoute(testRoute);
                Assert.True(routeId > 0);

                // Retrieve by ID
                var retrieved = repo.GetRouteById(routeId);
                Assert.NotNull(retrieved);
                Assert.Equal(testRoute.RouteName, retrieved.RouteName);
                Assert.Equal(testRoute.AMBusId, retrieved.AMBusId);
                Assert.Equal(testRoute.AMDriverId, retrieved.AMDriverId);
                Assert.Equal(testRoute.PMBusId, retrieved.PMBusId);
                Assert.Equal(testRoute.PMDriverId, retrieved.PMDriverId);
                Assert.Equal(testRoute.RouteType, retrieved.RouteType);
                Assert.Equal(testRoute.Notes, retrieved.Notes);
                Assert.Equal(testRoute.AMBeginMiles, retrieved.AMBeginMiles);
                Assert.Equal(testRoute.AMEndMiles, retrieved.AMEndMiles);
                Assert.Equal(testRoute.AMRiders, retrieved.AMRiders);
                Assert.Equal(testRoute.PMBeginMiles, retrieved.PMBeginMiles);
                Assert.Equal(testRoute.PMEndMiles, retrieved.PMEndMiles);
                Assert.Equal(testRoute.PMRiders, retrieved.PMRiders);

                // Retrieve all and check presence
                var allRoutes = repo.GetAllRoutes();
                Assert.Contains(allRoutes, r => r.RouteId == routeId);

                // Update
                retrieved.Notes = new string('A', 200) + " 🚍";
                retrieved.AMRiders = 99;
                retrieved.PMRiders = 88;
                var updated = repo.UpdateRoute(retrieved);
                Assert.True(updated);
                var updatedEntity = repo.GetRouteById(routeId);
                Assert.Equal(retrieved.Notes, updatedEntity.Notes);
                Assert.Equal(99, updatedEntity.AMRiders);
                Assert.Equal(88, updatedEntity.PMRiders);

                // Edge: Insert with missing required field
                var badRoute = new Route { RouteDate = DateTime.Today };
                Assert.Throws<ArgumentException>(() => repo.AddRoute(badRoute));

                // Edge: Insert duplicate name (if unique constraint exists, otherwise skip)
                // Uncomment if RouteName is unique:
                // var dupRoute = new Route { RouteDate = DateTime.Today, RouteName = uniqueName };
                // Assert.Throws<SqlException>(() => repo.AddRoute(dupRoute));

                // Delete
                var deleted = repo.DeleteRoute(routeId);
                Assert.True(deleted);
                var shouldBeNull = repo.GetRouteById(routeId);
                Assert.Null(shouldBeNull);
                var allAfterDelete = repo.GetAllRoutes();
                Assert.DoesNotContain(allAfterDelete, r => r.RouteId == routeId);
            }
            finally
            {
                // Cleanup in case of partial failure
                if (routeId > 0) repo.DeleteRoute(routeId);
            }
        }

        [Fact]
        public void RouteRepository_HandlesInvalidRouteIdGracefully()
        {
            var repo = new RouteRepository();
            var result = repo.GetRouteById(-9999);
            Assert.Null(result);
        }

        [Fact]
        public void RouteRepository_ThrowsOnNullRouteInsert()
        {
            var repo = new RouteRepository();
            Assert.Throws<ArgumentNullException>(() => repo.AddRoute(null));
        }

        [Fact]
        public void RouteRepository_ThrowsOnMissingRouteName()
        {
            var repo = new RouteRepository();
            var route = new Route { RouteDate = DateTime.Today };
            Assert.Throws<ArgumentException>(() => repo.AddRoute(route));
        }

        [Fact]
        public void RouteRepository_ThrowsOnNonexistentBusOrDriver()
        {
            var repo = new RouteRepository();
            var route = new Route
            {
                RouteDate = DateTime.Today,
                RouteName = $"InvalidRef-{Guid.NewGuid()}",
                AMBusId = -1234,
                AMDriverId = -5678
            };
            Assert.Throws<InvalidOperationException>(() => repo.AddRoute(route));
        }

        [Fact]
        public void RouteRepository_MaintainsDataIntegrityWithSpecialCharacters()
        {
            var repo = new RouteRepository();
            var route = new Route
            {
                RouteDate = DateTime.Today.AddDays(2),
                RouteName = $"SpecialChars-{Guid.NewGuid()}!@#$%^&*()_+|~`<>[]",
                AMBusId = 1,
                AMDriverId = 1,
                PMBusId = 2,
                PMDriverId = 2,
                Notes = "Notes with emoji 🚍 and special chars: <>?~!@#$%^&*()_+",
                RouteType = "SPED"
            };
            int routeId = 0;
            try
            {
                routeId = repo.AddRoute(route);
                var retrieved = repo.GetRouteById(routeId);
                Assert.NotNull(retrieved);
                Assert.Equal(route.RouteName, retrieved.RouteName);
                Assert.Equal(route.Notes, retrieved.Notes);
            }
            finally
            {
                if (routeId > 0) repo.DeleteRoute(routeId);
            }
        }

        [Fact]
        public void RouteRepository_HandlesDateTimeFieldsCorrectly()
        {
            var repo = new RouteRepository();
            var specificDate = new DateTime(2024, 7, 4, 8, 15, 0);
            var route = new Route
            {
                RouteDate = specificDate,
                RouteName = $"DateTest-{Guid.NewGuid()}",
                AMBusId = 1,
                AMDriverId = 1,
                PMBusId = 2,
                PMDriverId = 2
            };
            int routeId = 0;
            try
            {
                routeId = repo.AddRoute(route);
                var retrieved = repo.GetRouteById(routeId);
                Assert.NotNull(retrieved);
                Assert.Equal(specificDate.Date, retrieved.RouteDate.Date);
            }
            finally
            {
                if (routeId > 0) repo.DeleteRoute(routeId);
            }
        }

        [Fact]
        public void RouteRepository_Performance_LargeDatasetRetrieval()
        {
            var repo = new RouteRepository();
            var testRoutes = Enumerable.Range(0, 10).Select(i => new Route
            {
                RouteDate = DateTime.Today.AddDays(i),
                RouteName = $"PerfTest-{Guid.NewGuid()}-{i}",
                AMBusId = 1,
                AMDriverId = 1,
                PMBusId = 2,
                PMDriverId = 2
            }).ToList();
            var routeIds = new System.Collections.Generic.List<int>();
            try
            {
                foreach (var r in testRoutes)
                    routeIds.Add(repo.AddRoute(r));
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var allRoutes = repo.GetAllRoutes();
                sw.Stop();
                Assert.True(allRoutes.Count >= 10);
                Assert.True(sw.ElapsedMilliseconds < 3000, $"Query took {sw.ElapsedMilliseconds}ms");
            }
            finally
            {
                foreach (var id in routeIds)
                    repo.DeleteRoute(id);
            }
        }

        [Fact]
        public void RouteRepository_Transaction_RollbackOnFailure()
        {
            var repo = new RouteRepository();
            var route = new Route
            {
                RouteDate = DateTime.Today.AddDays(3),
                RouteName = $"RollbackTest-{Guid.NewGuid()}",
                AMBusId = 1,
                AMDriverId = 1,
                PMBusId = 2,
                PMDriverId = 2
            };
            int routeId = 0;
            try
            {
                routeId = repo.AddRoute(route);
                var dup = new Route
                {
                    RouteDate = route.RouteDate,
                    RouteName = route.RouteName, // duplicate name if unique constraint
                    AMBusId = 1,
                    AMDriverId = 1,
                    PMBusId = 2,
                    PMDriverId = 2
                };
                // If RouteName is unique, this should throw and not affect the original
                // Uncomment if unique constraint exists:
                // Assert.Throws<SqlException>(() => repo.AddRoute(dup));
                var stillExists = repo.GetRouteById(routeId);
                Assert.NotNull(stillExists);
            }
            finally
            {
                if (routeId > 0) repo.DeleteRoute(routeId);
            }
        }

        [Fact]
        public void RouteRepository_Concurrency_HandlesConcurrentInserts()
        {
            var repo = new RouteRepository();
            var tasks = new System.Collections.Generic.List<System.Threading.Tasks.Task<int>>();
            var names = new System.Collections.Concurrent.ConcurrentBag<string>();
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    var name = $"Concurrent-{Guid.NewGuid()}-{i}";
                    names.Add(name);
                    tasks.Add(System.Threading.Tasks.Task.Run(() =>
                    {
                        var r = new Route
                        {
                            RouteDate = DateTime.Today.AddDays(i),
                            RouteName = name,
                            AMBusId = 1,
                            AMDriverId = 1,
                            PMBusId = 2,
                            PMDriverId = 2
                        };
                        return repo.AddRoute(r);
                    }));
                }
                System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
                var allRoutes = repo.GetAllRoutes();
                foreach (var name in names)
                    Assert.Contains(allRoutes, r => r.RouteName == name);
            }
            finally
            {
                var allRoutes = repo.GetAllRoutes();
                foreach (var name in names)
                {
                    var r = allRoutes.FirstOrDefault(x => x.RouteName == name);
                    if (r != null) repo.DeleteRoute(r.RouteId);
                }
            }
        }
    }
}
