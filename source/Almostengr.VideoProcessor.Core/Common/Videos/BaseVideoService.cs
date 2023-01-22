using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract class BaseVideoService : IBaseVideoService
{
    protected readonly AppSettings _appSettings;
    protected readonly IFfmpegService _ffmpegService;
    protected readonly IGzipService _gzipService;
    protected readonly ITarballService _tarballService;
    protected readonly IFileSystemService _fileSystemService;
    protected readonly IRandomService _randomService;
    protected readonly IMusicService _musicService;

    protected const string END_SCREEN = "endscreen";

    protected BaseVideoService(
        AppSettings appSettings, IFfmpegService ffmpegService, IGzipService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        IMusicService musicService)
    {
        _appSettings = appSettings;
        _ffmpegService = ffmpegService;
        _gzipService = gzipService;
        _tarballService = tarballService;
        _fileSystemService = fileSystemService;
        _randomService = randomService;
        _musicService = musicService;
    }

    public abstract Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken);
    public abstract Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken);
    public abstract Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken);

    public virtual Task ProcessIncomingSubtitlesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    internal string RandomChannelBrandingText(string[] options)
    {
        return options.ElementAt(_randomService.Next(0, options.Count()));
    }

    internal async Task CompressTarballsInArchiveFolderAsync(
        string archiveDirectory, CancellationToken cancellationToken)
    {
        var archiveTarballs = _fileSystemService.GetFilesInDirectory(archiveDirectory)
            .Where(f => f.EndsWith(FileExtension.Tar) && !f.Contains(FileExtension.DraftTar));

        foreach (var archive in archiveTarballs)
        {
            await _gzipService.CompressFileAsync(archive, cancellationToken);
        }
    }

    internal void CreateFfmpegInputFile(string[] filesInDirectory, string inputFilePath)
    {
        StringBuilder text = new();
        const string FILE = "file";
        foreach (var file in filesInDirectory)
        {
            text.Append($"{FILE} '{file}' {Environment.NewLine}");
        }

        _fileSystemService.SaveFileContents(inputFilePath, text.ToString());
    }

    protected async Task ConvertVideoAudioFilesToAudioOnly(
        string workingDirectory, CancellationToken cancellationToken)
    {
        var audioAsVideoFiles = _fileSystemService.GetFilesInDirectory(workingDirectory)
            .Where(f => f.EndsWith(FileExtension.AudioMkv) || f.EndsWith(FileExtension.AudioMp4));

        foreach (var file in audioAsVideoFiles)
        {
            string outputFilePath = Path.Combine(workingDirectory,
                file.Replace(FileExtension.AudioMkv, FileExtension.Mp3)
                    .Replace(FileExtension.AudioMp4, FileExtension.Mp3));

            await _ffmpegService.ConvertVideoFileToMp3FileAsync(
                file, outputFilePath, workingDirectory, cancellationToken);
        }
    }

    public async Task CreateTarballsFromDirectoriesAsync(string incomingDirectory, CancellationToken cancellationToken)
    {
        var directories = _fileSystemService.GetDirectoriesInDirectory(incomingDirectory);
        foreach (var directory in directories)
        {
            await _tarballService.CreateTarballFromDirectoryAsync(directory, cancellationToken);
            _fileSystemService.DeleteDirectory(directory);
        }
    }

    public int FilterDurationInSeconds()
    {
        return _randomService.Next(240, 600);
    }

    public async Task CreateEndScreenVideoAsync(string workingDirectory, CancellationToken cancellationToken)
    {
        string imageFilePath = _fileSystemService.GetFilesInDirectory(workingDirectory)
            .Where(f => f.ToLower().EndsWith(FileExtension.Png))
            .Single();

        string audioFilePath = _fileSystemService.GetFilesInDirectory(workingDirectory)
            .Where(f => f.ToLower().EndsWith(FileExtension.Mp3))
            .Single();

        
        string outputFilePath = Path.Combine(
            workingDirectory,
            END_SCREEN + FileExtension.Mp4);

        await _ffmpegService.ConvertEndScreenImageToMp4VideoAsync(
            imageFilePath, audioFilePath, outputFilePath, cancellationToken);

        await _ffmpegService.ConvertVideoFileToTsFormatAsync(
            outputFilePath, 
            Path.Combine(workingDirectory.Replace(DirectoryName.Working, string.Empty), END_SCREEN + FileExtension.Ts),
            cancellationToken);
    }
}