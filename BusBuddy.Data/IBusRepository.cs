using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Data
{
    public interface IBusRepository
    {
        IEnumerable<Bus> GetAllBuses();
        Bus GetBusById(int busId);
        void AddBus(Bus bus);
        void UpdateBus(Bus bus);
        void DeleteBus(int busId);
    }
}

