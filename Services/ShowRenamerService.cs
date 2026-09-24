using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using RenamerCore.Models;
using RenamerCore.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace RenamerCore.Services;

public partial class ShowRenamerService(IMemoryCache cache, IConsole console, FileService fileService, TheMovieDbApiService tmdbApiService)
{
    private bool _verbose;

    public async Task RenameAsync(string inputPath, string outputPath, bool filesOnly, bool recurse, bool skipConfirmation, bool verbose)
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

        var files = fileService.GetFiles(inputPath, recurse);

        if (files.Count == 0)
            return;

        await MatchFilesAsync(files, outputPath, filesOnly);

        var anyToRename = files.Any(match => !match.NewPath.IsNullOrWhiteSpace());

        if (anyToRename && (skipConfirmation || Prompt.GetYesNo("Look good?", true)))
            fileService.RenameFiles(files);
        else
            console.WriteLine("Nothing has been changed.");
    }

    private async Task MatchFilesAsync(List<FileMatch> files, string outputPath, bool filesOnly)
    {
        foreach (var file in files)
        {
            console.WriteLine($"{Path.GetFileName(file.OldPath)} =>");

            var fileName = Path.GetFileNameWithoutExtension(file.OldPath);
            var ext = Path.GetExtension(file.OldPath);
            var regexMatch = FileNameRegex().Match(fileName);

            if (!regexMatch.Success)
            {
                console.WriteLine($"\tUnable to parse file name.");
                continue;
            }

            var show = (Show)null;

            if (regexMatch.Groups["id"].Success)
            {
                var showId = int.TryParse(regexMatch.Groups["id"].Value, out var id) ? id : 0;

                show = await FindShowByIdAsync(showId);
            }

            if (show == null)
            {
                var showName = regexMatch.Groups["name"].Value.Clean();

                show = await FindShowByNameAsync(showName);
            }

            if (show == null)
            {
                console.WriteLine($"\tNO MATCH!");
                console.WriteLine();
                continue;
            }

            var seasonNumber = int.TryParse(regexMatch.Groups["season"].Value, out var s) ? s : 0;
            var startEpisodeNumber = int.TryParse(regexMatch.Groups["start"].Value, out var e) ? e : 0;
            var startEpisode = await FindEpisodeAsync(show.Id, seasonNumber, startEpisodeNumber);
            var endEpisode = (Episode)null;

            if (startEpisode == null)
            {
                console.WriteLine($"\tNO MATCH!");
                console.WriteLine();
                continue;
            }

            if (regexMatch.Groups["end"].Success)
            {
                var endEpisodeNumber = int.TryParse(regexMatch.Groups["end"].Value, out var e1) ? e1 : 0;

                endEpisode = await FindEpisodeAsync(show.Id, seasonNumber, endEpisodeNumber);

                if (endEpisode == null)
                {
                    console.WriteLine($"\tNO MATCH!");
                    console.WriteLine();
                    continue;
                }
            }

            var newfilePath = GetNewFilePath(show, startEpisode, endEpisode, ext, filesOnly);

            file.NewPath = Path.Combine(outputPath, newfilePath);

            console.WriteLine($"\t{newfilePath}");
            console.WriteLine();
        }
    }

    // Matches "[[id]]Show Name s01e02", "Show Name 1x02", "Show Name s01e02-e03", and "Show Name Season 1, Episode 2"
    [GeneratedRegex(@"(?:\[{2}(?<id>\d+)\]{2})?(?<name>.+?)(?:season[\s._-]*(?<season>\d+)[\s._-]*,?[\s._-]*episode[\s._-]*(?<start>\d+)|s?(?<season>\d+)[ex](?<start>\d+)[ex-]{0,2}(?<end>\d+)?)", RegexOptions.IgnoreCase)]
    private static partial Regex FileNameRegex();

    private async Task<Show> FindShowByIdAsync(int id)
    {
        if (id == 0)
            return null;

        if (_verbose)
            console.WriteLine($"\t\t* Searching for ID: \"{id}\"");

        var show = (Show)null;
        var cacheKey = $"TmdbShowId-{id}";

        if (!cache.TryGetValue(cacheKey, out show))
        {
            try
            {
                show = await tmdbApiService.GetShowByIdAsync(id);
            }
            catch { }

            if (show != null)
                cache.Set(cacheKey, show);
        }

        return show;
    }

    private async Task<Show> FindShowByNameAsync(string name)
    {
        if (name.IsNullOrWhiteSpace())
            return null;

        if (_verbose)
            console.WriteLine($"\t\t* Searching for: \"{name}\"");

        var show = (Show)null;
        var cacheKey = $"TmdbShow-{name}";

        if (!cache.TryGetValue(cacheKey, out show))
        {
            try
            {
                show = await tmdbApiService.GetShowAsync(name);
            }
            catch { }

            if (show != null)
                cache.Set(cacheKey, show);
        }

        return show ?? await FindShowByNameAsync(name.DropLastWord());
    }

    private async Task<Episode> FindEpisodeAsync(long showId, int season, int episode)
    {
        try
        {
            if (_verbose)
                console.WriteLine($"\t\t* Searching for: Show: {showId}, Season: {season}, Episode: {episode}.");

            return await tmdbApiService.GetEpisodeAsync(showId, season, episode);
        }
        catch
        {
            return null;
        }
    }

    public string GetNewFilePath(Show show, Episode startEpisode, Episode endEpisode, string ext, bool filesOnly)
    {
        var sb = new StringBuilder();
        var showName = show.Name;

        if (show.FirstAirDate != null)
            showName += $" ({show.FirstAirDate:yyyy})";

        sb.Append(showName);

        sb.Append(" - s");
        sb.Append(startEpisode.SeasonNumber.ToString().PadLeft(2, '0'));
        sb.Append("e");
        sb.Append(startEpisode.EpisodeNumber.ToString().PadLeft(2, '0'));

        if (endEpisode != null)
        {
            sb.Append("-e");
            sb.Append(endEpisode.EpisodeNumber.ToString().PadLeft(2, '0'));
        }

        sb.Append(" - ");
        sb.Append(startEpisode.Name);

        if (endEpisode != null)
        {
            sb.Append(" - ");
            sb.Append(endEpisode.Name);
        }

        if (Regex.IsMatch(ext, @"srt|sub", RegexOptions.IgnoreCase))
            sb.Append(".en");

        sb.Append(ext);

        var newFileName = sb.ToString().CleanFileName();

        if (filesOnly)
            return newFileName;

        var showFolder = showName.CleanPath();
        var seasonFolder = $"Season {startEpisode.SeasonNumber.ToString().PadLeft(2, '0')}";

        return Path.Combine(showFolder, seasonFolder, newFileName);
    }
}
