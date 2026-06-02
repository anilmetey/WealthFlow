using System;
using System.Threading.Tasks;
using WealthFlow.Domain.Interfaces;
using WealthFlow.Infrastructure.Contexts;

namespace WealthFlow.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private ITransactionRepository? _transactions;
        private ICategoryRepository? _categories;
        private IBudgetRepository? _budgets;
        private IFinancialGoalRepository? _goals;
        private IWalletRepository? _wallets;
        private IAuditLogRepository? _auditLogs;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public ITransactionRepository Transactions => _transactions ??= new TransactionRepository(_context);
        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
        public IBudgetRepository Budgets => _budgets ??= new BudgetRepository(_context);
        public IFinancialGoalRepository Goals => _goals ??= new FinancialGoalRepository(_context);
        public IWalletRepository Wallets => _wallets ??= new WalletRepository(_context);
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
