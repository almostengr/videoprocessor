using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Exceptions;

namespace Almostengr.VideoProcessor.Domain.Music.Services;

public sealed class MusicService : IMusicService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly Random _random;
    private const string Mix = "mix";

    public MusicService(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
        _random = new Random();
    }

    public string GetRandomMixTrack()
    {
        var musicMixes = _fileSystemService.GetFilesInDirectory(Constants.MusicBaseDirectory)
            .Where(x => x.ToLower().Contains(Mix) && x.ToLower().EndsWith(FileExtension.Mp3));


        if (musicMixes.Count() == 0)
        {
            throw new MusicTracksNotAvailableException();
        }

        return musicMixes.ElementAt(_random.Next(0, musicMixes.Count()));
    }

    public string GetRandomMusicTracks()
    {
        var musicFiles = _fileSystemService.GetFilesInDirectory(Constants.MusicBaseDirectory)
            .Where(x => x.ToLower().Contains(Mix) == false && x.ToLower().EndsWith(FileExtension.Mp3));

        if (musicFiles.Count() == 0)
        {
            throw new MusicTracksNotAvailableException();
        }

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

    public string GetRandomNonMixTrack()
    {
        var nonMusicMixes = _fileSystemService.GetFilesInDirectory(Constants.MusicBaseDirectory)
            .Where(x => !x.ToLower().Contains(Mix) && x.ToLower().EndsWith(FileExtension.Mp3));

        if (nonMusicMixes.Count() == 0)
        {
            throw new MusicTracksNotAvailableException();
        }

        return nonMusicMixes.ElementAt(_random.Next(0, nonMusicMixes.Count()));
    }
}
