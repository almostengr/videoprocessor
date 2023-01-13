using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed class DashCamVideoService : BaseVideoService, IDashCamVideoService
{
    private readonly ILoggerService<DashCamVideoService> _loggerService;

    private readonly string IncomingDirectory;
    private readonly string ArchiveDirectory;
    private readonly string ErrorDirectory;
    private readonly string UploadDirectory;
    private readonly string WorkingDirectory;

    public DashCamVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IGzipService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<DashCamVideoService> loggerService, IMusicService musicService) :
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
        try
        {
            await base.CompressTarballsInArchiveFolderAsync(ArchiveDirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }

    public override async Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken)
    {
        DashCamVideo? video = null;

        try
        {
            string incomingTarball = _fileSystemService.GetRandomTarballFromDirectory(IncomingDirectory);
            _loggerService.LogInformation($"Processing ${incomingTarball}");

            video = new DashCamVideo(_appSettings.DashCamDirectory, Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                video.IncomingTarballFilePath(), WorkingDirectory, cancellationToken);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            CreateFfmpegInputFile(video);

            if (video.IsDraft())
            {
                await _ffmpegService.RenderVideoAsCopyAsync(
                    video.FfmpegInputFilePath(),
                    Path.Combine(ArchiveDirectory, video.OutputFileName()),
                    cancellationToken);
                _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
                _fileSystemService.SaveFileContents(
                    Path.Combine(ArchiveDirectory, video.ArchiveFileName + FileExtension.GraphicsAss),
                    string.Empty);
                _fileSystemService.DeleteDirectory(WorkingDirectory);
                return;
            }

            // brand video
            video.AddDrawTextVideoFilter(
                GetChannelBrandingText(video.BrandingTextOptions()),
                video.DrawTextFilterTextColor(),
                Opacity.Full,
                FfmpegFontSize.Medium,
                DrawTextPosition.UpperRight,
                video.DrawTextFilterBackgroundColor(),
                Opacity.Light);

            video.AddDrawTextVideoFilter(
                video.Title,
                video.DrawTextFilterTextColor(),
                Opacity.Full,
                FfmpegFontSize.Small,
                DrawTextPosition.UpperLeft,
                video.DrawTextFilterBackgroundColor(),
                Opacity.Light);

            var graphicsSubtitle = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.GraphicsAss))
                .Single();

            video.SetGraphicsSubtitleFileName(graphicsSubtitle);

            video.AddSubscribeVideoFilter(_randomService.SubscribeLikeDuration());

            await _ffmpegService.RenderVideoWithMixTrackAsync(
                video.FfmpegInputFilePath(),
                _musicService.GetRandomMixTrack(),
                video.VideoFilter,
                video.OutputFilePath(),
                cancellationToken);

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

    private void CreateFfmpegInputFile(DashCamVideo video)
    {
        _fileSystemService.DeleteFile(video.FfmpegInputFilePath());

        string[] videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
            .Where(f => f.EndsWith(FileExtension.Mov))
            .OrderBy(f => f)
            .ToArray();
        string ffmpegInput = FfmpegInputFileText(videoFiles, video.FfmpegInputFilePath());

        _fileSystemService.SaveFileContents(video.FfmpegInputFilePath(), ffmpegInput);
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
}