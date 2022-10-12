using System.Text;
using Almostengr.VideoProcessor.Domain.Interfaces;

namespace Almostengr.VideoProcessor.Domain.Videos.Services;

public sealed class ToastmastersVideoService : BaseVideoService, IToastmastersVideoService
{
    private readonly IFfmpegService _ffmpegSerivce;
    private readonly IFileSystemService _fileSystemService;
    private readonly ITarballService _tarballService;
    private readonly IVpLogger<ToastmastersVideoService> _logger;

    public ToastmastersVideoService(IFileSystemService fileSystemService, IFfmpegService ffmpegService, 
        ITarballService tarballService, IVpLogger<ToastmastersVideoService> logger
    ) : base(fileSystemService, ffmpegService)
    {
        _fileSystemService = fileSystemService;
        _ffmpegSerivce = ffmpegService;
        _tarballService = tarballService;
        _logger = logger;
    }

    public override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                ToastmastersVideo video = new();

                _fileSystemService.IsDiskSpaceAvailable(video.BaseDirectory);

                video.SetTarballFilePath(_fileSystemService.GetRandomTarballFromDirectory(video.BaseDirectory));

                _fileSystemService.DeleteDirectory(video.WorkingDirectory);
                _fileSystemService.CreateDirectory(video.WorkingDirectory);

                await _tarballService.ExtractTarballContentsAsync(
                    video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                CreateFfmpegInputFile(video);

                string videoFilter = FfmpegVideoFilter(video);

                await _ffmpegSerivce.FfmpegAsync(
                    $"-y {LOG_ERRORS} -safe 0 -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i {FFMPEG_INPUT_FILE} -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi \"{video.OutputFilePath}\"", // string.Empty,
                    video.WorkingDirectory,
                    stoppingToken
                );

                _fileSystemService.MoveFile(video.TarballFilePath, video.ArchiveDirectory);

                _fileSystemService.DeleteDirectory(video.WorkingDirectory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    internal override string FfmpegVideoFilter<ToastmastersVideo>(ToastmastersVideo video)
    {
        StringBuilder videoFilter = new();
        videoFilter.Append($"drawtext=textfile:'{video.ChannelBannerText()}':");
        videoFilter.Append($"fontcolor={video.TextColor()}@{DIM_TEXT}:");
        videoFilter.Append($"fontsize={SMALL_FONT}:");
        videoFilter.Append($"{_upperRight}");
        videoFilter.Append($"box=1:");
        videoFilter.Append($"boxborderw=10:");
        videoFilter.Append($"boxcolor={video.BoxColor()}@{DIM_BACKGROUND}");

        return videoFilter.ToString();
    }

    internal override void CreateFfmpegInputFile<ToastmastersVideo>(ToastmastersVideo video)
    {
        base.CreateFfmpegInputFile(video);
    }
}