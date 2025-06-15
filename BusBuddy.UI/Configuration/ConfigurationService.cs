using System;
using System.IO;
using System.Text.Json;
using System.Drawing;

namespace BusBuddy.UI.Configuration
{
    /// <summary>
    /// User preferences for UI customization
    /// </summary>
    public class UserPreferences
    {
        public bool DarkTheme { get; set; } = true;
        public string AccentColor { get; set; } = "#8AB4F8"; // Material Blue
        public float FontScale { get; set; } = 1.0f;
        public bool HighContrast { get; set; } = false;
        public bool ReducedMotion { get; set; } = false;
        public Size DefaultWindowSize { get; set; } = new Size(1400, 900);
        public bool RememberWindowState { get; set; } = true;
        public string LastUsedModule { get; set; } = "";
        public bool ShowInformationDialogs { get; set; } = false; // Disabled by default
    }

    /// <summary>
    /// Service for managing user preferences and configuration
    /// </summary>
    public class ConfigurationService
    {
        private readonly string _configPath;
        private UserPreferences _preferences;

        public ConfigurationService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configDir = Path.Combine(appData, "BusBuddy");
            Directory.CreateDirectory(configDir);
            _configPath = Path.Combine(configDir, "preferences.json");

            LoadPreferences();
        }

        public UserPreferences Preferences => _preferences;

        public void LoadPreferences()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    _preferences = JsonSerializer.Deserialize<UserPreferences>(json) ?? new UserPreferences();
                }
                else
                {
                    _preferences = new UserPreferences();
                }
            }
            catch (Exception)
            {
                // If loading fails, use defaults
                _preferences = new UserPreferences();
            }
        }

        public void SavePreferences()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(_preferences, options);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception)
            {
                // Silently fail if we can't save preferences
                // The application should continue working with defaults
            }
        }

        public void UpdatePreference<T>(string key, T value)
        {
            var property = typeof(UserPreferences).GetProperty(key);
            if (property != null && property.CanWrite)
            {
                property.SetValue(_preferences, value);
                SavePreferences();
            }
        }
    }
}
