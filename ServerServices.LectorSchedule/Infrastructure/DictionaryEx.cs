namespace ServerServices.LectorSchedule.Infrastructure;

internal static class DictionaryEx
{
    public static IReadOnlyDictionary<TKey, TValue> Combine<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> d1,
        params IReadOnlyDictionary<TKey, TValue>[] d2) 
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>(d1.Count + d2.Sum(d => d.Count));
        foreach (var d in d2.Prepend(d1))
        foreach (var (key, value) in d)
            result[key] = value;

        result.TrimExcess();
        return result;
    }

    public static FileInfo GetFileInfo(this DirectoryInfo dir, string RelatedPath) => new(Path.Combine(dir.FullName, RelatedPath));
}
