using System;

namespace BusBuddy.Models
{
    public class PTOBalance
    {
        public int PTOBalanceID { get; set; }
        public int Year { get; set; }
        public decimal AnnualPTOHours { get; set; } = 96; // 12 days * 8 hours = 96 hours per year
        public decimal PTOUsed { get; set; }
        public decimal PTORemaining => AnnualPTOHours - PTOUsed;
        public DateTime? LastUpdated { get; set; }
        public string? Notes { get; set; }
    }
}
