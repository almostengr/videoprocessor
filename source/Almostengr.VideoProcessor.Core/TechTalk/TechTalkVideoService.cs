using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed class TechTalkVideoService : BaseVideoService, ITechTalkVideoService
{
    // private readonly IRandomService _random;
    // private readonly IFfmpegService _ffmpegService;
    private readonly ILoggerService<TechTalkVideoService> _loggerService;
    // private readonly AppSettings _appSettings;

    public string IncomingDirectory { get; }
    public string ArchiveDirectory { get; }
    public string ErrorDirectory { get; }
    public string UploadDirectory { get; }
    public string WorkingDirectory { get; }

    // private IFileSystemService _fileSystemService;
    // private readonly ITarballService _tarballService;

    // public TechTalkVideoService(AppSettings appSettings, IFileSystemService fileSystem,
    // ILoggerService<TechTalkVideoService> loggerService,
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
    //     _ffmpegService = ffmpeg;
    //     _loggerService = loggerService;
    // }


    public TechTalkVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IGzipService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<TechTalkVideoService> loggerService, IMusicService musicService) :
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
        TechTalkVideo? video = null;

        try
        {
            string incomingTarball = _fileSystemService.GetRandomTarballFromDirectory(IncomingDirectory);
            _loggerService.LogInformation($"Processing ${incomingTarball}");

            video = new TechTalkVideo(_appSettings.DashCamDirectory, Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
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
                await _ffmpegService.RenderVideoAsync(
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

            await _ffmpegService.RenderVideoAsync(
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
        return options.ElementAt(_randomService.Next(0, options.Count()));
    }

    public override Task ProcessIncomingSubtitlesAsync(CancellationToken cancellationToken)
    {
        try
        {
            string filePath = _fileSystemService.GetRandomSrtFileFromDirectory(IncomingDirectory);

            TechTalkSrtSubtitle subtitle = new(_appSettings.TechnologyDirectory, filePath);

            subtitle.SetSubtitleText(_fileSystemService.GetFileContents(subtitle.IncomingFilePath()));

            _fileSystemService.SaveFileContents(subtitle.SubtitleOutputFilePath(), subtitle.SubtitleText());
            _fileSystemService.SaveFileContents(subtitle.BlogOutputFilePath(), subtitle.BlogPostText());
            _fileSystemService.MoveFile(subtitle.IncomingFilePath(), subtitle.ArchiveFilePath());
        }
        catch (NoSubtitleFilesPresentException)
        { }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }

        return Task.CompletedTask;
    }
}