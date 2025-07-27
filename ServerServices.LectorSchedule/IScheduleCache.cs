using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Unicode;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace ServerServices.LectorSchedule;

public interface IScheduleCache
{
    Task<ScheduleLector?> GetLectorScheduleAsync(Guid id, CancellationToken Cancel = default);
    Task SetLectorScheduleAsync(Guid id, ScheduleLector schedule, CancellationToken Cancel = default);
    Task<ScheduleStudent?> GetStudentScheduleAsync(string GroupName, CancellationToken Cancel = default);
    Task SetStudentScheduleAsync(string GroupName, ScheduleStudent schedule, CancellationToken Cancel = default);

    IAsyncEnumerable<ScheduleLector> EnumLectorsSchedulesAsync(CancellationToken Cancel = default);
    IAsyncEnumerable<ScheduleStudent> EnumStudentsSchedulesAsync(CancellationToken Cancel = default);

    Task<bool> ClearAsync(CancellationToken Cancel = default);
}

public class FileScheduleCache(
    [FromKeyedServices("schedule")] IFileProvider? files,
    ILogger<FileScheduleCache> log)
    : IScheduleCache
{
    private readonly DirectoryInfo? _LectorCacheDir = files?.GetFileInfo("lectors/file.info").PhysicalPath is not { Length: > 0 } cache_dir_file_path
       ? null
       : Directory.CreateDirectory(Path.GetDirectoryName(cache_dir_file_path)!);

    private readonly DirectoryInfo? _StudentCacheDir = files?.GetFileInfo("students/file.info").PhysicalPath is not { Length: > 0 } cache_dir_file_path
        ? null
        : Directory.CreateDirectory(Path.GetDirectoryName(cache_dir_file_path)!);

    private const int __CacheUpdateDays = 3;

    private static readonly JsonSerializerOptions __JsonCacheOptions = new()
    {
        //WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.Cyrillic, UnicodeRanges.BasicLatin),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public async Task<ScheduleLector?> GetLectorScheduleAsync(Guid id, CancellationToken Cancel)
    {
        _LectorCacheDir?.Create();
        if (_LectorCacheDir?.GetFiles($"*[{id}].json").FirstOrDefault() is not { LastWriteTime: var file_write_time } cache_file)
            log.LogTrace("Файл кеша данных расписания лектора {LectorId} не найден", id);
        else if ((DateTime.Now - file_write_time).TotalDays >= __CacheUpdateDays)
            log.LogInformation("Файл кеша {CacheFile} расписания лектора {LectorId} устарел более чем на {CacheElapsedDaysCount}", cache_file.Name, id, __CacheUpdateDays);
        else if (await cache_file.DeserializeJsonAsync<ScheduleLector>(Cancel: Cancel).ConfigureAwait(false) is not { } file_schedule)
            log.LogWarning("Ошибка загрузки данных кеша {FileName} расписания лектора {LectorId}", cache_file.Name, id);
        else
        {
            log.LogInformation("Файл кеша данных расписания лектора {lectorName}:{LectorId} найден и успешно прочитан", file_schedule.LectorName, id);
            return file_schedule;
        }

        return null;
    }

    public async Task SetLectorScheduleAsync(Guid id, ScheduleLector schedule, CancellationToken Cancel)
    {
        if (_LectorCacheDir is null) return;

        var new_cache_file = _LectorCacheDir.GetFileInfo($"{schedule.LectorName}[{id}].json");
        await schedule.SerializeJSONAsync(new_cache_file, __JsonCacheOptions, Cancel).ConfigureAwait(false);
        log.LogInformation("Расписание лектора {LectorName} {id} сохранено в кеш {FileName}", schedule.LectorName, id, new_cache_file.Name);
    }

    public async Task<ScheduleStudent?> GetStudentScheduleAsync(string GroupName, CancellationToken Cancel)
    {
        _StudentCacheDir?.Create();
        if (_StudentCacheDir?.GetFiles($"{GroupName}.json").FirstOrDefault() is not { LastWriteTime: var file_write_time } cache_file)
            log.LogTrace("Файл кеша данных расписания студенческой группы {GroupName} не найден", GroupName);
        else if ((DateTime.Now - file_write_time).TotalDays >= __CacheUpdateDays)
            log.LogInformation("Файл кеша {CacheFile} расписания студенческой группы {GroupName} устарел более чем на {CacheElapsedDaysCount}", cache_file.Name, GroupName, __CacheUpdateDays);
        else if (await cache_file.DeserializeJsonAsync<ScheduleStudent>(Cancel: Cancel).ConfigureAwait(false) is not { } file_schedule)
            log.LogWarning("Ошибка загрузки данных кеша {FileName} расписания студенческой группы {GroupName}", cache_file.Name, GroupName);
        else
        {
            log.LogInformation("Файл кеша данных расписания студенческой группы {GroupName} найден и успешно прочитан", GroupName);
            return file_schedule;
        }

        return null;
    }

    public async Task SetStudentScheduleAsync(string GroupName, ScheduleStudent schedule, CancellationToken Cancel)
    {
        if (_StudentCacheDir is null) return;

        var new_cache_file = _StudentCacheDir.GetFileInfo($"{GroupName}.json");
        await schedule.SerializeJSONAsync(new_cache_file, __JsonCacheOptions, Cancel).ConfigureAwait(false);
        log.LogInformation("Расписание студенческой группы {GroupName} сохранено в кеш {FileName}", schedule.GroupName, new_cache_file.Name);
    }

    public async IAsyncEnumerable<ScheduleLector> EnumLectorsSchedulesAsync([EnumeratorCancellation] CancellationToken Cancel)
    {
        if(_LectorCacheDir is not { Exists: true })
            yield break;

        foreach (var cache_file in _LectorCacheDir.EnumerateFiles("*[*].json"))
            if (await cache_file.DeserializeJsonAsync<ScheduleLector>(Cancel: Cancel).ConfigureAwait(false) is { } schedule)
                yield return schedule;
            else
                log.LogWarning("Ошибка десериализации расписания лектора из файла {FileName}", cache_file.FullName);
    }

    public async IAsyncEnumerable<ScheduleStudent> EnumStudentsSchedulesAsync([EnumeratorCancellation] CancellationToken Cancel)
    {
        if (_StudentCacheDir is not { Exists: true })
            yield break;

        foreach (var cache_file in _StudentCacheDir.EnumerateFiles("*.json"))
            if (await cache_file.DeserializeJsonAsync<ScheduleStudent>(Cancel: Cancel).ConfigureAwait(false) is { } schedule)
                yield return schedule;
            else
                log.LogWarning("Ошибка десериализации расписания студенческой группы из файла {FileName}", cache_file.FullName);
    }

    public async Task<bool> ClearAsync(CancellationToken Cancel = default)
    {
        log.LogTrace("Удаление кеша");

        var lectors_cache_exists = _LectorCacheDir?.EnumerateFiles().Any() ?? false;
        var students_cache_exists = _StudentCacheDir?.EnumerateFiles().Any() ?? false;

        if (!lectors_cache_exists && !students_cache_exists)
            return false;

        await Task.Delay(10, Cancel).ConfigureAwait(false);

        if (lectors_cache_exists) _LectorCacheDir!.Delete(true);
        if (students_cache_exists) _StudentCacheDir!.Delete(true);

        if (lectors_cache_exists)
            Directory.Delete(Path.GetDirectoryName(_LectorCacheDir!.FullName!)!, true);
        else
            Directory.Delete(Path.GetDirectoryName(_StudentCacheDir!.FullName!)!, true);

        return true;
    }
}
