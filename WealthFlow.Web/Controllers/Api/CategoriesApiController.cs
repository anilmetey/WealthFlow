using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WealthFlow.Application.DTOs;
using WealthFlow.Application.Interfaces;

namespace WealthFlow.Web.Controllers.Api
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesApiController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IValidator<CategoryDto> _validator;

        public CategoriesApiController(ICategoryService categoryService, IValidator<CategoryDto> validator)
        {
            _categoryService = categoryService;
            _validator = validator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Kategori bulunamadı." });
            }
            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var created = await _categoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
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

            var existing = await _categoryService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Güncellenecek kategori bulunamadı." });
            }

            await _categoryService.UpdateCategoryAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _categoryService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Silinecek kategori bulunamadı." });
            }

            // İlişkili işlem kontrolü
            var hasTransactions = await _categoryService.HasTransactionsAsync(id);
            if (hasTransactions)
            {
                return BadRequest(new { message = "Bu kategoriye bağlı harcama kayıtları bulunmaktadır. Önce onları silmeli veya düzenlemelisiniz." });
            }

            await _categoryService.DeleteCategoryAsync(id);
            return Ok(new { message = "Kategori başarıyla silindi." });
        }
    }
}
