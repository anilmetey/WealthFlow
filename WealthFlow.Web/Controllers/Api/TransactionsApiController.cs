using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WealthFlow.Application.DTOs;
using WealthFlow.Application.Interfaces;
using WealthFlow.Domain.Enums;

namespace WealthFlow.Web.Controllers.Api
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsApiController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IValidator<TransactionDto> _validator;

        public TransactionsApiController(ITransactionService transactionService, IValidator<TransactionDto> validator)
        {
            _transactionService = transactionService;
            _validator = validator;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? searchTerm, [FromQuery] int? categoryId, [FromQuery] TransactionType? type)
        {
            var transactions = await _transactionService.GetFilteredTransactionsAsync(searchTerm, categoryId, type);
            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transaction = await _transactionService.GetByIdAsync(id);
            if (transaction == null)
            {
                return NotFound(new { message = "İşlem bulunamadı." });
            }
            return Ok(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransactionDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var created = await _transactionService.CreateTransactionAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TransactionDto dto)
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

            var existing = await _transactionService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Güncellenecek işlem bulunamadı." });
            }

            await _transactionService.UpdateTransactionAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _transactionService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Silinecek işlem bulunamadı." });
            }

            await _transactionService.DeleteTransactionAsync(id);
            return Ok(new { message = "İşlem başarıyla silindi." });
        }

        [HttpGet("export")]
        public async Task<IActionResult> GetExport([FromQuery] string? searchTerm, [FromQuery] int? categoryId, [FromQuery] TransactionType? type)
        {
            var transactions = await _transactionService.GetFilteredTransactionsAsync(searchTerm, categoryId, type);
            var transactionList = transactions.ToList();

            var csvBuilder = new System.Text.StringBuilder();
            csvBuilder.AppendLine("Tarih,Aciklama,Hesap/Cuzdan,Kategori,Tur,Tutar");

            foreach (var t in transactionList)
            {
                var typeLabel = t.Type == TransactionType.Income ? "Gelir" : "Gider";
                var dateStr = t.Date.ToString("yyyy-MM-dd");
                var amountStr = t.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                
                var descEscaped = t.Description.Replace("\"", "\"\"");
                if (descEscaped.Contains(",") || descEscaped.Contains("\""))
                {
                    descEscaped = $"\"{descEscaped}\"";
                }

                csvBuilder.AppendLine($"{dateStr},{descEscaped},{t.WalletName},{t.CategoryName},{typeLabel},{amountStr}");
            }

            var csvBytes = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
            
            // Add UTF-8 BOM (Byte Order Mark) for Excel compatibility with Turkish characters
            var bomBytes = new byte[] { 0xEF, 0xBB, 0xBF };
            var fileBytes = new byte[bomBytes.Length + csvBytes.Length];
            System.Buffer.BlockCopy(bomBytes, 0, fileBytes, 0, bomBytes.Length);
            System.Buffer.BlockCopy(csvBytes, 0, fileBytes, bomBytes.Length, csvBytes.Length);

            return File(fileBytes, "text/csv", "WealthFlow_Islem_Defteri.csv");
        }
    }
}
