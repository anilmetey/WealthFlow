using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WealthFlow.Domain.Entities;
using WealthFlow.Domain.Interfaces;
using WealthFlow.Infrastructure.Contexts;

namespace WealthFlow.Infrastructure.Repositories
{
    public class BudgetRepository : GenericRepository<Budget>, IBudgetRepository
    {
        public BudgetRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Budget>> GetBudgetsByMonthYearAsync(int month, int year)
        {
            return await _dbSet
                .Include(b => b.Category)
                .Where(b => b.Month == month && b.Year == year)
                .ToListAsync();
        }

        public async Task<Budget?> GetBudgetByCategoryMonthYearAsync(int categoryId, int month, int year)
        {
            return await _dbSet
                .FirstOrDefaultAsync(b => b.CategoryId == categoryId && b.Month == month && b.Year == year);
        }
    }
}
