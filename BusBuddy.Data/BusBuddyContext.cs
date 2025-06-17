using System;
using System.Data.Common;
using BusBuddy.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Data
{
    // EF Core context class that can work with both SQL Server and SQLite
    public class BusBuddyContext : DbContext, IDisposable
    {
        private readonly DbConnection? _connection;

        // Parameterless constructor for EF Core
        public BusBuddyContext() : base()
        {
        }

        // Constructor with options (preferred for EF Core)
        public BusBuddyContext(DbContextOptions<BusBuddyContext> options) : base(options)
        {
        }

        // Constructor with connection (for compatibility with existing code)
        public BusBuddyContext(DbConnection connection) : base()
        {
            _connection = connection;
        }

        // DbSet properties for entities
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Driver> Drivers { get; set; } = null!;
        public DbSet<Fuel> Fuels { get; set; } = null!;
        public DbSet<Route> Routes { get; set; } = null!;
        public DbSet<Activity> Activities { get; set; } = null!;
        public DbSet<ActivitySchedule> ActivitySchedules { get; set; } = null!;
        public DbSet<Maintenance> Maintenances { get; set; } = null!;
        public DbSet<SchoolCalendar> SchoolCalendars { get; set; } = null!;
        public DbSet<PTOBalance> PTOBalances { get; set; } = null!;
        public DbSet<TimeCard> TimeCards { get; set; } = null!;        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (_connection != null)
                {
                    // Always use SQL Server
                    var provider = DatabaseConfiguration.DatabaseProvider;
                    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                    {
                        optionsBuilder.UseSqlServer(_connection);
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
                            optionsBuilder.UseSqlServer(connectionString);
                        }
                        else
                        {
                            throw new NotSupportedException($"Database provider '{provider}' is not supported. Only SqlServer is supported.");
                        }
                    }
                    catch
                    {
                        // Fallback to SQL Server Express
                        var defaultConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB;Integrated Security=True;TrustServerCertificate=True;";
                        optionsBuilder.UseSqlServer(defaultConnectionString);
                    }
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to match existing database schema
            modelBuilder.Entity<Fuel>().ToTable("Fuel"); // Use singular table name

            // Configure primary keys for entities that don't follow default naming conventions
            modelBuilder.Entity<Vehicle>().HasKey(v => v.Id);
            modelBuilder.Entity<Driver>().HasKey(d => d.DriverID);
            modelBuilder.Entity<ActivitySchedule>().HasKey(a => a.ScheduleID);
            modelBuilder.Entity<Fuel>().HasKey(f => f.FuelID);
            modelBuilder.Entity<Maintenance>().HasKey(m => m.MaintenanceID);
            modelBuilder.Entity<PTOBalance>().HasKey(p => p.PTOBalanceID);
            modelBuilder.Entity<SchoolCalendar>().HasKey(s => s.CalendarID);
            modelBuilder.Entity<Route>().HasKey(r => r.RouteID);
            modelBuilder.Entity<Activity>().HasKey(a => a.ActivityID);
            modelBuilder.Entity<TimeCard>().HasKey(t => t.TimeCardId);            // Configure Vehicle entity mappings
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Capacity)
                .HasColumnName("SeatingCapacity");

            // Configure concurrency token
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.RowVersion)
                .IsRowVersion();

            // Ignore computed properties that are just wrappers
            modelBuilder.Entity<Vehicle>()
                .Ignore(v => v.SeatingCapacity);

            modelBuilder.Entity<Vehicle>()
                .Ignore(v => v.VehicleID);

            // Map the BusNumber property - it's stored separately but can fall back to VehicleNumber
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.BusNumber)
                .HasColumnName("BusNumber");

            // Configure foreign key relationship for Fuel entity
            modelBuilder.Entity<Fuel>()
                .HasOne(f => f.VehicleFueled)
                .WithMany()
                .HasForeignKey(f => f.VehicleFueledID);

            // Configure TimeCard entity
            modelBuilder.Entity<TimeCard>()
                .ToTable("TimeCards");

            modelBuilder.Entity<TimeCard>()
                .HasOne(tc => tc.Driver)
                .WithMany()
                .HasForeignKey(tc => tc.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimeCard>()
                .Property(tc => tc.TotalTime)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TimeCard>()
                .Property(tc => tc.Overtime)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TimeCard>()
                .Property(tc => tc.PTOHours)
                .HasPrecision(18, 2);

            // Ignore computed DateTime helper properties that shouldn't be mapped to database columns
            modelBuilder.Entity<Activity>()
                .Ignore(a => a.DateAsDateTime)
                .Ignore(a => a.LeaveTimeSpan)
                .Ignore(a => a.EventTimeSpan)
                .Ignore(a => a.ReturnTimeSpan);

            modelBuilder.Entity<ActivitySchedule>()
                .Ignore(a => a.DateAsDateTime);

            modelBuilder.Entity<Route>()
                .Ignore(r => r.DateAsDateTime);

            modelBuilder.Entity<Fuel>()
                .Ignore(f => f.FuelDateAsDateTime);

            modelBuilder.Entity<Maintenance>()
                .Ignore(m => m.DateAsDateTime);

            modelBuilder.Entity<SchoolCalendar>()
                .Ignore(s => s.DateAsDateTime)
                .Ignore(s => s.EndDateAsDateTime);

            modelBuilder.Entity<Vehicle>()
                .Ignore(v => v.DateLastInspectionAsDateTime);

            // Configure Driver computed property
            modelBuilder.Entity<Driver>()
                .Ignore(d => d.Name)
                .Ignore(d => d.IsTrainingComplete);
        }

        public new void Dispose()
        {
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
