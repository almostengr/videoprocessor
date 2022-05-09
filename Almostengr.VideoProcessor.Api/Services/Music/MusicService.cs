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

        public MusicService(ILogger<BaseService> logger, AppSettings appSettings) : base(logger)
        {
            _appSettings = appSettings;
        }

        public string GetFfmpegMusicInputList()
        {
            var musicFiles = base.GetDirectoryContents(_appSettings.Directories.MusicDirectory, $"*{FileExtension.Mp3}")
                .Where(x => x.ToLower().Contains("mix") == false);
            string outputString = string.Empty;
            Random random = new();

            while (outputString.Split(Environment.NewLine).Length < musicFiles.Count())
            {
                int randomIndex = random.Next(0, musicFiles.Count());
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
            var musicMixes = base.GetDirectoryContents(_appSettings.Directories.MusicDirectory, $"{FileExtension.Mp3}")
                .Where(x => x.ToLower().Contains("mix"));
            Random random = new();
            return musicMixes.ElementAt(random.Next(0, musicMixes.Count()));
        }

    }
}
