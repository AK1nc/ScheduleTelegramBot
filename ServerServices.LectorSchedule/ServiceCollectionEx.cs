using System.Data.Common;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;

using MathCore.Extensions.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

// ReSharper disable SettingNotFoundInConfiguration

namespace ServerServices.LectorSchedule;

public static class ServiceCollectionEx
{
    public static IServiceCollection AddScheduleServices(this IServiceCollection services) => services
        .AddScoped<ILectorsStore, InDbLectorsStore>()
        .AddKeyedScoped<IFileProvider>("schedule", (s, _) => new PhysicalFileProvider(Directory.CreateDirectory(s.GetRequiredService<IConfiguration>()["schedule:cache"] ?? "data/cache").FullName))
        .AddScoped<IScheduleCache, FileScheduleCache>()
        .AddTransient<ILectorsIndex, LectorsIndex>()
        .AddHttpClient<IScheduleRequester, JsonScheduleRequester>()
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .Services
    ;

    public static IServiceCollection AddScheduleServiceDB(this IServiceCollection services, IConfigurationSection cfg)
    {
        //IFileProvider f = new PhysicalFileProvider()

        var type = cfg["type"] ?? "sqlite";

        var connection_string_source = cfg.GetConnectionString(type);
        var string_builder = new DbConnectionStringBuilder { ConnectionString = connection_string_source };
        foreach (var (name, value) in cfg.GetSection("properties").GetChildren().Where(s => s.Value is not null))
            string_builder[name] = value;

        var connection_string_result = string_builder.ConnectionString;

        if (type.ContainsIgnoreCase("sqlite"))
        {
            var data_source = string_builder["Data Source"] as string ?? throw new InvalidOperationException("Не задано значение Data Source в строке подключения к БД");
            if (new FileInfo(data_source) is { Directory: { Exists: false } db_dir })
                db_dir.Create();

            services.AddDbContext<LectorScheduleDB>(opt => opt.UseSqlite(connection_string_result));
        }
        else if (type.ContainsIgnoreCase("sqlserver"))
            services.AddDbContext<LectorScheduleDB>(opt => opt.UseSqlServer(connection_string_result));
        else
            throw new InvalidOperationException($"Тип подключения к БД {type} не поддерживается");

        services.AddHostedService<ScheduleDbInitializer>();

        return services;
    }
}

file class ScheduleDbInitializer(IServiceProvider services) : IHostedService
{
    async Task IHostedService.StartAsync(CancellationToken cancel)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LectorScheduleDB>();
        await db.Database.EnsureCreatedAsync(cancel).ConfigureAwait(false);
    }

    Task IHostedService.StopAsync(CancellationToken cancel) => Task.CompletedTask;
}