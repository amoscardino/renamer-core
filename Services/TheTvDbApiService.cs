using System;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using RenamerCore.Extensions;
using RenamerCore.Models;

namespace RenamerCore.Services
{
    public class TheTvDbApiService
    {
        private const string API_URL = "https://api.thetvdb.com/";

        private string _apiKey;
        private string _apiToken;

        /// <summary>
        /// Creates a new instance of The TV DB API Service
        /// </summary>
        /// <param name="configService"></param>
        public TheTvDbApiService(ConfigService configService)
        {
            _apiKey = configService.GetValue("TvDbApiKey");
        }

        /// <summary>
        /// Searches for a show by name using The TV DB API.
        /// If any results are found, the first is returned.
        /// If no results, null is returned.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<TvdbShow> GetShowAsync(string name)
        {
            await GetApiTokenAsync();

            var response = await API_URL
                    .AppendPathSegments("search", "series")
                    .SetQueryParams(new { name = name })
                    .WithOAuthBearerToken(_apiToken)
                    .GetJsonAsync<TvdbShowReponse>();

            if (response != null && response.Data != null && response.Data.Any())
                return response.Data.First();

            return null;
        }

        /// <summary>
        /// Attempts to find details of an episode for a given show using The TV DB API.
        /// Returns null if no episode is found.
        /// </summary>
        /// <param name="showId"></param>
        /// <param name="season"></param>
        /// <param name="episode"></param>
        /// <param name="useDvdOrder"></param>
        /// <returns></returns>
        public async Task<TvdbEpisode> GetEpisodeAsync(long showId, int season, int episode, bool useDvdOrder)
        {
            await GetApiTokenAsync();

            var response = await API_URL
                    .AppendPathSegments("series", showId, "episodes", "query")
                    .SetQueryParams(new
                    {
                        airedSeason = useDvdOrder ? null : season.ToString(),
                        airedEpisode = useDvdOrder ? null : episode.ToString(),
                        dvdSeason = useDvdOrder ? season.ToString() : null,
                        dvdEpisode = useDvdOrder ? episode.ToString() : null,
                    })
                    .WithOAuthBearerToken(_apiToken)
                    .GetJsonAsync<TvdbEpisodeReponse>();

            // If we get a response back, and it contains at least one result, return the first result
            if (response != null && response.Data != null && response.Data.Any())
                return response.Data.First();

            return null;
        }

        /// <summary>
        /// Sets the login token for future API requests for this instance.
        /// Nothing will happen if the token already exists.await
        /// </summary>
        /// <returns></returns>
        private async Task GetApiTokenAsync()
        {
            if (!_apiToken.IsNullOrWhiteSpace())
                return;

            var response = await API_URL
                    .AppendPathSegment("login")
                    .PostJsonAsync(new { apikey = _apiKey })
                    .ReceiveJson();

            _apiToken = response.token;
        }
    }
}