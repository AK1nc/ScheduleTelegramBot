using Microsoft.AspNetCore.Mvc;

namespace ScheduleAPI.EndPoints;

internal static class SystemApi
{
    public static IEndpointRouteBuilder MapSystemApi(this IEndpointRouteBuilder route, string url = "system", params string[] tags)
    {
        route.MapGroup(url)
             .MapSystemApi()
             .WithTags(tags is [] ? ["System"] : tags)
             .WithOpenApi();

        return route;
    }

    public static RouteGroupBuilder MapSystemApi(this RouteGroupBuilder group)
    {
        group.MapGet("stop", static (string? reason, HttpContext context, IHostApplicationLifetime host, ILogger<Program> log) =>
            {
                if (reason != DateTime.UtcNow.AddHours(3).ToString("ddTHH")) 
                    return Results.Ok();

                log.LogWarning("Получен запрос на остановку сервиса. Источник {ip}", context.Connection.RemoteIpAddress);
                _ = Task.Delay(5000).ContinueWith(_ => host.StopApplication());

                return Results.Ok();
            })
            .WithSummary("Остановка сервиса")
            .WithDescription("Выполнить принудительную остановку работы сервиса")
            ;

        group.MapGet("check", static () => TypedResults.Ok(DateTimeOffset.UtcNow.AddHours(3)))
            .WithSummary("Проверка работоспособности сервиса")
            .WithDescription("Выполнить проверку работоспособности сервиса")
            ;

        return group;
    }
}
