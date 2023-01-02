using System.Text;
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

                video.SetSubtitleFilePath(_fileSystem.GetFilesInDirectory(video.WorkingDirectory)
                    .Where(f => f.EndsWith(FileExtension.Srt))
                    .SingleOrDefault());

                string videoFilter = DashCamVideoFilter(video);

                await _ffmpeg.RenderVideoWithMixTrackAsync(
                    video.FfmpegInputFilePath, _musicService.GetRandomMixTrack(), videoFilter, video.OutputFilePath, stoppingToken);

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

    private string DashCamVideoFilter(DashCamVideo video)
    {
        StringBuilder videoFilter = new(base.DrawTextVideoFilter(video));

        // video title in upper left
        videoFilter.Append(Constant.CommaSpace);
        videoFilter.Append($"drawtext=textfile:'{(video.Title.Split())[0]}':");
        videoFilter.Append($"fontcolor={video.BannerTextColor()}@{DIM_TEXT}:");
        videoFilter.Append($"fontsize={SMALL_FONT}:");
        videoFilter.Append($"{_upperLeft}:");
        videoFilter.Append(BORDER_CHANNEL_TEXT);
        videoFilter.Append($"boxcolor={video.BannerBackgroundColor()}@{DIM_BACKGROUND}");

        // // mileage and roads taken
        bool detailFilesPresent = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
            .Where(f => f.EndsWith(video.GetDetailsFileName())).Any();

        videoFilter.Append(Constant.CommaSpace);
        videoFilter.Append(_subscribeFilter);

        if (detailFilesPresent)
        {
            const int DISPLAY_DURATION = 5;
            string[] fileContents = _fileSystem.GetFileContents(video.GetDetailsFilePath()).Split(Environment.NewLine);

            foreach (string content in fileContents)
            {
                string[] parseContent = content.Split("|");
                int startSeconds = Int32.Parse(parseContent[0]);
                int endSeconds = startSeconds + DISPLAY_DURATION;
                string displayText = parseContent[1].Replace(":", "\\:");

                videoFilter.Append(Constant.CommaSpace);
                videoFilter.Append($"drawtext=textfile:'{displayText.ToUpper()}':");
                videoFilter.Append($"fontcolor={video.SubtitleTextColor()}:");
                videoFilter.Append($"fontsize={SMALL_FONT}:");
                videoFilter.Append($"{_lowerRight}:");
                videoFilter.Append(BORDER_LOWER_THIRD);
                videoFilter.Append($"boxcolor={video.SubtitleBackgroundColor()}:");
                videoFilter.Append($"enable='between(t,{startSeconds},{endSeconds})'");
            }
        }

        return videoFilter.ToString();
    }

    // internal override string DrawTextVideoFilter<DashCamVideo>(DashCamVideo video)
    // {
    //     StringBuilder videoFilter = new(base.DrawTextVideoFilter(video));

    //     // video title in upper left
    //     videoFilter.Append(Constant.CommaSpace);
    //     videoFilter.Append($"drawtext=textfile:'{(video.Title.Split())[0]}':");
    //     videoFilter.Append($"fontcolor={video.BannerTextColor()}@{DIM_TEXT}:");
    //     videoFilter.Append($"fontsize={SMALL_FONT}:");
    //     videoFilter.Append($"{_upperLeft}:");
    //     videoFilter.Append(BORDER_CHANNEL_TEXT);
    //     videoFilter.Append($"boxcolor={video.BannerBackgroundColor()}@{DIM_BACKGROUND}");

    //     // // mileage and roads taken
    //     bool detailFilesPresent = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
    //         .Where(f => f.EndsWith("details.txt")).Any();

    //     videoFilter.Append(Constant.CommaSpace);
    //     videoFilter.Append(_subscribeFilter);

    //     if (detailFilesPresent)
    //     {
    //         const int DISPLAY_DURATION = 5;
    //         string detailsFilePath = Path.Combine(video.WorkingDirectory, "details.txt");
    //         string[] fileContents = _fileSystem.GetFileContents(detailsFilePath).Split(Environment.NewLine);

    //         for (int i = 0; i < fileContents.Count(); i++)
    //         {
    //             int startSeconds = i * DISPLAY_DURATION;
    //             int endSeconds = startSeconds + DISPLAY_DURATION;

    //             videoFilter.Append(Constant.CommaSpace);
    //             videoFilter.Append($"drawtext=textfile:'{fileContents[i].Replace(":", "\\:")}':");
    //             videoFilter.Append($"fontcolor={FfMpegColors.White}:");
    //             videoFilter.Append($"fontsize={MEDIUM_FONT}:");
    //             videoFilter.Append($"{_lowerRight}:");
    //             videoFilter.Append(BORDER_LOWER_THIRD);
    //             videoFilter.Append($"boxcolor={FfMpegColors.Green}:");
    //             videoFilter.Append($"enable='between(t,{startSeconds},{endSeconds})'");
    //         }
    //     }

    //     return videoFilter.ToString();
    // }
}