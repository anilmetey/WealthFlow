using System.Collections.Generic;
using System.Threading.Tasks;
using WealthFlow.Domain.Entities;

namespace WealthFlow.Domain.Interfaces
{
    public interface IWalletRepository : IGenericRepository<Wallet>
    {
        Task<Wallet?> GetByNameAsync(string name);
        Task<IEnumerable<Wallet>> GetWalletsWithTransactionsAsync();
    }
}
