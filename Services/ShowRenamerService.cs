using System;
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

namespace RenamerCore.Services
{
    public partial class ShowRenamerService
    {
        private IMemoryCache _cache;
        private IConsole _console;
        private FileService _fileService;
        private TheMovieDbApiService _tmdbApiService;

        private bool _verbose;

        public ShowRenamerService(IMemoryCache cache, IConsole console, FileService fileService, TheMovieDbApiService tmdbApiService)
        {
            _cache = cache;
            _console = console;
            _fileService = fileService;
            _tmdbApiService = tmdbApiService;
        }

        public async Task RenameAsync(string inputPath, string outputPath, bool filesOnly, bool recurse, bool skipConfirmation, bool verbose)
        {
            _verbose = verbose;

            inputPath = Path.GetFullPath(inputPath ?? Directory.GetCurrentDirectory());
            outputPath = Path.GetFullPath(outputPath ?? inputPath);

            if (!File.GetAttributes(inputPath).HasFlag(FileAttributes.Directory))
            {
                _console.WriteLine("Input path must be a directory, not a file.");
                return;
            }

            if (_verbose)
            {
                _console.WriteLine($"* Using Input Path: {inputPath}");
                _console.WriteLine($"* Using Output Path: {outputPath}");
                _console.WriteLine();
            }

            var files = _fileService.GetFiles(inputPath, recurse);

            if (!files.Any())
                return;

            await MatchFilesAsync(files, outputPath, filesOnly);

            var anyToRename = files.Any(match => !match.NewPath.IsNullOrWhiteSpace());

            if (anyToRename && (skipConfirmation || Prompt.GetYesNo("Look good?", true)))
                _fileService.RenameFiles(files);
            else
                _console.WriteLine("Nothing has been changed.");
        }

        private async Task MatchFilesAsync(List<FileMatch> files, string outputPath, bool filesOnly)
        {
            foreach (var file in files)
            {
                _console.WriteLine($"{Path.GetFileName(file.OldPath)} =>");

                var fileName = Path.GetFileNameWithoutExtension(file.OldPath);
                var ext = Path.GetExtension(file.OldPath);
                var regexMatch = Regex.Match(fileName, @"(?:\[{2}(\d+)\]{2})?(.+?)s?(\d+)[ex](\d+)[ex-]{0,2}(\d+)?", RegexOptions.IgnoreCase);

                if (!regexMatch.Success)
                {
                    _console.WriteLine($"\tUnable to parse file name.");
                    continue;
                }

                var show = (Show)null;

                if (regexMatch.Groups[1].Success)
                {
                    var showId = int.TryParse(regexMatch.Groups[1].Value, out var id) ? id : 0;

                    show = await FindShowByIdAsync(showId);
                }

                if (show == null)
                {
                    var showName = regexMatch.Groups[2].Value.Clean();

                    show = await FindShowByNameAsync(showName);
                }

                if (show == null)
                {
                    _console.WriteLine($"\tNO MATCH!");
                    _console.WriteLine();
                    continue;
                }

                var seasonNumber = int.TryParse(regexMatch.Groups[3].Value, out var s) ? s : 0;
                var startEpisodeNumber = int.TryParse(regexMatch.Groups[4].Value, out var e) ? e : 0;
                var startEpisode = await FindEpisodeAsync(show.Id, seasonNumber, startEpisodeNumber);
                var endEpisode = (Episode)null;

                if (startEpisode == null)
                {
                    _console.WriteLine($"\tNO MATCH!");
                    _console.WriteLine();
                    continue;
                }

                if (regexMatch.Groups[5].Success)
                {
                    var endEpisodeNumber = int.TryParse(regexMatch.Groups[5].Value, out var e1) ? e1 : 0;

                    endEpisode = await FindEpisodeAsync(show.Id, seasonNumber, endEpisodeNumber);

                    if (endEpisode == null)
                    {
                        _console.WriteLine($"\tNO MATCH!");
                        _console.WriteLine();
                        continue;
                    }
                }

                var newfilePath = GetNewFilePath(show, startEpisode, endEpisode, ext, filesOnly);

                file.NewPath = Path.Combine(outputPath, newfilePath);

                _console.WriteLine($"\t{newfilePath}");
                _console.WriteLine();
            }
        }

        private async Task<Show> FindShowByIdAsync(int id)
        {
            if (id == 0)
                return null;

            if (_verbose)
                _console.WriteLine($"\t\t* Searching for ID: \"{id}\"");

            var show = (Show)null;
            var cacheKey = $"TmdbShowId-{id}";

            if (!_cache.TryGetValue(cacheKey, out show))
            {
                try
                {
                    show = await _tmdbApiService.GetShowByIdAsync(id);
                }
                catch { }

                if (show != null)
                    _cache.Set(cacheKey, show);
            }

            return show;
        }

        private async Task<Show> FindShowByNameAsync(string name)
        {
            if (name.IsNullOrWhiteSpace())
                return null;

            if (_verbose)
                _console.WriteLine($"\t\t* Searching for: \"{name}\"");

            var show = (Show)null;
            var cacheKey = $"TmdbShow-{name}";

            if (!_cache.TryGetValue(cacheKey, out show))
            {
                try
                {
                    show = await _tmdbApiService.GetShowAsync(name);
                }
                catch { }

                if (show != null)
                    _cache.Set(cacheKey, show);
            }

            return show ?? await FindShowByNameAsync(name.DropLastWord());
        }

        private async Task<Episode> FindEpisodeAsync(long showId, int season, int episode)
        {
            try
            {
                if (_verbose)
                    _console.WriteLine($"\t\t* Searching for: Show: {showId}, Season: {season}, Episode: {episode}.");

                return await _tmdbApiService.GetEpisodeAsync(showId, season, episode);
            }
            catch
            {
                return null;
            }
        }

        public string GetNewFilePath(Show show, Episode startEpisode, Episode endEpisode, string ext, bool filesOnly)
        {
            var sb = new StringBuilder();
            sb.Append(show.Name);
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

            sb.Append(ext);

            var newFileName = sb.ToString().CleanFileName();

            if (filesOnly)
                return newFileName;

            var showFolder = show.Name.CleanPath();
            var seasonFolder = $"Season {startEpisode.SeasonNumber.ToString().PadLeft(2, '0')}";

            return Path.Combine(showFolder, seasonFolder, newFileName);
        }
    }
}
