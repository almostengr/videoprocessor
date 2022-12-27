using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Exceptions;
using Almostengr.VideoProcessor.Domain.Common.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Domain.Common.Services;
using System.Text;
using Almostengr.VideoProcessor.Domain.Common.Constants;

namespace Almostengr.VideoProcessor.Domain.Technology;

public sealed class TechnologyVideoService : BaseVideoService, ITechnologyVideoService
{
    private readonly IFileSystem _fileSystem;
    private readonly IFfmpeg _ffmpeg;
    private readonly ITarball _tarball;
    private readonly IMusicService _musicService;
    private readonly ILoggerService<TechnologyVideoService> _logger;
    private readonly AppSettings _appSettings;

    public TechnologyVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarballService, IMusicService musicService,
        ILoggerService<TechnologyVideoService> logger, ITarball tarball, AppSettings appSettings
    ) : base(fileSystemService, ffmpegService, tarball, appSettings)
    {
        _fileSystem = fileSystemService;
        _ffmpeg = ffmpegService;
        _tarball = tarballService;
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
                TechnologyVideo video = new TechnologyVideo(_appSettings.TechnologyDirectory);

                CreateVideoDirectories(video);
                DeleteFilesOlderThanSpecifiedDays(video.UploadDirectory);

                _fileSystem.IsDiskSpaceAvailable(video.BaseDirectory);

                await CreateTarballsFromDirectoriesAsync(video.IncomingDirectory, stoppingToken);

                string tarBallFilePath = _fileSystem.GetRandomTarballFromDirectory(video.IncomingDirectory);

                video.SetTarballFilePath(tarBallFilePath);

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
                _fileSystem.CreateDirectory(video.WorkingDirectory);

                await _tarball.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                video.ConfirmChristmasVideo();

                _fileSystem.PrepareAllFilesInDirectory(video.WorkingDirectory); // lowercase all file names

                if (!video.IsChristmasVideo)
                {
                    await AddMusicToVideoAsync(video, stoppingToken); // add audio to timelapse videos
                }

                await ConvertVideoFilesToCommonFormatAsync(video, stoppingToken);

                CreateFfmpegInputFile(video);

                // string videoFilter = DrawTextVideoFilter(video); // todo 
                string videoFilter = VideoFilter(video);

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

    private string VideoFilter(TechnologyVideo video)
    {
        StringBuilder videoFilter = new(base.DrawTextVideoFilter<TechnologyVideo>(video));

        if (video.IsChristmasVideo)
        {
            videoFilter.Append(Constant.CommaSpace);
            videoFilter.Append($"drawtext=textfile:'{video.Title}':");
            videoFilter.Append($"fontcolor={video.TextColor()}:");
            videoFilter.Append($"fontsize={MEDIUM_FONT}:");
            videoFilter.Append($"{_lowerLeft}:");
            videoFilter.Append(BORDER_CHANNEL_TEXT);
            videoFilter.Append($"boxcolor={video.BoxColor()}@{DIM_BACKGROUND}");
        }

        return videoFilter.ToString();
    }
}