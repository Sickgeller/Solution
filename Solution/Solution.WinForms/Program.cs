using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Solution.Data;
using Solution.Data.Repositories;

namespace Solution.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(cfg => cfg
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true))
            .ConfigureServices((ctx, services) =>
            {
                services.AddDbContextFactory<MesLiteDbContext>(opt =>
                    opt.UseSqlServer(ctx.Configuration.GetConnectionString("SqlServer")));
                services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();

                services.AddTransient<WorkOrdersBoardForm>();
            })
            .Build();
        
        var form = host.Services.GetRequiredService<WorkOrdersBoardForm>();
        Application.Run(form);
    }
}