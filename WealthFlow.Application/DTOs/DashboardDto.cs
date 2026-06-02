using System.Collections.Generic;

namespace WealthFlow.Application.DTOs
{
    public class DashboardDto
    {
        public decimal NetWorth { get; set; }
        public decimal TotalIncomeThisMonth { get; set; }
        public decimal TotalExpenseThisMonth { get; set; }
        public decimal SavingsRate { get; set; }
        
        public List<TransactionDto> RecentTransactions { get; set; } = new();
        public List<BudgetProgressDto> BudgetProgresses { get; set; } = new();
        public List<CategoryDistributionDto> ExpensesByCategory { get; set; } = new();
        
        // Chart elements
        public List<string> MonthlyLabels { get; set; } = new();
        public List<decimal> MonthlyIncomes { get; set; } = new();
        public List<decimal> MonthlyExpenses { get; set; } = new();
        public List<WalletAllocationDto> WalletAllocations { get; set; } = new();
    }

    public class BudgetProgressDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal ProgressPercentage => BudgetAmount > 0 ? (SpentAmount / BudgetAmount) * 100 : 0;
        public bool IsExceeded => SpentAmount > BudgetAmount;
    }

    public class CategoryDistributionDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public double Percentage { get; set; }
    }

    public class WalletAllocationDto
    {
        public string WalletName { get; set; } = string.Empty;
        public string WalletColor { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public double Percentage { get; set; }
    }
}
