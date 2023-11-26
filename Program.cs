using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RenamerCore.Commands;
using RenamerCore.Services;

try
{
    await Host
        .CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddMemoryCache();
            services.AddTransient<FileService>();
            services.AddTransient<ConfigService>();
            services.AddTransient<TheMovieDbApiService>();
            services.AddTransient<MovieRenamerService>();
            services.AddTransient<ShowRenamerService>();
        })
        .RunCommandLineApplicationAsync<MainCommand>(args);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
