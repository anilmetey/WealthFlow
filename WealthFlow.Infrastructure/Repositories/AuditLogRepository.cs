using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WealthFlow.Domain.Entities;
using WealthFlow.Domain.Interfaces;
using WealthFlow.Infrastructure.Contexts;

namespace WealthFlow.Infrastructure.Repositories
{
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count)
        {
            return await _dbSet
                .OrderByDescending(x => x.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
