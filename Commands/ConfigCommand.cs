using System;
using McMaster.Extensions.CommandLineUtils;
using RenamerCore.Services;
using RenamerCore.Extensions;

namespace RenamerCore.Commands
{
    [Command("config", FullName = "Configuration", Description = "Used to set API keys for The TV DB and The Movie DB.")]
    [HelpOption]
    public class ConfigCommand
    {
        [Option("-tmdb <APIKEY>", "Sets the API key for The Movie DB API.", CommandOptionType.SingleValue)]
        public string TmdbApiKey { get; set; }

        [Option("-tvdb <APIKEY>", "Sets the API key for The TV DB API.", CommandOptionType.SingleValue)]
        public string TvdbApiKey { get; set; }

        private void OnExecute(CommandLineApplication application, IConsole console, ConfigService configService)
        {
            if (TmdbApiKey.IsNullOrWhiteSpace() && TvdbApiKey.IsNullOrWhiteSpace())
            {
                console.WriteLine("Current Keys:");
                console.WriteLine($"\tThe Movie DB: {configService.GetValue("TheMovieDbApiKey")}");
                console.WriteLine($"\tThe TV DB: {configService.GetValue("TvDbApiKey")}");
                console.WriteLine();

                application.ShowHelp();
                return;
            }

            if (!TmdbApiKey.IsNullOrWhiteSpace())
                configService.SetValue("TheMovieDbApiKey", TmdbApiKey);

            if (!TvdbApiKey.IsNullOrWhiteSpace())
                configService.SetValue("TvDbApiKey", TvdbApiKey);
        }
    }
}