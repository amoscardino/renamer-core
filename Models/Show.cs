using System;
using Newtonsoft.Json;

namespace RenamerCore.Models
{
    public class Show
    {
        public long Id { get; set; }

        public string Name { get; set; }

        [JsonProperty("first_air_date")]
        public DateTime? FirstAirDate { get; set; }
    }
}
