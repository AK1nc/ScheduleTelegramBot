using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace ServerServices.LectorSchedule;

public interface IScheduleRequester
{
    Task<ScheduleLector> GetLectorScheduleByIdAsync(Guid id, bool UseCache = true, CancellationToken Cancel = default);
    Task<ScheduleLector?> GetLectorScheduleByIdAsync(string id, bool UseCache = true, CancellationToken Cancel = default);
    Task<ScheduleLector?> GetLectorScheduleByNameAsync(string Name, bool UseCache = true, CancellationToken Cancel = default);
    Task<ScheduleStudent?> GetStudentScheduleAsync(string GroupName, bool UseCache = true, CancellationToken Cancel = default);
    Task<ScheduleRoom?> GetRoomScheduleAsync(string RoomName, CancellationToken Cancel = default);
    Task<ScheduleRoom?> GetRoomScheduleAsync(Guid RoomId, CancellationToken Cancel = default);
    Task<FreeRooms> GetFreeRoomsAsync(IEnumerable<string> Rooms, DayOfWeek Day, TimeOnly Time, CancellationToken Cancel = default);
}

public class JsonScheduleRequester(
    HttpClient http,
    ILectorsStore store,
    IScheduleCache cache,
    ILogger<JsonScheduleRequester> log)
    : IScheduleRequester
{
    private static readonly JsonSerializerOptions __LectorMAIOptions = ScheduleLector.GetJsonOptions();

    private async ValueTask<ScheduleLector?> GetLectorScheduleBaseAsync(Guid id, bool UseCache, CancellationToken Cancel)
    {
        if (UseCache && await cache.GetLectorScheduleAsync(id, Cancel).ConfigureAwait(false) is { } schedule)
            return schedule;

        var uri = ScheduleLector.GetAddress(id);
        var timer = Stopwatch.StartNew();
        try
        {
            schedule = await http
                .GetFromJsonAsync<ScheduleLector>(uri, __LectorMAIOptions, Cancel)
                .ConfigureAwait(false);

            if (schedule is null)
            {
                log.LogWarning("Ошибка десериализации данных расписания лектора id {LectorId}", id);
                return null;
            }

            log.LogTrace("Загрузка данных расписания лектора id {LectorId} {LectorName} выполнена успешно за {time} ms", id, schedule.LectorName, timer.Elapsed);

        }
        catch (HttpRequestException error)
        {
            log.LogWarning("Ошибка сетевого подключения {ErrorMessage}: {error}", error.Message, error);
            return null;
        }

        await cache.SetLectorScheduleAsync(id, schedule, Cancel).ConfigureAwait(false);

        return schedule;
    }

    public async Task<ScheduleLector> GetLectorScheduleByIdAsync(Guid id, bool UseCache, CancellationToken Cancel)
    {
        if (await GetLectorScheduleBaseAsync(id, UseCache, Cancel).ConfigureAwait(false) is not { } schedule)
            throw new InvalidOperationException();

        await store.AddLectorAsync(schedule.LectorName, id, Cancel).ConfigureAwait(false);
        return schedule is { Id: null } 
            ? schedule with { Id = id }
            : schedule;
    }

    public async Task<ScheduleLector?> GetLectorScheduleByIdAsync(string id, bool UseCache, CancellationToken Cancel)
    {
        var guid = Guid.Parse(id);
        var schedule = await GetLectorScheduleByIdAsync(guid, UseCache, Cancel).ConfigureAwait(false);
        return schedule is { Id: null }
            ? schedule with { Id = guid }
            : schedule;
    }

    public async Task<ScheduleLector?> GetLectorScheduleByNameAsync(string Name, bool UseCache, CancellationToken Cancel)
    {
        if (await store.GetIdAsync(Name, Cancel).ConfigureAwait(false) is not { } id) 
            return null;

        var schedule = await GetLectorScheduleBaseAsync(id, UseCache, Cancel).ConfigureAwait(false);
        return schedule is { Id: null } 
            ? schedule with { Id = id } 
            : schedule;
    }

    private static readonly JsonSerializerOptions __StudentOptions = ScheduleStudent.GetJsonOptions();

    public async Task<ScheduleStudent?> GetStudentScheduleAsync(string GroupName, bool UseCache, CancellationToken Cancel)
    {
        if (UseCache && await cache.GetStudentScheduleAsync(GroupName, Cancel).ConfigureAwait(false) is { } schedule)
            return schedule;

        var uri = ScheduleStudent.GetAddress(GroupName);

        var timer = Stopwatch.StartNew();
        try
        {
            log.LogInformation("Запрос данных расписания для группы {GroupName} {uri}", GroupName, uri);
            var response = await http.GetAsync(uri, Cancel).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error_message = await response.Content.ReadAsStringAsync(Cancel).ConfigureAwait(false);
                log.LogInformation(
                    "При запросе расписания группы {GroupName} {uri} получен некорректный статусный код {StatusCode}: {msg}",
                    GroupName, uri, response.StatusCode,
                    error_message);

                return null;
            }

            schedule = await response
                .Content
                .ReadFromJsonAsync<ScheduleStudent>(__StudentOptions, Cancel)
                .ConfigureAwait(false);

            if (schedule is null)
            {
                log.LogWarning("Ошибка десериализации данных расписания студенческой группы {GroupName}", GroupName);
                return null;
            }

            log.LogTrace("Загрузка данных расписания студенческой группы {GroupName} выполнена успешно за {time} ms", GroupName, timer.Elapsed);
        }
        catch (HttpRequestException error)
        {
            log.LogWarning("Ошибка сетевого подключения {ErrorMessage}: {error}", error.Message, error);
            return null;
        }

        await cache.SetStudentScheduleAsync(GroupName, schedule, Cancel).ConfigureAwait(false);

        await store.AddLectorsAsync(schedule.Lectors, Cancel).ConfigureAwait(false);
        return schedule;
    }

    public async Task<ScheduleRoom?> GetRoomScheduleAsync(string RoomName, CancellationToken Cancel)
    {
        var students_lessons_task = cache.EnumStudentsSchedulesAsync(Cancel)
            .SelectManyAsync(s => s.Lessons, Cancel)
            .WhereAsync(l => l.Rooms.Values.Any(RoomName, StringComparison.OrdinalIgnoreCase), Cancel)
            .ToArrayAsync(Cancel);

        var lectors_lessons_task = cache.EnumLectorsSchedulesAsync(Cancel)
            .SelectManyAsync(l => l.Lessons, Cancel)
            .WhereAsync(l => l.Rooms.Values.Any(RoomName), Cancel)
            .ToArrayAsync(Cancel);

        var room_id_task = store.GetRoomId(RoomName, Cancel);

        await Task.WhenAll(students_lessons_task, lectors_lessons_task, room_id_task).ConfigureAwait(false);

        var students_lessons = await students_lessons_task;
        var lectors_lessons = await lectors_lessons_task;

        if (students_lessons.Length == 0 || lectors_lessons.Length == 0)
            return null;

        var lessons = students_lessons.Join(
            lectors_lessons,
            s => (s.Name, s.Start),
            l => (l.Name, l.Start),
            (s, l) => new ScheduleRoomLesson(s.Name, s.Start, s.Duration, s.Lectors, l.LessonType, l.Groups)
        );

        if (await room_id_task is not { } room_id)
            room_id = students_lessons[0].Rooms.First(r => r.Value == RoomName).Key;

        var room_schedule = new ScheduleRoom(RoomName, room_id, DateTime.Now, lessons.ToArray());
        return room_schedule;
    }

    public async Task<ScheduleRoom?> GetRoomScheduleAsync(Guid RoomId, CancellationToken Cancel)
    {
        var students_lessons_task = cache.EnumStudentsSchedulesAsync(Cancel)
            .SelectManyAsync(s => s.Lessons, Cancel)
            .WhereAsync(l => l.Rooms.ContainsKey(RoomId), Cancel)
            .ToArrayAsync(Cancel);

        var lectors_lessons_task = cache.EnumLectorsSchedulesAsync(Cancel)
            .SelectManyAsync(l => l.Lessons, Cancel)
            .WhereAsync(l => l.Rooms.ContainsKey(RoomId), Cancel)
            .ToArrayAsync(Cancel);

        var room_name_task = store.GetRoomName(RoomId, Cancel);

        await Task.WhenAll(students_lessons_task, lectors_lessons_task, room_name_task).ConfigureAwait(false);

        var students_lessons = await students_lessons_task;
        var lectors_lessons = await lectors_lessons_task;

        if (students_lessons.Length == 0 || lectors_lessons.Length == 0)
            return null;

        var lessons = students_lessons.Join(
            lectors_lessons,
            s => (s.Name, s.Start),
            l => (l.Name, l.Start),
            (s, l) => new ScheduleRoomLesson(s.Name, s.Start, s.Duration, s.Lectors, l.LessonType, l.Groups)
        );

        if (await room_name_task is not { } room_name)
            room_name = students_lessons[0].Rooms[RoomId];

        var room_schedule = new ScheduleRoom(room_name, RoomId, DateTime.Now, lessons.ToArray());
        return room_schedule;
    }

    public async Task<FreeRooms> GetFreeRoomsAsync(IEnumerable<string> Rooms, DayOfWeek Day, TimeOnly Time, CancellationToken Cancel)
    {
        if (Rooms.GetHashSet() is not { Count: > 0 } rooms)
            return new();

        var students_lessons = cache.EnumStudentsSchedulesAsync(Cancel);

        var free_rooms = rooms.ToHashSet();
        var used_rooms = new Dictionary<string, IReadOnlyCollection<ScheduleStudentLesson>>(rooms.Count);

        await foreach (var group in students_lessons.WithCancellation(Cancel).ConfigureAwait(false))
            foreach (var lesson in group.Lessons)
            {
                var start = lesson.Start;
                if (start.DayOfWeek != Day)
                    continue;
                if (start.ToTimeOnly() > Time || lesson.End.ToTimeOnly() < Time)
                    continue;

                foreach (var room in lesson.Rooms.Values)
                    if (rooms.Contains(room))
                    {
                        if (!used_rooms.TryGetValue(room, out var room_lessons))
                            used_rooms.Add(room, room_lessons = new List<ScheduleStudentLesson>());

                        ((List<ScheduleStudentLesson>)room_lessons).Add(lesson);

                        if (free_rooms.Remove(room) && rooms.Count == 0)
                            return new()
                            {
                                Free = [],
                                Used = used_rooms,
                            };
                    }
            }

        return new()
        {
            Free = free_rooms,
            Used = used_rooms,
        };
    }
}
