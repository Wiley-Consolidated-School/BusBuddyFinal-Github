using System;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Data
{
    public interface IDriverRepository
    {
        List<Driver> GetAllDrivers();
        Driver GetDriverById(int id);
        List<Driver> GetDriversByLicenseType(string licenseType);
        int AddDriver(Driver driver);
        bool UpdateDriver(Driver driver);
        bool DeleteDriver(int id);
    }
}

