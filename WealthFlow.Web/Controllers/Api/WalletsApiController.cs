using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WealthFlow.Application.DTOs;
using WealthFlow.Application.Interfaces;

namespace WealthFlow.Web.Controllers.Api
{
    [ApiController]
    [Route("api/wallets")]
    public class WalletsApiController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletsApiController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var wallets = await _walletService.GetAllWalletsAsync();
            return Ok(wallets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var wallet = await _walletService.GetByIdAsync(id);
            if (wallet == null)
            {
                return NotFound(new { message = "Cüzdan bulunamadı." });
            }
            return Ok(wallet);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WalletDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Cüzdan adı boş olamaz." });
            }

            var created = await _walletService.CreateWalletAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WalletDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "Kimlik eşleşmiyor." });
            }

            var existing = await _walletService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Cüzdan bulunamadı." });
            }

            await _walletService.UpdateWalletAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _walletService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Cüzdan bulunamadı." });
            }

            await _walletService.DeleteWalletAsync(id);
            return Ok(new { message = "Cüzdan başarıyla silindi." });
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] WalletTransferDto dto)
        {
            if (dto.Amount <= 0)
            {
                return BadRequest(new { message = "Transfer tutarı 0'dan büyük olmalıdır." });
            }

            var success = await _walletService.TransferFundsAsync(dto);
            if (!success)
            {
                return BadRequest(new { message = "Transfer başarısız. Kaynak cüzdan bakiye yetersiz veya cüzdanlar geçersiz olabilir." });
            }

            return Ok(new { message = "Bakiye transferi başarıyla tamamlandı!" });
        }
    }
}
