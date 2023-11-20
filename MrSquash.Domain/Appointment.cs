using MrSquash.Domain.Converters;
using Newtonsoft.Json;

namespace MrSquash.Domain;

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

    [JsonProperty(PropertyName = "price")]
    public int Price { get; set; }
}
