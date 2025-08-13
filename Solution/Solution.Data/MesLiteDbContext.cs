using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Solution.Domain.Entities;

namespace Solution.Data
{
    public class MesLiteDbContext : DbContext
    {
        public MesLiteDbContext(DbContextOptions<MesLiteDbContext> options) : base(options) { }
        public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkOrder>(e =>
            {
                e.ToTable("WorkOrders");
                e.HasKey(x => x.Id);
                e.Property(x => x.ItemCode).IsRequired().HasMaxLength(64);
                e.HasIndex(x => new { x.ItemCode, x.Status });
            });
        }

    }
}
