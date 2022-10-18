using System.Text;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Domain.Common.Exceptions;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos.ChristmasLightShowVideo;

public sealed class ChristmasLightVideoService : BaseVideoService, IChristmasLightVideoService
{
    private readonly IFileSystem _fileSystem;
    private readonly IFfmpeg _ffmpeg;
    private readonly ITarball _tarball;
    private readonly IMusicService _musicService;
    private readonly ILoggerService<ChristmasLightVideoService> _logger;

    public ChristmasLightVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarball, IMusicService musicService, ILoggerService<ChristmasLightVideoService> logger
        ) : base(fileSystemService, ffmpegService, tarball)
    {
        _fileSystem = fileSystemService;
        _ffmpeg = ffmpegService;
        _tarball = tarball;
        _musicService = musicService;
        _logger = logger;
    }

    public override async Task ProcessVideosAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                ChristmasLightVideo video =
                    new ChristmasLightVideo("/mnt/d74511ce-4722-471d-8d27-05013fd521b3/ChristmasLightShow");
                
                CreateVideoDirectories(video);
                DeleteFilesOlderThanSpecifiedDays(video.UploadDirectory);

                _fileSystem.IsDiskSpaceAvailable(video.BaseDirectory);

                await CreateTarballsFromDirectoriesAsync(video.IncomingDirectory, stoppingToken);

                string tarBallFilePath = _fileSystem.GetRandomTarballFromDirectory(video.BaseDirectory);

                video.SetTarballFilePath(tarBallFilePath);

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
                _fileSystem.CreateDirectory(video.WorkingDirectory);

                await _tarball.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                _fileSystem.PrepareAllFilesInDirectory(video.WorkingDirectory); // lowercase all file names

                await _ffmpeg.CreateThumbnailsFromVideoFilesAsync(video, stoppingToken);

                await ConvertVideoFilesToCommonFormatAsync(video, stoppingToken);

                CreateFfmpegInputFile(video);

                string videoFilter = DrawTextVideoFilter(video);

                // await _ffmpeg.FfmpegAsync(
                //     $"-y {LOG_ERRORS} -safe 0 -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i \"{FFMPEG_INPUT_FILE}\" -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -c:v h264_vaapi -bsf:a aac_adtstoasc \"{video.OutputFilePath}\"", //string.Empty,
                //     video.WorkingDirectory,
                //     stoppingToken);
                
                await _ffmpeg.RenderVideoAsync(
                    video.FfmpegInputFilePath, videoFilter, video.OutputFilePath, stoppingToken);

                _fileSystem.MoveFile(video.TarballFilePath, video.ArchiveDirectory);

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
            }
        }
        catch (NoTarballsPresentException)
        {
            _logger.LogInformation(ExceptionMessage.NoTarballsPresent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    internal override string DrawTextVideoFilter<ChristmasLightVideoService>(ChristmasLightVideoService video)
    {
        StringBuilder videoFilter = new(base.DrawTextVideoFilter(video));
        videoFilter.Append($"drawtext=textfile:'{video.Title}':");
        videoFilter.Append($"fontcolor={video.TextColor()}:");
        videoFilter.Append($"fontsize={SMALL_FONT}:");
        videoFilter.Append($"{_lowerLeft}:");
        videoFilter.Append($"box=1:");
        videoFilter.Append($"boxborderw=10:");
        videoFilter.Append($"boxcolor={video.BoxColor()}:");
        videoFilter.Append($":enable='between(t,0,5)'");

        return videoFilter.ToString();
    }

}
