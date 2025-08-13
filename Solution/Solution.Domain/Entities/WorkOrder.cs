using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solution.Domain.Entities
{
    public enum WorkOrderStatus { Planned = 0, InProgress = 1, Paused = 2, Completed = 3, Canceled = 4 }
    public class WorkOrder
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ItemCode { get; set; } = "";
        public int Quantity { get; set; }
        public DateTime DueDate { get; set; } = DateTime.Today;
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Planned;

    }
} 
