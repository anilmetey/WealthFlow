using System;

namespace WealthFlow.Application.DTOs
{
    public class FinancialGoalDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime TargetDate { get; set; } = DateTime.Today.AddYears(1);
        public int CategoryId { get; set; }
        
        // Flattened Category details
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;

        public decimal ProgressPercentage => TargetAmount > 0 ? (CurrentAmount / TargetAmount) * 100 : 0;
        public bool IsReached => CurrentAmount >= TargetAmount;
    }
}
