using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MrSquash.Core.Converters
{
    public class MyDateConverter : JsonConverter<DateTime>
    {
        public override DateTime ReadJson(JsonReader reader, Type objectType, [AllowNull] DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            string isoDate = jObject.Value<string>("iso");

            return DateTime.Parse(isoDate);
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] DateTime value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
