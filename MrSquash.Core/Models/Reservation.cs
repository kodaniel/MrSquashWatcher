using Newtonsoft.Json;
using System;

namespace MrSquash.Core
{
    public class Reservation
    {
        [JsonProperty(PropertyName = "start_time")]
        public TimeSpan StartTime { get; set; }

        [JsonProperty(PropertyName = "end_time")]
        public TimeSpan EndTime { get; set; }

        public bool Busy { get; set; }

        public bool Enabled { get; set; }
    }
}
