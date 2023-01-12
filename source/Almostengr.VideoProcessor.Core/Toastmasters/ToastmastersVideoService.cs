using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;

namespace Almostengr.VideoProcessor.Core.Toastmasters;

public sealed class ToastmastersVideoService : BaseVideoService, IToastmastersVideoService
{
    private readonly string IncomingDirectory;
    private readonly string ArchiveDirectory;
    private readonly string ErrorDirectory;
    private readonly string UploadDirectory;
    private readonly string WorkingDirectory;
    private readonly ILoggerService<ToastmastersVideoService> _loggerService;
    // private readonly IFileSystemService _fileSystemService;
    // private readonly ITarballService _tarballService;
    // private readonly IRandomService _random;
    // private readonly IFfmpegService _ffmpeg;
    // private readonly AppSettings _appSettings;

    // public ToastmastersVideoService(AppSettings appSettings, IFileSystemService fileSystem,
    // ILoggerService<ToastmastersVideoService> loggerService,
    //     ITarballService tarball, IRandomService random, IFfmpegService ffmpeg)
    // {
    //     _appSettings = appSettings;
    //     IncomingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Incoming);
    //     ArchiveDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Archive);
    //     ErrorDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Error);
    //     UploadDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Upload);
    //     WorkingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Working);
    //     _fileSystemService = fileSystem;
    //     _tarballService = tarball;
    //     _random = random;
    //     _ffmpeg = ffmpeg;
    //     _loggerService = loggerService;
    // }


    public ToastmastersVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IGzipService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<ToastmastersVideoService> loggerService, IMusicService musicService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService)
    {
        IncomingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Incoming);
        ArchiveDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Archive);
        ErrorDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Error);
        UploadDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Upload);
        WorkingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Working);
        _loggerService = loggerService;
    }

    public override async Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken)
    {
        await base.CompressTarballsInArchiveFolderAsync(ArchiveDirectory, cancellationToken);
    }

    public void CreateDirectories()
    {
        _fileSystemService.CreateDirectory(IncomingDirectory);
        _fileSystemService.CreateDirectory(ArchiveDirectory);
        _fileSystemService.CreateDirectory(ErrorDirectory);
        _fileSystemService.CreateDirectory(UploadDirectory);
        _fileSystemService.CreateDirectory(WorkingDirectory);
    }

    public async Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken)
    {
        var directories = _fileSystemService.GetDirectoriesInDirectory(IncomingDirectory);
        foreach (var directory in directories)
        {
            await _tarballService.CreateTarballFromDirectoryAsync(directory, cancellationToken);
            _fileSystemService.DeleteDirectory(directory);
        }
    }

    public override async Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken)
    {
        ToastmastersVideo? video = null;

        try
        {
            string incomingTarball = _fileSystemService.GetRandomTarballFromDirectory(IncomingDirectory);

            video = new ToastmastersVideo(_appSettings.DashCamDirectory, Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                video.IncomingTarballFilePath(), WorkingDirectory, cancellationToken);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            // create input file
            _fileSystemService.DeleteFile(video.FfmpegInputFilePath());

            string[] videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4))
                .OrderBy(f => f)
                .ToArray();
            string ffmpegInput = FfmpegInputFileText(videoFiles, video.FfmpegInputFilePath());

            _fileSystemService.SaveFileContents(video.FfmpegInputFilePath(), ffmpegInput);

            // brand video
            video.AddDrawTextVideoFilter(
                GetChannelBrandingText(video.BrandingTextOptions()),
                video.DrawTextFilterTextColor(),
                Opacity.Full,
                FfmpegFontSize.Large,
                DrawTextPosition.UpperRight,
                video.DrawTextFilterBackgroundColor(),
                Opacity.Light);

            video.AddLikeVideoFilter();

            await _ffmpegService.RenderVideoAsync(
                video.FfmpegInputFilePath(), video.VideoFilter, video.OutputFileName(), cancellationToken);

            _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _loggerService.LogInformation($"Completed processing {incomingTarball}");
        }
        catch (NoTarballsPresentException)
        {
            return;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);

            if (video != null)
            {
                _fileSystemService.MoveFile(video.IncomingTarballFilePath(), Path.Combine(ErrorDirectory, video.ArchiveFileName));
                _fileSystemService.SaveFileContents(
                    Path.Combine(ErrorDirectory, video.ArchiveFileName + FileExtension.Log),
                    ex.Message);
            }
        }
    }

    internal override string GetChannelBrandingText(string[] options)
    {
        return options.ElementAt(_randomService.Next(0, options.Count()));
    }

}