using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Configuration;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Core.Music
{
    public class MusicService : BaseService, IMusicService
    {
        private readonly AppSettings _appSettings;

        public MusicService(ILogger<MusicService> logger, AppSettings appSettings)  : base(logger)
        {
            _appSettings = appSettings;
        }

        public string GetRandomMusicTracks()
        {
            var musicFiles = GetFilesInDirectory(_appSettings.Directories.MusicDirectory)
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
            var musicMixes = GetFilesInDirectory(_appSettings.Directories.MusicDirectory)
                .Where(x => x.ToLower().Contains("mix") && x.ToLower().EndsWith(FileExtension.Mp3));
            return musicMixes.ElementAt(_random.Next(0, musicMixes.Count()));
        }

        public string GetRandomNonMixTrack()
        {
            var nonMusicMixes = GetFilesInDirectory(_appSettings.Directories.MusicDirectory)
                .Where(x => !x.ToLower().Contains("mix") && x.ToLower().EndsWith(FileExtension.Mp3));
            return nonMusicMixes.ElementAt(_random.Next(0, nonMusicMixes.Count()));
        }

    }
}
