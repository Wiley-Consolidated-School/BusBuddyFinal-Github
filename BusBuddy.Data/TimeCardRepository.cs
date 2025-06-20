using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BusBuddy.Models;
using TimeCardModel = BusBuddy.Models.TimeCard;

namespace BusBuddy.Data
{
    /// <summary>
    /// Repository implementation for TimeCard data operations
    /// </summary>
    public class TimeCardRepository : BaseRepository, ITimeCardRepository
    {
        private readonly BusBuddyContext _context;

        public TimeCardRepository(BusBuddyContext context) : base()
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Models.TimeCard>> GetAllAsync()
        {
            return await _context.Set<BusBuddy.Models.TimeCard>()
                .Include(tc => tc.Driver)
                .OrderByDescending(tc => tc.Date)
                .ToListAsync();
        }

        public async Task<Models.TimeCard> GetByIdAsync(int id)
        {
            return await _context.Set<BusBuddy.Models.TimeCard>()
                .Include(tc => tc.Driver)
                .FirstOrDefaultAsync(tc => tc.TimeCardId == id);
        }

        public async Task<IEnumerable<Models.TimeCard>> GetByDriverIdAsync(int driverId)
        {
            return await _context.Set<BusBuddy.Models.TimeCard>()
                .Include(tc => tc.Driver)
                .Where(tc => tc.DriverId == driverId)
                .OrderByDescending(tc => tc.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Models.TimeCard>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Set<BusBuddy.Models.TimeCard>()
                .Include(tc => tc.Driver)
                .Where(tc => tc.Date >= startDate && tc.Date <= endDate)
                .OrderBy(tc => tc.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Models.TimeCard>> GetWeeklyTimeCardsAsync(DateTime weekStartDate)
        {
            var weekEndDate = weekStartDate.AddDays(6);
            return await GetByDateRangeAsync(weekStartDate, weekEndDate);
        }

        public async Task<IEnumerable<Models.TimeCard>> GetMonthlyTimeCardsAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return await GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<Models.TimeCard> AddAsync(Models.TimeCard timeCard)
        {
            timeCard.CreatedDate = DateTime.Now;
            timeCard.CalculateTotalHours();

            _context.Set<BusBuddy.Models.TimeCard>().Add(timeCard);
            await _context.SaveChangesAsync();
            return timeCard;
        }        public async Task<Models.TimeCard> UpdateAsync(Models.TimeCard timeCard)
        {
            timeCard.ModifiedDate = DateTime.Now;
            timeCard.CalculateTotalHours();

            _context.Entry(timeCard).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return timeCard;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var timeCard = await _context.Set<BusBuddy.Models.TimeCard>().FindAsync(id);
            if (timeCard == null)
                return false;

            _context.Set<BusBuddy.Models.TimeCard>().Remove(timeCard);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<BusBuddy.Models.TimeCard>().AnyAsync(tc => tc.TimeCardId == id);
        }

        public async Task<double> GetWeeklyTotalHoursAsync(int driverId, DateTime weekStartDate)
        {
            var weeklyTimeCards = await GetWeeklyTimeCardsAsync(weekStartDate);
            return weeklyTimeCards
                .Where(tc => tc.DriverId == driverId)
                .Sum(tc => tc.TotalTime);
        }

        public async Task<double> GetMonthlyTotalHoursAsync(int driverId, int year, int month)
        {
            var monthlyTimeCards = await GetMonthlyTimeCardsAsync(year, month);
            return monthlyTimeCards
                .Where(tc => tc.DriverId == driverId)
                .Sum(tc => tc.TotalTime);
        }

        public async Task<double> GetWeeklyOvertimeHoursAsync(int driverId, DateTime weekStartDate)
        {
            var weeklyTimeCards = await GetWeeklyTimeCardsAsync(weekStartDate);
            return weeklyTimeCards
                .Where(tc => tc.DriverId == driverId)
                .Sum(tc => tc.Overtime);
        }

        public async Task<double> GetMonthlyOvertimeHoursAsync(int driverId, int year, int month)
        {
            var monthlyTimeCards = await GetMonthlyTimeCardsAsync(year, month);
            return monthlyTimeCards
                .Where(tc => tc.DriverId == driverId)
                .Sum(tc => tc.Overtime);
        }

        /// <summary>
        /// Get time cards for a specific driver and week starting on Monday
        /// </summary>
        public async Task<IEnumerable<BusBuddy.Models.TimeCard>> GetWeeklyTimeCardsForDriverAsync(int driverId, DateTime weekDate)
        {
            // Ensure we start on Monday
            var monday = weekDate.AddDays(-(int)weekDate.DayOfWeek + 1);
            var sunday = monday.AddDays(6);

            return await _context.Set<BusBuddy.Models.TimeCard>()
                .Include(tc => tc.Driver)
                .Where(tc => tc.DriverId == driverId &&
                           tc.Date >= monday &&
                           tc.Date <= sunday)
                .OrderBy(tc => tc.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Get time cards for a specific driver and month (1st to last day)
        /// </summary>
        public async Task<IEnumerable<BusBuddy.Models.TimeCard>> GetMonthlyTimeCardsForDriverAsync(int driverId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await _context.Set<BusBuddy.Models.TimeCard>()
                .Include(tc => tc.Driver)
                .Where(tc => tc.DriverId == driverId &&
                           tc.Date >= startDate &&
                           tc.Date <= endDate)
                .OrderBy(tc => tc.Date)
                .ToListAsync();
        }
    }
}
