using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusBuddy.Models;

namespace BusBuddy.Data
{    /// <summary>
    /// Repository interface for TimeCard data operations
    /// </summary>
    public interface ITimeCardRepository
    {
        Task<IEnumerable<Models.TimeCard>> GetAllAsync();
        Task<Models.TimeCard> GetByIdAsync(int id);
        Task<IEnumerable<Models.TimeCard>> GetByDriverIdAsync(int driverId);
        Task<IEnumerable<Models.TimeCard>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Models.TimeCard>> GetWeeklyTimeCardsAsync(DateTime weekStartDate);
        Task<IEnumerable<Models.TimeCard>> GetMonthlyTimeCardsAsync(int year, int month);
        Task<Models.TimeCard> AddAsync(Models.TimeCard timeCard);
        Task<Models.TimeCard> UpdateAsync(Models.TimeCard timeCard);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<double> GetWeeklyTotalHoursAsync(int driverId, DateTime weekStartDate);
        Task<double> GetMonthlyTotalHoursAsync(int driverId, int year, int month);
        Task<double> GetWeeklyOvertimeHoursAsync(int driverId, DateTime weekStartDate);
        Task<double> GetMonthlyOvertimeHoursAsync(int driverId, int year, int month);
    }
}
