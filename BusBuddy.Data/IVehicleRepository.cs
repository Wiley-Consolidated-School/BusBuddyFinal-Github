using System;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Data
{
    public interface IVehicleRepository
    {
        List<Vehicle> GetAllVehicles();
        Vehicle? GetVehicleById(int id);
        int AddVehicle(Vehicle vehicle);
        bool UpdateVehicle(Vehicle vehicle);
        bool DeleteVehicle(int id);
    }
}
