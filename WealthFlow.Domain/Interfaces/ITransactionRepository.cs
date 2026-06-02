using System.Collections.Generic;
using System.Threading.Tasks;
using WealthFlow.Domain.Entities;
using WealthFlow.Domain.Enums;

namespace WealthFlow.Domain.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetFilteredTransactionsAsync(string? searchTerm, int? categoryId, TransactionType? type);
        Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int count);
        Task<IEnumerable<Transaction>> GetTransactionsByMonthYearAsync(int month, int year);
    }
}
