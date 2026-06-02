using System.Collections.Generic;
using System.Threading.Tasks;
using WealthFlow.Application.DTOs;

namespace WealthFlow.Application.Interfaces
{
    public interface IBudgetService
    {
        Task<IEnumerable<BudgetDto>> GetBudgetsByMonthYearAsync(int month, int year);
        Task<BudgetDto?> GetBudgetByCategoryMonthYearAsync(int categoryId, int month, int year);
        Task<BudgetDto> CreateOrUpdateBudgetAsync(BudgetDto dto);
        Task UpdateBudgetAmountAsync(int id, decimal amount);
        Task DeleteBudgetAsync(int id);
    }
}
