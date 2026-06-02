using System.Collections.Generic;
using System.Threading.Tasks;
using WealthFlow.Application.DTOs;
using WealthFlow.Domain.Enums;

namespace WealthFlow.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionDto?> GetByIdAsync(int id);
        Task<IEnumerable<TransactionDto>> GetFilteredTransactionsAsync(string? searchTerm, int? categoryId, TransactionType? type);
        Task<TransactionDto> CreateTransactionAsync(TransactionDto dto);
        Task UpdateTransactionAsync(TransactionDto dto);
        Task DeleteTransactionAsync(int id);
    }
}
