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

        public VehicleService() : this(new VehicleRepository()) { }

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
            try
            {
                var allVehicles = _repository.GetAllVehicles();
                return allVehicles.FindAll(v => v.Status == "Active");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting active vehicles: {ex.Message}");
                throw new ApplicationException($"Failed to retrieve active vehicles: {ex.Message}", ex);
            }
        }

        public List<Vehicle> GetAllVehicles()
        {
            try
            {
                return _repository.GetAllVehicles();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting all vehicles: {ex.Message}");
                throw new ApplicationException($"Failed to retrieve vehicles: {ex.Message}", ex);
            }
        }

        public bool AddVehicle(Vehicle vehicle)
        {
            try
            {
                if (!IsValidVehicleNumber(vehicle.VehicleNumber))
                    return false;

                _repository.AddVehicle(vehicle);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding vehicle: {ex.Message}");
                throw new ApplicationException($"Failed to add vehicle: {ex.Message}", ex);
            }
        }

        public bool UpdateVehicle(Vehicle vehicle)
        {
            try
            {
                if (!IsValidVehicleNumber(vehicle.VehicleNumber))
                    return false;

                return _repository.UpdateVehicle(vehicle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating vehicle: {ex.Message}");
                throw new ApplicationException($"Failed to update vehicle: {ex.Message}", ex);
            }
        }

        public bool DeleteVehicle(int id)
        {
            try
            {
                if (id <= 0)
                    return false;

                return _repository.DeleteVehicle(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting vehicle: {ex.Message}");
                throw new ApplicationException($"Failed to delete vehicle: {ex.Message}", ex);
            }
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
