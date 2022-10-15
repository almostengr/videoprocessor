using System.Text;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Exceptions;

namespace Almostengr.VideoProcessor.Domain.Videos.DashCamVideo;

public sealed class DashCamVideoService : BaseVideoService, IDashCamVideoService
{
    private readonly IFfmpeg _ffmpeg;
    private readonly IFileSystem _fileSystem;
    private readonly ITarball _tarball;
    private readonly IMusicService _musicService;
    private readonly ILoggerService<DashCamVideoService> _logger;

    public DashCamVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarball, IMusicService musicService, ILoggerService<DashCamVideoService> logger
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
            // todo clean upload directory before starting 

            while (true)
            {
                DashCamVideo video = new DashCamVideo("/mnt/d74511ce-4722-471d-8d27-05013fd521b3/Kenny Ram Dash Cam");

                CreateVideoDirectories(video);

                _fileSystem.IsDiskSpaceAvailable(video.BaseDirectory);

                await CreateTarballsFromDirectoriesAsync(video.IncomingDirectory, stoppingToken);

                video.SetTarballFilePath(_fileSystem.GetRandomTarballFromDirectory(video.IncomingDirectory));

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
                _fileSystem.CreateDirectory(video.WorkingDirectory);

                await _tarball.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                CreateFfmpegInputFile(video);

                string videoFilter = base.DrawTextVideoFilter(video);

                // await _ffmpeg.FfmpegAsync(
                //     $"-y {LOG_ERRORS} -safe 0 -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i \"{FFMPEG_INPUT_FILE}\" -i \"{_musicService.GetRandomMixTrack()}\" -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi -shortest -bsf:a aac_adtstoasc -map 0:v:0 -map 1:a:0 \"{video.OutputFilePath}\"", // string.Empty,
                //     video.WorkingDirectory,
                //     stoppingToken
                // );

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

    internal override void CreateFfmpegInputFile<DashCamVideo>(DashCamVideo video)
    {
        _fileSystem.DeleteFile(video.FfmpegInputFilePath);

        using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
        {
            var filesInDirectory = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
            .Where(f => f.EndsWith(FileExtension.Mov))
            .OrderBy(f => f)
            .ToArray();

            foreach (var file in filesInDirectory)
            {
                writer.WriteLine($"file '{file}'");
            }
        }
    }

    internal override string DrawTextVideoFilter<DashCamVideo>(DashCamVideo video)
    {
        StringBuilder videoFilter = new();
        videoFilter.Append(base.DrawTextVideoFilter(video));
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