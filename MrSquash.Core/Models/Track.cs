using System.Collections.Generic;

namespace MrSquash.Core
{
    public class Track
    {
        public string Name { get; set; }
        public List<Reservation> Times { get; set; }
    }
}
