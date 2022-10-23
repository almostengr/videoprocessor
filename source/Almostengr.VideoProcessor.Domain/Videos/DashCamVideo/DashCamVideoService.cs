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
    private const string DESTINATION_FILE = "destination.txt";
    private const string MAJOR_ROADS_FILE = "majorroads.txt";
    private const string DETAILS_FILE = "details.txt";
    private const string VFLIP_FILE = "vflip.txt";
    private readonly AppSettings _appSettings;

    public DashCamVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarball, IMusicService musicService, ILoggerService<DashCamVideoService> logger,
        AppSettings appSettings
    ) : base(fileSystemService, ffmpegService, tarball)
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
                DashCamVideo video = new DashCamVideo(_appSettings.DashCamDirectory);

                CreateVideoDirectories(video);
                DeleteFilesOlderThanSpecifiedDays(video.UploadDirectory);

                _fileSystem.IsDiskSpaceAvailable(video.BaseDirectory);

                await CreateTarballsFromDirectoriesAsync(video.IncomingDirectory, stoppingToken);

                video.SetTarballFilePath(_fileSystem.GetRandomTarballFromDirectory(video.IncomingDirectory));

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
                _fileSystem.CreateDirectory(video.WorkingDirectory);

                await _tarball.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                foreach (var file in _fileSystem.GetFilesInDirectory(video.WorkingDirectory))
                {
                    await _ffmpeg.ConvertVideoFileToTsFormatAsync(file, video.WorkingDirectory, stoppingToken);
                }

                _fileSystem.PrepareAllFilesInDirectory(video.WorkingDirectory);

                CreateFfmpegInputFile(video);

                string videoFilter = base.DrawTextVideoFilter(video);

                await _ffmpeg.RenderVideoAsync(
                    video.FfmpegInputFilePath, videoFilter, video.OutputFilePath, stoppingToken);

                _fileSystem.DeleteFiles(
                    _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
                    .Where(f => f.EndsWith(FileExtension.Ts) == false ||
                        f.EndsWith(DESTINATION_FILE) == false)
                    .ToArray()
                );

                await _tarball.CreateTarballFromDirectoryAsync(
                    Path.Combine(video.ArchiveDirectory, video.TarballFileName),
                    video.WorkingDirectory,
                    stoppingToken);

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
                _fileSystem.DeleteFile(video.TarballFilePath);
            }
        }
        catch (NoTarballsPresentException)
        { }
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
            var filesInDirectory = (new DirectoryInfo(video.WorkingDirectory)).GetFiles()
                .OrderBy(f => f.Name)
                .ToArray();

            foreach (var file in filesInDirectory)
            {
                writer.WriteLine($"{FILE} '{file}'");
            }
        }
    }

    internal override string DrawTextVideoFilter<DashCamVideo>(DashCamVideo video)
    {
        StringBuilder videoFilter = new(base.DrawTextVideoFilter(video));

        // video title in upper left
        videoFilter.Append(Constants.CommaSpace);
        videoFilter.Append($"drawtext=textfile:'{(video.Title.Split())[0]}':");
        videoFilter.Append($"fontcolor={video.TextColor()}@{DIM_TEXT}:");
        videoFilter.Append($"fontsize={SMALL_FONT}:");
        videoFilter.Append($"{_upperLeft}:");
        videoFilter.Append(BORDER_CHANNEL_TEXT);
        videoFilter.Append($"boxcolor={video.BoxColor()}@{DIM_BACKGROUND}");

        // mileage and roads taken
        bool destinationFilePresent = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
            .Where(f => f.Contains(DESTINATION_FILE))
            .Any();

        if (destinationFilePresent)
        {
            var destinationText = _fileSystem.GetFileContents(
                Path.Combine(video.WorkingDirectory, DESTINATION_FILE));

            videoFilter.Append(Constants.CommaSpace);
            videoFilter.Append($"drawtext=textfile:'{destinationText}':");
            videoFilter.Append($"fontcolor={FfMpegColors.White}:");
            videoFilter.Append($"fontsize={LARGE_FONT}:");
            videoFilter.Append($"{_lowerLeft}:");
            videoFilter.Append(BORDER_LOWER_THIRD);
            videoFilter.Append($"boxcolor={FfMpegColors.Green}:");
            videoFilter.Append("enable='between(t,5,20)'");
        }

        videoFilter.Append(Constants.CommaSpace);
        videoFilter.Append(_subscribeFilter);

        return videoFilter.ToString();
    }
}