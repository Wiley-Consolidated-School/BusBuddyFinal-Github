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
            // Register repositories
            RegisterSingleton<IVehicleRepository>(() => new VehicleRepository());
            RegisterSingleton<IDriverRepository>(() => new DriverRepository());
            RegisterSingleton<IRouteRepository>(() => new RouteRepository());
            RegisterSingleton<IFuelRepository>(() => new FuelRepository());
            RegisterSingleton<IMaintenanceRepository>(() => new MaintenanceRepository());
            RegisterSingleton<IActivityRepository>(() => new ActivityRepository());
            RegisterSingleton<IActivityScheduleRepository>(() => new ActivityScheduleRepository());
            RegisterSingleton<ISchoolCalendarRepository>(() => new SchoolCalendarRepository());            // Register services
            RegisterSingleton<IVehicleService>(() => (IVehicleService)new VehicleService(GetService<IVehicleRepository>()));
            RegisterSingleton<IDatabaseHelperService>(() => (IDatabaseHelperService)new BusBuddy.Business.DatabaseHelperService());

            // Register form factory
            RegisterSingleton<IFormFactory>(() => this);

            // Register navigation service
            RegisterSingleton<INavigationService>(() => new NavigationService(GetService<IFormFactory>()));
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
    }
}
