using System;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Data
{
    public interface ITimeCardRepository
    {
        List<TimeCard> GetAllTimeCards();
        TimeCard GetTimeCardById(int id);
        List<TimeCard> GetTimeCardsByDate(DateTime date);
        List<TimeCard> GetTimeCardsByDateRange(DateTime startDate, DateTime endDate);
        List<TimeCard> GetTimeCardsByDayType(string dayType);
        int AddTimeCard(TimeCard timeCard);
        bool UpdateTimeCard(TimeCard timeCard);
        bool DeleteTimeCard(int id);
    }
}
