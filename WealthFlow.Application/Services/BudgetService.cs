using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using WealthFlow.Application.DTOs;
using WealthFlow.Application.Interfaces;
using WealthFlow.Domain.Entities;
using WealthFlow.Domain.Interfaces;

namespace WealthFlow.Application.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BudgetService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BudgetDto>> GetBudgetsByMonthYearAsync(int month, int year)
        {
            var budgets = await _unitOfWork.Budgets.GetBudgetsByMonthYearAsync(month, year);
            return _mapper.Map<IEnumerable<BudgetDto>>(budgets);
        }

        public async Task<BudgetDto?> GetBudgetByCategoryMonthYearAsync(int categoryId, int month, int year)
        {
            var budget = await _unitOfWork.Budgets.GetBudgetByCategoryMonthYearAsync(categoryId, month, year);
            return _mapper.Map<BudgetDto?>(budget);
        }

        public async Task<BudgetDto> CreateOrUpdateBudgetAsync(BudgetDto dto)
        {
            var existingBudget = await _unitOfWork.Budgets.GetBudgetByCategoryMonthYearAsync(dto.CategoryId, dto.Month, dto.Year);
            if (existingBudget != null)
            {
                existingBudget.Amount = dto.Amount;
                _unitOfWork.Budgets.Update(existingBudget);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<BudgetDto>(existingBudget);
            }
            else
            {
                var budget = _mapper.Map<Budget>(dto);
                await _unitOfWork.Budgets.AddAsync(budget);
                await _unitOfWork.SaveChangesAsync();
                
                var createdBudget = await _unitOfWork.Budgets.GetByIdAsync(budget.Id);
                return _mapper.Map<BudgetDto>(createdBudget ?? budget);
            }
        }

        public async Task UpdateBudgetAmountAsync(int id, decimal amount)
        {
            var budget = await _unitOfWork.Budgets.GetByIdAsync(id);
            if (budget != null)
            {
                budget.Amount = amount;
                _unitOfWork.Budgets.Update(budget);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task DeleteBudgetAsync(int id)
        {
            var budget = await _unitOfWork.Budgets.GetByIdAsync(id);
            if (budget != null)
            {
                _unitOfWork.Budgets.Delete(budget);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
