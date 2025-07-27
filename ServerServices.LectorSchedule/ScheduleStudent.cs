using System.Buffers;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using MathCore;

namespace ServerServices.LectorSchedule;

public record ScheduleStudent(
    string GroupName,
    DateTime Created,
    IReadOnlyList<ScheduleStudentLesson> Lessons)
{
    private static readonly string __BaserPartUriStr = "https://public.mai.ru/schedule/data/"; // length = 36
    private static readonly int __BaserPartUriLength = __BaserPartUriStr.Length;
    private static readonly string __EndPartUriStr = ".json"; // length = 5
    private static readonly int __EndPartUriLength = __EndPartUriStr.Length;
    private const int __MD5ByteLength = 16;
    private const int __MD5HexCharCount = __MD5ByteLength * 2;

    public static string GetId(string GroupName)
    {
        var byte_count = Encoding.UTF8.GetByteCount(GroupName);

        var buffer_array = byte_count > 128
            ? ArrayPool<byte>.Shared.Rent(byte_count)
            : null;
        var buffer = buffer_array is null
            ? stackalloc byte[byte_count]
            : buffer_array.AsSpan(0, byte_count);

        try
        {
            Encoding.UTF8.GetBytes(GroupName, buffer);

            Span<byte> md5 = stackalloc byte[__MD5ByteLength];
            MD5.HashData(buffer, md5);

            Span<char> result_buffer = stackalloc char[__MD5HexCharCount];
            FormatMD5(md5, result_buffer[__BaserPartUriLength..^__EndPartUriLength]);
            var id_str = result_buffer.ToString();

            return id_str;
        }
        finally
        {
            if (buffer_array is not null)
                ArrayPool<byte>.Shared.Return(buffer_array);
        }
    }

    public static Uri GetAddress(string GroupName)
    {
        var byte_count = Encoding.UTF8.GetByteCount(GroupName);

        var buffer_array = byte_count > 128
            ? ArrayPool<byte>.Shared.Rent(byte_count)
            : null;
        var buffer = buffer_array is null
            ? stackalloc byte[byte_count]
            : buffer_array.AsSpan(0, byte_count);

        try
        {
            Encoding.UTF8.GetBytes(GroupName, buffer);

            Span<byte> md5 = stackalloc byte[__MD5ByteLength];
            MD5.HashData(buffer, md5);

            Span<char> result_buffer = stackalloc char[__BaserPartUriLength + __MD5HexCharCount + __EndPartUriLength]; // length = 73
            __BaserPartUriStr.CopyTo(result_buffer);

            FormatMD5(md5, result_buffer[__BaserPartUriLength..^__EndPartUriLength]);

            __EndPartUriStr.CopyTo(result_buffer[^__EndPartUriLength..]);
            var uri_str = result_buffer.ToString();
            Uri uri = new(uri_str);
            return uri;
        }
        finally
        {
            if(buffer_array is not null)
                ArrayPool<byte>.Shared.Return(buffer_array);
        }
    }

    private static void FormatMD5(Span<byte> md5, Span<char> buffer)
    {
        for (var i = 0; i < md5.Length; i++)
        {
            Format(md5[i] >> 04, ref buffer[2 * i]);
            Format(md5[i] & 0xf, ref buffer[2 * i + 1]);
        }

        return;
        static void Format(int b, ref char c) => c = b switch
        {
            0 => '0',
            1 => '1',
            2 => '2',
            3 => '3',
            4 => '4',
            5 => '5',
            6 => '6',
            7 => '7',
            8 => '8',
            9 => '9',
            10 => 'a',
            11 => 'b',
            12 => 'c',
            13 => 'd',
            14 => 'e',
            15 => 'f',
            _ => throw new ArgumentOutOfRangeException(nameof(b), b, "Значение не должно быть больше 15 (4 бита)")
        };
    }

    private static JsonSerializerOptions? __JsonOptions;

    public static JsonSerializerOptions GetJsonOptions() => __JsonOptions ??= new() { Converters = { GetJsonConverter() } };

    public static JsonConverter<ScheduleStudent> GetJsonConverter() => new StudentScheduleJsonConverter();

    public string? Id { get; init; }

    [JsonIgnore]
    public IEnumerable<ScheduleStudentLesson> LessonsCombined
    {
        get
        {
            ScheduleStudentLesson? first_lesson = null;

            foreach (var lesson in Lessons)
            {
                if (first_lesson is not null
                    && lesson.Name == first_lesson.Name
                    && (lesson.Start - (first_lesson.Start + first_lesson.Duration)).TotalMinutes <= 45)
                {
                    first_lesson = new(
                        lesson.Name,
                        first_lesson.Start,
                        (lesson.Start + lesson.Duration) - first_lesson.Start,
                        first_lesson.Lectors.Combine(lesson.Lectors),
                        first_lesson.Types.Combine(lesson.Types),
                        first_lesson.Rooms.Combine(lesson.Rooms),
                        first_lesson.Lms,
                        first_lesson.Teams,
                        first_lesson.Other);
                    continue;
                }

                if (first_lesson is not null)
                    yield return first_lesson;

                first_lesson = lesson;

            }

            if (first_lesson is not null)
                yield return first_lesson;
        }
    }

    [JsonIgnore]
    public IEnumerable<(Guid id, string Name)> Lectors
    {
        get
        {
            HashSet<Guid> set = [];
            foreach (var (id, name) in Lessons.SelectMany(l => l.Lectors))
                if (set.Add(id))
                    yield return (id, name);
        }
    }

    public IEnumerable<(string Room, Guid Id)> Rooms
    {
        get
        {
            HashSet<Guid> rooms = [];
            foreach(var (id, room) in Lessons.SelectMany(l => l.Rooms))
                if (rooms.Add(id))
                    yield return (room, id);
        }
    }
}

public record ScheduleStudentLesson(
    string Name,
    DateTime Start,
    TimeSpan Duration,
    IReadOnlyDictionary<Guid, string> Lectors,
    IReadOnlyDictionary<string, int> Types,
    IReadOnlyDictionary<Guid, string> Rooms,
    string? Lms,
    string? Teams,
    string? Other)
{
    [JsonIgnore]
    public DateTime End => Start + Duration;

    [JsonIgnore]
    public Interval<DateTime> Interval => new(Start, End, true);

    public override string ToString() =>
        $"[{Start,14:d.MM.yy HH:mm} {Duration.TotalHours / 1.5:f0}п.]({string.Join(',', Types.Keys)}) {Name} - {string.Join(',', Lectors.Values)} - {string.Join(',', Rooms.Values)}";
}

file class StudentScheduleJsonConverter : JsonConverter<ScheduleStudent>
{
    private static readonly CultureInfo __RU = CultureInfo.GetCultureInfo("ru-RU");

    public override ScheduleStudent? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject) throw new FormatException();

        Span<char> buffer_date = stackalloc char[10];
        var buffer_time = buffer_date[..8];

        var lessons = new List<ScheduleStudentLesson>(100);

        if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || !reader.ValueTextEquals("group"u8)) throw new FormatException();
        if (!reader.Read() || reader.TokenType != JsonTokenType.String) throw new FormatException();

        var group_name = reader.GetString()!.Normalize();

        while (reader.Read() && reader.CurrentDepth >= 1)
            if (reader.CurrentDepth == 1 && reader.TokenType == JsonTokenType.PropertyName) // дни расписания
            {
                var buffer_fill_length = reader.CopyString(buffer_date);
                var date = DateOnly.Parse(buffer_date[..buffer_fill_length], __RU);

                if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) throw new FormatException();
                while (reader.Read() && reader.CurrentDepth >= 2)
                    if (reader.CurrentDepth == 2 && reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("pairs"u8)) // пары в день
                    {
                        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) throw new FormatException();

                        while (reader.Read() && reader.CurrentDepth >= 3) // часы занятий в день
                            if (reader.CurrentDepth == 3 && reader.TokenType == JsonTokenType.PropertyName)
                            {
                                if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) throw new FormatException();
                                while (reader.Read() && reader.CurrentDepth >= 4) // занятия в это время
                                    if (reader.CurrentDepth == 4 && reader.TokenType == JsonTokenType.PropertyName)
                                    {
                                        var lesson_name = reader.GetString();

                                        TimeOnly start = default;
                                        TimeOnly end = default;
                                        Dictionary<Guid, string> lectors = null!;
                                        Dictionary<string, int> types = null!;
                                        Dictionary<Guid, string> rooms = null!;
                                        string? lms = null;
                                        string? teams = null;
                                        string? other = null;

                                        List<Guid> lector_guid_to_remove = new(2);

                                        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) throw new FormatException();
                                        while (reader.Read() && reader.CurrentDepth >= 5)
                                            if (reader.CurrentDepth == 5 && reader.TokenType == JsonTokenType.PropertyName)
                                                if (reader.ValueTextEquals("time_start"u8))
                                                {
                                                    if (!reader.Read() || reader.TokenType != JsonTokenType.String) throw new FormatException();
                                                    buffer_fill_length = reader.CopyString(buffer_time);
                                                    start = TimeOnly.Parse(buffer_time[..buffer_fill_length], __RU);
                                                }
                                                else if (reader.ValueTextEquals("time_end"u8))
                                                {
                                                    if (!reader.Read() || reader.TokenType != JsonTokenType.String) throw new FormatException();
                                                    reader.CopyString(buffer_time);
                                                    end = TimeOnly.Parse(buffer_time, __RU);
                                                }
                                                else if (reader.ValueTextEquals("lector"u8))
                                                {
                                                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) throw new FormatException();
                                                    lectors = JsonSerializer.Deserialize<Dictionary<Guid, string>>(ref reader, options)!;
                                                    lector_guid_to_remove.AddRange(lectors.Keys);
                                                    foreach (var guid in lector_guid_to_remove)
                                                        if (guid == default)
                                                            lectors.Remove(guid);
                                                    lector_guid_to_remove.Clear();
                                                }
                                                else if (reader.ValueTextEquals("type"u8))
                                                {
                                                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) throw new FormatException();
                                                    types = JsonSerializer.Deserialize<Dictionary<string, int>>(ref reader, options)!;
                                                }
                                                else if (reader.ValueTextEquals("room"u8))
                                                {
                                                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) throw new FormatException();
                                                    rooms = JsonSerializer.Deserialize<Dictionary<Guid, string>>(ref reader, options)!;
                                                }
                                                else if (reader.ValueTextEquals("lms"u8))
                                                {
                                                    if (!reader.Read() || reader.TokenType != JsonTokenType.String) throw new FormatException();
                                                    lms = reader.GetString()!;
                                                    if (string.IsNullOrWhiteSpace(lms)) lms = null!;
                                                }
                                                else if (reader.ValueTextEquals("teams"u8))
                                                {
                                                    if (!reader.Read() || reader.TokenType != JsonTokenType.String) throw new FormatException();
                                                    teams = reader.GetString()!;
                                                    if (string.IsNullOrWhiteSpace(teams)) teams = null!;
                                                }
                                                else if (reader.ValueTextEquals("other"u8))
                                                {
                                                    if (!reader.Read() || reader.TokenType != JsonTokenType.String) throw new FormatException();
                                                    other = reader.GetString()!;
                                                    if (string.IsNullOrWhiteSpace(other)) other = null!;
                                                }

                                        var lesson = new ScheduleStudentLesson(lesson_name!, date.ToDateTime(start), end - start, lectors, types, rooms, lms, teams, other);
                                        lessons.Add(lesson);
                                    }
                            }
                    }
            }

        var schedule = new ScheduleStudent(group_name, DateTime.Now, [.. lessons]);
        return schedule;
    }

    public override void Write(Utf8JsonWriter writer, ScheduleStudent value, JsonSerializerOptions options) => throw new NotSupportedException();
}

[JsonSerializable(typeof(ScheduleStudent))]
[JsonSerializable(typeof(ScheduleStudentLesson))]
[JsonSourceGenerationOptions(AllowTrailingCommas = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal partial class ScheduleStudentJsonContext : JsonSerializerContext
{

}