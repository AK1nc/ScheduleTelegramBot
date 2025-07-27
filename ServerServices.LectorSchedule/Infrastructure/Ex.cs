namespace ServerServices.LectorSchedule.Infrastructure;

internal static class Ex
{
    public static IEnumerable<Task<TResult>> SelectAsync<T, TResult>(
        this IEnumerable<T> items,
        Func<T, CancellationToken, Task<TResult>> Selector, 
        CancellationToken Cancel = default)
    {
        foreach (var item in items)
            yield return Selector(item, Cancel);
    }

    public static IEnumerable<ValueTask<TResult>> SelectAsync<T, TResult>(
        this IEnumerable<T> items,
        Func<T, CancellationToken, ValueTask<TResult>> Selector,
        CancellationToken Cancel = default)
    {
        foreach (var item in items)
            yield return Selector(item, Cancel);
    }

    public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> items)
    {
        foreach(var items2 in items)
        foreach (var item in items2)
            yield return item;
    }
}
