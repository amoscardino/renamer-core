using System.Reflection;
using McMaster.Extensions.CommandLineUtils;

namespace RenamerCore.Commands
{
    [Command(Name = "renamer",
             FullName = "Renamer Core",
             Description = "Renamer of movies and shows for Plex.")]
    [HelpOption]
    [VersionOptionFromMember(MemberName = "GetVersion")]
    [Subcommand(typeof(RenamerCore.Commands.ConfigCommand),
                typeof(RenamerCore.Commands.MovieCommand),
                typeof(RenamerCore.Commands.ShowCommand))]
    public class MainCommand
    {
        private void OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
        }

        private string GetVersion()
        {
            return typeof(MainCommand)
              .Assembly?
              .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
              .InformationalVersion;
        }
    }
}
