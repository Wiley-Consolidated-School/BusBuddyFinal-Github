using System;
using Xunit;

namespace BusBuddy.Tests.UI
{
    /// <summary>
    /// Test attribute that skips UI tests when running in headless environments
    /// Based on Syncfusion testing best practices for Windows Forms applications
    /// </summary>
    public sealed class UITestFactAttribute : FactAttribute
    {
        public UITestFactAttribute()
        {
            if (HeadlessTestDetector.IsHeadlessEnvironment())
            {
                Skip = HeadlessTestDetector.GetSkipReason();
            }
        }
    }

    /// <summary>
    /// Theory attribute that skips UI tests when running in headless environments
    /// </summary>
    public sealed class UITestTheoryAttribute : TheoryAttribute
    {
        public UITestTheoryAttribute()
        {
            if (HeadlessTestDetector.IsHeadlessEnvironment())
            {
                Skip = HeadlessTestDetector.GetSkipReason();
            }
        }
    }
}
