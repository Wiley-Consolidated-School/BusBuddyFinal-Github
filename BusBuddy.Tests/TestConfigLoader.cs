using System;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using Xunit;

[CollectionDefinition("Config collection", DisableParallelization = true)]
public class ConfigCollection : ICollectionFixture<TestConfigLoader> { }

public class TestConfigLoader : IDisposable
{
    static bool _initialized = false;

    public TestConfigLoader()
    {
        if (_initialized) return;
        _initialized = true;

        // Use the output directory to find the config file reliably
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BusBuddy.Tests.dll.config");
        if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            throw new FileNotFoundException($"Test config file not found: {configPath ?? "(null)"}");

        var doc = XDocument.Load(configPath);
        var conn = doc.Root
            ?.Element("connectionStrings")
            ?.Element("add");

        if (conn == null)
            throw new InvalidOperationException("No <add> element found in <connectionStrings> in test config file.");

        var name = conn.Attribute("name")?.Value;
        var connStr = conn.Attribute("connectionString")?.Value;
        var provider = conn.Attribute("providerName")?.Value;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(connStr) || string.IsNullOrEmpty(provider))
            throw new InvalidOperationException("Test config connection string is missing required attributes.");

        // Note: We don't validate against ConfigurationManager here because xUnit test runners
        // may not load App.config consistently. We've already validated the config file exists
        // and has the required connection string attributes.
    }

    public void Dispose() { }
}
