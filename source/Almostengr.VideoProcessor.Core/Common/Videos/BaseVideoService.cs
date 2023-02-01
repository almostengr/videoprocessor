using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Music.Services;
using Almostengr.VideoProcessor.Core.TechTalk;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract class BaseVideoService : IBaseVideoService
{
    protected const string FFMPEG_FILE_NAME = "ffmpeginput.txt";
    protected readonly AppSettings _appSettings;
    protected readonly IFfmpegService _ffmpegService;
    protected readonly IFileCompressionService _compressionService;
    protected readonly ITarballService _tarballService;
    protected readonly IFileSystemService _fileSystemService;
    protected readonly IRandomService _randomService;
    protected readonly IMusicService _musicService;
    protected readonly IAssSubtitleFileService _assSubtitleFileService;

    protected string _ffmpegInputFilePath { get; init; }

    protected string IncomingDirectory { get; init; }
    protected string ArchiveDirectory { get; init; }
    protected string UploadDirectory { get; init; }
    protected string WorkingDirectory { get; init; }
    protected string? DraftDirectory { get; init; }

    protected BaseVideoService(
        AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        IMusicService musicService, IAssSubtitleFileService assSubtitleFileService)
    {
        _appSettings = appSettings;
        _ffmpegService = ffmpegService;
        _compressionService = gzipService;
        _tarballService = tarballService;
        _fileSystemService = fileSystemService;
        _randomService = randomService;
        _musicService = musicService;
        _assSubtitleFileService = assSubtitleFileService;
    }

    public abstract Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken);
    public abstract Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken);
    public abstract Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken);
    public abstract Task ConvertGzToXzAsync(CancellationToken cancellationToken);

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
            .Where(f => f.EndsWith(FileExtension.Tar.ToString()) && !f.Contains(FileExtension.DraftTar.ToString()));

        foreach (var archive in archiveTarballs)
        {
            await _compressionService.CompressFileAsync(archive, cancellationToken);
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
            .Where(f => f.EndsWith(FileExtension.AudioMkv.ToString()) || f.EndsWith(FileExtension.AudioMp4.ToString()));

        foreach (var file in audioAsVideoFiles)
        {
            string outputFilePath = Path.Combine(workingDirectory,
                file.Replace(FileExtension.AudioMkv.ToString(), FileExtension.Mp3.ToString())
                    .Replace(FileExtension.AudioMp4.ToString(), FileExtension.Mp3.ToString()));

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

    public bool DoesKdenliveFileExist(string directory)
    {
        return _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.ToLower().EndsWith(FileExtension.Kdenlive.ToString()))
            .Any();
    }

    public void CheckAndAddGraphicsSubtitle(TechTalkVideoFile video)
    {
        string? graphicsSubtitleFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
            .Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString()))
            .SingleOrDefault();

        if (graphicsSubtitleFile != null)
        {
            var subtitles = _assSubtitleFileService.ReadFile(graphicsSubtitleFile);
            video.AddDrawTextVideoFilterFromSubtitles(subtitles);
        }
    }

}