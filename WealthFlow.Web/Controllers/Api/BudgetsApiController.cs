using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WealthFlow.Application.DTOs;
using WealthFlow.Application.Interfaces;

namespace WealthFlow.Web.Controllers.Api
{
    [ApiController]
    [Route("api/budgets")]
    public class BudgetsApiController : ControllerBase
    {
        private readonly IBudgetService _budgetService;
        private readonly IValidator<BudgetDto> _validator;

        public BudgetsApiController(IBudgetService budgetService, IValidator<BudgetDto> validator)
        {
            _budgetService = budgetService;
            _validator = validator;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int month, [FromQuery] int year)
        {
            var budgets = await _budgetService.GetBudgetsByMonthYearAsync(month, year);
            return Ok(budgets);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate([FromBody] BudgetDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var result = await _budgetService.CreateOrUpdateBudgetAsync(dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAmount(int id, [FromBody] decimal amount)
        {
            if (amount <= 0)
            {
                return BadRequest(new { message = "Miktar 0'dan büyük olmalıdır." });
            }

            await _budgetService.UpdateBudgetAmountAsync(id, amount);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _budgetService.DeleteBudgetAsync(id);
            return Ok(new { message = "Bütçe limiti başarıyla kaldırıldı." });
        }
    }
}
