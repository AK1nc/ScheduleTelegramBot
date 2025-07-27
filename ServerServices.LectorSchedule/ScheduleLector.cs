using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using MathCore;
using MathCore.Annotations;

namespace ServerServices.LectorSchedule;

// https://public.mai.ru/schedule/data/{<GUID>}.json

[PublicAPI]
public record ScheduleLector(
    string LectorName,
    DateTime Created,
    IReadOnlyDictionary<string, int> Groups,
    IReadOnlyList<ScheduleLectorLesson> Lessons)
{
    public static Uri GetAddress(string LectorId)
    {
        if (LectorId is not { Length: 36 })
            throw new ArgumentException("Длина строки аргумента должна быть 36 символов формата GUID");

        const int result_buffer_length = 77;
        return new(new ValueStringBuilder(stackalloc char[result_buffer_length])
            .Append("https://public.mai.ru/schedule/data/")
            .Append(LectorId)
            .Append(".json"));
    }

    public static Uri GetAddress(Guid LectorId)
    {
        const int guid_buffer_length = 36;
        Span<char> guid = stackalloc char[guid_buffer_length];

        LectorId.TryFormat(guid, out _);

        const int result_buffer_length = 77;

        return new(new ValueStringBuilder(stackalloc char[result_buffer_length])
            .Append("https://public.mai.ru/schedule/data/")
            .Append(guid)
            .Append(".json"));
    }

    private static JsonSerializerOptions? __JsonOptions;
    public static JsonSerializerOptions GetJsonOptions() => __JsonOptions ??= new() { Converters = { GetJsonConverter(), ScheduleLectorLesson.GetJsonConverter() } };

    public static JsonConverter<ScheduleLector> GetJsonConverter() => new ScheduleJsonConverter();

    public Guid? Id { get; init; }

    [JsonIgnore]
    public IEnumerable<ScheduleLectorLesson> LessonsCombined
    {
        get
        {
            ScheduleLectorLesson? first_lesson = null;
            foreach (var lesson in Lessons)
            {
                if (first_lesson is not null
                    && first_lesson.Name == lesson.Name
                    && (lesson.Start - (first_lesson.Start + first_lesson.Duration)).TotalMinutes <= 45)
                {
                    first_lesson = new(
                        lesson.Name,
                        first_lesson.Start,
                        (lesson.Start + lesson.Duration) - first_lesson.Start,
                        first_lesson.LessonType.Concat(lesson.LessonType).Distinct().ToArray(),
                        first_lesson.Groups.Concat(lesson.Groups).Distinct().ToArray(),
                        first_lesson.Rooms.Combine(lesson.Rooms));
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
    public IEnumerable<(string Room, Guid Id)> Rooms
    {
        get
        {
            HashSet<Guid> rooms = [];
            foreach (var (id, room) in Lessons.SelectMany(l => l.Rooms))
                if (rooms.Add(id))
                    yield return (room, id);
        }
    }

    public override string ToString() => $"Расписание {LectorName} занятий: {Lessons.Count}, групп: {Groups.Count}";
}

[PublicAPI]
public record ScheduleLectorLesson(
    string Name,
    DateTime Start,
    TimeSpan Duration,
    IReadOnlyList<string> LessonType,
    IReadOnlyList<string> Groups,
    IReadOnlyDictionary<Guid, string> Rooms)
{
    [JsonIgnore]
    public DateTime End => Start + Duration;

    [JsonIgnore]
    public Interval<DateTime> Interval => new(Start, End, true);

    public static JsonConverter<ScheduleLectorLesson> GetJsonConverter() => new ScheduleLessonInfoJsonConverter();

    public override string ToString() => $"[{Start,5:dd.MM} {Start,5:H:mm}  {Duration.TotalHours / 1.5:f0}п.]({string.Join(',', LessonType)}) {Name} - {string.Join(", ", Groups)} - {string.Join(',', Rooms.Values)}";
}

file class ScheduleJsonConverter : JsonConverter<ScheduleLector>
{
    private static readonly CultureInfo __RU = CultureInfo.GetCultureInfo("ru-RU");

    public override ScheduleLector Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject) throw new FormatException();

        if (!options.Converters.OfType<JsonConverter<ScheduleLectorLesson>>().Any())
            options = new(options)
            {
                Converters = { ScheduleLectorLesson.GetJsonConverter() }
            };

        Span<char> buffer = stackalloc char[10];

        string teacher = null!;
        Dictionary<string, int> groups_counts = null!;
        List<ScheduleLectorLesson> lessons = new(100);

        while (reader.Read() && reader.CurrentDepth == 1)
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName when reader.ValueTextEquals("name"u8):
                    {
                        if (!reader.Read() || reader.TokenType != JsonTokenType.String) throw new FormatException();
                        teacher = reader.GetString()!;

                        var result = new StringBuilder(teacher.Length + 1);
                        foreach (var item in Regex.Split(teacher, @"\s", RegexOptions.Compiled))
                            if (item is [var a, .. var tail])
                                result.Append($"{char.ToUpper(a)}{tail.ToLowerInvariant()} ");
                            else
                                result.Append(item).Append(' ');
                        result.Length--;
                        teacher = result.ToString();
                    }
                    break;

                case JsonTokenType.PropertyName when reader.ValueTextEquals("groups"u8):
                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) throw new FormatException();
                    groups_counts = JsonSerializer.Deserialize<Dictionary<string, int>>(ref reader, options)!;
                    break;

                case JsonTokenType.PropertyName when reader.ValueTextEquals("schedule"u8):
                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) throw new FormatException();

                    DateOnly date = default;

                    while (reader.Read() && reader.CurrentDepth == 2)
                    {
                        int read_buffer_filled;
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.PropertyName:
                                read_buffer_filled = reader.CopyString(buffer);
                                date = DateOnly.Parse(buffer[..read_buffer_filled], __RU);
                                break;

                            case JsonTokenType.StartObject:
                                while (reader.Read() && reader.CurrentDepth == 3)
                                    switch (reader.TokenType)
                                    {
                                        case JsonTokenType.PropertyName when reader.ValueTextEquals("pairs"u8):
                                            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) throw new FormatException();

                                            Dictionary<TimeOnly, ScheduleLectorLesson> day_lessons = [];
                                            while (reader.Read() && reader.CurrentDepth == 4)
                                            {
                                                if (reader.TokenType != JsonTokenType.PropertyName) throw new FormatException();

                                                read_buffer_filled = reader.CopyString(buffer);
                                                var time = TimeOnly.Parse(buffer[..read_buffer_filled], __RU);

                                                if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) throw new FormatException();

                                                var lesson = JsonSerializer.Deserialize<ScheduleLectorLesson>(ref reader, options)!;
                                                var start = lesson.Start;
                                                lesson = lesson with { Start = new(date.Year, date.Month, date.Day, start.Hour, start.Minute, start.Second) };

                                                lessons.Add(lesson);
                                            }
                                            break;
                                    }
                                break;
                        }
                    }
                    break;
            }

        return new(teacher, DateTime.Now, groups_counts, lessons.ToArray());
    }

    public override void Write(Utf8JsonWriter writer, ScheduleLector value, JsonSerializerOptions options) => throw new NotSupportedException();
}

file class ScheduleLessonInfoJsonConverter : JsonConverter<ScheduleLectorLesson>
{
    private static readonly CultureInfo __RU = CultureInfo.GetCultureInfo("ru-RU");

    public override ScheduleLectorLesson? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        TimeOnly start = default, end = default;
        string name = null!;
        string[] groups = null!;
        string[] types = null!;
        Dictionary<Guid, string> rooms = null!;

        Span<char> buffer = stackalloc char[8];

        while (reader.Read())
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName when reader.ValueTextEquals("time_start"u8):
                    if (!reader.Read() || reader.TokenType != JsonTokenType.String) throw new FormatException();
                    reader.CopyString(buffer);
                    start = TimeOnly.Parse(buffer, __RU);
                    break;

                case JsonTokenType.PropertyName when reader.ValueTextEquals("time_end"u8):
                    if (!reader.Read() || reader.TokenType != JsonTokenType.String) throw new FormatException();
                    reader.CopyString(buffer);
                    end = TimeOnly.Parse(buffer, __RU);
                    break;

                case JsonTokenType.PropertyName when reader.ValueTextEquals("name"u8):
                    if (!reader.Read() || reader.TokenType != JsonTokenType.String) throw new FormatException();
                    name = reader.GetString()!.Trim();
                    break;

                case JsonTokenType.PropertyName when reader.ValueTextEquals("groups"u8):
                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray) throw new FormatException();
                    groups = JsonSerializer.Deserialize<string[]>(ref reader, options)!;
                    break;

                case JsonTokenType.PropertyName when reader.ValueTextEquals("types"u8):
                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray) throw new FormatException();
                    types = JsonSerializer.Deserialize<string[]>(ref reader, options)!;
                    break;

                case JsonTokenType.PropertyName when reader.ValueTextEquals("rooms"u8):
                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) throw new FormatException();
                    rooms = JsonSerializer.Deserialize<Dictionary<Guid, string>>(ref reader, options)!;
                    break;
            }

        return new(name, DateTime.MinValue + start.ToTimeSpan(), end - start, types, groups, rooms);
    }

    public override void Write(Utf8JsonWriter writer, ScheduleLectorLesson value, JsonSerializerOptions options) => throw new NotSupportedException();
}

[JsonSerializable(typeof(ScheduleLector))]
[JsonSerializable(typeof(ScheduleLectorLesson))]
[JsonSourceGenerationOptions(AllowTrailingCommas = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal partial class ScheduleLectorJsonContext : JsonSerializerContext
{

}