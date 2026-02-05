using System.Text.Json.Serialization;

namespace RenamerCore.Models;

public class Episode
{
    [JsonPropertyName("episode_number")]
    public int EpisodeNumber { get; set; }

    [JsonPropertyName("season_number")]
    public int SeasonNumber { get; set; }

    public string Name { get; set; }
}
