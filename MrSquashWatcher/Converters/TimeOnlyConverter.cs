namespace MrSquashWatcher.Converters;

public class TimeOnlyConverter : JsonConverter<TimeOnly>
{
    private readonly string _serializationFormat;

    public TimeOnlyConverter() : this(null)
    {
    }

    public TimeOnlyConverter(string? serializationFormat)
    {
        _serializationFormat = serializationFormat ?? "HH:mm:ss.fff";
    }

    public override TimeOnly ReadJson(JsonReader reader, Type objectType, TimeOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var value = (string)reader.Value;
        return TimeOnly.Parse(value!);
    }

    public override void WriteJson(JsonWriter writer, TimeOnly value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(_serializationFormat));
    }
}
