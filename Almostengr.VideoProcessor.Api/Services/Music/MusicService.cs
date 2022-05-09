using System;
using System.IO;
using System.Linq;
using Almostengr.VideoProcessor.Api.Common;
using Almostengr.VideoProcessor.Constants;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services
{
    public class MusicService : BaseService, IMusicService
    {
        private readonly AppSettings _appSettings;
        private readonly Random _random;

        public MusicService(ILogger<BaseService> logger, AppSettings appSettings) : base(logger)
        {
            _appSettings = appSettings;
            _random = new Random();
        }

        public string GetFfmpegMusicInputList()
        {
            var musicFiles = base.GetDirectoryContents(_appSettings.Directories.MusicDirectory, $"*{FileExtension.Mp3}")
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
            var musicMixes = base.GetDirectoryContents(_appSettings.Directories.MusicDirectory, $"*{FileExtension.Mp3}")
                .Where(x => x.ToLower().Contains("mix"));
            return musicMixes.ElementAt(_random.Next(0, musicMixes.Count()-1));
        }

    }
}
