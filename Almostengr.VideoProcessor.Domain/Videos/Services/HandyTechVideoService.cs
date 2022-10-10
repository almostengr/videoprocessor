using Almostengr.VideoProcessor.Application.Common;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Domain.Videos;

namespace Almostengr.VideoProcessor.Application.Video.Services;

public sealed class HandyTechVideoService : BaseVideoService, IHandyTechVideoService
{
    // private const string BASE_DIRECTORY = "";
    // private readonly string IncomingDirectory = Path.Combine(BASE_DIRECTORY, "incoming");
    // private readonly string WorkingDirectory = Path.Combine(BASE_DIRECTORY, "working");
    // private readonly string ArchiveDirectory = Path.Combine(BASE_DIRECTORY, "archive");
    // private readonly string UploadDirectory = Path.Combine(BASE_DIRECTORY, "upload");
    private const string _xResolution = "1920";
    private const string _yResolution = "1080";
    private const string _audioBitRate = "196000";
    private const string _audioSampleRate = "48000";
    private const string NO_INTRO_FILE = "nointro.txt";
    private const string SHOW_INTRO_FILENAME_MP4 = "rhtservicesintro.mp4";

    private readonly IFileSystemService _fileSystemService;
    private readonly IFfmpegService _ffmpegService;
    private readonly ITarballService _tarballService;
    private readonly IMusicService _musicService;

    public HandyTechVideoService(IFileSystemService fileSystemService, IFfmpegService ffmpegService,
    ITarballService tarballService, IMusicService musicService) :
        base(fileSystemService, ffmpegService)
    {
        _fileSystemService = fileSystemService;
        _ffmpegService = ffmpegService;
        _tarballService = tarballService;
        _musicService = musicService;
    }

    public override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            HandyTechVideo video = new HandyTechVideo();

            // check for disk space
            if (_fileSystemService.IsDiskSpaceAvailable(video.BaseDirectory) == false)
            {
                return Task.CompletedTask;
            }

            string? tarBallFilePath = _fileSystemService.GetRandomTarballFromDirectory(video.BaseDirectory); // check for files in incoming directory

            video.SetTarballFilePath(tarBallFilePath);

            _fileSystemService.DeleteDirectory(video.WorkingDirectory);
            _fileSystemService.CreateDirectory(video.WorkingDirectory);

            _tarballService.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, stoppingToken);

            // lowercase all file names

            // add audio to timelapse videos

            // copy show intro to working directory

            // convert files to common format

            // string ffmpegFilePath = _fileSystemService.CreateFfmpegInputFile(video.WorkingDirectory);
            // string ffmpegInputFile = Path.Combine(vide, FFMPEG_INPUT_FILE);
            CreateFfmpegInputFile(video);

            // string channelBannerText = GetChannelBannerText();

            string videoFilter = FfmpegVideoFilter(video);

            _ffmpegService.RenderVideoAsync(video.WorkingDirectory, videoFilter);

            _fileSystemService.MoveFile(video.TarballFilePath, video.ArchiveDirectory);

            _fileSystemService.DeleteDirectory(video.WorkingDirectory);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return Task.CompletedTask;
    }

    private void CreateFfmpegInputFile(HandyTechVideo video)
    {
        _fileSystemService.DeleteFile(video.FfmpegInputFilePath);

        using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
        {
            // _logger.LogInformation("Creating FFMPEG input file");
            var filesInDirectory = _fileSystemService.GetFilesInDirectory(video.WorkingDirectory)
                .Where(x => x.EndsWith(FileExtension.Mp4))
                .OrderBy(x => x)
                .ToArray();

            const string rhtservicesintro = "rhtservicesintro.1920x1080.mp4";
            for (int i = 0; i < filesInDirectory.Length; i++)
            {
                if (filesInDirectory[i].Contains(rhtservicesintro))
                {
                    continue;
                }

                if (i == 1 && _fileSystemService.DoesFileExist(Path.Combine(video.WorkingDirectory, NO_INTRO_FILE)) == false)
                {
                    writer.WriteLine($"file '{rhtservicesintro}'"); // add video files
                }

                writer.WriteLine($"file '{Path.GetFileName(filesInDirectory[i])}'"); // add video files
            }
        }
    }

    private string FfmpegVideoFilter(HandyTechVideo video)
    {
        string videoFilter = $"drawtext=textfile:'{video.ChannelBannerText()}':";
        videoFilter += $"fontcolor={video.TextColor()}@{DIM_TEXT}:";
        videoFilter += $"fontsize={SMALL_FONT}:";
        videoFilter += $"{_upperRight}:";
        videoFilter += $"box=1:";
        videoFilter += $"boxborderw={RHT_BORDER_WIDTH.ToString()}:";
        videoFilter += $"boxcolor={video.BoxColor()}@{DIM_BACKGROUND}";
        return videoFilter;
    }

    // private string GetChannelBannerText()
    // {
    //     Random random = new Random();
    //     string[] bannerText = {
    //         "rhtservices.net",
    //         "Robinson Handy and Technology Services",
    //         "rhtservices.net/facebook",
    //         "rhtservices.net/instagram",
    //         "rhtservices.net/youtube",
    //         "@rhtservicesllc"
    //         };

    //     return bannerText.ElementAt(random.Next(0, bannerText.Length));
    // }

    // protected override void CheckOrCreateFfmpegInputFile()
    // {
    //     string ffmpegInputFile = Path.Combine(_workingDirectory, FFMPEG_INPUT_FILE);
    //     DeleteFile(ffmpegInputFile);

    //     using (StreamWriter writer = new StreamWriter(ffmpegInputFile))
    //     {
    //         _logger.LogInformation("Creating FFMPEG input file");
    //         var filesInDirectory = _fileSystemService.GetFilesInDirectory(_workingDirectory)
    //             .Where(x => x.EndsWith(FileExtension.Mp4))
    //             .OrderBy(x => x)
    //             .ToArray();

    //         string rhtservicesintro = "rhtservicesintro.1920x1080.mp4";
    //         for (int i = 0; i < filesInDirectory.Length; i++)
    //         {
    //             if (filesInDirectory[i].Contains(rhtservicesintro))
    //             {
    //                 continue;
    //             }

    //             if (i == 1 && DoesFileExist(Path.Combine(_workingDirectory, NO_INTRO_FILE)) == false)
    //             {
    //                 writer.WriteLine($"file '{rhtservicesintro}'"); // add video files
    //             }

    //             writer.WriteLine($"file '{Path.GetFileName(filesInDirectory[i])}'"); // add video files
    //         }
    //     }
    // }

    private async Task AddAudioToTimelapseAsync(string workingDirectory, CancellationToken cancellationToken)
    {
        var videoFiles = _fileSystemService.GetFilesInDirectory(workingDirectory)
            .Where(x => !x.Contains("narration") || !x.Contains("narrative"))
            .Where(x => x.EndsWith(FileExtension.Mp4));

        var narrationFiles = _fileSystemService.GetFilesInDirectory(workingDirectory)
            .Where(x => x.Contains("narration") || x.Contains("narrative"))
            .Where(x => x.EndsWith(FileExtension.Mp4) || x.EndsWith(FileExtension.Mkv))
            .ToArray();

        foreach (var videoFileName in videoFiles)
        {
            // var result = await RunCommandAsync(
            //     ProgramPaths.Ffprobe,
            //     $"-hide_banner \"{videoFileName}\"",
            //     workingDirectory,
            //     cancellationToken,
            //     1
            // );

            var result = await _ffmpegService.FfprobeAsync(videoFileName, workingDirectory, cancellationToken);

            if (result.stdErr.ToLower().Contains("audio"))
            {
                continue;
            }

            string? audioFile = narrationFiles.Where(
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

            // await RunCommandAsync(
            //     ProgramPaths.Ffmpeg,
            //     $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -i \"{audioFile}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
            //     workingDirectory,
            //     cancellationToken,
            //     10);

            await _ffmpegService.FfmpegAsync(
                $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -i \"{audioFile}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
                workingDirectory, 
                cancellationToken);

            _fileSystemService.DeleteFile(videoFileName);
            _fileSystemService.MoveFile(Path.Combine(workingDirectory, outputFileName), videoFileName);
        }

        _fileSystemService.DeleteFiles(narrationFiles);
    }

    private async Task ConvertVideoFilesToCommonFormatAsync(string directory, CancellationToken cancellationToken)
    {
        var videoFiles = _fileSystemService.GetFilesInDirectory(directory)
            .Where(x => x.EndsWith(FileExtension.Mkv) || x.EndsWith(FileExtension.Mov) || x.EndsWith(FileExtension.Mp4))
            .OrderBy(x => x)
            .ToArray();

        foreach (var videoFileName in videoFiles)
        {
            // _logger.LogInformation($"Checking resolution of {videoFileName}");

            // var result = await RunCommandAsync(
            //     ProgramPaths.Ffprobe,
            //     $"-hide_banner \"{videoFileName}\"",
            //     directory,
            //     cancellationToken,
            //     1
            // );

            var result = await _ffmpegService.FfprobeAsync(videoFileName, directory, cancellationToken);

            string scaledFile = $"{Path.GetFileNameWithoutExtension(videoFileName)}.{_xResolution}x{_yResolution}{FileExtension.Mp4}";

            if (result.stdErr.Contains($"{_xResolution}x{_yResolution}") &&
                result.stdErr.Contains($"{_audioBitRate} Hz") &&
                result.stdErr.Contains($"196 kb/s") &&
                videoFileName.EndsWith(FileExtension.Mp4))
            {
                _fileSystemService.MoveFile(Path.Combine(directory, videoFileName), Path.Combine(directory, scaledFile));
                continue;
            }

            // _logger.LogInformation($"Converting video {videoFileName} to common format");

            // await RunCommandAsync(
            //     ProgramPaths.Ffmpeg,
            //     $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -r 30 -vf \"scale_vaapi=w={_xResolution}:h={_yResolution}\" -vcodec h264_vaapi -ar {_audioSampleRate} -b:a {_audioBitRate} \"{scaledFile}\"",
            //     directory,
            //     cancellationToken,
            //     10
            // );

            await _ffmpegService.FfmpegAsync(
                $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -r 30 -vf \"scale_vaapi=w={_xResolution}:h={_yResolution}\" -vcodec h264_vaapi -ar {_audioSampleRate} -b:a {_audioBitRate} \"{scaledFile}\"",
                directory, 
                cancellationToken);

            _fileSystemService.DeleteFile(Path.Combine(directory, videoFileName));

            string outputFileName = Path.GetFileNameWithoutExtension(videoFileName) + FileExtension.Mp4;
        }
    }

}