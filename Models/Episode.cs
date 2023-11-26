using Newtonsoft.Json;

namespace RenamerCore.Models;

public class Episode
{
    [JsonProperty("episode_number")]
    public int EpisodeNumber { get; set; }

    [JsonProperty("season_number")]
    public int SeasonNumber { get; set; }

    public string Name { get; set; }
}
