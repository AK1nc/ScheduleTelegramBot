using System.ComponentModel;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using ScheduleAPI.Infrastructure;

using ServerServices.LectorSchedule;

using static Microsoft.AspNetCore.Http.TypedResults;

namespace ScheduleAPI.EndPoints.Schedule;

internal static class ScheduleLectors
{
    public static RouteGroupBuilder MapScheduleLectorsApi(this RouteGroupBuilder group, string url = "lector", params string[] tags)
    {
        var lectors_group = group
            .MapGroup(url)
            .WithTags(tags is [] ? ["ScheduleLectors"] : tags);

        lectors_group.MapGet("{lector}", GetLectorsSchedule);
        lectors_group.MapGet("{lector}/now", GetLectorsScheduleNow);
        lectors_group.MapGet("{lector}/now/future", GetLectorsScheduleNowAndFuture);
        lectors_group.MapGet("{lector}/today", GetLectorsScheduleNow);
        lectors_group.MapGet("{lector}/today/future", GetLectorsScheduleNowAndFuture);
        lectors_group.MapGet("{lector}.ics", GetLectorsCalendar);

        lectors_group.MapGet("id/{id:guid}", GetLectorsScheduleById);
        lectors_group.MapGet("id/{id:guid}/now", GetLectorsScheduleNowById);
        lectors_group.MapGet("id/{id:guid}/now/future", GetLectorsScheduleNowAndFutureById);
        lectors_group.MapGet("id/{id:guid}/today", GetLectorsScheduleTodayById);
        lectors_group.MapGet("id/{id:guid}/today/future", GetLectorsScheduleTodayAndFutureById);

        lectors_group.MapGet("name/{name}", GetLectorsScheduleByName);
        lectors_group.MapGet("name/{name}/now", GetLectorsScheduleNowByName);
        lectors_group.MapGet("name/{name}/now/future", GetLectorsScheduleNowAndFutureByName);
        lectors_group.MapGet("name/{name}/today", GetLectorsScheduleTodayByName);
        lectors_group.MapGet("name/{name}/today/future", GetLectorsScheduleTodayAndFutureByName);


        lectors_group.MapGet("id/{id:guid}.ics", GetLectorsCalendarById);
        lectors_group.MapGet("name/{name}.ics", GetLectorsCalendarByName);

        return group;
    }

    private static async Task<Results<Ok<ScheduleLector>, NotFound<string>>> GetLectorsSchedule(
        string lector,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (Guid.TryParse(lector, out var id))
            return await mai.GetLectorScheduleByIdAsync(id, UseCache ?? true, cancel) is { } schedule_id
                ? Ok(schedule_id)
                : NotFound($"Сервису не известен преподаватель с идентификатором {lector}");

        return await mai.GetLectorScheduleByNameAsync(lector, UseCache ?? true, cancel) is { } schedule_name
            ? Ok(schedule_name)
            : NotFound($"Сервису не известен преподаватель с именем {lector}");
    }

    private static async Task<Results<Ok<ScheduleLectorLesson?>, BadRequest<string>, NotFound<string>>> GetLectorsScheduleNow(
        string lector,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (Guid.TryParse(lector, out var id))
            return await mai.GetLectorScheduleByIdAsync(id, UseCache ?? true, cancel) is { } schedule_id
                ? schedule_id.Lessons.GetTimeLessons(now ?? DateTime.Now).FirstOrDefault().Ok()!
                : NotFound($"Сервису не известен преподаватель с идентификатором {lector}");

        return await mai.GetLectorScheduleByNameAsync(lector, UseCache ?? true, cancel) is { } schedule_name
            ? schedule_name.Lessons.GetTimeLessons(now ?? DateTime.Now).FirstOrDefault().Ok()!
            : NotFound($"Сервису не известен преподаватель с именем {lector}");
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleLectorLesson>>, BadRequest<string>, NotFound<string>>> GetLectorsScheduleNowAndFuture(
        string lector,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (Guid.TryParse(lector, out var id))
            return await mai.GetLectorScheduleByIdAsync(id, UseCache ?? true, cancel) is { } schedule_id
                ? schedule_id.Lessons.GetTimeLessons(now ?? DateTime.Now).Ok()!
                : NotFound($"Сервису не известен преподаватель с идентификатором {lector}");

        return await mai.GetLectorScheduleByNameAsync(lector, UseCache ?? true, cancel) is { } schedule_name
            ? schedule_name.Lessons.GetTimeLessons(now ?? DateTime.Now).Ok()!
            : NotFound($"Сервису не известен преподаватель с именем {lector}");
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleLectorLesson>>, BadRequest<string>, NotFound<string>>> GetLectorsScheduleToday(
        string lector,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (Guid.TryParse(lector, out var id))
            return await mai.GetLectorScheduleByIdAsync(id, UseCache ?? true, cancel) is { } schedule_id
                ? schedule_id.Lessons.GetDayLessons(now ?? DateTime.Now).Ok()!
                : NotFound($"Сервису не известен преподаватель с идентификатором {lector}");

        return await mai.GetLectorScheduleByNameAsync(lector, UseCache ?? true, cancel) is { } schedule_name
            ? schedule_name.Lessons.GetDayLessons(now ?? DateTime.Now).Ok()!
            : NotFound($"Сервису не известен преподаватель с именем {lector}");
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleLectorLesson>>, BadRequest<string>, NotFound<string>>> GetLectorsScheduleTodayAndFuture(
        string lector,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (Guid.TryParse(lector, out var id))
            return await mai.GetLectorScheduleByIdAsync(id, UseCache ?? true, cancel) is { } schedule_id
                ? schedule_id.Lessons.GetDayFutureLessons(now ?? DateTime.Now).Ok()!
                : NotFound($"Сервису не известен преподаватель с идентификатором {lector}");

        return await mai.GetLectorScheduleByNameAsync(lector, UseCache ?? true, cancel) is { } schedule_name
            ? schedule_name.Lessons.GetDayFutureLessons(now ?? DateTime.Now).Ok()!
            : NotFound($"Сервису не известен преподаватель с именем {lector}");
    }

    private static async Task<Results<CalendarResult, NotFound<string>>> GetLectorsCalendar(
        string lector,
        [DefaultValue(false)] bool? aggregate,
        [DefaultValue(false)] bool? alarm,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel,
        ILogger<ScheduleLector> log)
    {
        log.LogInformation("Запрос календаря для лектора {str}. Объединять занятия:{AggregateLessons}",
            lector,
            aggregate ?? false);

        if (Guid.TryParse(lector, out var id))
            return await mai.GetLectorScheduleByIdAsync(id, UseCache ?? true, cancel) is { } schedule_id
                ? new CalendarResult(schedule_id.ToCalendar(aggregate ?? false, alarm ?? false))
                : NotFound($"Сервису не известен преподаватель с идентификатором {lector}");

        return await mai.GetLectorScheduleByNameAsync(lector, UseCache ?? true, cancel) is { } schedule_name
            ? new CalendarResult(schedule_name.ToCalendar(aggregate ?? false, alarm ?? false))
            : NotFound($"Сервису не известен преподаватель с именем {lector}");
    }

    private static async Task<Results<Ok<ScheduleLector>, NotFound<string>>> GetLectorsScheduleById(
        Guid id,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetLectorScheduleByIdAsync(id, UseCache ?? true, cancel) is { } schedule)
            return Ok(schedule);

        return NotFound($"Сервису не известен преподаватель с id {id}");
    }

    private static async Task<Results<Ok<ScheduleLectorLesson?>, BadRequest<string>, NotFound<string>>> GetLectorsScheduleNowById(
        Guid id,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetLectorScheduleByIdAsync(id, UseCache ?? true, cancel) is not { } schedule)
            return NotFound($"Сервису не известен преподаватель с id {id}");

        return schedule.Lessons.GetTimeLessons(now ?? DateTime.Now).FirstOrDefault().Ok()!;
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleLectorLesson>>, BadRequest<string>, NotFound<string>>> GetLectorsScheduleNowAndFutureById(
        Guid id,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetLectorScheduleByIdAsync(id, UseCache ?? true, cancel) is not { } schedule)
            return NotFound($"Сервису не известен преподаватель с id {id}");

        return schedule.Lessons.GetTimeLessons(now ?? DateTime.Now).Ok()!;
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleLectorLesson>>, BadRequest<string>, NotFound<string>>> GetLectorsScheduleTodayById(
        Guid id,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetLectorScheduleByIdAsync(id, UseCache ?? true, cancel) is not { } schedule)
            return NotFound($"Сервису не известен преподаватель с id {id}");

        return schedule.Lessons.GetDayLessons(now ?? DateTime.Now).Ok()!;
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleLectorLesson>>, BadRequest<string>, NotFound<string>>> GetLectorsScheduleTodayAndFutureById(
        Guid id,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetLectorScheduleByIdAsync(id, UseCache ?? true, cancel) is not { } schedule)
            return NotFound($"Сервису не известен преподаватель с id {id}");

        return schedule.Lessons.GetDayFutureLessons(now ?? DateTime.Now).Ok()!;
    }

    private static async Task<Results<Ok<ScheduleLector>, NotFound<string>>> GetLectorsScheduleByName(
        string name,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetLectorScheduleByNameAsync(name, UseCache ?? true, cancel) is { } schedule)
            return schedule.Ok();

        return NotFound($"Сервису не известен преподаватель с именем {name}");
    }

    private static async Task<Results<Ok<ScheduleLectorLesson?>, BadRequest<string>, NotFound<string>>> GetLectorsScheduleNowByName(
        string name,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetLectorScheduleByNameAsync(name, UseCache ?? true, cancel) is not { } schedule)
            return NotFound($"Сервису не известен преподаватель с именем {name}");

        var lesson = schedule.Lessons.GetTimeLessons(now ?? DateTime.Now).FirstOrDefault();
        return lesson.Ok()!;
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleLectorLesson>>, BadRequest<string>, NotFound<string>>> GetLectorsScheduleNowAndFutureByName(
        string name,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetLectorScheduleByNameAsync(name, UseCache ?? true, cancel) is not { } schedule)
            return NotFound($"Сервису не известен преподаватель с именем {name}");

        return schedule.Lessons.GetTimeLessons(now ?? DateTime.Now).AsEnumerable().Ok();
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleLectorLesson>>, BadRequest<string>, NotFound<string>>> GetLectorsScheduleTodayByName(
        string name,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetLectorScheduleByNameAsync(name, UseCache ?? true, cancel) is not { } schedule)
            return NotFound($"Сервису не известен преподаватель с именем {name}");

        return schedule.Lessons.GetDayLessons(now ?? DateTime.Now).Ok();
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleLectorLesson>>, BadRequest<string>, NotFound<string>>> GetLectorsScheduleTodayAndFutureByName(
        string name,
        DateTime? now,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetLectorScheduleByNameAsync(name, UseCache ?? true, cancel) is not { } schedule)
            return NotFound($"Сервису не известен преподаватель с именем {name}");

        return schedule.Lessons.GetDayFutureLessons(now ?? DateTime.Now).Ok();
    }

    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/calendar")]
    private static async Task<Results<ContentHttpResult, NotFound<string>>> GetLectorsCalendarById(
        Guid id,
        [DefaultValue(false)] bool? aggregate,
        [DefaultValue(false)] bool? alarm,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        ILogger<ScheduleLector> log,
        CancellationToken cancel)
    {
        log.LogInformation("Запрос календаря для лектора id {str}. Объединять занятия:{AggregateLessons}, включить уведомления: {EnableAlarms}",
            id,
            aggregate ?? false,
            alarm ?? false);

        if (await mai.GetLectorScheduleByIdAsync(id, UseCache ?? true, cancel) is { } schedule)
            return Text(schedule.ToCalendar(aggregate ?? false, alarm ?? false).SerializeToString(), contentType: "text/calendar");

        return NotFound($"Сервису не известен преподаватель с id {id}");
    }

    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/calendar")]
    private static async Task<Results<ContentHttpResult, NotFound<string>>> GetLectorsCalendarByName(
        string name,
        [DefaultValue(false)] bool? aggregate,
        [DefaultValue(false)] bool? alarm,
        [DefaultValue(true)] bool? UseCache,
        IScheduleRequester mai,
        ILogger<ScheduleLector> log,
        CancellationToken cancel)
    {
        log.LogInformation("Запрос календаря для лектора {str}. Объединять занятия:{AggregateLessons}, включить уведомления: {EnableAlarms}",
            name,
            aggregate ?? false,
            alarm ?? false);

        if (await mai.GetLectorScheduleByNameAsync(name, UseCache ?? true, cancel) is { } schedule)
            return Text(schedule.ToCalendar(aggregate ?? false, alarm ?? false).SerializeToString(), contentType: "text/calendar");

        return NotFound($"Сервису не известен преподаватель с именем {name}");
    }
}

file static class LessonsSelector
{
    public static IEnumerable<ScheduleLectorLesson> GetTimeLessons(this IEnumerable<ScheduleLectorLesson> lessons, DateTime now)
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

    public static IEnumerable<ScheduleLectorLesson> GetDayLessons(this IEnumerable<ScheduleLectorLesson> lessons, DateTime now)
    {
        var now_date = now.Date;
        foreach (var lesson in lessons)
            if (lesson.Start.Date == now_date)
                yield return lesson;
    }

    public static IEnumerable<ScheduleLectorLesson> GetDayFutureLessons(this IEnumerable<ScheduleLectorLesson> lessons, DateTime now)
    {
        foreach (var lesson in lessons)
            if (now.Date < lesson.End.Date)
                yield return lesson;
    }
}