using System.Text.Json;

namespace ServerServices.LectorSchedule.Infrastructure;

internal static class JsonSerializerEx
{
    public static async Task<T?> DeserializeJsonAsync<T>(this FileInfo file, JsonSerializerOptions? opts = null, CancellationToken Cancel = default)
    {
        await using var stream = file.OpenRead();
        var result = await JsonSerializer.DeserializeAsync<T>(stream, opts, Cancel).ConfigureAwait(false);
        return result;
    }

    public static async Task SerializeJSONAsync<T>(
        this T obj,
        string FileName,
        JsonSerializerOptions? opt = null,
        CancellationToken Cancel = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FileName)!);
        await using var stream = File.Create(FileName);
        await JsonSerializer.SerializeAsync(stream, obj, opt, Cancel);
    }

    public static async Task SerializeJSONAsync<T>(
        this T obj,
        FileInfo file,
        JsonSerializerOptions? opt = null,
        CancellationToken Cancel = default)
    {
        file.Directory!.Create();
        await using var stream = file.Create();
        await JsonSerializer.SerializeAsync(stream, obj, opt, Cancel);
    }
}