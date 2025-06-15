using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusBuddy.Models;

namespace BusBuddy.Data
{
    /// <summary>
    /// Repository interface for TimeCard data operations
    /// </summary>
    public interface ITimeCardRepository
    {
        Task<IEnumerable<TimeCard>> GetAllAsync();
        Task<TimeCard> GetByIdAsync(int id);
        Task<IEnumerable<TimeCard>> GetByDriverIdAsync(int driverId);
        Task<IEnumerable<TimeCard>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<TimeCard>> GetWeeklyTimeCardsAsync(DateTime weekStartDate);
        Task<IEnumerable<TimeCard>> GetMonthlyTimeCardsAsync(int year, int month);
        Task<TimeCard> AddAsync(TimeCard timeCard);
        Task<TimeCard> UpdateAsync(TimeCard timeCard);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<double> GetWeeklyTotalHoursAsync(int driverId, DateTime weekStartDate);
        Task<double> GetMonthlyTotalHoursAsync(int driverId, int year, int month);
        Task<double> GetWeeklyOvertimeHoursAsync(int driverId, DateTime weekStartDate);
        Task<double> GetMonthlyOvertimeHoursAsync(int driverId, int year, int month);
    }
}
