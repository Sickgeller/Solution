using Microsoft.EntityFrameworkCore;
using Solution.Data;
using Solution.Domain.Entities;

// Simple seeder: inserts 500 random WorkOrders into MesLite database

var options = new DbContextOptionsBuilder<MesLiteDbContext>()
    .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MesLite;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;")
    .Options;

using var db = new MesLiteDbContext(options);

var rnd = new Random();
var baseDate = DateTime.Today;

var items = Enumerable.Range(1, 500).Select(i => new WorkOrder
{
    ItemCode = $"ITEM-{rnd.Next(1000, 9999)}",
    Quantity = rnd.Next(1, 1000),
    DueDate = baseDate.AddDays(rnd.Next(-30, 60)),
    Status = (WorkOrderStatus)rnd.Next(0, 5)
}).ToList();

db.WorkOrders.AddRange(items);
await db.SaveChangesAsync();

var total = await db.WorkOrders.CountAsync();
Console.WriteLine($"Seeded 500 records. Total rows: {total}");
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
