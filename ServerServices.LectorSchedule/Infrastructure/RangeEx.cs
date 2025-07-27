
namespace ServerServices.LectorSchedule.Infrastructure;

internal static class RangeEx
{
    public static void Deconstruct(this Range range, out Index start, out Index end) => (start, end) = (range.Start, range.End);

    public static (int Start, int End) ToIndexes(this Range range, int length)
    {
        var (start, end) = range;
        return (start.ToIndex(length), end.ToIndex(length));
    }
}

internal static class IndexEx
{
    public static int ToIndex(this Index index, int length) => index.IsFromEnd ? length - index.Value : index.Value;
}
