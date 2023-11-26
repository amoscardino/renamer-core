using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using RenamerCore.Models;

namespace RenamerCore.Services;

public class TheMovieDbApiService
{
    private const string API_URL = "https://api.themoviedb.org/3";

    private readonly string _apiKey;

    /// <summary>
    /// Creates a new instance of The Movie DB API Service
    /// </summary>
    /// <param name="configService"></param>
    public TheMovieDbApiService(ConfigService configService)
    {
        _apiKey = configService.GetValue("TheMovieDbApiKey");
    }

    /// <summary>
    /// Searches The Movie DB for a movie by name.
    /// If any matches are found, the first is returned.
    /// If no matches, null is returned.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<Movie> GetMovieAsync(string name)
    {
        var response = await API_URL
                .AppendPathSegments("search", "movie")
                .SetQueryParams(new
                {
                    api_key = _apiKey,
                    query = name
                })
                .GetJsonAsync<MovieResponse>();

        if (response != null && response.Results != null && response.Results.Any())
            return response.Results.First();

        return null;
    }

    /// <summary>
    /// Searches The Movie DB for a TV show by name.
    /// If any matches are found, the first is returned.
    /// If no matches, null is returned.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<Show> GetShowAsync(string name)
    {
        var response = await API_URL
                .AppendPathSegments("search", "tv")
                .SetQueryParams(new
                {
                    api_key = _apiKey,
                    query = name
                })
                .GetJsonAsync<ShowResponse>();

        if (response != null && response.Results != null && response.Results.Any())
            return response.Results.First();

        return null;
    }

    /// <summary>
    /// Searches for a show by ID using The TV DB API.
    /// If any results are found, the first is returned.
    /// If no results, null is returned.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Show> GetShowByIdAsync(int id)
    {
        var response = await API_URL
                .AppendPathSegments("tv", id)
                .SetQueryParam("api_key", _apiKey)
                .GetJsonAsync<Show>();

        return response;
    }

    /// <summary>
    /// Attempts to find details of an episode for a given show using The Movie DB API.
    /// Returns null if no episode is found.
    /// </summary>
    /// <param name="showId"></param>
    /// <param name="seasonNumber"></param>
    /// <param name="episodeNumber"></param>
    /// <returns></returns>
    public async Task<Episode> GetEpisodeAsync(long showId, int seasonNumber, int episodeNumber)
    {
        var episode = await API_URL
                .AppendPathSegments("tv", showId, "season", seasonNumber, "episode", episodeNumber)
                .SetQueryParam("api_key", _apiKey)
                .GetJsonAsync<Episode>();

        return episode;
    }
}
