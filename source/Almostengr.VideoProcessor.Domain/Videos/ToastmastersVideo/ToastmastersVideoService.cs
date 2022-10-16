using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Exceptions;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Videos;

namespace Almostengr.VideoProcessor.Domain.ToastmastersVideo;

public sealed class ToastmastersVideoService : BaseVideoService, IToastmastersVideoService
{
    private readonly IFfmpeg _ffmpegSerivce;
    private readonly IFileSystem _fileSystem;
    private readonly ITarball _tarball;
    private readonly ILoggerService<ToastmastersVideoService> _logger;

    public ToastmastersVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarballService, ILoggerService<ToastmastersVideoService> logger, ITarball tarball
    ) : base(fileSystemService, ffmpegService, tarball)
    {
        _fileSystem = fileSystemService;
        _ffmpegSerivce = ffmpegService;
        _tarball = tarballService;
        _logger = logger;
    }

    public override async Task ProcessVideosAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                ToastmastersVideo video = new("/mnt/d74511ce-4722-471d-8d27-05013fd521b3/Toastmasters");

                CreateVideoDirectories(video);
                DeleteFilesOlderThanSpecifiedDays(video.UploadDirectory);

                _fileSystem.IsDiskSpaceAvailable(video.BaseDirectory);

                foreach (var directory in _fileSystem.GetDirectoriesInDirectory(video.IncomingDirectory))
                {
                    await _tarball.CreateTarballFromDirectoryAsync(directory, stoppingToken);
                }

                video.SetTarballFilePath(_fileSystem.GetRandomTarballFromDirectory(video.IncomingDirectory));

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
                _fileSystem.CreateDirectory(video.WorkingDirectory);

                await _tarball.ExtractTarballContentsAsync(
                    video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                CreateFfmpegInputFile(video);

                string videoFilter = DrawTextVideoFilter(video);

                // await _ffmpegSerivce.FfmpegAsync(
                //     $"-y {LOG_ERRORS} -safe 0 -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i {FFMPEG_INPUT_FILE} -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi \"{video.OutputFilePath}\"", // string.Empty,
                //     video.WorkingDirectory,
                //     stoppingToken
                // );

                await _ffmpegSerivce.RenderVideoAsync(
                    video.FfmpegInputFilePath, videoFilter, video.OutputFilePath, stoppingToken);

                _fileSystem.MoveFile(video.TarballFilePath, video.UploadDirectory);

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

    // internal override string ChannelBrandingVideoFilter<ToastmastersVideo>(ToastmastersVideo video)
    // {
    //     StringBuilder videoFilter = new();
    //     videoFilter.Append($"drawtext=textfile:'{video.ChannelBannerText()}':");
    //     videoFilter.Append($"fontcolor={video.TextColor()}@{DIM_TEXT}:");
    //     videoFilter.Append($"fontsize={SMALL_FONT}:");
    //     videoFilter.Append($"{_upperRight}");
    //     videoFilter.Append($"box=1:");
    //     videoFilter.Append($"boxborderw=10:");
    //     videoFilter.Append($"boxcolor={video.BoxColor()}@{DIM_BACKGROUND}");

    //     return videoFilter.ToString();
    // }

    // internal override void CreateFfmpegInputFile<ToastmastersVideo>(ToastmastersVideo video)
    // {
    //     base.CreateFfmpegInputFile(video);
    // }
}