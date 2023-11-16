namespace MrSquashWatcher.Models;

public record Appointment
{
    [JsonProperty(PropertyName = "start_time")]
    [JsonConverter(typeof(TimeOnlyConverter), "HH:mm")]
    public TimeOnly StartTime { get; set; }

    [JsonProperty(PropertyName = "end_time")]
    [JsonConverter(typeof(TimeOnlyConverter), "HH:mm")]
    public TimeOnly EndTime { get; set; }

    [JsonProperty(PropertyName = "busy")]
    public bool Reserved { get; set; }

    [JsonProperty(PropertyName = "enabled")]
    public bool Enabled { get; set; }
}
