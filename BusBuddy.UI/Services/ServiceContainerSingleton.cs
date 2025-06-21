using System;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Singleton accessor for ServiceContainer to ensure only one instance exists
    /// </summary>
    public static class ServiceContainerSingleton
    {
        private static ServiceContainer _instance;
        private static readonly object _lock = new object();
        private static bool _initialized = false;

        /// <summary>
        /// Gets the singleton instance of ServiceContainer
        /// </summary>
        public static ServiceContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            Console.WriteLine("🏭 Creating ServiceContainer singleton instance");
                            _instance = new ServiceContainer();
                            _initialized = true;
                            Console.WriteLine("✅ ServiceContainer singleton initialized");
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Checks if the ServiceContainer singleton is initialized
        /// </summary>
        public static bool IsInitialized => _initialized;
          /// <summary>
        /// Force initialization of the ServiceContainer singleton
        /// </summary>
        public static void Initialize()
        {
            var instance = Instance;

            // Validate key repositories
            try
            {
                var vehicleRepo = instance.GetService<BusBuddy.Data.IVehicleRepository>();
                if (vehicleRepo != null)
                {
                    var count = vehicleRepo.GetAllVehicles()?.Count ?? 0;
                    Console.WriteLine($"✅ VehicleRepository verified: {count} vehicles found");
                }
                else
                {
                    Console.WriteLine("❌ VehicleRepository is null after initialization");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ VehicleRepository validation warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures a repository is initialized by retrieving it from the singleton instance
        /// Use this before accessing any repository through a form or service
        /// </summary>
        /// <typeparam name="T">The repository interface type</typeparam>
        /// <returns>A repository instance from the singleton container</returns>
        public static T EnsureRepository<T>() where T : class
        {
            if (!IsInitialized)
            {
                Console.WriteLine($"⚠️ ServiceContainerSingleton not initialized when requesting {typeof(T).Name}, initializing now");
                Initialize();
            }

            var repository = Instance.GetService<T>();
            if (repository == null)
            {
                Console.WriteLine($"❌ Failed to resolve {typeof(T).Name} from ServiceContainerSingleton");
                throw new InvalidOperationException($"Failed to resolve {typeof(T).Name} from ServiceContainerSingleton");
            }

            Console.WriteLine($"✅ Successfully resolved {typeof(T).Name} from ServiceContainerSingleton");
            return repository;
        }
    }
}
