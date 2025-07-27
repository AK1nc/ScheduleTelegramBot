namespace ScheduleAPI.Infrastructure.Middleware;

public class PrintRequestMiddleware(RequestDelegate next, ILogger<PrintRequestMiddleware> log)
{
    public async Task InvokeAsync(HttpContext context)
    {
        log.LogInformation("Request from {ip}", context.Connection.RemoteIpAddress);
        await next(context);
    }
}