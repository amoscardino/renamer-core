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

        public FileService(IConsole console)
        {
            _console = console;
        }

        public List<FileMatch> GetFiles(string inputDirectory, bool recurse = false)
        {
            var files = Directory
                .GetFiles(inputDirectory, "*.*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(x => new FileMatch { OldPath = x })
                .ToList();

            _console.WriteLine($"Found {files.Count} files.");
            _console.WriteLine();

            return files;
        }

        public void RenameFiles(List<FileMatch> files)
        {
            foreach (var file in files)
            {
                if (file.NewPath.IsNullOrWhiteSpace())
                    continue;

                Directory.CreateDirectory(Path.GetDirectoryName(file.NewPath));

                File.Move(file.OldPath, file.NewPath);
            }

            _console.WriteLine($"Renamed {files.Count(x => !x.NewPath.IsNullOrWhiteSpace())} files.");
        }
    }
}
