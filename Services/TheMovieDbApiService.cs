using System;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using RenamerCore.Models;

namespace RenamerCore.Services
{
    public class TheMovieDbApiService
    {
        private const string API_URL = "https://api.themoviedb.org/3";

        private string _apiKey;

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
