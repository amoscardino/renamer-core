using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using RenamerCore.Services;

namespace RenamerCore.Commands;

[Command("m", FullName = "Movie Renamer", Description = "Renames movie files based on The Movie DB searches.")]
[HelpOption]
public class MovieCommand
{
    [Option("-i|--input <PATH>", "Input path. Defaults to current directory.", CommandOptionType.SingleValue)]
    public string InputPath { get; set; }

    [Option("-o|--output <PATH>", "Output path. Defaults to input path.", CommandOptionType.SingleValue)]
    public string OutputPath { get; set; }

    [Option("-y|--yes", "Skip confirmation prompt. Be careful as files will be renamed immediately.", CommandOptionType.NoValue)]
    public bool SkipConfirmation { get; set; }

    [Option("--verbose", "Verbose output.", CommandOptionType.NoValue)]
    public bool Verbose { get; set; }

    private async Task OnExecuteAsync(CommandLineApplication application, IConsole console, MovieRenamerService movieRenamer)
        => await movieRenamer.RenameAsync(InputPath, OutputPath, SkipConfirmation, Verbose);
}
