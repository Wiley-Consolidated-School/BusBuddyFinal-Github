using System;
using System.Data.Common;
using System.Threading.Tasks;
using BusBuddy.Data; // Add using directive for DatabaseConfiguration
using BusBuddy.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Data
{
    // EF Core context class that can work with both SQL Server and SQLite
    public class BusBuddyContext : DbContext, IDisposable
    {
        private readonly DbConnection? _connection;
        private static bool _hasAttemptedRepair = false;

        // Parameterless constructor for EF Core
        public BusBuddyContext() : base()
        {
        }

        // Constructor with options (preferred for EF Core)
        public BusBuddyContext(DbContextOptions<BusBuddyContext> options) : base(options)
        {
            // Set extended command timeout to prevent OperationCanceledException
            Database.SetCommandTimeout(120); // 2 minutes timeout
        }

        // Constructor with connection (for compatibility with existing code)
        public BusBuddyContext(DbConnection connection) : base()
        {
            _connection = connection;
        }

        // DbSet properties for entities - Updated to match database table names
        public DbSet<Bus> Buses { get; set; } = null!; // Renamed from Vehicles to Buses
        public DbSet<Driver> Drivers { get; set; } = null!;
        public DbSet<Fuel> Fuels { get; set; } = null!;
        public DbSet<Route> Routes { get; set; } = null!;
        public DbSet<Activity> Activities { get; set; } = null!;
        public DbSet<ActivitySchedule> ActivitySchedules { get; set; } = null!;
        public DbSet<Maintenance> Maintenances { get; set; } = null!;
        public DbSet<SchoolCalendar> SchoolCalendars { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (_connection != null)
                {
                    // Always use SQL Server
                    var provider = DatabaseConfiguration.DatabaseProvider;
                    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                    {
                        optionsBuilder.UseSqlServer(_connection, options =>
                        {
                            options.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorNumbersToAdd: null);
                            options.CommandTimeout(120); // Increase timeout to 2 minutes
                        });
                    }
                    else
                    {
                        throw new NotSupportedException($"Database provider '{provider}' is not supported. Only SqlServer is supported.");
                    }
                }
                else
                {
                    try
                    {
                        // Use the new DatabaseConfiguration for automatic environment detection
                        var connectionString = DatabaseConfiguration.GetConnectionString();
                        var provider = DatabaseConfiguration.DatabaseProvider;
                        // Always use SQL Server
                        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                        {
                            optionsBuilder.UseSqlServer(connectionString, options =>
                            {
                                options.EnableRetryOnFailure(
                                    maxRetryCount: 5,
                                    maxRetryDelay: TimeSpan.FromSeconds(10),
                                    errorNumbersToAdd: null);
                                options.CommandTimeout(120); // Increase timeout to 2 minutes
                            });
                        }
                        else
                        {
                            throw new NotSupportedException($"Database provider '{provider}' is not supported. Only SqlServer is supported.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error configuring database: {ex.Message}");
                        // Check if the error indicates a database offline issue
                        bool isDatabaseOffline = ex.Message.Contains("offline") ||
                                               ex.Message.Contains("database is not accessible") ||
                                               ex.Message.Contains("Cannot open database");
                        if (isDatabaseOffline && !_hasAttemptedRepair)
                        {
                            _hasAttemptedRepair = true;
                            Console.WriteLine("Database appears to be offline. Attempting to repair...");
                            try
                            {
                                // Use fallback connection string for recovery attempts
                                var connectionString = DatabaseConfiguration.GetConnectionString();
                                Console.WriteLine($"Retry with connection string after timeout increase: {connectionString}");
                                optionsBuilder.UseSqlServer(connectionString, options =>
                                {
                                    options.EnableRetryOnFailure(
                                        maxRetryCount: 3,
                                        maxRetryDelay: TimeSpan.FromSeconds(5),
                                        errorNumbersToAdd: null);
                                    options.CommandTimeout(120);
                                });
                            }
                            catch (Exception repairEx)
                            {
                                Console.WriteLine($"Database repair failed: {repairEx.Message}");
                                // Fall back to default connection
                                var defaultConnectionString = "Server=localhost\\SQLEXPRESS;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=60;Integrated Security=True;";
                                Console.WriteLine($"Using fallback connection string: {defaultConnectionString}");
                                optionsBuilder.UseSqlServer(defaultConnectionString, options =>
                                {
                                    options.CommandTimeout(120);
                                });
                            }
                        }
                        else
                        {
                            // Fallback to SQL Server Express with local server name and database
                            var defaultConnectionString = "Server=localhost\\SQLEXPRESS;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=60;Integrated Security=True;";
                            Console.WriteLine($"Using fallback connection string: {defaultConnectionString}");
                            optionsBuilder.UseSqlServer(defaultConnectionString, options =>
                            {
                                options.CommandTimeout(120);
                            });
                        }
                    }
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure table names to match existing database schema
            modelBuilder.Entity<Bus>().ToTable("Buses"); // Explicitly map to Buses table
            modelBuilder.Entity<Fuel>().ToTable("Fuel"); // Use singular table name

            // Configure primary keys for entities that don't follow default naming conventions
            modelBuilder.Entity<Bus>().HasKey(b => b.BusId);
            modelBuilder.Entity<Driver>().HasKey(d => d.DriverId);
            modelBuilder.Entity<ActivitySchedule>().HasKey(a => a.ScheduleID);
            modelBuilder.Entity<Fuel>().HasKey(f => f.FuelID);
            modelBuilder.Entity<Maintenance>().HasKey(m => m.MaintenanceID);
            modelBuilder.Entity<SchoolCalendar>().HasKey(s => s.CalendarID);
            modelBuilder.Entity<Route>().HasKey(r => r.RouteId);
            modelBuilder.Entity<Activity>().HasKey(a => a.ActivityID);

            // Configure relationships between entities (new schema)
            modelBuilder.Entity<Route>()
                .HasOne<Bus>()
                .WithMany()
                .HasForeignKey(r => r.AMBusId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Route>()
                .HasOne<Bus>()
                .WithMany()
                .HasForeignKey(r => r.PMBusId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Route>()
                .HasOne<Driver>()
                .WithMany()
                .HasForeignKey(r => r.AMDriverId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Route>()
                .HasOne<Driver>()
                .WithMany()
                .HasForeignKey(r => r.PMDriverId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Maintenance>()
                .HasOne<Bus>()
                .WithMany()
                .HasForeignKey(m => m.BusId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Fuel>()
                .HasOne<Bus>()
                .WithMany()
                .HasForeignKey(f => f.VehicleFueledID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ActivitySchedule>()
                .HasOne<Bus>()
                .WithMany()
                .HasForeignKey(a => a.ScheduledVehicleID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ActivitySchedule>()
                .HasOne<Driver>()
                .WithMany()
                .HasForeignKey(a => a.ScheduledDriverID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Activity>()
                .HasOne<Bus>()
                .WithMany()
                .HasForeignKey(a => a.AssignedBusID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Activity>()
                .HasOne<Driver>()
                .WithMany()
                .HasForeignKey(a => a.DriverId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

