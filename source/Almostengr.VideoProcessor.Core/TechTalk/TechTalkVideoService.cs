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

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            if (video.ArchiveFileName.ToLower().StartsWith(END_SCREEN))
            {
                await CreateEndScreenVideoAsync(WorkingDirectory, cancellationToken);
                _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
                _fileSystemService.DeleteDirectory(WorkingDirectory);
                return;
            }

            _fileSystemService.DeleteFile(video.FfmpegInputFilePath());

            var videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4) || f.EndsWith(FileExtension.Mkv))
                .OrderBy(f => f)
                .ToList();

            await MergeVideoAndAudioFilesAsync(cancellationToken);

            foreach (var file in videoFiles)
            {
                await _ffmpegService.ConvertVideoFileToTsFormatAsync(
                    file, file + FileExtension.Ts, cancellationToken);
            }

            videoFiles.Clear();

            videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Ts))
                .OrderBy(f => f)
                .ToList();

            // if (video.SubType == TechTalkVideoSubType.TechTalk && video.IsDraft() == false)
            // {
            //     videoFiles.Add(video.EndScreenFilePath());
            // }


            CreateFfmpegInputFile(videoFiles.ToArray(), video.FfmpegInputFilePath());

            if (video.IsDraft())
            {
                await _ffmpegService.ConcatTsFilesToMp4FileAsync(
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
                RandomChannelBrandingText(video.BrandingTextOptions()),
                video.DrawTextFilterTextColor(),
                Opacity.Full,
                FfmpegFontSize.Large,
                DrawTextPosition.UpperRight,
                video.DrawTextFilterBackgroundColor(),
                Opacity.Light,
                10);

            if (_fileSystemService.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss)).Any())
            {
                video.AddSubtitleVideoFilter(
                    _fileSystemService.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss)).Single());
            }

            var graphicsSubtitle = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.GraphicsAss) || f.EndsWith(FileExtension.GraphicsSrt))
                .SingleOrDefault();
            video.SetGraphicsSubtitleFileName(graphicsSubtitle);

            await _ffmpegService.ConcatTsFilesToMp4FileAsync(
                video.FfmpegInputFilePath(), video.OutputVideoFilePath(), video.VideoFilter, cancellationToken);

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

    private async Task MergeVideoAndAudioFilesAsync(CancellationToken cancellationToken)
    {
        var workingDirVideos = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
            .Where(f => f.EndsWith(FileExtension.Mp4) || f.EndsWith(FileExtension.Mkv));

        foreach (var videoFilePath in workingDirVideos)
        {
            string? audioFilePath = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.Contains(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3))
                // .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3))
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(audioFilePath))
            {
                continue;
            }

            string tempOutputFilePath = Path.Combine(WorkingDirectory,
                Path.GetFileNameWithoutExtension(videoFilePath) + ".tmp" + Path.GetExtension(videoFilePath));

            await _ffmpegService.AddAccAudioToVideoAsync(
                videoFilePath, audioFilePath, tempOutputFilePath, cancellationToken);

            _fileSystemService.DeleteFile(videoFilePath);
            _fileSystemService.MoveFile(tempOutputFilePath, videoFilePath);
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