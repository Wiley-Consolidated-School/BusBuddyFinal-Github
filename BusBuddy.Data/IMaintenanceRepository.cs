using System;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Data
{
    public interface IMaintenanceRepository
    {
        List<Maintenance> GetAllMaintenanceRecords();
        List<Maintenance> GetAllMaintenances(); // Alias for compatibility
        Maintenance GetMaintenanceById(int id);
        List<Maintenance> GetMaintenanceByDate(DateTime date);
        List<Maintenance> GetMaintenanceByBus(int busId);
        List<Maintenance> GetMaintenanceByType(string maintenanceType);
        int AddMaintenance(Maintenance maintenance);
        bool UpdateMaintenance(Maintenance maintenance);
        bool DeleteMaintenance(int id);
        bool DeleteMaintenanceRecord(int id); // Alias for compatibility
        int Add(Maintenance maintenance);
    }
}

