# Syncfusion License Registration Guidelines

## CORRECT Pattern ✅

Register the Syncfusion license directly in Program.cs before Application.Run() as shown below:

```csharp
[STAThread]
static void Main()
{
    // Register Syncfusion license directly as per Syncfusion documentation
    // https://help.syncfusion.com/common/essential-studio/licensing/how-to-register-in-an-application
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
    
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new Form1());
}
```

## INCORRECT Patterns ❌

The following patterns are explicitly forbidden:

### ❌ DO NOT create a license manager class:

```csharp
// DO NOT CREATE THIS TYPE OF CLASS
public static class SyncfusionLicenseManager
{
    public static void RegisterLicense()
    {
        // This is an anti-pattern
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("LICENSE_KEY");
    }
}
```

### ❌ DO NOT create a helper class:

```csharp
// DO NOT CREATE THIS TYPE OF CLASS
public static class LicenseHelper
{
    public static void SetupLicense()
    {
        // This is an anti-pattern
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("LICENSE_KEY");
    }
}
```

### ❌ DO NOT use a service for license registration:

```csharp
// DO NOT CREATE THIS TYPE OF CLASS
public class LicenseService : ILicenseService
{
    public void Initialize()
    {
        // This is an anti-pattern
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("LICENSE_KEY");
    }
}
```

## Why This Matters

1. **Syncfusion Documentation**: The official documentation recommends registering the license directly in the Program.cs file
2. **Simplicity**: Direct registration is simpler and less error-prone
3. **Troubleshooting**: When license issues occur, it's easier to diagnose with centralized registration
4. **Testing**: Helper classes can complicate test environments and create threading/UI issues

## Handling Test Failures

If you encounter license validation errors during testing such as:
- "Value Close() cannot be called while doing CreateHandle()"
- "Object is currently in use elsewhere"

DO NOT create a license manager to fix this. Instead:

1. Ensure license is registered early in test initialization
2. Use environment variables to detect test mode
3. Mock UI controls in tests instead of creating real instances
4. Create test-specific fixtures that initialize once before all tests

## Official References

- [Syncfusion License Registration Documentation](https://help.syncfusion.com/common/essential-studio/licensing/how-to-register-in-an-application)
