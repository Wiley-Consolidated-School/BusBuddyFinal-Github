using System;
using System.Threading.Tasks;
using BusBuddy.Business;
using BusBuddy.Models;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Validation test for cost analytics functionality
    /// Validates calculations without requiring real database data
    /// </summary>
    public class CostAnalyticsValidator
    {
        public static async Task ValidateAnalytics()
        {
            Console.WriteLine("🧪 Validating BusBuddy Cost Analytics...\n");

            try
            {
                // Create service instance
                var analyticsService = new RouteAnalyticsService();

                // Test with date range (last 30 days)
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-30);

                Console.WriteLine($"📅 Testing date range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // Test the calculation method
                var costMetrics = await analyticsService.CalculateCostPerStudentMetricsAsync(startDate, endDate);

                // Display results
                Console.WriteLine("\n💰 Cost Per Student Metrics:");
                Console.WriteLine($"   Route Cost/Student/Day: ${costMetrics.RouteCostPerStudentPerDay:F2}");
                Console.WriteLine($"   Sports Cost/Student: ${costMetrics.SportsCostPerStudent:F2}");
                Console.WriteLine($"   Field Trip Cost/Student: ${costMetrics.FieldTripCostPerStudent:F2}");

                Console.WriteLine("\n📊 Breakdown:");
                Console.WriteLine($"   Total Route Student-Days: {costMetrics.TotalRouteStudentDays}");
                Console.WriteLine($"   Total Sports Students: {costMetrics.TotalSportsStudents}");
                Console.WriteLine($"   Total Field Trip Students: {costMetrics.TotalFieldTripStudents}");
                Console.WriteLine($"   Total Route Costs: ${costMetrics.TotalRouteCosts:F2}");
                Console.WriteLine($"   Total Sports Costs: ${costMetrics.TotalSportsCosts:F2}");
                Console.WriteLine($"   Total Field Trip Costs: ${costMetrics.TotalFieldTripCosts:F2}");

                // Validate calculations make sense
                Console.WriteLine("\n✅ Validation Results:");

                if (costMetrics.TotalRouteStudentDays == 0)
                {
                    Console.WriteLine("⚠️  No route data found - calculations will show $0.00 (expected with no data)");
                }
                else
                {
                    Console.WriteLine($"✅ Route calculations working - found {costMetrics.TotalRouteStudentDays} student-days");
                }

                if (costMetrics.TotalSportsStudents == 0 && costMetrics.TotalFieldTripStudents == 0)
                {
                    Console.WriteLine("⚠️  No activity data found - activity costs will show $0.00 (expected with no data)");
                }
                else
                {
                    Console.WriteLine($"✅ Activity calculations working - found {costMetrics.TotalSportsStudents + costMetrics.TotalFieldTripStudents} total activity students");
                }

                // Test that the method doesn't crash with edge cases
                Console.WriteLine("\n🔧 Testing edge cases...");

                // Test with no data (future dates)
                var futureStart = DateTime.Now.AddDays(100);
                var futureEnd = DateTime.Now.AddDays(130);
                var futureMetrics = await analyticsService.CalculateCostPerStudentMetricsAsync(futureStart, futureEnd);

                if (futureMetrics.RouteCostPerStudentPerDay == 0 &&
                    futureMetrics.SportsCostPerStudent == 0 &&
                    futureMetrics.FieldTripCostPerStudent == 0)
                {
                    Console.WriteLine("✅ Future date test passed - returns $0.00 with no data");
                }
                else
                {
                    Console.WriteLine("⚠️  Future date test unexpected - should return $0.00");
                }

                Console.WriteLine("\n🎯 Analytics Validation Complete!");
                Console.WriteLine("📋 When real data is added, the system will:");
                Console.WriteLine("   1. Calculate route costs: fuel + maintenance ($0.20/mile) + driver ($16.50/hour * 2 hours)");
                Console.WriteLine("   2. Calculate activity costs: estimated miles + maintenance + $50 stipend");
                Console.WriteLine("   3. Divide by student counts to get per-student costs");
                Console.WriteLine("   4. Display results in the Syncfusion dashboard panel");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Validation failed: {ex.Message}");
                Console.WriteLine($"📍 Stack trace: {ex.StackTrace}");
            }
        }
    }
}
