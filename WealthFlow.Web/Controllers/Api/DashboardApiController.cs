using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WealthFlow.Application.Interfaces;

namespace WealthFlow.Web.Controllers.Api
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardApiController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardApiController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? month, [FromQuery] int? year)
        {
            var today = System.DateTime.Today;
            var targetMonth = month ?? today.Month;
            var targetYear = year ?? today.Year;

            var data = await _dashboardService.GetDashboardDataAsync(targetMonth, targetYear);
            return Ok(data);
        }
    }
}
