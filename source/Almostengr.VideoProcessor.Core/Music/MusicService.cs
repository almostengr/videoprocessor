using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Music.Exceptions;

namespace Almostengr.VideoProcessor.Core.Music.Services;

public sealed class MusicService : IMusicService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IRandomService _randomService;
    private readonly AppSettings _appSettings;
    private const string MIX = "mix";

    public MusicService(IFileSystemService fileSystemService, ILoggerService<MusicService> logger,
    IRandomService randomService, AppSettings appSettings)
    {
        _fileSystemService = fileSystemService;
        _randomService = randomService;
        _appSettings = appSettings;
    }

    public AudioFile GetRandomMixTrack()
    {
        var musicMixes = _fileSystemService.GetFilesInDirectory(_appSettings.MusicDirectory)
            .Where(x => x.ContainsIgnoringCase(MIX) && x.EndsWithIgnoringCase(FileExtension.Mp3.Value))
            .Select(x => new AudioFile(x));

        if (musicMixes.Count() == 0)
        {
            throw new MusicTracksNotAvailableException();
        }

        return musicMixes.ElementAt(_randomService.Next(0, musicMixes.Count()));
    }
}
