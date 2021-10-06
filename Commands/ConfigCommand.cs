using McMaster.Extensions.CommandLineUtils;
using RenamerCore.Services;
using RenamerCore.Extensions;

namespace RenamerCore.Commands
{
    [Command("config", FullName = "Configuration", Description = "Used to set API key for The Movie DB.")]
    [HelpOption]
    public class ConfigCommand
    {
        [Option("-tmdb <APIKEY>", "Sets the API key for The Movie DB API.", CommandOptionType.SingleValue)]
        public string TmdbApiKey { get; set; }

        private void OnExecute(CommandLineApplication application, IConsole console, ConfigService configService)
        {
            if (TmdbApiKey.IsNullOrWhiteSpace())
            {
                console.WriteLine("Current Keys:");
                console.WriteLine($"\tThe Movie DB: {configService.GetValue("TheMovieDbApiKey")}");
                console.WriteLine();

                application.ShowHelp();
                return;
            }

            if (!TmdbApiKey.IsNullOrWhiteSpace())
                configService.SetValue("TheMovieDbApiKey", TmdbApiKey);
        }
    }
}
