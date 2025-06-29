using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Helper class to resolve the service provider ambiguity issues
    /// </summary>
    public static class ServiceProviderHelpers
    {
        /// <summary>
        /// Get service with explicit Microsoft DI implementation
        /// </summary>
        public static T GetService<T>(System.IServiceProvider provider) where T : class
        {
            return Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<T>(provider);
        }

        /// <summary>
        /// Get required service with explicit Microsoft DI implementation
        /// </summary>
        public static T GetRequiredService<T>(System.IServiceProvider provider) where T : notnull
        {
            return Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(provider);
        }
    }
}

