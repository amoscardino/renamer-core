using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using RenamerCore.Services;

namespace RenamerCore.Commands
{
    [Command("s", FullName = "Show Renamer", Description = "Renames show episode files based on TvDB searches.")]
    [HelpOption]
    public class ShowCommand
    {
        [Option("-i|--input <PATH>", "Input path. Defaults to current directory.", CommandOptionType.SingleValue)]
        public string InputPath { get; set; }

        [Option("-o|--output <PATH>", "Output path. Defaults to input path.", CommandOptionType.SingleValue)]
        public string OutputPath { get; set; }

        [Option("-y|--yes", "Skip confirmation prompt. Be careful as files will be renamed immediately.", CommandOptionType.NoValue)]
        public bool SkipConfirmation { get; set; }

        [Option("--verbose", "Verbose output.", CommandOptionType.NoValue)]
        public bool Verbose { get; set; }

        [Option("-di|--dvd-input", "Input is DVD Order. Reads season and episode numbers as DVD order instead of aired order.", CommandOptionType.NoValue)]
        public bool DvdOrderInput { get; set; }

        [Option("-do|--dvd-output", "DVD Order Output. Outputs files using season and episode numbers as DVD order instead of aired order.", CommandOptionType.NoValue)]
        public bool DvdOrderOutput { get; set; }

        [Option("-f|--files-only", "Files Only. Will not create folders for shows or seasons and will only rename the files.", CommandOptionType.NoValue)]
        public bool FilesOnly { get; set; }

        [Option("-r|--recurse", "Recursive search. Will scan folders and subfolders of the Input Path.", CommandOptionType.NoValue)]
        public bool Recurse { get; set; }

        private async Task OnExecuteAsync(CommandLineApplication application, IConsole console, ShowRenamerService showRenamer)
        {
            await showRenamer.RenameAsync(InputPath, OutputPath, DvdOrderInput, DvdOrderOutput, FilesOnly, Recurse, SkipConfirmation, Verbose);
        }
    }
}