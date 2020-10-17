using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using RenamerCore.Models;
using RenamerCore.Extensions;
using McMaster.Extensions.CommandLineUtils;

namespace RenamerCore.Services
{
    public class FileService
    {
        private IConsole _console;

        /// <summary>
        /// Creates a new instance of the File Service.
        /// </summary>
        /// <param name="console"></param>
        public FileService(IConsole console)
        {
            _console = console;
        }

        /// <summary>
        /// Retreives all files from a directory as FileMatch objects. The path for each file
        /// will be the OldPath value and NewPath will be null. Only traverses the files in
        /// the input directory unless the recurse flag is passed.
        /// </summary>
        /// <param name="inputDirectory"></param>
        /// <param name="recurse"></param>
        /// <returns></returns>
        public List<FileMatch> GetFiles(string inputDirectory, bool recurse = false)
        {
            var files = Directory
                    .EnumerateFiles(inputDirectory, "*.*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(path => !Path.GetFileName(path).StartsWith('.'))
                    .Select(path => new FileMatch { OldPath = path })
                    .OrderBy(match => match.OldPath)
                    .ToList();

            _console.WriteLine($"Found {files.Count} files.");
            _console.WriteLine();

            return files;
        }

        /// <summary>
        /// Renames a collection of FileMatch objects. Each OldPath will be renamed and moved to NewPath.
        /// Any missing directories will be created as needed.
        /// </summary>
        /// <param name="files"></param>
        public void RenameFiles(List<FileMatch> files)
        {
            foreach (var file in files)
            {
                if (file.NewPath.IsNullOrWhiteSpace())
                    continue;

                Directory.CreateDirectory(Path.GetDirectoryName(file.NewPath));

                File.Move(file.OldPath, file.NewPath);
            }

            _console.WriteLine($"Renamed {files.Count(match => !match.NewPath.IsNullOrWhiteSpace())} files.");
        }
    }
}
