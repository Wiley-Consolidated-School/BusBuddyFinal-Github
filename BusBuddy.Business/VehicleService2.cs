using System;
using System.Collections.Generic;

namespace BusBuddy.Business
{
    // Test vehicle service implementation for the tests
    public class VehicleService2
    {
        private readonly BusBuddy.Data.IVehicleRepository _repository;

        public VehicleService2(BusBuddy.Data.IVehicleRepository repository)
        {
            _repository = repository;
        }

        public List<BusBuddy.Models.Vehicle> GetActiveVehicles()
        {
            var allVehicles = _repository.GetAllVehicles();
            return allVehicles.FindAll(v => v.Status == "Active");
        }

        public bool AddVehicle(BusBuddy.Models.Vehicle vehicle)
        {
            if (string.IsNullOrEmpty(vehicle.VehicleNumber) || vehicle.VehicleNumber.Length < 3)
                return false;

            _repository.AddVehicle(vehicle);
            return true;
        }

        public bool UpdateVehicle(BusBuddy.Models.Vehicle vehicle)
        {
            if (string.IsNullOrEmpty(vehicle.VehicleNumber) || vehicle.VehicleNumber.Length < 3)
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
