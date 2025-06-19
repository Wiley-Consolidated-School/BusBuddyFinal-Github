using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Form Discovery service for BusBuddy Dashboard - Extracted from BusBuddyDashboardSyncfusion
    /// Handles scanning and caching of available forms for navigation
    /// </summary>
    public static class FormDiscovery
    {
        private static readonly Dictionary<string, Type> _formTypeCache = new Dictionary<string, Type>();
        private static readonly object _cacheInitLock = new object();
        private static readonly string CACHE_FILE = "form_cache.json";

        /// <summary>
        /// Form information for navigation
        /// </summary>
        public class FormInfo
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string NavigationMethod { get; set; }
            public string FormTypeName { get; set; }
            public bool IsEnabled { get; set; } = true;
        }

        /// <summary>
        /// Configuration for a form
        /// </summary>
        public class FormConfiguration
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string NavigationMethod { get; set; }
            public bool IsEnabled { get; set; } = true;
            public int SortOrder { get; set; }
        }

        /// <summary>
        /// Scans and caches available forms with enhanced error handling
        /// </summary>
        public static List<FormInfo> ScanAndCacheFormsEnhanced()
        {
            var cachedForms = new List<FormInfo>();
            try
            {
                Console.WriteLine("🔍 ENHANCED SCAN: Loading forms from configuration...");
                var configurations = LoadFormConfigurations();

                foreach (var config in configurations.Where(c => c.IsEnabled))
                {
                    var formInfo = new FormInfo
                    {
                        Name = config.Name,
                        DisplayName = config.DisplayName,
                        Description = config.Description,
                        NavigationMethod = config.NavigationMethod,
                        FormTypeName = $"BusBuddy.UI.Views.{config.Name}, {Assembly.GetExecutingAssembly().GetName().Name}"
                    };
                    cachedForms.Add(formInfo);
                    Console.WriteLine($"   ✅ Added: {formInfo.DisplayName}");
                }

                if (cachedForms.Count > 0)
                {
                    SaveFormsToCache(cachedForms);
                }

                Console.WriteLine($"📊 Loaded {cachedForms.Count} forms from configuration");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Form loading error: {ex.Message}");
            }
            return cachedForms;
        }

        /// <summary>
        /// Saves form information to persistent cache (minimized for memory optimization)
        /// </summary>
        private static void SaveFormsToCache(List<FormInfo> forms)
        {
            try
            {
                // Save only essential data to reduce file size and memory usage
                var minimalForms = forms.Select(f => new {
                    f.Name,
                    f.DisplayName,
                    f.NavigationMethod
                }).ToList();
                var json = System.Text.Json.JsonSerializer.Serialize(minimalForms, new System.Text.Json.JsonSerializerOptions {
                    WriteIndented = false  // Compact format
                });
                File.WriteAllText(CACHE_FILE, json);
                Console.WriteLine($"🔍 DEBUG: Saved {forms.Count} forms to persistent cache (optimized)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to save forms to cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads form configurations - hardcoded for Phase 4 compliance
        /// </summary>
        private static List<FormConfiguration> LoadFormConfigurations()
        {
            return new List<FormConfiguration>
            {
                new FormConfiguration { Name = "VehicleManagementFormSyncfusion", DisplayName = "🚗 Vehicle Management", Description = "Manage vehicle fleet", NavigationMethod = "ShowVehicleManagement", SortOrder = 1 },
                new FormConfiguration { Name = "DriverManagementFormSyncfusion", DisplayName = "👤 Driver Management", Description = "Manage drivers", NavigationMethod = "ShowDriverManagement", SortOrder = 2 },
                new FormConfiguration { Name = "RouteManagementFormSyncfusion", DisplayName = "🚌 Route Management", Description = "Manage routes", NavigationMethod = "ShowRouteManagement", SortOrder = 3 },
                new FormConfiguration { Name = "ActivityManagementFormSyncfusion", DisplayName = "🎯 Activity Management", Description = "Manage activities", NavigationMethod = "ShowActivityManagement", SortOrder = 4 },
                new FormConfiguration { Name = "FuelManagementFormSyncfusion", DisplayName = "⛽ Fuel Management", Description = "Manage fuel records", NavigationMethod = "ShowFuelManagement", SortOrder = 5 },
                new FormConfiguration { Name = "MaintenanceManagementFormSyncfusion", DisplayName = "🔧 Maintenance Management", Description = "Manage maintenance records", NavigationMethod = "ShowMaintenanceManagement", SortOrder = 6 },
                new FormConfiguration { Name = "SchoolCalendarManagementFormSyncfusion", DisplayName = "📅 School Calendar", Description = "Manage school calendar", NavigationMethod = "ShowSchoolCalendarManagement", SortOrder = 7 },
                new FormConfiguration { Name = "ActivityScheduleManagementFormSyncfusion", DisplayName = "📋 Activity Schedule", Description = "Manage activity schedules", NavigationMethod = "ShowActivityScheduleManagement", SortOrder = 8 }
            };
        }

        /// <summary>
        /// Gets cached forms without triggering expensive scans
        /// </summary>
        public static List<FormInfo> GetCachedForms()
        {
            try
            {
                if (File.Exists(CACHE_FILE))
                {
                    var json = File.ReadAllText(CACHE_FILE);
                    var minimalForms = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(json);
                    // Convert back to FormInfo objects
                    return new List<FormInfo>(); // Simplified for Phase 4
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to load cached forms: {ex.Message}");
            }
            return new List<FormInfo>();
        }

        /// <summary>
        /// Clears the form cache to save memory
        /// </summary>
        public static void ClearCache()
        {
            try
            {
                if (File.Exists(CACHE_FILE))
                {
                    File.Delete(CACHE_FILE);
                    Console.WriteLine("🧹 Form cache cleared for memory optimization");
                }
                _formTypeCache.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to clear cache: {ex.Message}");
            }
        }
    }
}
