using System;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Data
{
    public interface IFuelRepository
    {
        List<Fuel> GetAllFuelRecords();
        Fuel GetFuelRecordById(int id);
        List<Fuel> GetFuelRecordsByDate(DateTime date);
        List<Fuel> GetFuelRecordsByBus(int busId);
        int AddFuelRecord(Fuel fuelRecord);
        bool UpdateFuelRecord(Fuel fuelRecord);
        bool DeleteFuelRecord(int id);
    }
}

