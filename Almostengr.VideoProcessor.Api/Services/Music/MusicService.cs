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

        public string GetRandomMusicTracks()
        {
            var musicFiles = _fileSystem.GetFilesInDirectory(_appSettings.Directories.MusicDirectory)
                .Where(x => x.ToLower().Contains("mix") == false && x.ToLower().EndsWith(FileExtension.Mp3));
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

        public string GetRandomMixTrack()
        {
            var musicMixes = _fileSystem.GetFilesInDirectory(_appSettings.Directories.MusicDirectory)
                .Where(x => x.ToLower().Contains("mix") && x.ToLower().EndsWith(FileExtension.Mp3));
            return musicMixes.ElementAt(_random.Next(0, musicMixes.Count() - 1));
        }

    }
}
