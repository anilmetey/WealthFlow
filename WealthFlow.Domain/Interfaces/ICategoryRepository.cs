using System.Threading.Tasks;
using WealthFlow.Domain.Entities;

namespace WealthFlow.Domain.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
        Task<bool> HasTransactionsAsync(int categoryId);
    }
}
