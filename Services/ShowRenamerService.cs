using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Flurl;
using Flurl.Http;
using RenamerCore.Models;
using RenamerCore.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace RenamerCore.Services
{
    public partial class ShowRenamerService
    {
        private IMemoryCache _cache;
        private IConsole _console;
        private FileService _fileService;
        private TheTvDbApiService _tvDbApiService;

        private bool _verbose;

        public ShowRenamerService(IMemoryCache cache, IConsole console, FileService fileService, TheTvDbApiService tvDbApiService)
        {
            _cache = cache;
            _console = console;
            _fileService = fileService;
            _tvDbApiService = tvDbApiService;
        }

        public async Task RenameAsync(string inputPath, string outputPath, bool dvdOrderInput, bool dvdOrderOutput, bool recurse, bool skipConfirmation, bool verbose)
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

            await MatchFilesAsync(files, outputPath, dvdOrderInput, dvdOrderOutput);

            var anyToRename = files.Any(x => !string.IsNullOrWhiteSpace(x.NewPath));

            if (anyToRename && (skipConfirmation || Prompt.GetYesNo("Look good?", true)))
                _fileService.RenameFiles(files);
            else
                _console.WriteLine("Nothing has been changed.");
        }

        private async Task MatchFilesAsync(List<FileMatch> files, string outputPath, bool dvdOrderInput, bool dvdOrderOutput)
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

                var show = (TvdbShow)null;

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
                var startEpisode = await FindEpisodeAsync(show.Id, seasonNumber, startEpisodeNumber, dvdOrderInput);
                var endEpisode = (TvdbEpisode)null;

                if (startEpisode == null)
                {
                    _console.WriteLine($"\tNO MATCH!");
                    _console.WriteLine();
                    continue;
                }

                if (regexMatch.Groups[5].Success)
                {
                    var endEpisodeNumber = int.TryParse(regexMatch.Groups[5].Value, out var e1) ? e1 : 0;

                    endEpisode = await FindEpisodeAsync(show.Id, seasonNumber, endEpisodeNumber, dvdOrderInput);

                    if (endEpisode == null)
                    {
                        _console.WriteLine($"\tNO MATCH!");
                        _console.WriteLine();
                        continue;
                    }
                }

                var newfilePath = GetNewFilePath(show, startEpisode, endEpisode, ext, dvdOrderOutput);

                file.NewPath = Path.Combine(outputPath, newfilePath);

                _console.WriteLine($"\t{newfilePath}");
                _console.WriteLine();
            }
        }

        private async Task<TvdbShow> FindShowByIdAsync(int id)
        {
            if (id == 0)
                return null;

            if (_verbose)
                _console.WriteLine($"\t\t* Searching for ID: \"{id}\"");

            var show = (TvdbShow)null;
            var cacheKey = $"TvdbShowId-{id}";

            if (!_cache.TryGetValue(cacheKey, out show))
            {
                try
                {
                    show = await _tvDbApiService.GetShowByIdAsync(id);
                }
                catch { }

                if (show != null)
                    _cache.Set(cacheKey, show);
            }

            return show;
        }

        private async Task<TvdbShow> FindShowByNameAsync(string name)
        {
            if (name.IsNullOrWhiteSpace())
                return null;

            if (_verbose)
                _console.WriteLine($"\t\t* Searching for: \"{name}\"");

            var show = (TvdbShow)null;
            var cacheKey = $"TvdbShow-{name}";

            if (!_cache.TryGetValue(cacheKey, out show))
            {
                try
                {
                    show = await _tvDbApiService.GetShowByNameAsync(name);
                }
                catch { }

                if (show != null)
                    _cache.Set(cacheKey, show);
            }

            return show ?? await FindShowByNameAsync(name.DropLastWord());
        }

        private async Task<TvdbEpisode> FindEpisodeAsync(long showId, int season, int episode, bool useDvdOrder)
        {
            try
            {
                if (_verbose)
                {
                    _console.WriteLine($"\t\t* Searching for: Show: {showId}, Season: {season}, Episode: {episode}.");

                    if (useDvdOrder)
                        _console.WriteLine($"\t\t* Using DVD Ordering.");
                }

                return await _tvDbApiService.GetEpisodeAsync(showId, season, episode, useDvdOrder);
            }
            catch
            {
                return null;
            }
        }

        public string GetNewFilePath(TvdbShow show, TvdbEpisode startEpisode, TvdbEpisode endEpisode, string ext, bool useDvdOrder)
        {
            var seasonNumber = useDvdOrder ? startEpisode.DvdSeason ?? startEpisode.AiredSeason : startEpisode.AiredSeason;
            var startEpisodeNumber = useDvdOrder ? startEpisode.DvdEpisodeNumber ?? startEpisode.AiredEpisodeNumber : startEpisode.AiredEpisodeNumber;

            var newFileName = $"{show.SeriesName} - s{seasonNumber.PadLeft(2, '0')}e{startEpisodeNumber.PadLeft(2, '0')}";

            if (endEpisode != null)
            {
                var endEpisodeNumber = useDvdOrder ? endEpisode.DvdEpisodeNumber ?? endEpisode.AiredEpisodeNumber : endEpisode.AiredEpisodeNumber;

                newFileName += $"-e{endEpisodeNumber.PadLeft(2, '0')}";
            }

            newFileName += $" - {startEpisode.EpisodeName}";

            if (endEpisode != null)
                newFileName += $" - {endEpisode.EpisodeName}";

            newFileName = $"{newFileName}{ext}".CleanFileName();

            var showFolder = show.SeriesName.CleanPath();
            var seasonFolder = $"Season {seasonNumber.PadLeft(2, '0')}";

            return Path.Combine(showFolder, seasonFolder, newFileName);
        }
    }
}
