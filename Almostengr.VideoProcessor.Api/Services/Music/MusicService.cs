using System;
using System.IO;
using System.Linq;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Services.FileSystem;
using Almostengr.VideoProcessor.Constants;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.MusicService
{
    public class MusicService : IMusicService
    {
        private readonly AppSettings _appSettings;
        private readonly Random _random;
        private readonly IFileSystemService _fileSystem;

        public MusicService(ILogger<MusicService> logger, AppSettings appSettings, IFileSystemService fileSystem)
        {
            _appSettings = appSettings;
            _random = new Random();
            _fileSystem = fileSystem;
        }

        public string GetFfmpegMusicInputList()
        {
            var musicFiles = _fileSystem.GetDirectoryContents(_appSettings.Directories.MusicDirectory, $"*{FileExtension.Mp3}")
                .Where(x => x.ToLower().Contains("mix") == false);
            string outputString = string.Empty;

            while (outputString.Split(Environment.NewLine).Length < musicFiles.Count())
            {
                int randomIndex = _random.Next(0, musicFiles.Count());
                string musicFilename = Path.GetFileName(musicFiles.ElementAt(randomIndex));

                if (outputString.Contains(musicFilename) == false)
                {
                    outputString += $"file '{musicFilename}'{Environment.NewLine}";
                }
            }

            return outputString;
        }

        public string PickRandomMusicTrack()
        {
            var musicMixes = _fileSystem.GetDirectoryContents(_appSettings.Directories.MusicDirectory, $"*{FileExtension.Mp3}")
                .Where(x => x.ToLower().Contains("mix"));
            return musicMixes.ElementAt(_random.Next(0, musicMixes.Count() - 1));
        }

    }
}
