using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

using Calendar = Ical.Net.Calendar;

namespace ServerServices.LectorSchedule;

public static partial class ScheduleLectorEx
{
    [GeneratedRegex("(?<=.+-).*(?=-.+)", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex GetGroupNumberRegex();

    public static Calendar ToCalendar(this ScheduleLector schedule, bool AggregateLessons, bool Alarm)
    {
        var calendar = CalendarEx.New();

        ScheduleLectorLesson? last_lesson = null;

        var lessons_groups = EnumLessonsSeries(AggregateLessons ? schedule.LessonsCombined : schedule.Lessons);

        var group_name_regex = GetGroupNumberRegex();

        foreach (var (lesson, count, step) in lessons_groups)
        {
            var pairs_count = (int)(lesson.Duration.TotalHours / 1.5);
            var pairs_str = $" {pairs_count}п.";

            var groups = string.Join(',', lesson.Groups);
            var groups_short = string.Join(',', lesson.Groups.Select(g => group_name_regex.Match(g).Value));
            var location = string.Join("; ", lesson.Rooms.Values);

            RecurrencePattern repeat_rule = step % 7 is (> 0 and var weeks_step)
                ? new(FrequencyType.Weekly, weeks_step) { Count = count }
                : new(FrequencyType.Daily, step) { Count = count };

            var @event = new CalendarEvent
            {
                Summary = AggregateLessons
                    ? $"[{string.Join(',', lesson.LessonType)}{pairs_str}]{lesson.Name} ({groups_short})"
                    : $"[{string.Join(',', lesson.LessonType)}]{lesson.Name} ({groups_short})",
                Start = new CalDateTime(lesson.Start, CalendarEx.TimeZoneMoscow),
                Duration = lesson.Duration,
                Location = location,
                Description = $"Группы: {groups}",
                RecurrenceRules = { repeat_rule }
            };

            foreach (var group in lesson.Groups)
            {
                var attendee = new Attendee
                {
                    CommonName = group,
                    Value = ScheduleStudent.GetAddress(group),
                };
                @event.Attendees.Add(attendee);
            }

            if (Alarm)
                if (last_lesson is null || (lesson.Start - last_lesson.End).TotalHours > 10)
                {
                    Alarm alarm_first = new()
                    {
                        Action = "DISPLAY",
                        Description = "This is an event reminder",
                        Properties = { new CalendarProperty("TRIGGER", "-P0DT2H0M0S") },
                    };

                    Alarm alarm_second = new()
                    {
                        Action = "DISPLAY",
                        Description = "This is an event reminder",
                        Properties = { new CalendarProperty("TRIGGER", "-P0DT0H10M0S") },
                    };

                    @event.Alarms.Add(alarm_first);
                    @event.Alarms.Add(alarm_second);
                }

            last_lesson = lesson;

            calendar.Events.Add(@event);
        }

        return calendar;
    }

    private static IEnumerable<(ScheduleLectorLesson Lesson, int Count, int DaysStep)> EnumLessonsSeries(
        this IEnumerable<ScheduleLectorLesson> Lessons)
    {
        var lessons = Lessons.ToArray();

        var week_lessons = lessons.GroupBy(l => (l.Start.DayOfWeek, l.Start.TimeOfDay, l.Name));

        foreach (var lessons_group in week_lessons)
        {
            ScheduleLectorLesson? first_lesson = null;
            ScheduleLectorLesson? last_lesson = null;

            var (lesson_day, lesson_start, lesson_name) = lessons_group.Key;
            var series_count = 0;
            var days_period = 0;
            foreach (var lesson in lessons_group)
            {
                if (first_lesson is null)
                {
                    first_lesson = last_lesson = lesson;
                    series_count = 1;
                    continue;
                }

                series_count++;
                var days_delta = (int)(lesson.Start - last_lesson!.Start).TotalDays;
                last_lesson = lesson;

                if (days_period == 0)
                {
                    days_period = days_delta;
                    continue;
                }

                if (days_delta == days_period) continue;

                //var day_name = DateTimeFormatInfo.CurrentInfo.GetDayName(first_lesson.Start.DayOfWeek);
                yield return (first_lesson, series_count, days_period);
                first_lesson = null;
            }

            if (first_lesson is not null)
            {
                //var day_name = DateTimeFormatInfo.CurrentInfo.GetDayName(first_lesson.Start.DayOfWeek);
                yield return (first_lesson, series_count, days_period);
            }
        }
    }

    public static Calendar ToCalendar(this ScheduleStudent schedule, bool AggregateLessons, bool Alarm)
    {
        var calendar = CalendarEx.New();

        ScheduleStudentLesson? last_lesson = null;
        foreach (var lesson in AggregateLessons ? schedule.LessonsCombined : schedule.Lessons)
        {
            var pairs_count = (int)(lesson.Duration.TotalHours / 1.5);
            var pairs_str = $" {pairs_count}п.";

            var lectors = string.Join(',', lesson.Lectors.Values);
            var location = string.Join("; ", lesson.Rooms.Values.Select(r => r.StartsWith("24Б") ? $"Волоколамское шоссе, 4к24 {r}" : r));

            var @event = new CalendarEvent
            {
                Summary = AggregateLessons
                    ? $"[{string.Join(',', lesson.Types.Keys)}{pairs_str}]{lesson.Name} ({lectors})"
                    : $"[{string.Join(',', lesson.Types.Keys)}]{lesson.Name} ({lectors})",
                Start = new CalDateTime(lesson.Start.ToUniversalTime()),
                Duration = lesson.Duration,
                Location = location,
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

    public static void Serialize(this Calendar calendar, string FilePath)
    {
        var serializer = new CalendarSpecialSerializer();

        using var stream = File.Create(FilePath);
        serializer.SerializeToStream(calendar, stream, Encoding.UTF8);
    }

    public static string SerializeToString(this Calendar calendar)
    {
        var serializer = new CalendarSpecialSerializer();
        var result = serializer.SerializeToString(calendar);
        return result;
    }

    public static void SerializeTo(this Calendar calendar, Stream stream)
    {
        var serializer = new CalendarSpecialSerializer();
        serializer.SerializeToStream(calendar, stream, Encoding.UTF8);
    }

    public static async Task SerializeToAsync(this Calendar calendar, Stream stream, CancellationToken Cancel = default)
    {
        var serializer = new CalendarSpecialSerializer();
        await serializer.SerializeToStreamAsync(calendar, stream, Encoding.UTF8, Cancel).ConfigureAwait(false);
    }
}
