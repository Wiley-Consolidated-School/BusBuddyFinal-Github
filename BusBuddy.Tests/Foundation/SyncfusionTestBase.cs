using System;
using Xunit;
using FluentAssertions;
using BusBuddy.UI.Views;
using BusBuddy.UI.Services;
using BusBuddy.Business;
using BusBuddy.UI.Helpers;
using Moq;

namespace BusBuddy.Tests.Foundation
{
    /// <summary>
    /// Base test class for Syncfusion Windows Forms testing
    /// Based on official Syncfusion testing documentation patterns
    /// Reference: https://help.syncfusion.com/windowsforms/testing/coded-ui
    /// </summary>
    public abstract class SyncfusionTestBase : IDisposable
    {
        protected Mock<INavigationService> MockNavigationService { get; private set; } = null!;
        protected Mock<BusBuddy.Business.IDatabaseHelperService> MockDatabaseService { get; private set; } = null!;
        protected Mock<IRouteAnalyticsService> MockRouteAnalyticsService { get; private set; } = null!;
        protected Mock<IReportService> MockReportService { get; private set; } = null!;
        protected Mock<BusBuddy.UI.Services.IAnalyticsService> MockAnalyticsService { get; private set; } = null!;
        protected Mock<IErrorHandlerService> MockErrorHandlerService { get; private set; } = null!;

        protected SyncfusionTestBase()
        {
            InitializeMocks();
        }

        /// <summary>
        /// Initialize all service mocks with default behaviors
        /// Following Syncfusion testing patterns for dependency isolation
        /// </summary>
        private void InitializeMocks()
        {
            MockNavigationService = new Mock<INavigationService>();
            MockDatabaseService = new Mock<BusBuddy.Business.IDatabaseHelperService>();
            MockRouteAnalyticsService = new Mock<IRouteAnalyticsService>();
            MockReportService = new Mock<IReportService>();
            MockAnalyticsService = new Mock<BusBuddy.UI.Services.IAnalyticsService>();
            MockErrorHandlerService = new Mock<IErrorHandlerService>();

            // Setup default mock behaviors
            SetupDefaultMockBehaviors();
        }

        /// <summary>
        /// Configure default mock behaviors for stable testing
        /// Based on Syncfusion testing recommendations for control isolation
        /// </summary>
        private void SetupDefaultMockBehaviors()
        {
            // Navigation service defaults
            MockNavigationService.Setup(x => x.IsModuleAvailable(It.IsAny<string>())).Returns(true);
            MockNavigationService.Setup(x => x.Navigate(It.IsAny<string>(), It.IsAny<object[]>())).Returns(true);

            // Error handler defaults - simplified to avoid interface issues
            MockErrorHandlerService.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<string>()));
        }

        /// <summary>
        /// Create a Dashboard instance for testing
        /// Following Syncfusion testing pattern for form instantiation
        /// </summary>
        protected BusBuddy.UI.Views.Dashboard CreateDashboard()
        {
            return new BusBuddy.UI.Views.Dashboard();
        }

        /// <summary>
        /// Helper method to safely dispose of test forms
        /// Prevents memory leaks in test execution
        /// </summary>
        protected void SafeDisposeForm(IDisposable form)
        {
            try
            {
                form?.Dispose();
            }
            catch (Exception)
            {
                // Ignore disposal errors in tests
            }
        }

        public virtual void Dispose()
        {
            // Base disposal logic
            GC.SuppressFinalize(this);
        }
    }
}
