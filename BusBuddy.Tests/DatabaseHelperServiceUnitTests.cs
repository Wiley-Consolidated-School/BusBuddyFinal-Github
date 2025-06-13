using Xunit;
using BusBuddy.Business;
using Moq;
using BusBuddy.Data;

public class DatabaseHelperServiceUnitTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_InitializesSuccessfully()
    {
        var mockVehicleRepo = new Mock<IVehicleRepository>();
        var mockDriverRepo = new Mock<IDriverRepository>();
        var mockRouteRepo = new Mock<IRouteRepository>();
        var mockActivityRepo = new Mock<IActivityRepository>();
        var mockFuelRepo = new Mock<IFuelRepository>();
        var mockMaintenanceRepo = new Mock<IMaintenanceRepository>();
        var mockSchoolCalendarRepo = new Mock<ISchoolCalendarRepository>();
        var mockActivityScheduleRepo = new Mock<IActivityScheduleRepository>();
        var mockTimeCardRepo = new Mock<ITimeCardRepository>();
        var service = new DatabaseHelperService();
        Assert.NotNull(service);
    }

    // Add more focused, independent tests for DatabaseHelperService here...
}
