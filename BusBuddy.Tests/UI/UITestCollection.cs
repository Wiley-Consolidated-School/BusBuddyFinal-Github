using Xunit;

namespace BusBuddy.Tests.UI
{
    /// <summary>
    /// Test collection for UI tests that need to run sequentially
    /// because they interact with the Windows Forms UI thread.
    /// </summary>
    [CollectionDefinition("UI Tests", DisableParallelization = true)]
    public class UITestCollection
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}

