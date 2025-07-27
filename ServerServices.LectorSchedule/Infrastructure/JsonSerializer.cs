using System.Text.Json;

namespace ServerServices.LectorSchedule.Infrastructure;

internal static class JsonSerializer<T>
{
    public static async Task<T?> DeserializeAsync(
        string FilePath, 
        JsonSerializerOptions? opts = null,
        CancellationToken Cancel = default)
    {
        if (FilePath.AsSpan().ContainsAny('*', '?'))
        {
            var path = Path.GetFullPath(FilePath);
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) return default;
            var file_mask = Path.GetFileName(path);
            FilePath = Directory.EnumerateFiles(dir, file_mask).FirstOrDefault()!;
        }

        if (!File.Exists(FilePath))
            return default;

        await using var file = File.OpenRead(FilePath);
        var value = await JsonSerializer.DeserializeAsync<T>(file, opts, Cancel).ConfigureAwait(false);
        return value;
    }
}