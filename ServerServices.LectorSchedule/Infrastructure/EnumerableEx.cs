namespace ServerServices.LectorSchedule.Infrastructure;

internal static class EnumerableEx
{
    public static bool Any<T>(this IEnumerable<T> items, T value)
    {
        foreach(var item in items)
            if (Equals(item, value))
                return true;

        return false;
    }

    public static bool Any(this IEnumerable<string> items, string value, StringComparison comparison)
    {
        foreach(var item in items)
            if (string.Equals(item, value, comparison))
                return true;

        return false;
    }
}
