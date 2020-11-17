
# Renamer Core

Command line tool for renaming movie and show files for Plex.

## Installation

> Requires [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0)

Clone the repo to you computer. Pull to get updates. Open a command prompt to the source directory.

If you are updating, remove the old version first:

```cmd
> dotnet tool uninstall -g renamer-core
```

To install:

```cmd
> dotnet build
> dotnet pack
> dotnet tool install -g --add-source ./dist renamer-core
```

## Usage

There are 3 commands: `config`, `m`, and `s`. The last two are the Movie Renamer and the Show Renamer, respectively.

### The  `config` command

The `config` command is used to set the API keys for The TV DB API and The Movie DB API. You will need to get your own API keys.

```bash
> renamer config -tvdb <APIKEY>
> renamer config -tmdb <APIKEY>
```

Config values are stored in a JSON file in your local app storage directory (`~\AppData\Local` on Windows, `~/.config` on *nix).

### Movie Renamer

```bash
renamer m [OPTIONS]
```

The Movie Renamer looks for any files in the input directly, runs them through a simple search algorithm against The Movie Database, then names them in a way that Plex likes.

For instance, given a file called `star.wars.1977.h264.foo.bar.mkv`, it will match against The Movie DB and rename the file to `Star Wars (1977).mkv`. Special tags will be added for 4k and 1080p movies if the original file name indicates the resolution. Subtitle files (`srt` and `sub` extensions) are treated as English, and `.en` will be inserted before the extension.

#### Movie Renamer Options

- `-i <PATH>` or `--input <PATH>` - Input Directory. Defaults to current directory.
- `-o <PATH>` or `--output <PATH>` - Output Directory. Defaults to the input directory.
    - Note that files will be **moved** if the input and output directories are not the same.
- `-y` or `--yes` - Skip Confirmation. If provided, the confirmation prompt before renaming will be skipped. Be aware that files will be renamed (and possibly moved) immediately after matches are made.
- `--verbose` - Verbose Output. Will show search queries as they are made, as well as other extra output.

### Show Renamer

```bash
renamer s [OPTIONS]
```

The Show Renamer looks for any files in the input directly, attempts to match the show/season/episode to The TV DB, then moves the files to a folder structure that Plex likes.

For instance, given a file called `doctor.who.2005.s01e01.mkv`, it will be moved to `Doctor Who (2005)\Season 01\Doctor Who (2005) - s01e01 - Rose.mkv`. In order for the episode to be matched correctly, the season and episode must be in the file name using `sXXeYY` or `YYxZZ`. Any text before the season/episode info is considered to be the show name and is searched against The TV DB using a similar algorithm to the movie renamer.

> NEW! In 2.4+, you can now prepend the show ID from The TV DB to the beginning of the filename. Wrap it in double-square brackets to override the show name search. For example, you can name the file something like `[[311711]]the.good.place.s01e01` to have the show matched directly based on the ID `311711`. This can be helpful for shows which do not match well with only their name (like The Good Place).

#### Show Renamer Options

- `-i <PATH>` or `--input <PATH>` - Input Directory. Defaults to current directory.
- `-o <PATH>` or `--output <PATH>` - Output Directory. Defaults to the input directory.
    - Note that files will be **moved** if the input and output directories are not the same.
- `-di` or `--dvd-input` - Input is DVD Order. Will search the season and episode numbers based on DVD order. If this flag is not use, the season and episode numbers will be searched based on the aired order.
- `-do` or `dvd-output` - Output is DVD Order. Will use the season and episode numbers based on DVD order for the renamed files. If this flag is not use, the season and episode numbers will be from the aired order.
- `-f` or `--files-only` - Files Only Mode. Will not create folders for shows or seasons. Only the files will be renamed. They may still be moved to the output directory.
- `-r` or `--recurse` - Recursive Mode. Will recursively scan all folders and subfolders to find files in the Input Directory.
- `-y` or `--yes` - Skip Confirmation. If provided, the confirmation prompt before renaming will be skipped. Be aware that files will be renamed (and possibly moved) immediately after matches are made.
- `--verbose` - Verbose Output. Will show search queries as they are made, as well as other extra output.
