using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Core.Videos.TechTalk;

public sealed class TechTalkVideoService : BaseVideoService, ITechTalkVideoService
{
    private readonly IRandomService _random;
    private readonly IFfmpegService _ffmpeg;
    private readonly ILoggerService<TechTalkVideoService> _loggerService;
    private readonly AppSettings _appSettings;

    public string IncomingDirectory { get; }
    public string ArchiveDirectory { get; }
    public string ErrorDirectory { get; }
    public string UploadDirectory { get; }
    public string WorkingDirectory { get; }

    private IFileSystemService _fileSystemService;
    private readonly ITarballService _tarball;

    public TechTalkVideoService(AppSettings appSettings, IFileSystemService fileSystem,
    ILoggerService<TechTalkVideoService> loggerService,
        ITarballService tarball, IRandomService random, IFfmpegService ffmpeg)
    {
        _appSettings = appSettings;
        IncomingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Incoming);
        ArchiveDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Archive);
        ErrorDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Error);
        UploadDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Upload);
        WorkingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Working);
        _fileSystemService = fileSystem;
        _tarball = tarball;
        _random = random;
        _ffmpeg = ffmpeg;
        _loggerService = loggerService;
    }

    public override async Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken)
    {
        TechTalkVideo? video = null;

        try
        {
            string incomingTarball = _fileSystemService.GetRandomTarballFromDirectory(IncomingDirectory);
            _loggerService.LogInformation($"Processing ${incomingTarball}");

            video = new TechTalkVideo(_appSettings.DashCamDirectory, Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(WorkingDirectory);

            await _tarball.ExtractTarballContentsAsync(
                video.IncomingTarballFilePath(), WorkingDirectory, cancellationToken);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            // create input file
            _fileSystemService.DeleteFile(video.FfmpegInputFilePath());
            string[] videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4) || f.EndsWith(FileExtension.Mkv))
                .OrderBy(f => f)
                .ToArray();
            string ffmpegInput = FfmpegInputFileText(videoFiles, video.FfmpegInputFilePath());

            _fileSystemService.SaveFileContents(video.FfmpegInputFilePath(), ffmpegInput);

            if (video.IsDraft())
            {
                await _ffmpeg.RenderVideoAsync(
                    video.FfmpegInputFilePath(), string.Empty, Path.Combine(UploadDirectory, video.OutputFileName()), cancellationToken);
                _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
                _fileSystemService.DeleteDirectory(WorkingDirectory);
                return;
            }

            if (_fileSystemService.DoesFileExist(video.ChristmasLightMetaFile()))
            {
                video.ConfirmChristmasLightVideo();
            }

            if (_fileSystemService.DoesFileExist(video.IndependenceDayMetaFile()))
            {
                video.ConfirmIndependenceDayVideo();
            }

            // brand video
            video.AddDrawTextVideoFilter(
                GetChannelBrandingText(video.BrandingTextOptions()),
                video.DrawTextFilterTextColor(),
                Opacity.Full,
                FfmpegFontSize.Large,
                DrawTextPosition.UpperRight,
                video.DrawTextFilterBackgroundColor(),
                Opacity.Light);

            if (_fileSystemService.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss)).Any())
            {
                video.AddSubtitleVideoFilter(
                    _fileSystemService.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss)).Single());
            }

            await _ffmpeg.RenderVideoAsync(
                video.FfmpegInputFilePath(), video.VideoFilter, video.OutputFileName(), cancellationToken);

            _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
            _fileSystemService.DeleteDirectory(WorkingDirectory);
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
        return options.ElementAt(_random.Next(0, options.Count()));
    }
}