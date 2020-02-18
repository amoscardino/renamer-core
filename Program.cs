using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RenamerCore.Commands;
using RenamerCore.Services;

namespace RenamerCore
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            await Host
                .CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddMemoryCache();
                    services.AddTransient<FileService>();
                    services.AddTransient<ConfigService>();
                    services.AddTransient<TheMovieDbApiService>();
                    services.AddTransient<TheTvDbApiService>();
                    services.AddTransient<MovieRenamerService>();
                    services.AddTransient<ShowRenamerService>();
                })
                .RunCommandLineApplicationAsync<MainCommand>(args);
        }
    }
}
