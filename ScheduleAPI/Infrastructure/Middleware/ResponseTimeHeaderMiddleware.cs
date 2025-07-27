using System.Diagnostics;

namespace ScheduleAPI.Infrastructure.Middleware;

public class ResponseTimeHeaderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var timer = new ResponseTimeInfo(context.Response);

        context.Response.OnStarting(timer.SetHeaders);

        await next(context);
    }

    private class ResponseTimeInfo(HttpResponse response)
    {
        private readonly Stopwatch _Timer = Stopwatch.StartNew();

        public Task SetHeaders()
        {
            response.Headers["X-Response-Time-Milliseconds"] = _Timer.ElapsedMilliseconds.ToString();
            return Task.CompletedTask;
        }
    }
}