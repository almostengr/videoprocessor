using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Videos;
using Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Domain.Technology;

public sealed class TechnologyVideoService : BaseVideoService, ITechnologyVideoService
{
    private readonly IFileSystem _fileSystem;
    private readonly IFfmpeg _ffmpeg;
    private readonly ITarball _tarball;
    private readonly IMusicService _musicService;
    private readonly ILoggerService<TechnologyVideoService> _logger;
    private readonly AppSettings _appSettings;
    private readonly IRandomService _randomService;

    public TechnologyVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarballService, IMusicService musicService,
        ILoggerService<TechnologyVideoService> logger, ITarball tarball, AppSettings appSettings,
        IRandomService randomService
    ) : base(fileSystemService, ffmpegService, tarball, appSettings)
    {
        _fileSystem = fileSystemService;
        _ffmpeg = ffmpegService;
        _tarball = tarballService;
        _musicService = musicService;
        _logger = logger;
        _appSettings = appSettings;
        _randomService = randomService;
    }

    public override async Task<bool> ProcessVideosAsync(CancellationToken stoppingToken)
    {
        TechnologyVideo video = new TechnologyVideo(_appSettings.TechnologyDirectory);

        try
        {
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

            _fileSystem.PrepareAllFilesInDirectory(video.WorkingDirectory);

            if (video.IsChristmasVideo)
            {
                video.SetChannelBannerText(SelectChristmasChannelBannerText());
            }
            else
            {
                video.SetChannelBannerText(SelectChannelBannerText());
            }

            if (!video.IsChristmasVideo)
            {
                await AddMusicToVideoAsync(video, stoppingToken); // add audio to timelapse videos
            }

            await ConvertVideoFilesToCommonFormatAsync(video, stoppingToken);

            CreateFfmpegInputFile(video);

            if (video.IsChristmasVideo)
            {
                video.AddDrawTextFilter(
                    video.Title,
                    FfMpegColor.White,
                    Constant.SolidText,
                    FfmpegFontSize.Large,
                    DrawTextPosition.LowerLeft,
                    FfMpegColor.Maroon,
                    Constant.SolidBackground);
            }

            await _ffmpeg.RenderVideoAsync(
                video.FfmpegInputFilePath, video.VideoFilter, video.OutputFilePath, stoppingToken);

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
            _fileSystem.MoveFile(video.TarballFilePath, video.TarballErrorFilePath, false);
            _fileSystem.SaveFileContents(video.ErrorLogFilePath, ex.Message);
        }

        return false;
    }

    internal override string SelectChannelBannerText()
    {
        var options = RhtServicesBannerTextOptions();
        return options.ElementAt(_randomService.Next(0, options.Count()));
    }

    private string SelectChristmasChannelBannerText()
    {
        string[] text = { "rhtservices.net", "twitter.com/hplightshow" };
        return text[_randomService.Next(0, text.Count())];
    }
}