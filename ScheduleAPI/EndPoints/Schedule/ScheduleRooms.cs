using System.Collections.Frozen;
using System.ComponentModel;
using System.Globalization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ScheduleAPI.Infrastructure;
using ServerServices.LectorSchedule;

namespace ScheduleAPI.EndPoints.Schedule;

internal static class ScheduleRooms
{
    public static RouteGroupBuilder MapScheduleRoomsApi(this RouteGroupBuilder group, string url = "rooms", params string[] tags)
    {
        var rooms_group = group
                .MapGroup(url)
                .WithTags(tags is [] ? ["ScheduleRooms"] : tags)
                .WithOpenApi();

        rooms_group.MapGet("id/{RoomId}", GetRoomScheduleById);
        rooms_group.MapGet("id/{RoomId}/now", GetRoomScheduleByIdNow);
        rooms_group.MapGet("id/{RoomId}/today", GetRoomScheduleByIdNowAndFuture);
        rooms_group.MapGet("id/{RoomId}/now/future", GetRoomScheduleByIdToday);
        rooms_group.MapGet("id/{RoomId}/today/future", GetRoomScheduleByIdTodayAndFuture);

        rooms_group.MapGet("name/{RoomName}", GetRoomScheduleByName);
        rooms_group.MapGet("name/{RoomName}/now", GetRoomScheduleByNameNow);
        rooms_group.MapGet("name/{RoomName}/now/future", GetRoomScheduleByNameNowAndFuture);
        rooms_group.MapGet("name/{RoomName}/today", GetRoomScheduleByNameToday);
        rooms_group.MapGet("name/{RoomName}/today/future", GetRoomScheduleByNameTodayAndFuture);

        rooms_group.MapGet("id/{RoomId}.ics", GetRoomCalendarById);
        rooms_group.MapGet("name/{RoomName}.ics", GetRoomCalendarByName);

        rooms_group.MapGet("free", GetFreeRooms);

        return group;
    }

    private static async Task<Results<Ok<ScheduleRoom>, NotFound<string>>> GetRoomScheduleById(
        Guid RoomId,
        bool? aggregate,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetRoomScheduleAsync(RoomId, cancel) is { } schedule)
            return schedule.Ok();

        return TypedResults.NotFound($"Сервису не известна аудитория с идентификатором {RoomId}");
    }

    private static async Task<Results<Ok<ScheduleRoomLesson?>, NotFound<string>>> GetRoomScheduleByIdNow(
        Guid RoomId,
        DateTime? now,
        bool? aggregate,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetRoomScheduleAsync(RoomId, cancel) is { } schedule)
            return schedule.Lessons.GetTimeLessons(now ?? DateTime.Now).FirstOrDefault().Ok()!;

        return TypedResults.NotFound($"Сервису не известна аудитория с идентификатором {RoomId}");
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleRoomLesson>>, NotFound<string>>> GetRoomScheduleByIdNowAndFuture(
        Guid RoomId,
        DateTime? now,
        bool? aggregate,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetRoomScheduleAsync(RoomId, cancel) is { } schedule)
            return schedule.Lessons.GetTimeLessons(now ?? DateTime.Now).Ok();

        return TypedResults.NotFound($"Сервису не известна аудитория с идентификатором {RoomId}");
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleRoomLesson>>, NotFound<string>>> GetRoomScheduleByIdToday(
        Guid RoomId,
        DateTime? now,
        bool? aggregate,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetRoomScheduleAsync(RoomId, cancel) is { } schedule)
            return schedule.Lessons.GetDayLessons(now ?? DateTime.Now).Ok();

        return TypedResults.NotFound($"Сервису не известна аудитория с идентификатором {RoomId}");
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleRoomLesson>>, NotFound<string>>> GetRoomScheduleByIdTodayAndFuture(
        Guid RoomId,
        DateTime? now,
        bool? aggregate,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetRoomScheduleAsync(RoomId, cancel) is { } schedule)
            return schedule.Lessons.GetDayFutureLessons(now ?? DateTime.Now).Ok();

        return TypedResults.NotFound($"Сервису не известна аудитория с идентификатором {RoomId}");
    }

    private static async Task<Results<Ok<ScheduleRoom>, NotFound<string>>> GetRoomScheduleByName(
        string RoomName,
        bool? aggregate,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetRoomScheduleAsync(RoomName, cancel) is { } schedule)
            return schedule.Ok();

        return TypedResults.NotFound($"Сервису не известна аудитория с названием {RoomName}");
    }

    private static async Task<Results<Ok<ScheduleRoomLesson?>, NotFound<string>>> GetRoomScheduleByNameNow(
        string RoomName,
        DateTime? now,
        bool? aggregate,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetRoomScheduleAsync(RoomName, cancel) is { } schedule)
            return schedule.Lessons.GetTimeLessons(now ?? DateTime.Now).FirstOrDefault().Ok()!;

        return TypedResults.NotFound($"Сервису не известна аудитория с названием {RoomName}");
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleRoomLesson>>, NotFound<string>>> GetRoomScheduleByNameNowAndFuture(
        string RoomName,
        DateTime? now,
        bool? aggregate,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetRoomScheduleAsync(RoomName, cancel) is { } schedule)
            return schedule.Lessons.GetTimeLessons(now ?? DateTime.Now).Ok();

        return TypedResults.NotFound($"Сервису не известна аудитория с названием {RoomName}");
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleRoomLesson>>, NotFound<string>>> GetRoomScheduleByNameToday(
        string RoomName,
        DateTime? now,
        bool? aggregate,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetRoomScheduleAsync(RoomName, cancel) is { } schedule)
            return schedule.Lessons.GetDayLessons(now ?? DateTime.Now).Ok();

        return TypedResults.NotFound($"Сервису не известна аудитория с названием {RoomName}");
    }

    private static async Task<Results<Ok<IEnumerable<ScheduleRoomLesson>>, NotFound<string>>> GetRoomScheduleByNameTodayAndFuture(
        string RoomName,
        DateTime? now,
        bool? aggregate,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetRoomScheduleAsync(RoomName, cancel) is { } schedule)
            return schedule.Lessons.GetDayFutureLessons(now ?? DateTime.Now).Ok();

        return TypedResults.NotFound($"Сервису не известна аудитория с названием {RoomName}");
    }

    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/calendar")]
    private static async Task<Results<PushStreamHttpResult, NotFound<string>>> GetRoomCalendarById(
        Guid RoomId,
        bool? aggregate,
        bool? alarm,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetRoomScheduleAsync(RoomId, cancel) is { } schedule)
            return TypedResults.Stream(stream =>
                {
                    schedule.ToCalendar(aggregate ?? false, alarm ?? false).SerializeTo(stream);
                    return Task.CompletedTask;
                },
                "text/calendar"/*, $"{GroupName}.isc"*/);

        return TypedResults.NotFound($"Сервису не известна аудитория с идентификатором {RoomId}");
    }

    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/calendar")]
    private static async Task<Results<PushStreamHttpResult, NotFound<string>>> GetRoomCalendarByName(
        string RoomName,
        bool? aggregate,
        bool? alarm,
        IScheduleRequester mai,
        CancellationToken cancel)
    {
        if (await mai.GetRoomScheduleAsync(RoomName, cancel) is { } schedule)
            return TypedResults.Stream(stream =>
                {
                    schedule.ToCalendar(aggregate ?? false, alarm ?? false).SerializeTo(stream);
                    return Task.CompletedTask;
                },
                "text/calendar"/*, $"{GroupName}.isc"*/);

        return TypedResults.NotFound($"Сервису не известна аудитория с названием {RoomName}");
    }

    private static async Task<Results<Ok<FreeRooms>, BadRequest<string>>> GetFreeRooms(
        [DefaultValue("24Б-620,24Б-621,24Б-623,24Б-624,24Б-710")] string Rooms, 
        [DefaultValue("Четверг")] string Day, 
        [DefaultValue("13:00")] string Time,
        IScheduleRequester mai,
        CancellationToken Cancel)
    {
        if (!TimeOnly.TryParse(Time, out var time))
            return TypedResults.BadRequest($"Значение параметра Time:\"{Time}\" не является форматом времени HH:mm:ss");

        if (!__DayNames.Value.TryGetValue(Day.Trim(), out var day))
            return TypedResults.BadRequest($"Неверно указано название дня недели {day}");

        if (Rooms.Trim().Split((char[]?)[',', ';', ' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries) is not { Length: > 0 } rooms)
            return TypedResults.BadRequest("Не указаны аудитории");

        var free_rooms = await mai.GetFreeRoomsAsync(rooms, day, time, Cancel);
        return free_rooms.Ok();
    }

    private static readonly Lazy<FrozenDictionary<string, DayOfWeek>> __DayNames = new(() =>
    {
        var result = new Dictionary<string, DayOfWeek>(7 * 2 * 2);

        var inv_culture = CultureInfo.InvariantCulture.DateTimeFormat;
        var rus_culture = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat;
        
        foreach(DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            result.Add(rus_culture.GetDayName(day), day);
            result.Add(rus_culture.GetAbbreviatedDayName(day), day);
            result.Add(inv_culture.GetDayName(day), day);
            result.Add(inv_culture.GetAbbreviatedDayName(day), day);
        }

        return result.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }, LazyThreadSafetyMode.PublicationOnly);
}

file static class LessonsSelector
{
    public static IEnumerable<ScheduleRoomLesson> GetTimeLessons(this IEnumerable<ScheduleRoomLesson> lessons, DateTime now)
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

    public static IEnumerable<ScheduleRoomLesson> GetDayLessons(this IEnumerable<ScheduleRoomLesson> lessons, DateTime now)
    {
        var now_date = now.Date;
        foreach (var lesson in lessons)
            if (lesson.Start.Date == now_date)
                yield return lesson;
    }

    public static IEnumerable<ScheduleRoomLesson> GetDayFutureLessons(this IEnumerable<ScheduleRoomLesson> lessons, DateTime now)
    {
        foreach (var lesson in lessons)
            if (now.Date < lesson.End.Date)
                yield return lesson;
    }
}