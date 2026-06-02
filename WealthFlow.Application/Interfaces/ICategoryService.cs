using System.Collections.Generic;
using System.Threading.Tasks;
using WealthFlow.Application.DTOs;

namespace WealthFlow.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CategoryDto dto);
        Task UpdateCategoryAsync(CategoryDto dto);
        Task DeleteCategoryAsync(int id);
        Task<bool> HasTransactionsAsync(int id);
    }
}
