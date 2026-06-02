using System.Collections.Generic;
using System.Threading.Tasks;
using WealthFlow.Application.DTOs;

namespace WealthFlow.Application.Interfaces
{
    public interface IInsightService
    {
        Task<IEnumerable<InsightDto>> GenerateInsightsAsync(int month, int year);
        Task<int> CalculateFinancialHealthScoreAsync();
        Task<string> ProcessChatQueryAsync(string query);
    }
}
