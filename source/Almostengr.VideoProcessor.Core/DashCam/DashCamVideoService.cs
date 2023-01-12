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
    // private readonly AppSettings _appSettings;
    // private readonly IFileSystemService _fileSystemService;
    // private readonly ITarballService _tarballService;
    // private readonly IRandomService _randomService;
    // private readonly IFfmpegService _ffmpegService;
    // private readonly IGzipService _gzipService;
    private readonly ILoggerService<DashCamVideoService> _loggerService;

    public string IncomingDirectory { get; }
    public string ArchiveDirectory { get; }
    public string ErrorDirectory { get; }
    public string UploadDirectory { get; }
    public string WorkingDirectory { get; }

    // public DashCamVideoService(AppSettings appSettings, IFileSystemService fileSystemService,
    //     ILoggerService<DashCamVideoService> loggerService, IGzipService gzipService,
    //     ITarballService tarballService, IRandomService randomService, IFfmpegService ffmpegService)
    // {
    //     _appSettings = appSettings;
    //     IncomingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Incoming);
    //     ArchiveDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Archive);
    //     ErrorDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Error);
    //     UploadDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Upload);
    //     WorkingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Working);
    //     _fileSystemService = fileSystemService;
    //     _tarballService = tarballService;
    //     _randomService = randomService;
    //     _ffmpegService = ffmpegService;
    //     _loggerService = loggerService;
    //     _gzipService = gzipService;
    // }

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
        await base.CompressTarballsInArchiveFolderAsync(ArchiveDirectory, cancellationToken);
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

            await _tarballService.ExtractTarballContentsAsync(
                video.IncomingTarballFilePath(), WorkingDirectory, cancellationToken);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            CreateFfmpegInputFile(video);

            if (video.IsDraft())
            {
                // await _ffmpegService.RenderVideoAsync(
                //     video.FfmpegInputFilePath(), string.Empty, video.OutputFileName(), cancellationToken);
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

            video.AddLikeVideoFilter();

            await _ffmpegService.RenderVideoAsync(
                video.FfmpegInputFilePath(), video.VideoFilter, video.OutputFileName(), cancellationToken);

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

    internal override string GetChannelBrandingText(string[] options)
    {
        return options[0];
    }

}