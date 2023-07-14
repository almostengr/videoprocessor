using System.Text;
using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Music.Exceptions;

namespace Almostengr.VideoProcessor.Core.Music.Services;

public sealed class MusicService : IMusicService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IRandomService _randomService;
    private readonly IFfmpegService _ffmpegService;
    private readonly ILoggerService<MusicService> _loggerService;
    private readonly AppSettings _appSettings;
    private const string MIX = "mix";

    public MusicService(IFileSystemService fileSystemService, ILoggerService<MusicService> logger,
        IFfmpegService ffmpegService,
        IRandomService randomService, AppSettings appSettings)
    {
        _fileSystemService = fileSystemService;
        _randomService = randomService;
        _appSettings = appSettings;
        _ffmpegService = ffmpegService;
        _loggerService = logger;
    }

    public AudioFile GetRandomMixTrack()
    {
        var musicMixes = _fileSystemService.GetFilesInDirectory(_appSettings.MusicDirectory)
            .Where(f => f.ContainsIgnoringCase(MIX) && f.EndsWithIgnoringCase(FileExtension.Mp3.Value))
            .Select(f => new AudioFile(f));

        if (musicMixes.Count() == 0)
        {
            throw new MusicTracksNotAvailableException();
        }

        return musicMixes.ElementAt(_randomService.Next(0, musicMixes.Count()));
    }

    public async Task GenerateMixTrackAsync(CancellationToken cancellationToken)
    {
        var musicFiles = _fileSystemService.GetFilesInDirectory(_appSettings.MusicDirectory)
            .Where(f => f.DoesNotContainIgnoringCase(MIX) && f.EndsWithIgnoringCase(FileExtension.Mp3.Value));

        StringBuilder sb = new();
        while (sb.Length < musicFiles.Count())
        {
            int randomIndex = _randomService.Next(0, musicFiles.Count());
            string musicFilePath = musicFiles.ElementAt(randomIndex);

            if (sb.ToString().Contains(musicFilePath) == false)
            {
                sb.Append($"file '{musicFilePath}'{Environment.NewLine}");
            }
        }

        string ffmpegInputFile = Path.Combine(_appSettings.MusicDirectory, "music" + FileExtension.FfmpegInput);
        
        try
        {
            _fileSystemService.SaveFileContents(ffmpegInputFile, sb.ToString());

            string outputFile = 
                Path.Combine(_appSettings.MusicDirectory, $"{MIX}{DateTime.Now.ToString("yyyyMMddHHmm")}{FileExtension.Mp3.Value}");
            await _ffmpegService.CreateMusicMixTrackAsync(
                ffmpegInputFile, outputFile, _appSettings.MusicDirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }

        _fileSystemService.DeleteFile(ffmpegInputFile);
    }
}
