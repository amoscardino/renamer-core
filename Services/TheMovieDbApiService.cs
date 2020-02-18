using System;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using RenamerCore.Extensions;
using RenamerCore.Models;

namespace RenamerCore.Services
{
    public class TheMovieDbApiService
    {
        private const string API_URL = "https://api.themoviedb.org/3";

        private string _apiKey;

        public TheMovieDbApiService(ConfigService configService)
        {
            _apiKey = configService.GetValue("TheMovieDbApiKey");
        }

        public async Task<TmdbResult> GetMovieAsync(string name)
        {
            var response = await API_URL
                    .AppendPathSegments("search", "movie")
                    .SetQueryParams(new
                    {
                        api_key = _apiKey,
                        query = name
                    })
                    .GetJsonAsync<TmdbResponse>();

            if (response != null && response.Results != null && response.Results.Any())
                return response.Results.First();

            return null;
        }
    }
}
