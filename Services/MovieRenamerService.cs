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

namespace RenamerCore.Services
{
    public class MovieRenamerService
    {
        private IConsole _console;
        private FileService _fileService;
        private TheMovieDbApiService _movieDbApiService;

        private bool _verbose;

        public MovieRenamerService(IConsole console, FileService fileService, TheMovieDbApiService movieDbApiService)
        {
            _console = console;
            _fileService = fileService;
            _movieDbApiService = movieDbApiService;
        }

        public async Task RenameAsync(string inputPath, string outputPath, bool skipConfirmation, bool verbose)
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

            var files = _fileService.GetFiles(inputPath);

            if (!files.Any())
                return;

            await MatchFilesAsync(files, outputPath);

            var anyToRename = files.Any(match => !match.NewPath.IsNullOrWhiteSpace());

            if (anyToRename && (skipConfirmation || Prompt.GetYesNo("Look good?", true)))
                _fileService.RenameFiles(files);
            else
                _console.WriteLine("Nothing has been changed.");
        }

        private async Task MatchFilesAsync(List<FileMatch> files, string outputPath)
        {
            foreach (var file in files)
            {
                _console.WriteLine($"{Path.GetFileName(file.OldPath)} =>");

                var ext = Path.GetExtension(file.OldPath);
                var fileName = Path.GetFileNameWithoutExtension(file.OldPath);
                var cleanFileName = fileName.Clean();
                var movie = await FindMovieRecursiveAsync(cleanFileName);

                if (movie == null)
                {
                    _console.WriteLine($"\tNO MATCH!");
                    continue;
                }

                var newFileName = GetNewFileName(movie, fileName, ext);

                file.NewPath = Path.Combine(outputPath, newFileName);

                _console.WriteLine($"\t{newFileName}");
                _console.WriteLine();
            }
        }

        private async Task<Movie> FindMovieRecursiveAsync(string name)
        {
            if (_verbose)
                _console.WriteLine($"\t\t* Searching for: \"{name}\"");

            if (name.IsNullOrWhiteSpace())
                return null;

            try
            {
                var result = await _movieDbApiService.GetMovieAsync(name);

                if (result != null)
                    return result;
            }
            catch { }

            //check if the title has [year - title] format. (1900 - 2099)
            string pattern = @"^[1,2]{1}[9,0]{1}\d{2}\s\-\s.+";
            RegexOptions options = RegexOptions.Singleline;
            Match m = Regex.Match(name, pattern, options);
            Boolean foundYearFormat = m.Success;
            _console.WriteLine(m.Value);
            _console.WriteLine(m.Success);
            if(foundYearFormat){
                return await FindMovieRecursiveAsync(name.DropFirstWord());
            }else{
                
                return await FindMovieRecursiveAsync(name.DropLastWord());
            }
        }

        private string GetNewFileName(Movie movie, string oldFileName, string ext)
        {
            var sb = new StringBuilder();
            sb.Append(movie.Title);
            sb.AppendFormat(" ({0})", movie.ReleaseDate.Year);

            if (Regex.IsMatch(oldFileName, @"4k|2160|uhd", RegexOptions.IgnoreCase))
                sb.Append(" - 4k");

            if (Regex.IsMatch(oldFileName, @"1080", RegexOptions.IgnoreCase))
                sb.Append(" - 1080p");

            if (Regex.IsMatch(ext, @"srt|sub", RegexOptions.IgnoreCase))
                sb.Append(".en");

            sb.Append(ext);

            return sb.ToString().CleanFileName();
        }
    }
}