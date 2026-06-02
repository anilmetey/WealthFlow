using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WealthFlow.Domain.Entities;
using WealthFlow.Domain.Interfaces;
using WealthFlow.Infrastructure.Contexts;

namespace WealthFlow.Infrastructure.Repositories
{
    public class FinancialGoalRepository : GenericRepository<FinancialGoal>, IFinancialGoalRepository
    {
        public FinancialGoalRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FinancialGoal>> GetGoalsWithCategoriesAsync()
        {
            return await _dbSet
                .Include(g => g.Category)
                .ToListAsync();
        }

        public async Task<FinancialGoal?> GetGoalWithCategoryByIdAsync(int id)
        {
            return await _dbSet
                .Include(g => g.Category)
                .FirstOrDefaultAsync(g => g.Id == id);
        }
    }
}
