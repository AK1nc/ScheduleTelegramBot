namespace ServerServices.LectorSchedule.Infrastructure;

internal static class DateTimeEx
{
    public static TimeOnly ToTimeOnly(this DateTime time) => new(time.Hour, time.Minute, time.Second, time.Millisecond, time.Microsecond);
}
