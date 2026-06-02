using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WealthFlow.Application.Interfaces;

namespace WealthFlow.Web.Controllers.Api
{
    [ApiController]
    [Route("api/insights")]
    public class InsightsApiController : ControllerBase
    {
        private readonly IInsightService _insightService;

        public InsightsApiController(IInsightService insightService)
        {
            _insightService = insightService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? month, [FromQuery] int? year)
        {
            var today = System.DateTime.Today;
            var targetMonth = month ?? today.Month;
            var targetYear = year ?? today.Year;

            var insights = await _insightService.GenerateInsightsAsync(targetMonth, targetYear);
            return Ok(insights);
        }

        [HttpGet("health-score")]
        public async Task<IActionResult> GetHealthScore()
        {
            var score = await _insightService.CalculateFinancialHealthScoreAsync();
            return Ok(new { score = score });
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { response = "Lütfen geçerli bir soru yazın." });
            }
            var response = await _insightService.ProcessChatQueryAsync(request.Message);
            return Ok(new { response = response });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
