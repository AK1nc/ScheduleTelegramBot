using System.Runtime.CompilerServices;

namespace ServerServices.LectorSchedule.Infrastructure;

internal static class AsyncEnumerableEx
{
    public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> items, CancellationToken Cancel = default)
    {
        var result = new List<T>();

        await foreach (var item in items.WithCancellation(Cancel).ConfigureAwait(false))
            result.Add(item);

        return result.ToArray();
    }

    public static async IAsyncEnumerable<TValue> SelectManyAsync<T, TValue>(
        this IAsyncEnumerable<T> items, 
        Func<T, IEnumerable<TValue>> selector,
        [EnumeratorCancellation] CancellationToken Cancel = default)
    {
        await foreach (var item in items.WithCancellation(Cancel).ConfigureAwait(false))
            if(selector(item) is { } values)
                foreach (var value in values)
                    yield return value;
    }

    public static async  IAsyncEnumerable<T> WhereAsync<T>(
        this IAsyncEnumerable<T> items, 
        Func<T, bool> selector,
        [EnumeratorCancellation] CancellationToken Cancel = default)
    {
        await foreach(var item in items.WithCancellation(Cancel).ConfigureAwait(false))
            if (selector(item))
                yield return item;
    }

    public static async IAsyncEnumerable<TResult> SelectAsync<T, TResult>(
        this IAsyncEnumerable<T> items,
        Func<T, TResult> selector,
        [EnumeratorCancellation] CancellationToken Cancel = default)
    {
        await foreach (var item in items.WithCancellation(Cancel).ConfigureAwait(false))
            yield return selector(item);
    }
}
