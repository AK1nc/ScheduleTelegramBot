using System.Text;

using Ical.Net;
using Ical.Net.Serialization;

namespace ServerServices.LectorSchedule.Infrastructure;

internal sealed class CalendarSpecialSerializer : CalendarSerializer
{
    public static string DefaultProductId { get; set; } = LibraryMetadata.ProdId;

    private readonly ComponentSerializer _Serializer = new();

    public override string SerializeToString(object obj)
    {
        if (obj is not Calendar calendar)
            return base.SerializeToString(obj);

        calendar.Version = "2.0";
        calendar.ProductId ??= DefaultProductId;
        var result = _Serializer.SerializeToString(obj);
        return result;
    }

    public void SerializeToStream(object obj, Stream stream, Encoding? encoding)
    {
        using var writer = new StreamWriter(stream, encoding, 1024, true);

        SerializationContext.Push(obj);

        writer.Write(SerializeToString(obj));

        SerializationContext.Pop();
    }

    public async Task SerializeToStreamAsync(object obj, Stream stream, Encoding? encoding, CancellationToken Cancel = default)
    {
        await using var writer = new StreamWriter(stream, encoding, 1024, true);

        SerializationContext.Push(obj);

        var str = SerializeToString(obj).AsMemory();
        await writer.WriteAsync(str, Cancel).ConfigureAwait(false);

        SerializationContext.Pop();
    }
}
