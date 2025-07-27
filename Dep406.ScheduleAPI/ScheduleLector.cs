using Dep406.ScheduleAPI;
using System.Text.Json.Serialization;

namespace ScheduleTelegramBot.ScheduleAPI;

public record ScheduleLector
{
    [JsonPropertyName("id")] 
    public string Id { get; set; } = null!;
    
    [JsonPropertyName("lectorName")] 
    public string LectorName { get; set; } = null!;
    
    [JsonPropertyName("created")] 
    public DateTime Created { get; set; }
    
    [JsonPropertyName("groups")]
    public IReadOnlyDictionary<string, int> Groups { get; set; } = null!;
    
    [JsonPropertyName("lessons")] 
    public IReadOnlyList<SchedultLectorLesson> Lessons { get; set; } = null!;
}

