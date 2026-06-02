using System.Collections.Generic;
using System.Threading.Tasks;
using WealthFlow.Domain.Entities;

namespace WealthFlow.Domain.Interfaces
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count);
    }
}
