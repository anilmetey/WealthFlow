using System;

namespace WealthFlow.Domain.Entities
{
    public class FinancialGoal
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime TargetDate { get; set; } = DateTime.Today.AddYears(1);

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
