using System.Text.Json.Serialization;
using Dep406.ScheduleAPI;
using Microsoft.Extensions.DependencyInjection;

using Refit;


namespace ScheduleTelegramBot.ScheduleAPI;

public static class Registrator
{
    public static IServiceCollection AddMAISchedule(this IServiceCollection services, string url) => services
        .AddRefitClient<ILectorSchedule>(new()
        {
            //ContentSerializer = new SystemTextJsonContentSerializer(ScheduleSerializerContext.Default.Options)
        })
        .ConfigureHttpClient(http => http.BaseAddress = new(url))
        .Services;
}

[JsonSerializable(typeof(ScheduleLector))]
[JsonSerializable(typeof(SchedultLectorLesson))]
[JsonSerializable(typeof(LectorInfo))]
internal partial class ScheduleSerializerContext : JsonSerializerContext;
