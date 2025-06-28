using System;
using Xunit;
using Xunit.Sdk;

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// Attribute for marking Dashboard test classes.
    /// This allows for selecting specific test classes for execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DashboardTestsAttribute : Attribute, ITraitAttribute
    {
        public DashboardTestsAttribute()
        {
        }
    }
}

