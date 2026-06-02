using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WealthFlow.Domain.Entities;
using WealthFlow.Domain.Enums;
using WealthFlow.Domain.Interfaces;
using WealthFlow.Infrastructure.Contexts;

namespace WealthFlow.Infrastructure.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Transaction>> GetFilteredTransactionsAsync(string? searchTerm, int? categoryId, TransactionType? type)
        {
            var query = _dbSet
                .Include(t => t.Category)
                .Include(t => t.Wallet)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.Description.ToLower().Contains(searchTerm.ToLower()));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type.Value);
            }

            return await query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int count)
        {
            return await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Wallet)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByMonthYearAsync(int month, int year)
        {
            return await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Wallet)
                .Where(t => t.Date.Month == month && t.Date.Year == year)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }
    }
}
