using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.Business;
using BusBuddy.UI.Views;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Simple service container for dependency injection in WinForms
    /// </summary>
    public class ServiceContainer : IServiceProvider, IFormFactory
    {
        private readonly Dictionary<Type, object> _services = new();
        private readonly Dictionary<Type, Func<object>> _factories = new();

        public ServiceContainer()
        {
            RegisterDefaultServices();
        }

        private void RegisterDefaultServices()
        {
            try
            {
                // Register repositories with error handling
                RegisterSingleton<IVehicleRepository>(() => {
                    try
                    {
                        var repo = new VehicleRepository();
                        Console.WriteLine("✅ VehicleRepository created successfully");
                        return repo;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to create VehicleRepository: {ex.Message}");
                        throw new InvalidOperationException($"Failed to initialize VehicleRepository: {ex.Message}", ex);
                    }
                });

                RegisterSingleton<IDriverRepository>(() => {
                    try
                    {
                        var repo = new DriverRepository();
                        Console.WriteLine("✅ DriverRepository created successfully");
                        return repo;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to create DriverRepository: {ex.Message}");
                        throw new InvalidOperationException($"Failed to initialize DriverRepository: {ex.Message}", ex);
                    }
                });

                RegisterSingleton<IRouteRepository>(() => new RouteRepository());
                RegisterSingleton<IFuelRepository>(() => new FuelRepository());
                RegisterSingleton<IMaintenanceRepository>(() => new MaintenanceRepository());
                RegisterSingleton<IActivityRepository>(() => new ActivityRepository());
                RegisterSingleton<IActivityScheduleRepository>(() => new ActivityScheduleRepository());
                RegisterSingleton<ISchoolCalendarRepository>(() => new SchoolCalendarRepository());

                // Register services
                RegisterSingleton<IVehicleService>(() => (IVehicleService)new VehicleService(GetService<IVehicleRepository>()));
                RegisterSingleton<IDatabaseHelperService>(() => (IDatabaseHelperService)new BusBuddy.Business.DatabaseHelperService());
                RegisterSingleton<IRouteAnalyticsService>(() => new BusBuddy.Business.RouteAnalyticsService(GetService<IRouteRepository>()));

                // Register form factory
                RegisterSingleton<IFormFactory>(() => this);

                // Register navigation service
                RegisterSingleton<INavigationService>(() => new NavigationService(GetService<IFormFactory>()));

                // Task 5: Register report service with xAI Grok 3 API integration
                RegisterSingleton<IReportService>(() => new ReportService(GetService<IDatabaseHelperService>()));

                // Task 6: Register analytics service for driver pay and CDE-40 reporting
                RegisterSingleton<IAnalyticsService>(() => new AnalyticsService(GetService<IDatabaseHelperService>(), GetService<IRouteAnalyticsService>()));

                // Task 7: Register error handler service for centralized error management
                RegisterSingleton<IErrorHandlerService>(() => new ErrorHandlerService());

                Console.WriteLine("✅ All services registered successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to register services: {ex.Message}");
                MessageBox.Show($"Failed to initialize services: {ex.Message}\n\nPlease check database connection.", "Service Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        public void RegisterSingleton<T>(Func<T> factory)
        {
            _factories[typeof(T)] = () => factory();
        }

        public void RegisterTransient<T>(Func<T> factory)
        {
            _factories[typeof(T)] = () => factory();
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public object GetService(Type serviceType)
        {
            // Try to get existing singleton instance
            if (_services.TryGetValue(serviceType, out var existingService))
            {
                return existingService;
            }

            // Try to create new instance using factory
            if (_factories.TryGetValue(serviceType, out var factory))
            {
                var service = factory();

                // Cache singletons
                if (!serviceType.Name.Contains("Transient"))
                {
                    _services[serviceType] = service;
                }

                return service;
            }

            throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
        }

        public T CreateForm<T>() where T : Form
        {
            return CreateForm<T>(Array.Empty<object>());
        }

        public T CreateForm<T>(params object[] parameters) where T : Form
        {
            var formType = typeof(T);
            Console.WriteLine($"🔍 BREADCRUMB: CreateForm<{formType.Name}>() called");

            try
            {
                // Try to find constructor that matches our services
                var constructors = formType.GetConstructors();
                Console.WriteLine($"🔍 BREADCRUMB: Found {constructors.Length} constructors for {formType.Name}");

                foreach (var constructor in constructors)
                {
                    var paramTypes = constructor.GetParameters();
                    Console.WriteLine($"🔍 BREADCRUMB: Trying constructor with {paramTypes.Length} parameters");

                    // Try to resolve all dependencies for this constructor first
                    var dependencies = new object[paramTypes.Length];
                    bool canResolveAll = true;

                    for (int i = 0; i < paramTypes.Length; i++)
                    {
                        var paramType = paramTypes[i].ParameterType;
                        try
                        {
                            dependencies[i] = GetService(paramType);
                            Console.WriteLine($"🔍 BREADCRUMB: Resolved dependency {paramType.Name}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"🔍 BREADCRUMB: Failed to resolve dependency {paramType.Name}: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Failed to resolve dependency {paramType.Name}: {ex.Message}");
                            canResolveAll = false;
                            break;
                        }
                    }

                    if (canResolveAll)
                    {
                        Console.WriteLine($"🔍 BREADCRUMB: Creating {formType.Name} with dependency injection");
                        return (T)Activator.CreateInstance(formType, dependencies)!;
                    }
                }

                // If no dependency injection constructor worked, try parameterless
                var parameterlessConstructor = formType.GetConstructor(Type.EmptyTypes);
                if (parameterlessConstructor != null)
                {
                    Console.WriteLine($"🔍 BREADCRUMB: Creating {formType.Name} with parameterless constructor");
                    return (T)Activator.CreateInstance(formType)!;
                }

                // If all else fails, throw an exception
                throw new InvalidOperationException($"Cannot create form {formType.Name}: no suitable constructor found or dependencies cannot be resolved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔍 BREADCRUMB ERROR: Failed to create form {formType.Name}: {ex.Message}");
                throw new InvalidOperationException($"Failed to create form of type {formType.Name}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Clean up all services and forms for application shutdown
        /// </summary>
        public void Cleanup()
        {
            try
            {
                Console.WriteLine("🧽 ServiceContainer cleanup started...");

                // Clear all service instances
                lock (_services)
                {
                    foreach (var service in _services.Values)
                    {
                        try
                        {
                            if (service is IDisposable disposableService)
                            {
                                disposableService.Dispose();
                                Console.WriteLine($"🧽 Disposed service: {service.GetType().Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error disposing service {service?.GetType().Name}: {ex.Message}");
                        }
                    }
                    _services.Clear();
                }

                // Clear all factories
                lock (_factories)
                {
                    _factories.Clear();
                }

                Console.WriteLine("✅ ServiceContainer cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during ServiceContainer cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Enhanced form creation with disposal tracking
        /// </summary>
        private readonly Dictionary<string, WeakReference> _createdForms = new Dictionary<string, WeakReference>();
        private readonly object _formTrackingLock = new object();

        /// <summary>
        /// Create form with disposal tracking for shutdown cleanup
        /// </summary>
        public T CreateFormWithTracking<T>(params object[] parameters) where T : Form
        {
            var form = CreateForm<T>(parameters);

            // Track the form for cleanup during shutdown
            lock (_formTrackingLock)
            {
                var formKey = $"{typeof(T).Name}_{Guid.NewGuid()}";
                _createdForms[formKey] = new WeakReference(form);

                // Set up cleanup when form is disposed
                form.FormClosed += (sender, e) =>
                {
                    lock (_formTrackingLock)
                    {
                        _createdForms.Remove(formKey);
                    }
                };
            }

            return form;
        }

        /// <summary>
        /// Dispose all tracked forms for application shutdown
        /// </summary>
        public void DisposeAllTrackedForms()
        {
            try
            {
                Console.WriteLine("🧽 Disposing all tracked forms...");

                lock (_formTrackingLock)
                {
                    var formsToDispose = new List<Form>();

                    foreach (var weakRef in _createdForms.Values)
                    {
                        if (weakRef.Target is Form form && !form.IsDisposed)
                        {
                            formsToDispose.Add(form);
                        }
                    }

                    foreach (var form in formsToDispose)
                    {
                        try
                        {
                            Console.WriteLine($"🧽 Disposing tracked form: {form.GetType().Name}");
                            form.Close();
                            form.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error disposing form {form.GetType().Name}: {ex.Message}");
                        }
                    }

                    _createdForms.Clear();
                    Console.WriteLine("✅ All tracked forms disposed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error disposing tracked forms: {ex.Message}");
            }
        }
    }
}
