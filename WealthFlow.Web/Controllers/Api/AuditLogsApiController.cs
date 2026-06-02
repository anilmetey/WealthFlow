using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WealthFlow.Domain.Interfaces;

namespace WealthFlow.Web.Controllers.Api
{
    [ApiController]
    [Route("api/auditlogs")]
    public class AuditLogsApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogsApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? limit)
        {
            var count = limit ?? 50;
            var logs = await _unitOfWork.AuditLogs.GetLatestLogsAsync(count);
            return Ok(logs);
        }
    }
}
