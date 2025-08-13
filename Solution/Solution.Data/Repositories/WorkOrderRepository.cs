using Microsoft.EntityFrameworkCore;
using Solution.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solution.Data.Repositories
{
    public class WorkOrderRepository : IWorkOrderRepository
    {
        private readonly IDbContextFactory<MesLiteDbContext> _dbFactory;
        public WorkOrderRepository(IDbContextFactory<MesLiteDbContext> dbFactory)
            => _dbFactory = dbFactory;

        public async Task AddAsync(WorkOrder entity, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            db.WorkOrders.Add(entity);
            await db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            var e = await db.WorkOrders.FindAsync(id, ct);
            if (e is null) return;
            db.WorkOrders.Remove(e);
            await db.SaveChangesAsync(ct);
        }

        public async Task<List<WorkOrder>> GetAllAsync(CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            return await db.WorkOrders
                .OrderBy(x => x.DueDate)
                .ThenByDescending(x => x.Status)
                .ToListAsync(ct);
        }

        public async Task UpdateAsync(WorkOrder entity, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            db.WorkOrders.Update(entity);
            await db.SaveChangesAsync(ct);
        }

        public async Task<(List<WorkOrder> Items, int TotalCount)> GetPageAsync(
            string? itemCode,
            WorkOrderStatus? status,
            DateTime? dueFrom,
            DateTime? dueTo,
            int offset,
            int limit,
            CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            var q = db.WorkOrders.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(itemCode))
                q = q.Where(x => x.ItemCode.Contains(itemCode));
            if (status.HasValue)
                q = q.Where(x => x.Status == status.Value);
            if (dueFrom.HasValue)
                q = q.Where(x => x.DueDate >= dueFrom.Value.Date);
            if (dueTo.HasValue)
                q = q.Where(x => x.DueDate <= dueTo.Value.Date);

            var total = await q.CountAsync(ct);
            var items = await q
                .OrderBy(x => x.DueDate).ThenByDescending(x => x.Status)
                .Skip(offset).Take(limit)
                .ToListAsync(ct);
            return (items, total);
        }
    }
}
