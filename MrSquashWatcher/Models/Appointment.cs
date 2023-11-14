namespace MrSquashWatcher.Models;

public record Appointment
{
    [JsonProperty(PropertyName = "start_time")]
    public TimeSpan StartTime { get; set; }

    [JsonProperty(PropertyName = "end_time")]
    public TimeSpan EndTime { get; set; }

    public bool Busy { get; set; }

    public bool Enabled { get; set; }
}
