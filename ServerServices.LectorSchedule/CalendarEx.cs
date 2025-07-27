using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

using Calendar = Ical.Net.Calendar;

namespace ServerServices.LectorSchedule;

public static class CalendarEx
{
    public const string TimeZoneMoscow = "Europe/Moscow";

    public static Calendar New()
    {
        var time_zone = new VTimeZone
        {
            TzId = TimeZoneMoscow,
            //Properties =
            //{
            //    new CalendarProperty("X-LIC-LOCATION", TimeZoneMoscow),
            //},
            Location = TimeZoneMoscow,
            TimeZoneInfos =
            {
                new("STANDARD")
                {
                    TZOffsetFrom = new("+0300"),
                    TZOffsetTo = new("+0300"),
                    DtStart = new CalDateTime(1970, 1, 1),
                    TimeZoneName = "GMT+3",
                }
            }
        };
        //var time_zone = VTimeZone.FromDateTimeZone(TimeZoneMoscow);

        var calendar = new Calendar
        {
            TimeZones = { time_zone },
            ProductId = "-//MAI-dep406//MAI Dep406 1.01//RU",
            Properties =
            {
                new CalendarProperty("CALSCALE", "GREGORIAN"),
                new CalendarProperty("METHOD", "PUBLISH"),
                new CalendarProperty("X-WR-TIMEZONE", TimeZoneMoscow),
            }
        };

        return calendar;
    }
}
