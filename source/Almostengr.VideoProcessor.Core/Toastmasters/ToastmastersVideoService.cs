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
    private readonly ILoggerService<ToastmastersVideoService> _loggerService;
    
    private readonly string IncomingDirectory;
    private readonly string ArchiveDirectory;
    private readonly string ErrorDirectory;
    private readonly string UploadDirectory;
    private readonly string WorkingDirectory;

    public ToastmastersVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IGzipService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<ToastmastersVideoService> loggerService, IMusicService musicService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService)
    {
        IncomingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Incoming);
        ArchiveDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Archive);
        ErrorDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Error);
        UploadDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Upload);
        WorkingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Working);
        _loggerService = loggerService;
    }

    public override async Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken)
    {
        try
        {
            await base.CompressTarballsInArchiveFolderAsync(ArchiveDirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }

    public void CreateDirectories()
    {
        _fileSystemService.CreateDirectory(IncomingDirectory);
        _fileSystemService.CreateDirectory(ArchiveDirectory);
        _fileSystemService.CreateDirectory(ErrorDirectory);
        _fileSystemService.CreateDirectory(UploadDirectory);
        _fileSystemService.CreateDirectory(WorkingDirectory);
    }

    public override async Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await base.CreateTarballsFromDirectoriesAsync(IncomingDirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }

    public override async Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken)
    {
        ToastmastersVideo? video = null;

        try
        {
            string incomingTarball = _fileSystemService.GetRandomTarballFromDirectory(IncomingDirectory);

            video = new ToastmastersVideo(_appSettings.ToastmastersDirectory, Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

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

            video.AddLikeVideoFilter(_randomService.SubscribeLikeDuration());

            await _ffmpegService.RenderVideoAsync(
                video.FfmpegInputFilePath(), video.VideoFilter, video.OutputFilePath(), cancellationToken);

            _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _loggerService.LogInformation($"Completed processing {incomingTarball}");
        }
        catch (NoTarballsPresentException)
        {
            throw;
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
}
