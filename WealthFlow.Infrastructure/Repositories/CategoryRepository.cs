using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WealthFlow.Domain.Entities;
using WealthFlow.Domain.Interfaces;
using WealthFlow.Infrastructure.Contexts;

namespace WealthFlow.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> HasTransactionsAsync(int categoryId)
        {
            return await _context.Transactions.AnyAsync(t => t.CategoryId == categoryId);
        }
    }
}
