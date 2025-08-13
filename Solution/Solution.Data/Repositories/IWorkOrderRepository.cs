using Solution.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solution.Data.Repositories
{
    public interface IWorkOrderRepository
    {
        Task<List<WorkOrder>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(WorkOrder entity, CancellationToken ct = default);
        Task UpdateAsync(WorkOrder entity, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);

        Task<(List<WorkOrder> Items, int TotalCount)> GetPageAsync(
            string? itemCode,
            WorkOrderStatus? status,
            DateTime? dueFrom,
            DateTime? dueTo,
            int offset,
            int limit,
            CancellationToken ct = default);
    }
}
