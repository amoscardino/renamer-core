using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using RenamerCore.Services;
using RenamerCore.Extensions;
using System.Reflection;

namespace RenamerCore.Commands
{
    [Command(Name = "renamer",
             FullName = "Renamer Core",
             Description = "Renamer for movies and shows for Plex.")]
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
