using System.Text.Json.Serialization;

using MathCore;

namespace Dep406.ScheduleAPI;

public record SchedultLectorLesson
{
    [JsonPropertyName("name")] 
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("start")] 
    public DateTime Start { get; set; }
    
    [JsonPropertyName("duration")] 
    public TimeSpan Duration { get; set; }

    [JsonIgnore]
    public DateTime End => Start + Duration;

    [JsonIgnore]
    public Interval<DateTime> Interval => new(Start, End, true);

    [JsonPropertyName("lessonType")]
    public IEnumerable<string> LessonType { get; set; } = null!;
    
    [JsonPropertyName("groups")] 
    public IReadOnlyList<string> Groups { get; set; } = null!;
    
    [JsonPropertyName("rooms")] 
    public IDictionary<Guid, string> Rooms { get; set; } = null!;
}

