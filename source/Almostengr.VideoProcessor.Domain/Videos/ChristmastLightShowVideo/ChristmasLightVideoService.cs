using System.Text;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Domain.Common.Exceptions;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Constants;

namespace Almostengr.VideoProcessor.Domain.Videos.ChristmasLightShow;

public sealed class ChristmasLightVideoService : BaseVideoService, IChristmasLightVideoService
{
    private readonly IFileSystem _fileSystem;
    private readonly IFfmpeg _ffmpeg;
    private readonly ITarball _tarball;
    private readonly IMusicService _musicService;
    private readonly ILoggerService<ChristmasLightVideoService> _logger;
    private readonly AppSettings _appSettings;

    public ChristmasLightVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarball, IMusicService musicService, ILoggerService<ChristmasLightVideoService> logger,
        AppSettings appSettings
    ) : base(fileSystemService, ffmpegService, tarball, appSettings)
    {
        _fileSystem = fileSystemService;
        _ffmpeg = ffmpegService;
        _tarball = tarball;
        _musicService = musicService;
        _logger = logger;
        _appSettings = appSettings;
    }

    public override async Task ProcessVideosAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                ChristmasLightVideo video = new ChristmasLightVideo(_appSettings.ChristmasLightShowDirectory);

                CreateVideoDirectories(video);
                DeleteFilesOlderThanSpecifiedDays(video.UploadDirectory);

                _fileSystem.IsDiskSpaceAvailable(video.BaseDirectory);

                await CreateTarballsFromDirectoriesAsync(video.IncomingDirectory, stoppingToken);

                video.SetTarballFilePath(_fileSystem.GetRandomTarballFromDirectory(video.IncomingDirectory));

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
                _fileSystem.CreateDirectory(video.WorkingDirectory);

                await _tarball.ExtractTarballContentsAsync(
                    video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                _fileSystem.PrepareAllFilesInDirectory(video.WorkingDirectory);

                await _ffmpeg.CreateThumbnailsFromVideoFilesAsync(video, stoppingToken);

                await ConvertVideoFilesToCommonFormatAsync(video, stoppingToken);

                CreateFfmpegInputFile(video);

                string videoFilter = DrawTextVideoFilter(video);

                await _ffmpeg.RenderVideoAsync(
                    video.FfmpegInputFilePath, videoFilter, video.OutputFilePath, stoppingToken);

                _fileSystem.MoveFile(video.TarballFilePath, video.TarballArchiveFilePath, false);

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
            }
        }
        catch (NoTarballsPresentException)
        { }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    internal override string DrawTextVideoFilter<ChristmasLightVideoService>(ChristmasLightVideoService video)
    {
        StringBuilder videoFilter = new(base.DrawTextVideoFilter(video));
        videoFilter.Append(Constant.CommaSpace);
        videoFilter.Append($"drawtext=textfile:'{video.Title}':");
        videoFilter.Append($"fontcolor={video.TextColor()}:");
        videoFilter.Append($"fontsize={MEDIUM_FONT}:");
        videoFilter.Append($"{_lowerLeft}:");
        videoFilter.Append(BORDER_CHANNEL_TEXT);
        videoFilter.Append($"boxcolor={video.BoxColor()}@{DIM_BACKGROUND}");

        return videoFilter.ToString();
    }

}
