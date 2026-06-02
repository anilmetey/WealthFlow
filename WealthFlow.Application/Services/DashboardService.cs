using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using WealthFlow.Application.DTOs;
using WealthFlow.Application.Interfaces;
using WealthFlow.Domain.Enums;
using WealthFlow.Domain.Interfaces;

namespace WealthFlow.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DashboardService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DashboardDto> GetDashboardDataAsync(int month, int year)
        {
            var today = DateTime.Today;

            // 1. Fetch all wallets for overall NetWorth calculation (sum of all wallet balances)
            var activeWallets = await _unitOfWork.Wallets.GetAllAsync();
            var walletList = activeWallets.ToList();
            var netWorth = walletList.Sum(w => w.Balance);

            // Fetch all transactions
            var allTransactions = await _unitOfWork.Transactions.GetFilteredTransactionsAsync(null, null, null);
            var transactionList = allTransactions.ToList();

            // 2. This Month's Income & Expenses
            var thisMonthTransactions = transactionList
                .Where(t => t.Date.Month == month && t.Date.Year == year)
                .ToList();

            var thisMonthIncome = thisMonthTransactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var thisMonthExpense = thisMonthTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            // 3. Savings Rate
            decimal savingsRate = 0;
            if (thisMonthIncome > 0)
            {
                savingsRate = ((thisMonthIncome - thisMonthExpense) / thisMonthIncome) * 100;
                if (savingsRate < 0) savingsRate = 0;
            }

            // 4. Recent Transactions (Top 5)
            var recentEntities = await _unitOfWork.Transactions.GetRecentTransactionsAsync(5);
            var recentTransactions = _mapper.Map<List<TransactionDto>>(recentEntities);

            // 5. Budget Progresses
            var activeBudgets = await _unitOfWork.Budgets.GetBudgetsByMonthYearAsync(month, year);
            var budgetProgresses = new List<BudgetProgressDto>();

            foreach (var b in activeBudgets)
            {
                var spent = thisMonthTransactions
                    .Where(t => t.CategoryId == b.CategoryId && t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount);

                budgetProgresses.Add(new BudgetProgressDto
                {
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category?.Name ?? "Bilinmeyen",
                    CategoryIcon = b.Category?.Icon ?? "fa-tag",
                    CategoryColor = b.Category?.Color ?? "#6B7280",
                    BudgetAmount = b.Amount,
                    SpentAmount = spent
                });
            }

            // 6. Expenses By Category
            var expensesByCategory = thisMonthTransactions
                .Where(t => t.Type == TransactionType.Expense && t.Category != null)
                .GroupBy(t => new { t.CategoryId, t.Category!.Name, t.Category.Color })
                .Select(g => new CategoryDistributionDto
                {
                    CategoryName = g.Key.Name,
                    CategoryColor = g.Key.Color,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToList();

            var totalCategoryExpenses = expensesByCategory.Sum(ce => ce.TotalAmount);
            foreach (var item in expensesByCategory)
            {
                item.Percentage = totalCategoryExpenses > 0 ? (double)(item.TotalAmount / totalCategoryExpenses) * 100 : 0;
            }

            // 7. Last 6 Months Cash Flow
            var monthlyLabels = new List<string>();
            var monthlyIncomes = new List<decimal>();
            var monthlyExpenses = new List<decimal>();

            var culture = new System.Globalization.CultureInfo("tr-TR");

            for (int i = 5; i >= 0; i--)
            {
                var targetDate = today.AddMonths(-i);
                var targetMonth = targetDate.Month;
                var targetYear = targetDate.Year;

                var label = targetDate.ToString("MMMM yyyy", culture);
                monthlyLabels.Add(label);

                var inc = transactionList
                    .Where(t => t.Type == TransactionType.Income && t.Date.Month == targetMonth && t.Date.Year == targetYear)
                    .Sum(t => t.Amount);

                var exp = transactionList
                    .Where(t => t.Type == TransactionType.Expense && t.Date.Month == targetMonth && t.Date.Year == targetYear)
                    .Sum(t => t.Amount);

                monthlyIncomes.Add(inc);
                monthlyExpenses.Add(exp);
            }

            // 8. Asset Allocation (Wallet Allocations)
            var totalAssets = walletList.Where(w => w.Balance > 0).Sum(w => w.Balance);
            var walletAllocations = walletList
                .Where(w => w.Balance > 0)
                .Select(w => new WalletAllocationDto
                {
                    WalletName = w.Name,
                    WalletColor = w.Color,
                    Balance = w.Balance,
                    Percentage = totalAssets > 0 ? (double)(w.Balance / totalAssets) * 100 : 0
                })
                .OrderByDescending(w => w.Balance)
                .ToList();

            return new DashboardDto
            {
                NetWorth = netWorth,
                TotalIncomeThisMonth = thisMonthIncome,
                TotalExpenseThisMonth = thisMonthExpense,
                SavingsRate = savingsRate,
                RecentTransactions = recentTransactions,
                BudgetProgresses = budgetProgresses,
                ExpensesByCategory = expensesByCategory.OrderByDescending(x => x.TotalAmount).ToList(),
                MonthlyLabels = monthlyLabels,
                MonthlyIncomes = monthlyIncomes,
                MonthlyExpenses = monthlyExpenses,
                WalletAllocations = walletAllocations
            };
        }
    }
}
