using System.ComponentModel;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ScheduleAPI.Infrastructure;
using ServerServices.LectorSchedule;

namespace ScheduleAPI.EndPoints.Schedule;

internal static class ScheduleStudents
{
    public static RouteGroupBuilder MapScheduleStudentsApi(this RouteGroupBuilder group, string url = "students", params string[] tags)
    {
        var students = group
            .MapGroup(url)
            .WithTags(tags is [] ? ["ScheduleStudents"] : tags)
            .WithOpenApi();

        students.MapGet("{GroupName:length(11,20)}", GetStudentSchedule);
        students.MapGet("{GroupName:length(11,20)}/now", GetStudentScheduleNow);
        students.MapGet("{GroupName:length(11,20)}/now/future", GetStudentScheduleNowAndFuture);
        students.MapGet("{GroupName:length(11,20)}/today", GetStudentScheduleToday);
        students.MapGet("{GroupName:length(11,20)}/today/future", GetStudentScheduleTodayAndFuture);

        students.MapGet("/calendar/{GroupName:length(11,20)}.isc", GetStudentCalendar);

        return group;
    }

    private static async Task<Results<Ok<ScheduleStudent>, NotFound<string>>> GetStudentSchedule(
        string GroupName,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetStudentScheduleAsync(GroupName, UseCache ?? true, cancel) is { } schedule)
            return TypedResults.Ok(schedule);

        return TypedResults.NotFound($"Сервису не известна группа с названием {GroupName}");
    }

    private static async Task<Results<Ok<ScheduleStudentLesson?>, NotFound<string>>> GetStudentScheduleNow(
        string GroupName,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetStudentScheduleAsync(GroupName, UseCache ?? true, cancel) is { } schedule)
            return schedule.Lessons.GetTimeLessons(now ?? DateTime.Now).FirstOrDefault().Ok()!;

        return TypedResults.NotFound($"Сервису не известна группа с названием {GroupName}");
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleStudentLesson>>, NotFound<string>>> GetStudentScheduleNowAndFuture(
        string GroupName,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetStudentScheduleAsync(GroupName, UseCache ?? true, cancel) is { } schedule)
            return schedule.Lessons.GetTimeLessons(now ?? DateTime.Now).Ok();

        return TypedResults.NotFound($"Сервису не известна группа с названием {GroupName}");
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleStudentLesson>>, NotFound<string>>> GetStudentScheduleToday(
        string GroupName,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetStudentScheduleAsync(GroupName, UseCache ?? true, cancel) is { } schedule)
            return schedule.Lessons.GetDayLessons(now ?? DateTime.Now).Ok();

        return TypedResults.NotFound($"Сервису не известна группа с названием {GroupName}");
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleStudentLesson>>, NotFound<string>>> GetStudentScheduleTodayAndFuture(
        string GroupName,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetStudentScheduleAsync(GroupName, UseCache ?? true, cancel) is { } schedule)
            return schedule.Lessons.GetDayFutureLessons(now ?? DateTime.Now).Ok();

        return TypedResults.NotFound($"Сервису не известна группа с названием {GroupName}");
    }

    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/calendar")]
    private static async Task<IResult> GetStudentCalendar(
        string GroupName,
        [DefaultValue(false)] bool? aggregate,
        [DefaultValue(false)] bool? alarm,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        ILogger<ScheduleStudent> log,
        CancellationToken cancel)
    {
        log.LogInformation("Запрос календаря для группы {str}. Объединять занятия:{AggregateLessons}, включить уведомления: {EnableAlarms}",
            GroupName,
            aggregate ?? false,
            alarm ?? false);

        if (await mai.GetStudentScheduleAsync(GroupName, UseCache ?? true, cancel) is { } schedule)
            return TypedResults.Stream(stream =>
            {
                schedule.ToCalendar(aggregate ?? false, alarm ?? false).SerializeTo(stream);
                return Task.CompletedTask;
            },
            "text/calendar"/*, $"{GroupName}.isc"*/);

        return TypedResults.NotFound($"Сервису не известна группа с названием {GroupName}");
    }
}

file static class LessonsSelector
{
    public static IEnumerable<ScheduleStudentLesson> GetTimeLessons(this IEnumerable<ScheduleStudentLesson> lessons, DateTime now)
    {
        var now_date = now.Date;
        foreach (var lesson in lessons)
            if (lesson.End.Date == now_date)
            {
                var lesson_interval = lesson.Interval;
                if (lesson_interval.Check(now) || now < lesson_interval)
                    yield return lesson;
            }
    }

    public static IEnumerable<ScheduleStudentLesson> GetDayLessons(this IEnumerable<ScheduleStudentLesson> lessons, DateTime now)
    {
        var now_date = now.Date;
        foreach (var lesson in lessons)
            if (lesson.Start.Date == now_date)
                yield return lesson;
    }

    public static IEnumerable<ScheduleStudentLesson> GetDayFutureLessons(this IEnumerable<ScheduleStudentLesson> lessons, DateTime now)
    {
        foreach (var lesson in lessons)
            if (now.Date < lesson.End.Date)
                yield return lesson;
    }
}