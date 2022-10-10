using Almostengr.VideoProcessor.Application.Common;
using Almostengr.VideoProcessor.Domain.Videos;

namespace Almostengr.VideoProcessor.Application.Video;

public sealed class HandyTechVideoService : BaseVideoService, IHandyTechVideoService
{
    private const string BASE_DIRECTORY = "";
    private readonly string IncomingDirectory = Path.Combine(BASE_DIRECTORY, "incoming");
    private readonly string WorkingDirectory = Path.Combine(BASE_DIRECTORY, "working");
    private readonly string ArchiveDirectory = Path.Combine(BASE_DIRECTORY, "archive");
    private readonly string UploadDirectory = Path.Combine(BASE_DIRECTORY, "upload");
    private const string _xResolution = "1920";
    private const string _yResolution = "1080";
    private const string _audioBitRate = "196000";
    private const string _audioSampleRate = "48000";
    private const string NO_INTRO_FILE = "nointro.txt";
    private const string SHOW_INTRO_FILENAME_MP4 = "rhtservicesintro.mp4";

    private readonly IFileSystemService _fileSystemService;

    public HandyTechVideoService(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    public override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // check for disk space
            if (_fileSystemService.IsDiskSpaceAvailable(WorkingDirectory) == false)
            {
                return Task.CompletedTask;
            }

            string? tarBall = _fileSystemService.GetRandomTarballFromDirectory(IncomingDirectory); // check for files in incoming directory

            if (string.IsNullOrEmpty(tarBall))
            {
                return Task.CompletedTask;
            }

            Video video = new Video(tarBall);

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            _fileSystemService.ExtractTarballContents(tarBall, WorkingDirectory);

            // lowercase all file names

            // add audio to timelapse videos

            // copy show intro to working directory

            // convert files to common format

            string ffmpegFilePath = _fileSystemService.CreateFfmpegInputFile(WorkingDirectory);

            string channelBannerText = GetChannelBannerText();

            string ffmpegFilters = string.Empty;

            _fileSystemService.RenderVideo(WorkingDirectory, ffmpegFilters);

            _fileSystemService.MoveFile(tarBall, ArchiveDirectory);

            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return Task.CompletedTask;
    }

    private string GetChannelBannerText()
    {
        Random random = new Random();
        string[] bannerText = {
            "rhtservices.net",
            "Robinson Handy and Technology Services",
            "rhtservices.net/facebook",
            "rhtservices.net/instagram",
            "rhtservices.net/youtube",
            "@rhtservicesllc"
            };

        return bannerText.ElementAt(random.Next(0, bannerText.Length));
    }

    protected override string GetFfmpegVideoFilters(string videoTitle)
    {
        string textColor = GetTextColor(videoTitle);

        string videoFilter = $"drawtext=textfile:'{GetBrandingText()}':";
        videoFilter += $"fontcolor={textColor}@{DIM_TEXT}:";
        videoFilter += $"fontsize={SMALL_FONT}:";
        videoFilter += $"{_upperRight}:";
        videoFilter += $"box=1:";
        videoFilter += $"boxborderw={RHT_BORDER_WIDTH.ToString()}:";
        videoFilter += $"boxcolor={GetBoxColor(videoTitle)}@{DIM_BACKGROUND}";

        return videoFilter;
    }

    protected override string GetTextColor(string videoTitle)
    {
        videoTitle = videoTitle.ToLower();
        if (videoTitle.Contains("christmas"))
        {
            return FfMpegColors.Green;
        }

        return FfMpegColors.White;
    }

    private string GetBoxColor(string videoTitle)
    {
        videoTitle = videoTitle.ToLower();

        if (videoTitle.Contains("christmas"))
        {
            return _random.Next(0, 50) >= 25 ? FfMpegColors.Green : FfMpegColors.Maroon;
        }

        return FfMpegColors.Black;
    }

    protected override void CheckOrCreateFfmpegInputFile()
    {
        string ffmpegInputFile = Path.Combine(_workingDirectory, FFMPEG_INPUT_FILE);
        DeleteFile(ffmpegInputFile);

        using (StreamWriter writer = new StreamWriter(ffmpegInputFile))
        {
            _logger.LogInformation("Creating FFMPEG input file");
            var filesInDirectory = GetFilesInDirectory(_workingDirectory)
                .Where(x => x.EndsWith(FileExtension.Mp4))
                .OrderBy(x => x)
                .ToArray();

            string rhtservicesintro = "rhtservicesintro.1920x1080.mp4";
            for (int i = 0; i < filesInDirectory.Length; i++)
            {
                if (filesInDirectory[i].Contains(rhtservicesintro))
                {
                    continue;
                }

                if (i == 1 && DoesFileExist(Path.Combine(_workingDirectory, NO_INTRO_FILE)) == false)
                {
                    writer.WriteLine($"file '{rhtservicesintro}'"); // add video files
                }

                writer.WriteLine($"file '{Path.GetFileName(filesInDirectory[i])}'"); // add video files
            }
        }
    }

    private async Task AddAudioToTimelapseAsync(string workingDirectory, CancellationToken cancellationToken)
    {
        var videoFiles = GetFilesInDirectory(workingDirectory)
            .Where(x => !x.Contains("narration") || !x.Contains("narrative"))
            .Where(x => x.EndsWith(FileExtension.Mp4));

        var narrationFiles = GetFilesInDirectory(workingDirectory)
            .Where(x => x.Contains("narration") || x.Contains("narrative"))
            .Where(x => x.EndsWith(FileExtension.Mp4) || x.EndsWith(FileExtension.Mkv))
            .ToArray();

        foreach (var videoFileName in videoFiles)
        {
            var result = await RunCommandAsync(
                ProgramPaths.Ffprobe,
                $"-hide_banner \"{videoFileName}\"",
                workingDirectory,
                cancellationToken,
                1
            );

            if (result.stdErr.ToLower().Contains("audio"))
            {
                continue;
            }

            string audioFile = narrationFiles.Where(
                    x => x.Contains(Path.GetFileNameWithoutExtension(videoFileName))
                )
                .SingleOrDefault();

            if (string.IsNullOrEmpty(audioFile))
            {
                audioFile = _musicService.GetRandomNonMixTrack();
            }

            string outputFileName = $"{Path.GetFileNameWithoutExtension(videoFileName)}.tmp{FileExtension.Mp4}";

            outputFileName = outputFileName.Replace("narration", string.Empty)
                .Replace("narrative", string.Empty);

            await RunCommandAsync(
                ProgramPaths.Ffmpeg,
                $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -i \"{audioFile}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
                workingDirectory,
                cancellationToken,
                10);

            DeleteFile(videoFileName);
            MoveFile(Path.Combine(workingDirectory, outputFileName), videoFileName);
        }

        DeleteFiles(narrationFiles);
    }

    protected override async Task ConvertVideoFilesToCommonFormatAsync(string directory, CancellationToken cancellationToken)
    {
        var videoFiles = GetFilesInDirectory(directory)
            .Where(x => x.EndsWith(FileExtension.Mkv) || x.EndsWith(FileExtension.Mov) || x.EndsWith(FileExtension.Mp4))
            .OrderBy(x => x)
            .ToArray();

        foreach (var videoFileName in videoFiles)
        {
            _logger.LogInformation($"Checking resolution of {videoFileName}");

            var result = await RunCommandAsync(
                ProgramPaths.Ffprobe,
                $"-hide_banner \"{videoFileName}\"",
                directory,
                cancellationToken,
                1
            );

            string scaledFile = $"{Path.GetFileNameWithoutExtension(videoFileName)}.{_xResolution}x{_yResolution}{FileExtension.Mp4}";

            if (result.stdErr.Contains($"{_xResolution}x{_yResolution}") &&
                result.stdErr.Contains($"{_audioBitRate} Hz") &&
                result.stdErr.Contains($"196 kb/s") &&
                videoFileName.EndsWith(FileExtension.Mp4))
            {
                MoveFile(Path.Combine(directory, videoFileName), Path.Combine(directory, scaledFile));
                continue;
            }

            _logger.LogInformation($"Converting video {videoFileName} to common format");

            await RunCommandAsync(
                ProgramPaths.Ffmpeg,
                $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -r 30 -vf \"scale_vaapi=w={_xResolution}:h={_yResolution}\" -vcodec h264_vaapi -ar {_audioSampleRate} -b:a {_audioBitRate} \"{scaledFile}\"",
                directory,
                cancellationToken,
                10
            );

            DeleteFile(Path.Combine(directory, videoFileName));

            string outputFileName = Path.GetFileNameWithoutExtension(videoFileName) + FileExtension.Mp4;
        }
    }

    private void CopyShowIntroToWorkingDirectory(string introVideoPath, string workingDirectory)
    {
        CopyFile(introVideoPath, Path.Combine(workingDirectory, SHOW_INTRO_FILENAME_MP4));
    }

}