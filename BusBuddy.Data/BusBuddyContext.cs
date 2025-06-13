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

        // Legacy constructor for backwards compatibility
        public BusBuddyContext(DbConnection connection) : base()
        {
            _connection = connection;
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        // DbSets for Entity Framework
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Driver> Drivers { get; set; } = null!;
        public DbSet<Fuel> Fuels { get; set; } = null!;
        public DbSet<Route> Routes { get; set; } = null!;
        public DbSet<TimeCard> TimeCards { get; set; } = null!;
        public DbSet<Activity> Activities { get; set; } = null!;
        public DbSet<ActivitySchedule> ActivitySchedules { get; set; } = null!;
        public DbSet<Maintenance> Maintenances { get; set; } = null!;
        public DbSet<SchoolCalendar> SchoolCalendars { get; set; } = null!;
        public DbSet<PTOBalance> PTOBalances { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (_connection != null)
                {
                    // Use the provided connection (legacy support)
                    optionsBuilder.UseSqlite(_connection);
                }
                else
                {
                    // Default to SQL Server for production
                    optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BusBuddy;Trusted_Connection=true;");
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
            modelBuilder.Entity<TimeCard>().HasKey(t => t.TimeCardID);
            modelBuilder.Entity<Activity>().HasKey(a => a.ActivityID);

            // Configure Vehicle entity mappings
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Capacity)
                .HasColumnName("SeatingCapacity");

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
        }        public new void Dispose()
        {
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
