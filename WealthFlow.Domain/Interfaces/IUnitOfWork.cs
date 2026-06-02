using System;
using System.Threading.Tasks;

namespace WealthFlow.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ITransactionRepository Transactions { get; }
        ICategoryRepository Categories { get; }
        IBudgetRepository Budgets { get; }
        IFinancialGoalRepository Goals { get; }
        IWalletRepository Wallets { get; }
        IAuditLogRepository AuditLogs { get; }
        Task<int> SaveChangesAsync();
    }
}
