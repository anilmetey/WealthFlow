using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WealthFlow.Domain.Entities;
using WealthFlow.Domain.Interfaces;
using WealthFlow.Infrastructure.Contexts;

namespace WealthFlow.Infrastructure.Repositories
{
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Wallet?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(w => w.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Wallet>> GetWalletsWithTransactionsAsync()
        {
            return await _dbSet
                .Include(w => w.Transactions)
                .ToListAsync();
        }
    }
}
