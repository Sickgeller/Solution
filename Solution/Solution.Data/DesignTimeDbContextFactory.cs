using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solution.Data
{
    public class MesLiteDbContextFactory : IDesignTimeDbContextFactory<MesLiteDbContext>
    {
        public MesLiteDbContext CreateDbContext(string[] args)
        {
            var opts = new DbContextOptionsBuilder<MesLiteDbContext>()
                 .UseSqlServer(
                     //"Server=localhost\\SQLEXPRESS;Database=MesLite;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;",
                     "Server=localhost\\SQLEXPRESS;Database=MesLite;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;",
                     sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null))
                 .Options;
            return new MesLiteDbContext(opts);
        }
    }
}
