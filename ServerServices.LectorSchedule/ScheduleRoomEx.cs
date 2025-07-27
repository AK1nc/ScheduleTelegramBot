using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace ServerServices.LectorSchedule;

public static class ScheduleRoomEx
{
    public static Calendar ToCalendar(this ScheduleRoom schedule, bool AggregateLessons, bool Alarm)
    {
        var calendar = CalendarEx.New();

        ScheduleRoomLesson? last_lesson = null;
        foreach (var lesson in AggregateLessons ? schedule.LessonsCombined : schedule.Lessons)
        {
            var pairs_count = (int)(lesson.Duration.TotalHours / 1.5);
            var pairs_str = $" {pairs_count}п.";

            var lectors = string.Join(',', lesson.Lectors.Values);
            var @event = new CalendarEvent
            {
                Summary = AggregateLessons
                    ? $"[{string.Join(',', lesson.LessonType)}{pairs_str}]{lesson.Name} ({lectors})"
                    : $"[{string.Join(',', lesson.LessonType)}]{lesson.Name} ({lectors})",
                Start = new CalDateTime(lesson.Start, CalendarEx.TimeZoneMoscow),
                Duration = lesson.Duration,
                Location = schedule.Name,
                Description = $"Преподаватель: {lectors}",
            };

            foreach (var (lector_id, lector) in lesson.Lectors)
            {
                var attendee = new Attendee
                {
                    CommonName = lector,
                    Value = ScheduleLector.GetAddress(lector_id),
                };
                @event.Attendees.Add(attendee);
            }

            if (Alarm)
                if (last_lesson is null || (lesson.Start - last_lesson.End).TotalHours > 10)
                {
                    @event.Alarms.Add(new() { Duration = TimeSpan.FromHours(2), Repeat = 2 });
                    @event.Alarms.Add(new() { Duration = TimeSpan.FromMinutes(20) });
                }
            last_lesson = lesson;

            calendar.Events.Add(@event);
        }

        return calendar;
    }
}