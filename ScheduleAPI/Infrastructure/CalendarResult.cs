using Ical.Net;
using ServerServices.LectorSchedule;

namespace ScheduleAPI.Infrastructure;

public class CalendarResult(Calendar calendar) : IResult
{
    public Task ExecuteAsync(HttpContext context)
    {
        context.Response.ContentType = "text/calendar";
        calendar.SerializeTo(context.Response.Body);
        return context.Response.CompleteAsync();
    }
}
