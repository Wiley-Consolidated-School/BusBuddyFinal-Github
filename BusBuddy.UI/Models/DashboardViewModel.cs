using System.Collections.Generic;

namespace BusBuddy.UI.Models
{
    public class DashboardViewModel
    {
        public VehicleData VehicleData { get; set; }
        public RouteData RouteData { get; set; }
        public ActivityData ActivityData { get; set; }
        public List<ChartDataPoint> ChartData { get; set; }
        public object ExtraData1 { get; set; }
        public object ExtraData2 { get; set; }

        public DashboardViewModel() { }

        public DashboardViewModel(
            VehicleData vehicleData,
            RouteData routeData,
            ActivityData activityData,
            List<ChartDataPoint> chartData,
            object extraData1,
            object extraData2)
        {
            VehicleData = vehicleData;
            RouteData = routeData;
            ActivityData = activityData;
            ChartData = chartData;
            ExtraData1 = extraData1;
            ExtraData2 = extraData2;
        }
    }
}
