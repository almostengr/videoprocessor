using Almostengr.VideoProcessor.Domain.Common.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Videos;
using Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Domain.DashCam;

public sealed class DashCamVideoService : BaseVideoService, IDashCamVideoService
{
    private readonly IFfmpeg _ffmpeg;
    private readonly IFileSystem _fileSystem;
    private readonly ITarball _tarball;
    private readonly IMusicService _musicService;
    private readonly ILoggerService<DashCamVideoService> _logger;
    private readonly AppSettings _appSettings;

    public DashCamVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarball, IMusicService musicService, ILoggerService<DashCamVideoService> logger,
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

    public override async Task<bool> ProcessVideosAsync(CancellationToken stoppingToken)
    {
        try
        {
            DashCamVideo video = new DashCamVideo(_appSettings.DashCamDirectory);

            CreateVideoDirectories(video);
            DeleteFilesOlderThanSpecifiedDays(video.UploadDirectory);

            _fileSystem.IsDiskSpaceAvailable(video.BaseDirectory);

            await CreateTarballsFromDirectoriesAsync(video.IncomingDirectory, stoppingToken);

            video.SetTarballFilePath(_fileSystem.GetRandomTarballFromDirectory(video.IncomingDirectory));

            _fileSystem.DeleteDirectory(video.WorkingDirectory);
            _fileSystem.CreateDirectory(video.WorkingDirectory);

            await _tarball.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, stoppingToken);

            _fileSystem.PrepareAllFilesInDirectory(video.WorkingDirectory);

            if (_fileSystem.DoesFileExist(video.GetDetailsFilePath()))
            {
                string[] fileContents = _fileSystem.GetFileContents(video.GetDetailsFilePath()).Split(Environment.NewLine);
                video.AddDetailsContentToVideoFilter(fileContents);
            }

            CreateFfmpegInputFile(video);

            video.AddSubscribeTextFilter();
            video.AddDrawTextFilter(
                video.GetStrippedTitle(), video.BannerTextColor(),
                Constant.DimText,
                FfmpegFontSize.Small,
                DrawTextPosition.UpperLeft,
                video.BannerBackgroundColor(),
                Constant.DimBackground); // video title filter

            if (_fileSystem.DoesFileExist(video.GetNoMusicFilePath()))
            {
                await _ffmpeg.RenderVideoAsync(
                    video.FfmpegInputFilePath, video.VideoFilter, video.OutputFilePath, stoppingToken);
            }
            else
            {
                await _ffmpeg.RenderVideoWithMixTrackAsync(
                    video.FfmpegInputFilePath, _musicService.GetRandomMixTrack(), video.VideoFilter, video.OutputFilePath, stoppingToken);
            }

            _fileSystem.MoveFile(video.TarballFilePath, video.TarballArchiveFilePath, false);
            _fileSystem.DeleteDirectory(video.WorkingDirectory);
        }
        catch (NoTarballsPresentException)
        {
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
        
        return false;
    }

    internal override void CreateFfmpegInputFile<DashCamVideo>(DashCamVideo video)
    {
        _fileSystem.DeleteFile(video.FfmpegInputFilePath);

        using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
        {
            var filesInDirectory = (new DirectoryInfo(video.WorkingDirectory)).GetFiles()
                .OrderBy(f => f.CreationTimeUtc)
                .ToArray();

            foreach (var file in filesInDirectory)
            {
                if (file.Name.EndsWith(FileExtension.Md) ||
                    file.Name.EndsWith(FileExtension.Srt) ||
                    file.Name.EndsWith(FileExtension.Txt))
                {
                    continue;
                }

                writer.WriteLine($"{FILE} '{file}'");
            }
        }
    }
}