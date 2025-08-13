using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Solution.Data;
using Solution.Data.Repositories;
using Solution.ViewModels;

namespace Solution;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(cfg =>
            {
                cfg.SetBasePath(AppContext.BaseDirectory)
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((ctx, services) =>
            {
                services.AddDbContextFactory<MesLiteDbContext>(opt =>
                    opt.UseSqlServer(
                        ctx.Configuration.GetConnectionString("SqlServer"),
                        sql => sql.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(2),
                            errorNumbersToAdd: null
                        )));
                services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();

                services.AddTransient<WorkOrdersViewModel>();
                services.AddTransient<MainWindow>(sp =>
                    new MainWindow(sp.GetRequiredService<WorkOrdersViewModel>()));
            })
            .Build();

        await _host.StartAsync();

        var main = _host.Services.GetRequiredService<MainWindow>();
        main.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null) await _host.StopAsync();
        _host?.Dispose();
        base.OnExit(e);
    }
}