using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace ServerServices.LectorSchedule;

public interface ILectorsIndex
{
    Task<int> BuildByLectorIdAsync(Guid FirstLectorId, bool ClearIndex = false, bool ClearCache = false, CancellationToken Cancel = default);
    Task<int> BuildByGroupNameAsync(string GroupName, bool ClearIndex = false, bool ClearCache = false, CancellationToken Cancel = default);
    Task<bool> ClearIndexAsync(CancellationToken Cancel = default);
    Task<bool> ClearCacheAsync(CancellationToken Cancel = default);
}

public class LectorsIndex(
    IScheduleRequester requester,
    IScheduleCache cache,
    ILectorsStore store,
    ILogger<LectorsIndex> log)
    : ILectorsIndex
{
    public async Task<int> BuildByLectorIdAsync(
        Guid FirstLectorId, 
        bool ClearIndex, 
        bool ClearCache, 
        CancellationToken Cancel = default)
    {
        log.LogInformation("Обновление индекса лекторов по графу расписания начиная с лектора {LectorId}, удаление индекса: {ClearIndex}, удаление кеша: {ClearCache}", 
            FirstLectorId, ClearIndex, ClearCache);

        if (ClearCache || ClearIndex)
        {
            var clear_tasks = new List<Task<bool>>(2);

            if (ClearIndex) clear_tasks.Add(ClearIndexAsync(Cancel));
            if (ClearCache) clear_tasks.Add(ClearCacheAsync(Cancel));

            await Task.WhenAll(clear_tasks).ConfigureAwait(false);
        }

        var processed_lectors = new HashSet<Guid>();
        var processed_groups = new HashSet<string>();
        var processed_rooms = new HashSet<Guid>();

        var to_process = new Queue<Guid>(100000);
        to_process.Enqueue(FirstLectorId);

        var timer = Stopwatch.StartNew();
        while (true)
        {
            if (to_process.Count == 0) break;

            Cancel.ThrowIfCancellationRequested();

            var guid = to_process.Dequeue();

            if (!processed_lectors.Add(guid))
                continue;

            await ProcessLectorScheduleIdAsync(guid, Cancel).ConfigureAwait(false);

            log.LogTrace("Обновление индекса лекторов {LectorsCount}, групп {GroupsCount}, аудиторий {RoomsCount}. В очереди {ToProcessQueueLength}",
                processed_lectors.Count, processed_groups.Count, processed_rooms.Count, to_process.Count);
        }

        log.LogTrace("Обновление индекса лекторов завершено за {ElapsedTime}, записей лекторов {LectorsCount}, групп {GroupsCount}, аудиторий {RoomsCount}",
            timer.Elapsed, processed_lectors.Count, processed_groups.Count, processed_rooms.Count);

        return processed_lectors.Count;

        async Task ProcessLectorScheduleIdAsync(Guid guid, CancellationToken cancel)
        {
            var schedule = await requester.GetLectorScheduleByIdAsync(guid, UseCache: true, cancel).ConfigureAwait(false);
            await store.AddLectorAsync(schedule.LectorName, guid, cancel).ConfigureAwait(false);

            await ProcessLectorScheduleAsync(schedule, cancel).ConfigureAwait(false);
        }

        async Task ProcessLectorScheduleAsync(ScheduleLector schedule, CancellationToken cancel)
        {
            foreach (var (room, id) in schedule.Rooms)
                if (processed_rooms.Add(id))
                    await store.AddRoomAsync(room, id, cancel).ConfigureAwait(false);

            foreach (var group in schedule.Groups.Keys)
                if (processed_groups.Add(group))
                    await ProcessGroupScheduleAsync(group, cancel).ConfigureAwait(false);
        }

        async ValueTask ProcessGroupScheduleAsync(string GroupName, CancellationToken cancel)
        {
            await store.AddGroupAsync(GroupName, cancel).ConfigureAwait(false);

            if (await requester.GetStudentScheduleAsync(GroupName, UseCache: true, cancel).ConfigureAwait(false) is not { } group_schedule)
                return;

            foreach(var (room, id) in group_schedule.Rooms)
                if (processed_rooms.Add(id))
                    await store.AddRoomAsync(room, id, cancel).ConfigureAwait(false);

            foreach (var (lector_id, lector_name) in group_schedule.LessonsCombined
                         .SelectMany(lesson => lesson.Lectors)
                         .Where(l => l.Key != default))
                if (!processed_lectors.Contains(lector_id))
                {
                    to_process.Enqueue(lector_id);
                    await store.AddLectorAsync(lector_name, lector_id, cancel).ConfigureAwait(false);
                }
        }
    }

    public async Task<int> BuildByGroupNameAsync(
        string GroupName,
        bool ClearIndex,
        bool ClearCache,
        CancellationToken Cancel)
    {
        if (ClearCache || ClearIndex)
        {
            var clear_tasks = new List<Task<bool>>(2);

            if (ClearIndex) clear_tasks.Add(ClearIndexAsync(Cancel));
            if (ClearCache) clear_tasks.Add(ClearCacheAsync(Cancel));

            await Task.WhenAll(clear_tasks).ConfigureAwait(false);
        }

        log.LogInformation("Обновление индекса лекторов по графу расписания начиная группы {GroupName}", GroupName);

        var processed_lectors = new HashSet<Guid>();
        var processed_groups = new HashSet<string>();
        var processed_rooms = new HashSet<Guid>();

        var to_process = new Queue<string>(100000);
        to_process.Enqueue(GroupName);

        var timer = Stopwatch.StartNew();
        while (true)
        {
            if (to_process.Count == 0) break;

            Cancel.ThrowIfCancellationRequested();

            var group_name = to_process.Dequeue();

            if (!processed_groups.Add(group_name))
                continue;

            await ProcessStudentsGroupScheduleIdAsync(group_name, Cancel).ConfigureAwait(false);

            log.LogTrace("Обновление индекса лекторов {LectorsCount}, групп {GroupsCount}, аудиторий {RoomsCount}. В очереди {ToProcessQueueLength}",
                processed_lectors.Count, processed_groups.Count, processed_rooms.Count, to_process.Count);
        }

        log.LogTrace("Обновление индекса лекторов завершено за {ElapsedTime}, записей лекторов {LectorsCount}, групп {GroupsCount}, аудиторий {RoomsCount}",
            timer.Elapsed, processed_lectors.Count, processed_groups.Count, processed_rooms.Count);

        return processed_lectors.Count;

        async Task ProcessStudentsGroupScheduleIdAsync(string group, CancellationToken cancel)
        {
            await store.AddGroupAsync(GroupName, cancel).ConfigureAwait(false);

            var schedule = await requester.GetStudentScheduleAsync(group, UseCache: true, cancel).ConfigureAwait(false);

            foreach(var (room, id) in schedule!.Rooms)
                if (processed_rooms.Add(id))
                    await store.AddRoomAsync(room, id, cancel).ConfigureAwait(false);

            foreach(var (id, lector) in schedule.Lectors)
                if(processed_lectors.Add(id))
                {
                    await store.AddLectorAsync(lector, id, cancel).ConfigureAwait(false);
                    await ProcessLectorScheduleByIdAsync(id, cancel).ConfigureAwait(false);
                }
        }

        async Task ProcessLectorScheduleByIdAsync(Guid LectorId, CancellationToken cancel)
        {
            var schedule = await requester.GetLectorScheduleByIdAsync(LectorId, UseCache: true, cancel).ConfigureAwait(false);

            foreach (var (room, id) in schedule.Rooms)
                if (processed_rooms.Add(id))
                    await store.AddRoomAsync(room, id, cancel).ConfigureAwait(false);

            foreach (var group in schedule.Groups.Keys)
                if(!processed_groups.Contains(group))
                    to_process.Enqueue(group);
        }
    }


    public async Task<bool> ClearIndexAsync(CancellationToken Cancel)
    {
        log.LogTrace("Удаление индекса");
        return await store.ClearAsync(Cancel).ConfigureAwait(false);
    }

    public async Task<bool> ClearCacheAsync(CancellationToken Cancel)
    {
        log.LogTrace("Удаление кеша");
        return await cache.ClearAsync(Cancel).ConfigureAwait(false);
    }
}