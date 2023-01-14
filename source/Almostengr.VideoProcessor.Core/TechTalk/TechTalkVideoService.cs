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
    private readonly ILoggerService<TechTalkVideoService> _loggerService;

    public readonly string IncomingDirectory;
    public readonly string ArchiveDirectory;
    public readonly string UploadDirectory;
    public readonly string WorkingDirectory;

    public TechTalkVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IGzipService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<TechTalkVideoService> loggerService, IMusicService musicService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService)
    {
        IncomingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Incoming);
        ArchiveDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Archive);
        UploadDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Upload);
        WorkingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Working);
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
        TechTalkVideo? video = null;

        try
        {
            string incomingTarball = _fileSystemService.GetRandomTarballFromDirectory(IncomingDirectory);
            _loggerService.LogInformation($"Processing ${incomingTarball}");

            video = new TechTalkVideo(_appSettings.TechnologyDirectory, Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                video.IncomingTarballFilePath(), WorkingDirectory, cancellationToken);

            _fileSystemService.CopyFile(
                video.EndScreenFilePath(), 
                Path.Combine(WorkingDirectory, video.EndScreenFileName()));

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            // create input file
            _fileSystemService.DeleteFile(video.FfmpegInputFilePath());
            var videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4) || f.EndsWith(FileExtension.Mkv))
                .OrderBy(f => f)
                // .ToArray();
                .ToList();

            videoFiles.Add(video.EndScreenFilePath());
            // string ffmpegInput = CreateFfmpegInputFile(videoFiles, video.FfmpegInputFilePath());
            CreateFfmpegInputFile(videoFiles.ToArray(), video.FfmpegInputFilePath());

            // _fileSystemService.SaveFileContents(video.FfmpegInputFilePath(), ffmpegInput);

            if (video.IsDraft())
            {
                await _ffmpegService.RenderVideoAsync(
                    // video.FfmpegInputFilePath(), string.Empty, Path.Combine(UploadDirectory, video.OutputFileName()), cancellationToken);
                    video.FfmpegInputFilePath(), string.Empty, video.OutputVideoFilePath(), cancellationToken);
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
                video.FfmpegInputFilePath(), video.VideoFilter, video.OutputVideoFilePath(), cancellationToken);

            _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
            _fileSystemService.DeleteDirectory(WorkingDirectory);
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
                _fileSystemService.MoveFile(video.IncomingTarballFilePath(), Path.Combine(ArchiveDirectory, video.ArchiveFileName));
                _fileSystemService.SaveFileContents(
                    Path.Combine(ArchiveDirectory, video.ArchiveFileName + FileExtension.Log),
                    ex.Message);
            }
        }
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