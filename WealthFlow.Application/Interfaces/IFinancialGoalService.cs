using System.Collections.Generic;
using System.Threading.Tasks;
using WealthFlow.Application.DTOs;

namespace WealthFlow.Application.Interfaces
{
    public interface IFinancialGoalService
    {
        Task<IEnumerable<FinancialGoalDto>> GetAllGoalsAsync();
        Task<FinancialGoalDto?> GetByIdAsync(int id);
        Task<FinancialGoalDto> CreateGoalAsync(FinancialGoalDto dto);
        Task UpdateGoalAsync(FinancialGoalDto dto);
        Task DeleteGoalAsync(int id);
        Task<FinancialGoalDto> ContributeToGoalAsync(int goalId, decimal amount);
    }
}
