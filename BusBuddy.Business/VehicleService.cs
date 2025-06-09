using System;
using System.Collections.Generic;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.Business
{
    public class VehicleService
    {
        private readonly IVehicleRepository _repository;

        public VehicleService(IVehicleRepository repository)
        {
            _repository = repository;
        }

        public bool IsValidVehicleNumber(string? vehicleNumber)
        {
            return !string.IsNullOrEmpty(vehicleNumber) && vehicleNumber.Length >= 3;
        }

        public int CalculateVehicleAge(int vehicleYear)
        {
            return DateTime.Now.Year - vehicleYear;
        }

        public List<Vehicle> GetActiveVehicles()
        {
            var allVehicles = _repository.GetAllVehicles();
            return allVehicles.FindAll(v => v.Status == "Active");
        }

        public bool AddVehicle(Vehicle vehicle)
        {
            if (!IsValidVehicleNumber(vehicle.VehicleNumber))
                return false;

            _repository.AddVehicle(vehicle);
            return true;
        }

        public bool UpdateVehicle(Vehicle vehicle)
        {
            if (!IsValidVehicleNumber(vehicle.VehicleNumber))
                return false;

            return _repository.UpdateVehicle(vehicle);
        }

        public bool DeleteVehicle(int id)
        {
            if (id <= 0)
                return false;

            return _repository.DeleteVehicle(id);
        }
    }
}
