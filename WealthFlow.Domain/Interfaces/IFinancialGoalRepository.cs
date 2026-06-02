using System.Collections.Generic;
using System.Threading.Tasks;
using WealthFlow.Domain.Entities;

namespace WealthFlow.Domain.Interfaces
{
    public interface IFinancialGoalRepository : IGenericRepository<FinancialGoal>
    {
        Task<IEnumerable<FinancialGoal>> GetGoalsWithCategoriesAsync();
        Task<FinancialGoal?> GetGoalWithCategoryByIdAsync(int id);
    }
}
