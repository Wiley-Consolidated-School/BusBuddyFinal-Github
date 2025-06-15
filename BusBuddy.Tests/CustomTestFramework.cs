using System;
using System.IO;
using Xunit;

[assembly: Xunit.TestFramework("BusBuddy.Tests.CustomTestFramework", "BusBuddy.Tests")]

namespace BusBuddy.Tests
{
    /// <summary>
    /// Custom test framework that ensures configuration is set up before any tests run
    /// </summary>
    public class CustomTestFramework : Xunit.Sdk.XunitTestFramework
    {
        public CustomTestFramework(Xunit.Abstractions.IMessageSink messageSink)
            : base(messageSink)
        {
            SetupTestConfiguration();
        }

        private void SetupTestConfiguration()
        {
            try
            {
                // Get the test assembly directory
                var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

                if (assemblyDirectory == null)
                    return;

                var appConfigPath = Path.Combine(assemblyDirectory, "App.config");
                var testhostConfigPath = Path.Combine(assemblyDirectory, "testhost.dll.config");

                // Copy App.config to testhost.dll.config if it exists and target doesn't exist or is older
                if (File.Exists(appConfigPath))
                {
                    if (!File.Exists(testhostConfigPath) ||
                        File.GetLastWriteTime(appConfigPath) > File.GetLastWriteTime(testhostConfigPath))
                    {
                        File.Copy(appConfigPath, testhostConfigPath, true);
                        Console.WriteLine($"Copied {appConfigPath} to {testhostConfigPath}");
                    }
                }

                // Also copy to testhost.x86.dll.config for 32-bit test hosts
                var testhostX86ConfigPath = Path.Combine(assemblyDirectory, "testhost.x86.dll.config");
                if (File.Exists(appConfigPath))
                {
                    if (!File.Exists(testhostX86ConfigPath) ||
                        File.GetLastWriteTime(appConfigPath) > File.GetLastWriteTime(testhostX86ConfigPath))
                    {
                        File.Copy(appConfigPath, testhostX86ConfigPath, true);
                        Console.WriteLine($"Copied {appConfigPath} to {testhostX86ConfigPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to setup test configuration: {ex.Message}");
            }
        }
    }
}
