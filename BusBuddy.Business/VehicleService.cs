using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.Business
{
    public class VehicleService : IVehicleService
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
                return false;            return _repository.DeleteVehicle(id);
        }

        // IVehicleService async implementations
        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            return await Task.FromResult(_repository.GetAllVehicles());
        }

        public async Task<bool> IsVehicleDueForInspectionAsync(int vehicleId)
        {
            var vehicle = _repository.GetVehicleById(vehicleId);
            if (vehicle?.LastInspectionDate == null) return true;

            var daysSinceInspection = (DateTime.Now - vehicle.LastInspectionDate.Value).Days;
            return await Task.FromResult(daysSinceInspection > 365); // Due if over 1 year
        }

        public async Task<int> GetTotalMileageForVehicleAsync(int vehicleId)
        {
            // This would typically query route/mileage data
            // For now, return a placeholder
            return await Task.FromResult(0);
        }

        public async Task<int> AddVehicleAsync(Vehicle vehicle)
        {
            if (!IsValidVehicleNumber(vehicle.VehicleNumber))
                return await Task.FromResult(0);

            return await Task.FromResult(_repository.AddVehicle(vehicle));
        }

        public async Task<bool> UpdateVehicleAsync(Vehicle vehicle)
        {
            if (!IsValidVehicleNumber(vehicle.VehicleNumber))
                return await Task.FromResult(false);

            return await Task.FromResult(_repository.UpdateVehicle(vehicle));
        }

        public async Task<bool> DeleteVehicleAsync(int vehicleId)
        {
            if (vehicleId <= 0)
                return await Task.FromResult(false);

            return await Task.FromResult(_repository.DeleteVehicle(vehicleId));
        }
    }
}
