using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NickBuhro.Translit;

using ServerServices.LectorSchedule.Entities;

using LectorIdInfo = ServerServices.LectorSchedule.ILectorsStore.LectorIdInfo;
using GroupIdInfo = ServerServices.LectorSchedule.ILectorsStore.GroupIdInfo;
using RoomIdInfo = ServerServices.LectorSchedule.ILectorsStore.RoomIdInfo;
using System.Linq;

namespace ServerServices.LectorSchedule;

public interface ILectorsStore
{
    Task<int> GetLectorsCountAsync(CancellationToken Cancel);
    Task<IEnumerable<LectorIdInfo>> GetLectorsAsync(int Skip = 0, int Take = 0, CancellationToken Cancel = default);
    Task<IEnumerable<LectorIdInfo>> GetLectorsSimilarNameAsync(string Name, int Skip = 0, int Take = 0, CancellationToken Cancel = default);
    record LectorIdInfo(Guid ScheduleId, string Name);

    Task<int> GetGroupsCountAsync(CancellationToken Cancel = default);
    Task<IEnumerable<GroupIdInfo>> GetGroupsAsync(int Skip = 0, int Take = 0, CancellationToken Cancel = default);
    Task<IEnumerable<GroupIdInfo>> GetGroupsSimilarNameAsync(string Name, int Skip = 0, int Take = 0, CancellationToken Cancel = default);
    record GroupIdInfo(string ScheduleId, string Name);

    Task<int> GetRoomsCountAsync(CancellationToken Cancel);
    Task<IEnumerable<RoomIdInfo>> GetRoomsAsync(int Skip = 0, int Take = 0, CancellationToken Cancel = default);
    record RoomIdInfo(Guid ScheduleId, string Name);

    Task<bool> AddRoomAsync(string Name, Guid id, CancellationToken Cancel = default);

    Task<bool> AddGroupAsync(string GroupName, CancellationToken Cancel = default);
    Task<string?> GetGroupName(string ScheduleId, CancellationToken Cancel = default);

    Task<string?> GetRoomName(Guid RoomId, CancellationToken Cancel = default);
    Task<Guid?> GetRoomId(string RoomName, CancellationToken Cancel = default);

    Task<bool> AddLectorAsync(string Name, Guid id, CancellationToken Cancel = default);
    Task AddLectorsAsync(IEnumerable<(Guid Id, string Name)> Lectors, CancellationToken Cancel = default);
    Task<Guid?> GetIdAsync(string Name, CancellationToken Cancel = default);
    Task<string?> GetNameAsync(Guid id, CancellationToken Cancel = default);
    Task<string[]> GetLectorSimilarNames(string Name, CancellationToken Cancel);

    Task<bool> ClearAsync(CancellationToken Cancel = default);
    Task<(Guid id, string Name)[]> GetIdsAsync(string Name, CancellationToken Cancel);
}

public class InDbLectorsStore(
    LectorScheduleDB db,
    ILogger<InDbLectorsStore> log)
    : ILectorsStore
{
    public async Task<int> GetLectorsCountAsync(CancellationToken Cancel)
    {
        var count = await db.Lectors.CountAsync(Cancel).ConfigureAwait(false);
        return count;
    }

    public async Task<int> GetGroupsCountAsync(CancellationToken Cancel)
    {
        var count = await db.Groups.CountAsync(Cancel).ConfigureAwait(false);
        return count;
    }

    public async Task<int> GetRoomsCountAsync(CancellationToken Cancel)
    {
        var count = await db.Rooms.CountAsync(Cancel).ConfigureAwait(false);
        return count;
    }

    public async Task<IEnumerable<LectorIdInfo>> GetLectorsAsync(int Skip, int Take, CancellationToken Cancel)
    {
        IQueryable<Lector> query = db.Lectors.OrderBy(l => l.Name);
        if (Skip > 0 || Take > 0)
        {
            if (Skip > 0)
                query = query.Skip(Skip);
            if (Take > 0)
                query = query.Take(Take);
        }

        var db_lectors = await query
            .AsNoTracking()
            .ToArrayAsync(Cancel)
            .ConfigureAwait(false);

        var lectors = db_lectors.Select(l => new LectorIdInfo(l.ScheduleId, l.Name.ToUpperWords()));
        return lectors;
    }

    public async Task<IEnumerable<LectorIdInfo>> GetLectorsSimilarNameAsync(string Name, int Skip, int Take, CancellationToken Cancel)
    {
        var normalized_name = Name.Trim().ToUpperInvariant();

        var rus_name = normalized_name.AsSpan().ContainsAny(__LatinChars)
            ? Transliteration.LatinToCyrillic(normalized_name)
            : normalized_name;

        IQueryable<Lector> query = db.Lectors
            .Where(l => l.Name.Contains(rus_name))
            .OrderBy(l => l.Name)
            ;

        if (Skip > 0 || Take > 0)
        {
            if (Skip > 0)
                query = query.Skip(Skip);
            if (Take > 0)
                query = query.Take(Take);
        }

        var db_lectors = await query
            .AsNoTracking()
            .ToArrayAsync(Cancel)
            .ConfigureAwait(false);

        var lectors = db_lectors.Select(l => new LectorIdInfo(l.ScheduleId, l.Name.ToUpperWords()));
        return lectors;
    }

    public async Task<IEnumerable<GroupIdInfo>> GetGroupsAsync(int Skip, int Take, CancellationToken Cancel)
    {
        IQueryable<StudentsGroup> query = db.Groups.OrderBy(g => g.Name);
        if (Skip > 0 || Take > 0)
        {
            if (Skip > 0)
                query = query.Skip(Skip);
            if (Take > 0)
                query = query.Take(Take);
        }

        var db_groups = await query
            .AsNoTracking()
            .ToArrayAsync(Cancel)
            .ConfigureAwait(false);

        var groups = db_groups.Select(l => new GroupIdInfo(l.ScheduleId, l.Name));

        return groups;
    }

    public async Task<IEnumerable<GroupIdInfo>> GetGroupsSimilarNameAsync(string Name, int Skip, int Take, CancellationToken Cancel)
    {
        var normalized_name = Name.Trim();

        var rus_name = normalized_name.AsSpan().ContainsAny(__LatinChars)
            ? Transliteration.LatinToCyrillic(normalized_name)
            : normalized_name;

        IQueryable<StudentsGroup> query = db.Groups
                .Where(l => l.Name.Contains(rus_name))
                .OrderBy(l => l.Name)
            ;

        if (Skip > 0 || Take > 0)
        {
            if (Skip > 0)
                query = query.Skip(Skip);
            if (Take > 0)
                query = query.Take(Take);
        }

        var db_groups = await query
            .AsNoTracking()
            .ToArrayAsync(Cancel)
            .ConfigureAwait(false);

        var groups = db_groups.Select(l => new GroupIdInfo(l.ScheduleId, l.Name));
        return groups;
    }


    public async Task<IEnumerable<RoomIdInfo>> GetRoomsAsync(int Skip, int Take, CancellationToken Cancel)
    {
        IQueryable<Room> query = db.Rooms.OrderBy(r => r.Name);
        if (Skip > 0 || Take > 0)
        {
            if (Skip > 0)
                query = query.Skip(Skip);
            if (Take > 0)
                query = query.Take(Take);
        }

        var db_rooms = await query
            .AsNoTracking()
            .ToArrayAsync(Cancel)
            .ConfigureAwait(false);

        var rooms = db_rooms.Select(l => new RoomIdInfo(l.ScheduleId, l.Name));

        return rooms;
    }

    public async Task<bool> AddRoomAsync(string Name, Guid id, CancellationToken Cancel)
    {
        Name = Name.Trim();
        if (await db.Rooms.AnyAsync(l => l.Name == Name || l.ScheduleId == id, Cancel).ConfigureAwait(false))
        {
            log.LogTrace("Добавляемая аудитория id {GroupId} {GroupName} уже существует в БД", id, Name);
            return false;
        }

        var room = new Room
        {
            Name = Name,
            ScheduleId = id
        };

        db.Add(room);
        await db.SaveChangesAsync(Cancel).ConfigureAwait(false);
        log.LogInformation("Новая аудитория id {GroupId} {GroupName} добавлена в БД успешно", id, Name);

        return true;
    }

    public async Task<string?> GetRoomName(Guid RoomId, CancellationToken Cancel)
    {
        var room = await db.Rooms
            .FirstOrDefaultAsync(r => r.ScheduleId == RoomId, Cancel)
            .ConfigureAwait(false);

        return room?.Name;
    }

    public async Task<Guid?> GetRoomId(string RoomName, CancellationToken Cancel)
    {
        RoomName = RoomName.Trim();
        if (RoomName.IsNullOrWhiteSpace())
            throw new ArgumentException("Не задано имя комнаты", RoomName);

        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Name.StartsWith(RoomName), Cancel).ConfigureAwait(false);
        return room?.ScheduleId;
    }

    public async Task<bool> AddGroupAsync(string GroupName, CancellationToken Cancel)
    {
        GroupName = GroupName.Trim();
        if (await db.Groups.AnyAsync(l => l.Name == GroupName, Cancel).ConfigureAwait(false))
        {
            log.LogTrace("Добавляемая группа {GroupName} уже существует в БД", GroupName);
            return false;
        }

        var group = new StudentsGroup
        {
            Name = GroupName,
            ScheduleId = Hash(GroupName),
        };

        db.Add(group);
        var changes = await db.SaveChangesAsync(Cancel).ConfigureAwait(false);

        log.LogInformation("Новая группа {GroupName}[{GroupHash}] добавлена в БД", GroupName, group.ScheduleId);

        return true;

        static string Hash(string group)
        {
            Span<byte> source = stackalloc byte[Encoding.UTF8.GetByteCount(group)];
            Encoding.UTF8.GetBytes(group, source);

            Span<byte> md5 = stackalloc byte[16];
            MD5.HashData(source, md5);

            Span<char> result = stackalloc char[32];
            var str = new StringBuilder(32);
            foreach (var b in md5)
                str.Append($"{b:x2}");
            return str.ToString();
        }
    }

    public async Task<string?> GetGroupName(string ScheduleId, CancellationToken Cancel)
    {
        StudentsGroup? group;

        var id = ScheduleId.Trim().ToLowerInvariant();
        switch (id.Length)
        {
            case 0: throw new ArgumentException("Не задан идентификатор группы", nameof(ScheduleId));
            case > 32: throw new ArgumentOutOfRangeException(nameof(ScheduleId), id, "Размер строки идентификатор а не может быть больше 32 символов");

            case 32:
                group = await db.Groups.FirstOrDefaultAsync(g => g.ScheduleId == id.Trim(), Cancel).ConfigureAwait(false);
                return group?.Name;

            default:
                id = ScheduleId.Trim().ToLowerInvariant();

                group = await db.Groups.FirstOrDefaultAsync(g => g.ScheduleId.StartsWith(id), Cancel).ConfigureAwait(false);
                return group?.ScheduleId;
        }
    }

    public async Task<bool> AddLectorAsync(string Name, Guid id, CancellationToken Cancel)
    {
        Name = Name.Trim();
        var name = Name.ToUpperInvariant();
        if (await db.Lectors.AnyAsync(l => l.Name == name || l.ScheduleId == id, Cancel).ConfigureAwait(false))
        {
            log.LogTrace("Добавляемый лектор id {LectorId} {LectorName} уже существует в БД", id, Name);
            return false;
        }

        var lector = new Lector
        {
            Name = name,
            ScheduleId = id
        };

        try
        {
            db.Add(lector);
            await db.SaveChangesAsync(Cancel).ConfigureAwait(false);
            log.LogInformation("Новый лектор id {LectorId} {LectorName} добавлен в БД успешно", id, Name);

            return true;
        }
        catch (DbUpdateException error)
        {
            log.LogError("Ошибка при добавлении лектора {LectorName}[{LectorId}] в БД {msg}: {error}",
                Name, id,
                error.Message, error);
            throw new InvalidOperationException($"Ошибка при добавлении лектора {Name}[{id}] в БД", error);
        }
    }

    public async Task AddLectorsAsync(IEnumerable<(Guid Id, string Name)> Lectors, CancellationToken Cancel)
    {
        var lectors = Lectors.ToArray();
        var ids_to_add = lectors.Select(l => l.Id).ToArray();
        var known_ids = await db.Lectors
            .Where(l => ids_to_add.Contains(l.ScheduleId))
            .Select(l => l.ScheduleId)
            .ToArrayAsync(Cancel)
            .ConfigureAwait(false);
        var known_ids_set = known_ids.ToHashSet();

        var lectors_to_add = lectors
            .Where(l => known_ids_set.Add(l.Id))
            .Select(l => new Lector { Name = l.Name.ToUpperInvariant(), ScheduleId = l.Id });

        db.Lectors.AddRange(lectors_to_add);
        await db.SaveChangesAsync(Cancel).ConfigureAwait(false);
    }

    private static readonly System.Buffers.SearchValues<char> __LatinChars = System.Buffers.SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");

    public async Task<Guid?> GetIdAsync(string Name, CancellationToken Cancel)
    {
        Name = Name.Trim();
        if (Name.StartsWith("shma", StringComparison.InvariantCultureIgnoreCase))
            Name = "Шмачилин Павел Александрович";

        var upper_name = Name.ToUpperInvariant();

        var rus_name = upper_name.AsSpan().ContainsAny(__LatinChars)
            ? Transliteration.LatinToCyrillic(upper_name)
            : upper_name;

        if ((upper_name.StartsWith("KONDR") || rus_name.StartsWith("КОНДР") && rus_name.Length < 10) || upper_name.StartsWith("KONDRATIEF"))
            rus_name = "КОНДРАТЬЕВА СВЕТЛАНА ГЕННАДЬЕВНА";

        var lector = await db.Lectors
            .FirstOrDefaultAsync(l => l.Name == rus_name, Cancel)
            .ConfigureAwait(false);

        if (lector is null)
            lector = await db.Lectors
                .FirstOrDefaultAsync(l => l.Name.StartsWith(rus_name), Cancel)
                .ConfigureAwait(false);

        if (lector is null)
            lector = await db.Lectors
                .FirstOrDefaultAsync(l => l.Name.Contains(rus_name), Cancel)
                .ConfigureAwait(false);

        return lector?.ScheduleId;
    }

    public async Task<(Guid id, string Name)[]> GetIdsAsync(string Name, CancellationToken Cancel)
    {
        var upper_name = Name.Trim().ToUpperInvariant();

        var rus_name = upper_name.AsSpan().ContainsAny(__LatinChars)
            ? Transliteration.LatinToCyrillic(upper_name)
            : upper_name;

        var result = new List<(Guid, string)>();
        await foreach (var lector in db.Lectors
                           .Where(l => EF.Functions.Like(l.Name, $"{rus_name}%"))
                           .AsAsyncEnumerable()
                           .WithCancellation(Cancel)
                           .ConfigureAwait(false))
            result.Add((lector.ScheduleId, lector.Name.ToUpperWords()));

        return result.ToArray();
    }

    public async Task<string?> GetNameAsync(Guid id, CancellationToken Cancel)
    {
        var lector = await db.Lectors
            .FirstOrDefaultAsync(l => l.ScheduleId == id, Cancel)
            .ConfigureAwait(false);

        return lector is { Name: { Length: > 0 } name }
            ? name.ToUpperWords()
            : null;
    }

    public async Task<string[]> GetLectorSimilarNames(string Name, CancellationToken Cancel)
    {
        Name = Name.Trim();
        if (Name is not { Length: > 1 })
            return [];

        var lectors_query = db.Lectors
            .Where(l => EF.Functions.Like(l.Name, $"%{Name.ToUpper()}%"))
            .Select(l => l.Name);

        var result = new List<string>(10);
        await foreach (var name in lectors_query.AsAsyncEnumerable().WithCancellation(Cancel).ConfigureAwait(false))
            result.Add(name.ToUpperWords());

        return result.ToArray();
    }

    public async Task<bool> ClearAsync(CancellationToken Cancel)
    {
        var any = await db.Lectors.AnyAsync(Cancel).ConfigureAwait(false)
            || await db.Groups.AnyAsync(Cancel).ConfigureAwait(false)
            || await db.Rooms.AnyAsync(Cancel).ConfigureAwait(false);

        log.LogTrace("Удаление индекса");
        await db.Database.EnsureDeletedAsync(Cancel).ConfigureAwait(false);
        await db.Database.EnsureCreatedAsync(Cancel).ConfigureAwait(false);

        return any;
    }
}