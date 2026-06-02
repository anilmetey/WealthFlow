using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using WealthFlow.Application.DTOs;
using WealthFlow.Application.Interfaces;
using WealthFlow.Domain.Entities;
using WealthFlow.Domain.Enums;
using WealthFlow.Domain.Interfaces;

namespace WealthFlow.Application.Services
{
    public class FinancialGoalService : IFinancialGoalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FinancialGoalService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FinancialGoalDto>> GetAllGoalsAsync()
        {
            var goals = await _unitOfWork.Goals.GetGoalsWithCategoriesAsync();
            return _mapper.Map<IEnumerable<FinancialGoalDto>>(goals);
        }

        public async Task<FinancialGoalDto?> GetByIdAsync(int id)
        {
            var goal = await _unitOfWork.Goals.GetGoalWithCategoryByIdAsync(id);
            return _mapper.Map<FinancialGoalDto?>(goal);
        }

        public async Task<FinancialGoalDto> CreateGoalAsync(FinancialGoalDto dto)
        {
            var goal = _mapper.Map<FinancialGoal>(dto);
            await _unitOfWork.Goals.AddAsync(goal);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Goals.GetGoalWithCategoryByIdAsync(goal.Id);
            return _mapper.Map<FinancialGoalDto>(created ?? goal);
        }

        public async Task UpdateGoalAsync(FinancialGoalDto dto)
        {
            var goal = await _unitOfWork.Goals.GetByIdAsync(dto.Id);
            if (goal != null)
            {
                _mapper.Map(dto, goal);
                _unitOfWork.Goals.Update(goal);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task DeleteGoalAsync(int id)
        {
            var goal = await _unitOfWork.Goals.GetByIdAsync(id);
            if (goal != null)
            {
                _unitOfWork.Goals.Delete(goal);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<FinancialGoalDto> ContributeToGoalAsync(int goalId, decimal amount)
        {
            var goal = await _unitOfWork.Goals.GetGoalWithCategoryByIdAsync(goalId);
            if (goal == null)
            {
                throw new KeyNotFoundException("Birikim katkısı yapılacak hedef bulunamadı.");
            }

            // 1. Hedefin biriken tutarını artır
            goal.CurrentAmount += amount;
            _unitOfWork.Goals.Update(goal);

            // 2. Bu katkıyı harcama işlemi olarak kaydet (Tasarruf/Birikim aktarımı)
            var savingTransaction = new Transaction
            {
                Description = $"Birikim Katkısı: {goal.Title}",
                Amount = amount,
                Date = DateTime.Today,
                Type = TransactionType.Expense,
                CategoryId = goal.CategoryId
            };
            await _unitOfWork.Transactions.AddAsync(savingTransaction);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<FinancialGoalDto>(goal);
        }
    }
}
