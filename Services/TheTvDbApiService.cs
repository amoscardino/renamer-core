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

        public TheTvDbApiService(ConfigService configService)
        {
            _apiKey = configService.GetValue("TvDbApiKey");
        }

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