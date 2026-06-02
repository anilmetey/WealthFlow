using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WealthFlow.Application.DTOs;
using WealthFlow.Application.Interfaces;

namespace WealthFlow.Web.Controllers.Api
{
    [ApiController]
    [Route("api/goals")]
    public class FinancialGoalsApiController : ControllerBase
    {
        private readonly IFinancialGoalService _goalService;
        private readonly IValidator<FinancialGoalDto> _validator;

        public FinancialGoalsApiController(IFinancialGoalService goalService, IValidator<FinancialGoalDto> validator)
        {
            _goalService = goalService;
            _validator = validator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var goals = await _goalService.GetAllGoalsAsync();
            return Ok(goals);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var goal = await _goalService.GetByIdAsync(id);
            if (goal == null)
            {
                return NotFound(new { message = "Hedef bulunamadı." });
            }
            return Ok(goal);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FinancialGoalDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var created = await _goalService.CreateGoalAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FinancialGoalDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "Kimlik eşleşmiyor." });
            }

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var existing = await _goalService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Güncellenecek hedef bulunamadı." });
            }

            await _goalService.UpdateGoalAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _goalService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Silinecek hedef bulunamadı." });
            }

            await _goalService.DeleteGoalAsync(id);
            return Ok(new { message = "Hedef başarıyla silindi." });
        }

        [HttpPost("{id}/contribute")]
        public async Task<IActionResult> Contribute(int id, [FromBody] decimal amount)
        {
            if (amount <= 0)
            {
                return BadRequest(new { message = "Katkı tutarı 0'dan büyük olmalıdır." });
            }

            try
            {
                var updatedGoal = await _goalService.ContributeToGoalAsync(id, amount);
                return Ok(updatedGoal);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
