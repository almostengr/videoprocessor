using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Music.Exceptions;

namespace Almostengr.VideoProcessor.Core.Music.Services;

public sealed class MusicService : IMusicService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IRandomService _randomService;
    private readonly ILoggerService<MusicService> _loggerService;
    private readonly AppSettings _appSettings;
    private const string Mix = "mix";

    public MusicService(IFileSystemService fileSystemService, ILoggerService<MusicService> logger,
    IRandomService randomService, AppSettings appSettings)
    {
        _fileSystemService = fileSystemService;
        _randomService = randomService;
        _appSettings = appSettings;
        _loggerService = logger;
    }

    public string GetRandomMixTrack()
    {
        var musicMixes = _fileSystemService.GetFilesInDirectory(_appSettings.MusicDirectory)
            .Where(x => x.ToLower().Contains(Mix) && x.ToLower().EndsWith(FileExtension.Mp3));

        if (musicMixes.Count() == 0)
        {
            throw new MusicTracksNotAvailableException();
        }

        return musicMixes.ElementAt(_randomService.Next(0, musicMixes.Count()));
    }

    public string GetRandomMusicTracks()
    {
        var musicFiles = _fileSystemService.GetFilesInDirectory(_appSettings.MusicDirectory)
            .Where(x => x.ToLower().Contains(Mix) == false && x.ToLower().EndsWith(FileExtension.Mp3));

        if (musicFiles.Count() == 0)
        {
            throw new MusicTracksNotAvailableException();
        }

        string outputString = string.Empty;
        while (outputString.Split(Environment.NewLine).Length < musicFiles.Count())
        {
            int randomIndex = _randomService.Next(0, musicFiles.Count());
            string musicFilename = Path.GetFileName(musicFiles.ElementAt(randomIndex));

            if (outputString.Contains(musicFilename) == false)
            {
                outputString += $"file '{musicFilename}'{Environment.NewLine}";
            }
        }

        return outputString;
    }

    public string GetRandomNonMixTrack()
    {
        var nonMusicMixes = _fileSystemService.GetFilesInDirectory(_appSettings.MusicDirectory)
            .Where(x => !x.ToLower().Contains(Mix) && x.ToLower().EndsWith(FileExtension.Mp3));

        if (nonMusicMixes.Count() == 0)
        {
            throw new MusicTracksNotAvailableException();
        }

        return nonMusicMixes.ElementAt(_randomService.Next(0, nonMusicMixes.Count()));
    }
}
