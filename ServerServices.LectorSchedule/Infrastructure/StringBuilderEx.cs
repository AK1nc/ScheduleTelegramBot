using System.Text;

namespace ServerServices.LectorSchedule.Infrastructure;

internal static class StringBuilderEx
{
    public static string ToString(this StringBuilder str, Range range)
    {
        var (start, end) = range.ToIndexes(str.Length);
        return str.ToString(start, end - start);
    }
}
