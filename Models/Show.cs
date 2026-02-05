using System;
using System.Text.Json.Serialization;

namespace RenamerCore.Models;

public class Show
{
    public long Id { get; set; }

    public string Name { get; set; }

    [JsonPropertyName("first_air_date")]
    public DateTime? FirstAirDate { get; set; }
}
