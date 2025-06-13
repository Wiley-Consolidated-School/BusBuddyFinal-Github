using System;
using System.Linq;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{
    public interface IPTOBalanceRepository
    {
        PTOBalance GetPTOBalanceForYear(int year);
        void UpdatePTOBalance(PTOBalance ptoBalance);
        void InitializePTOBalanceForYear(int year);
    }

    public class PTOBalanceRepository : BaseRepository, IPTOBalanceRepository
    {
        public PTOBalanceRepository() : base()
        {
        }

        public PTOBalance GetPTOBalanceForYear(int year)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    SELECT PTOBalanceID, Year, AnnualPTOHours, PTOUsed, LastUpdated, Notes
                    FROM PTOBalance
                    WHERE Year = @Year";

                var row = connection.QuerySingleOrDefault(sql, new { Year = year });
                if (row == null)
                {
                    // Initialize PTO balance for the year if it doesn't exist
                    InitializePTOBalanceForYear(year);

                    // Query again after initialization, but don't recurse if still not found
                    row = connection.QuerySingleOrDefault(sql, new { Year = year });
                    if (row == null)
                    {
                        // If still not found after initialization, return default values
                        return new PTOBalance
                        {
                            PTOBalanceID = 0,
                            Year = year,
                            AnnualPTOHours = 96m,
                            PTOUsed = 0m,
                            LastUpdated = DateTime.Now,
                            Notes = $"Default PTO balance for year {year} - initialization may have failed"
                        };
                    }
                }

                return new PTOBalance
                {
                    PTOBalanceID = (int)Convert.ToInt64(row.PTOBalanceID),
                    Year = (int)Convert.ToInt64(row.Year),
                    AnnualPTOHours = row.AnnualPTOHours != null ? (decimal)Convert.ToDecimal(row.AnnualPTOHours) : 96m,
                    PTOUsed = row.PTOUsed != null ? (decimal)Convert.ToDecimal(row.PTOUsed) : 0m,
                    LastUpdated = row.LastUpdated != null ? DateTime.Parse(row.LastUpdated.ToString()) : (DateTime?)null,
                    Notes = row.Notes?.ToString()
                };
            }
        }

        public void UpdatePTOBalance(PTOBalance ptoBalance)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    UPDATE PTOBalance
                    SET PTOUsed = @PTOUsed,
                        LastUpdated = @LastUpdated,
                        Notes = @Notes
                    WHERE Year = @Year";

                connection.Execute(sql, new
                {
                    Year = ptoBalance.Year,
                    PTOUsed = ptoBalance.PTOUsed,
                    LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Notes = ptoBalance.Notes
                });
            }
        }

        public void InitializePTOBalanceForYear(int year)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    INSERT INTO PTOBalance (Year, AnnualPTOHours, PTOUsed, LastUpdated, Notes)
                    VALUES (@Year, @AnnualPTOHours, @PTOUsed, @LastUpdated, @Notes)";

                connection.Execute(sql, new
                {
                    Year = year,
                    AnnualPTOHours = 96m, // 12 days * 8 hours
                    PTOUsed = 0m,
                    LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Notes = $"PTO balance initialized for year {year}"
                });
            }
        }
    }
}
