using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RenamerCore.Models;
using RenamerCore.Extensions;
using System.Text;

namespace RenamerCore.Services;

public class MovieRenamerService(IConsole console, FileService fileService, TheMovieDbApiService movieDbApiService)
{
    private bool _verbose;

    public async Task RenameAsync(string inputPath, string outputPath, bool skipConfirmation, bool verbose)
    {
        _verbose = verbose;

        inputPath = Path.GetFullPath(inputPath ?? Directory.GetCurrentDirectory());
        outputPath = Path.GetFullPath(outputPath ?? inputPath);

        if (!File.GetAttributes(inputPath).HasFlag(FileAttributes.Directory))
        {
            console.WriteLine("Input path must be a directory, not a file.");
            return;
        }

        if (_verbose)
        {
            console.WriteLine($"* Using Input Path: {inputPath}");
            console.WriteLine($"* Using Output Path: {outputPath}");
            console.WriteLine();
        }

        var files = fileService.GetFiles(inputPath);

        if (!files.Any())
            return;

        await MatchFilesAsync(files, outputPath);

        var anyToRename = files.Any(match => !match.NewPath.IsNullOrWhiteSpace());

        if (anyToRename && (skipConfirmation || Prompt.GetYesNo("Look good?", true)))
            fileService.RenameFiles(files);
        else
            console.WriteLine("Nothing has been changed.");
    }

    private async Task MatchFilesAsync(List<FileMatch> files, string outputPath)
    {
        foreach (var file in files)
        {
            console.WriteLine($"{Path.GetFileName(file.OldPath)} =>");

            var ext = Path.GetExtension(file.OldPath);
            var fileName = Path.GetFileNameWithoutExtension(file.OldPath);
            var cleanFileName = fileName.Clean();
            var movie = await FindMovieRecursiveAsync(cleanFileName);

            if (movie == null)
            {
                console.WriteLine($"\tNO MATCH!");
                continue;
            }

            var newFolderName = GetNewFolderName(movie);
            var newFileName = GetNewFileName(movie, fileName, ext);

            file.NewPath = Path.Combine(outputPath, newFolderName, newFileName);

            console.WriteLine($"\t{Path.Combine(newFolderName, newFileName)}");
            console.WriteLine();
        }
    }

    private async Task<Movie> FindMovieRecursiveAsync(string name)
    {
        if (_verbose)
            console.WriteLine($"\t\t* Searching for: \"{name}\"");

        if (name.IsNullOrWhiteSpace())
            return null;

        try
        {
            var result = await movieDbApiService.GetMovieAsync(name);

            if (result != null)
                return result;
        }
        catch { }

        // Check if the title has [year - title] format. (1900 - 2099)
        return Regex.IsMatch(name, @"^[1,2]{1}[9,0]{1}\d{2}\s{1,}[A-Z,a-z]+.*")
            ? await FindMovieRecursiveAsync(name.DropFirstWord())
            : await FindMovieRecursiveAsync(name.DropLastWord());
    }

    private string GetNewFileName(Movie movie, string oldFileName, string ext)
    {
        var sb = new StringBuilder();
        sb.Append(GetNewFolderName(movie));

        if (Regex.IsMatch(oldFileName, @"4k|2160|uhd", RegexOptions.IgnoreCase))
            sb.Append(" - 4k");

        if (Regex.IsMatch(oldFileName, @"1080", RegexOptions.IgnoreCase))
            sb.Append(" - 1080p");

        if (Regex.IsMatch(ext, @"srt|sub", RegexOptions.IgnoreCase))
            sb.Append(".en");

        sb.Append(ext);

        return sb.ToString().CleanFileName();
    }

    private string GetNewFolderName(Movie movie)
    {
        var sb = new StringBuilder();
        sb.Append(movie.Title);
        sb.AppendFormat(" ({0})", movie.ReleaseDate.Year);

        return sb.ToString().CleanFileName();
    }
}
