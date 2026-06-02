using System.Collections.Generic;
using System.Threading.Tasks;
using WealthFlow.Domain.Entities;

namespace WealthFlow.Domain.Interfaces
{
    public interface IBudgetRepository : IGenericRepository<Budget>
    {
        Task<IEnumerable<Budget>> GetBudgetsByMonthYearAsync(int month, int year);
        Task<Budget?> GetBudgetByCategoryMonthYearAsync(int categoryId, int month, int year);
    }
}
