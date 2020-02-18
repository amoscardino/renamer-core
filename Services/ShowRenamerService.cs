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
                var regexMatch = Regex.Match(fileName, @"(.+?)s?(\d+)[ex](\d+)", RegexOptions.IgnoreCase);

                if (!regexMatch.Success)
                {
                    _console.WriteLine($"\tUnable to parse file name.");
                    continue;
                }

                var showName = regexMatch.Groups[1].Value.Clean();
                var show = await FindShowRecursiveAsync(showName);

                if (show == null)
                {
                    _console.WriteLine($"\tNO MATCH!");
                    _console.WriteLine();
                    continue;
                }

                var seasonNumber = int.TryParse(regexMatch.Groups[2].Value, out var s) ? s : 0;
                var episodeNumber = int.TryParse(regexMatch.Groups[3].Value, out var e) ? e : 0;
                var episode = await FindEpisodeAsync(show.Id, seasonNumber, episodeNumber, dvdOrderInput);

                if (episode == null)
                {
                    _console.WriteLine($"\tNO MATCH!");
                    _console.WriteLine();
                    continue;
                }

                var newfilePath = GetNewFilePath(show, episode, ext, dvdOrderOutput);

                file.NewPath = Path.Combine(outputPath, newfilePath);

                _console.WriteLine($"\t{newfilePath}");
                _console.WriteLine();
            }
        }

        private async Task<TvdbShow> FindShowRecursiveAsync(string name)
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
                    show = await _tvDbApiService.GetShowAsync(name);
                }
                catch { }

                if (show != null)
                    _cache.Set(cacheKey, show);
            }

            return show ?? await FindShowRecursiveAsync(name.DropLastWord());
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

        public string GetNewFilePath(TvdbShow show, TvdbEpisode episode, string ext, bool useDvdOrder)
        {
            var seasonNumber = useDvdOrder ? episode.DvdSeason ?? episode.AiredSeason : episode.AiredSeason;
            var episodeNumber = useDvdOrder ? episode.DvdEpisodeNumber ?? episode.AiredEpisodeNumber : episode.AiredEpisodeNumber;

            var newFileName = $"{show.SeriesName}";
            newFileName += $" - s{seasonNumber.PadLeft(2, '0')}e{episodeNumber.PadLeft(2, '0')}";
            newFileName += $" - {episode.EpisodeName}{ext}";
            newFileName = newFileName.CleanFileName();

            var showFolder = show.SeriesName.CleanPath();
            var seasonFolder = $"Season {seasonNumber}";

            return Path.Combine(showFolder, seasonFolder, newFileName);
        }
    }
}
