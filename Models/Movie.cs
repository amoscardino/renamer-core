using System;
using System.Text.Json.Serialization;

namespace RenamerCore.Models;

public class Movie
{
    public long Id { get; set; }

    public string Title { get; set; }

    [JsonPropertyName("release_date")]
    public DateTime ReleaseDate { get; set; }
}
