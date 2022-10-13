using System.Text;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;

namespace Almostengr.VideoProcessor.Domain.Videos.Services;

public sealed class DashCamVideoService : BaseVideoService, IDashCamVideoService
{
    private readonly IFfmpegService _ffmpegService;
    private readonly IFileSystemService _fileSystemService;
    private readonly ITarballService _tarballService;
    private readonly IMusicService _musicService;
    private readonly IVpLogger<DashCamVideoService> _logger;

    public DashCamVideoService(IFileSystemService fileSystemService, IFfmpegService ffmpegService,
        ITarballService tarballService, IMusicService musicService, IVpLogger<DashCamVideoService> logger
    ) : base(fileSystemService, ffmpegService)
    {
        _fileSystemService = fileSystemService;
        _ffmpegService = ffmpegService;
        _tarballService = tarballService;
        _musicService = musicService;
        _logger = logger;
    }

    public override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                DashCamVideo video = new DashCamVideo();

                _fileSystemService.IsDiskSpaceAvailable(video.BaseDirectory);

                video.SetTarballFilePath(_fileSystemService.GetRandomTarballFromDirectory(video.IncomingDirectory));

                _fileSystemService.DeleteDirectory(video.WorkingDirectory);
                _fileSystemService.CreateDirectory(video.WorkingDirectory);

                await _tarballService.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                CreateFfmpegInputFile(video);

                string videoFilter = FfmpegVideoFilter(video);

                await _ffmpegService.FfmpegAsync(
                    $"-y {LOG_ERRORS} -safe 0 -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i \"{FFMPEG_INPUT_FILE}\" -i \"{_musicService.GetRandomMixTrack()}\" -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi -shortest -bsf:a aac_adtstoasc -map 0:v:0 -map 1:a:0 \"{video.OutputFilePath}\"", // string.Empty,
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

    // private void CreateFfmpegInputFile(DashCamVideo video)
    // {
    //     _fileSystemService.DeleteFile(video.FfmpegInputFilePath);

    //     using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
    //     {
    //         var filesInDirectory = _fileSystemService.GetFilesInDirectory(video.WorkingDirectory)
    //         .Where(f => f.EndsWith(FileExtension.Mp4))
    //         .OrderBy(f => f)
    //         .ToArray();

    //         foreach(var file in filesInDirectory)
    //         {
    //             writer.WriteLine($"file '{file}'");
    //         }
    //     }
    // }

    internal override void CreateFfmpegInputFile<DashCamVideo>(DashCamVideo video)
    {
        base.CreateFfmpegInputFile(video);
    }

    internal override string FfmpegVideoFilter<DashCamVideo>(DashCamVideo video)
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
}