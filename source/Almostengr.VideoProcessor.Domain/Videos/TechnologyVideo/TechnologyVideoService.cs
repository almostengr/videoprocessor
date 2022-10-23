using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Exceptions;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;

namespace Almostengr.VideoProcessor.Domain.Videos.TechnologyVideo;

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
        ) : base(fileSystemService, ffmpegService, tarball)
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

                string tarBallFilePath = _fileSystem.GetRandomTarballFromDirectory(video.BaseDirectory);

                video.SetTarballFilePath(tarBallFilePath);

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
                _fileSystem.CreateDirectory(video.WorkingDirectory);

                await _tarball.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                _fileSystem.PrepareAllFilesInDirectory(video.WorkingDirectory); // lowercase all file names

                // await AddMusicToVideoAsync(video, stoppingToken); // add audio to timelapse videos

                await _ffmpeg.CreateThumbnailsFromVideoFilesAsync(video, stoppingToken);

                _fileSystem.CopyFile(video.RhtServicesIntroPath, video.WorkingDirectory);

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

    internal override void CreateFfmpegInputFile<TechnologyVideo>(TechnologyVideo video)
    {
        base.RhtCreateFfmpegInputFile<TechnologyVideo>(video);
    }

}