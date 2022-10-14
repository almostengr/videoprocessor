using System.Text;
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

    public TechnologyVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarballService, IMusicService musicService, ILoggerService<TechnologyVideoService> logger
        ) : base(fileSystemService, ffmpegService)
    {
        _fileSystem = fileSystemService;
        _ffmpeg = ffmpegService;
        _tarball = tarballService;
        _musicService = musicService;
        _logger = logger;
    }

    public override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                TechnologyVideo video = new TechnologyVideo("/mnt/d74511ce-4722-471d-8d27-05013fd521b3/RhtTechnology");

                _fileSystem.IsDiskSpaceAvailable(video.BaseDirectory);

                string? tarBallFilePath = _fileSystem.GetRandomTarballFromDirectory(video.BaseDirectory);

                video.SetTarballFilePath(tarBallFilePath);

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
                _fileSystem.CreateDirectory(video.WorkingDirectory);

                await _tarball.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                _fileSystem.PrepareAllFilesInDirectory(video.WorkingDirectory); // lowercase all file names

                await AddMusicToVideoAsync(video, stoppingToken); // add audio to timelapse videos

                _fileSystem.CopyFile(video.RhtServicesIntroPath, video.WorkingDirectory);

                await ConvertVideoFilesToCommonFormatAsync(video, stoppingToken);

                CreateFfmpegInputFile(video);

                string videoFilter = FfmpegVideoFilter(video);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    // private void CreateFfmpegInputFile(TechnologyVideo video)
    // {
    //     _fileSystem.DeleteFile(video.FfmpegInputFilePath);

    //     using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
    //     {
    //         var filesInDirectory = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
    //             .Where(x => x.EndsWith(FileExtension.Mp4))
    //             .OrderBy(x => x)
    //             .ToArray();

    //         const string rhtservicesintro = "rhtservicesintro.1920x1080.mp4";
    //         const string file = "file";
    //         for (int i = 0; i < filesInDirectory.Length; i++)
    //         {
    //             if (filesInDirectory[i].Contains(rhtservicesintro))
    //             {
    //                 continue;
    //             }

    //             if (i == 1 && _fileSystem.DoesFileExist(video.NoIntroFilePath()) == false)
    //             {
    //                 writer.WriteLine($"{file} '{rhtservicesintro}'");
    //             }

    //             writer.WriteLine($"{file} '{Path.GetFileName(filesInDirectory[i])}'");
    //         }
    //     }
    // }

    internal override string FfmpegVideoFilter<HandyTechVideo>(HandyTechVideo video)
    {
        StringBuilder videoFilter = new();

        videoFilter.Append($"drawtext=textfile:'{video.ChannelBannerText()}':");
        videoFilter.Append($"fontcolor={video.TextColor()}@{DIM_TEXT}:");
        videoFilter.Append($"fontsize={SMALL_FONT}:");
        videoFilter.Append($"{_upperRight}:");
        videoFilter.Append($"box=1:");
        videoFilter.Append($"boxborderw=8:");
        videoFilter.Append($"boxcolor={video.BoxColor()}@{DIM_BACKGROUND}");

        return videoFilter.ToString();
    }

    // private async Task AddMusicToVideoAsync(TechnologyVideo video, CancellationToken cancellationToken)
    // {
    //     const string narration = "narration";
    //     const string narrative = "narrative";
    //     const string audio = "audio";

    //     var videoFiles = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
    //         .Where(x => !x.Contains(narration) || !x.Contains(narrative))
    //         .Where(x => x.EndsWith(FileExtension.Mp4));

    //     var narrationFiles = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
    //         .Where(x => x.Contains(narration) || x.Contains(narrative))
    //         .Where(x => x.EndsWith(FileExtension.Mp4) || x.EndsWith(FileExtension.Mkv))
    //         .ToArray();

    //     foreach (var videoFilePath in videoFiles)
    //     {
    //         // var result = await RunCommandAsync(
    //         //     ProgramPaths.Ffprobe,
    //         //     $"-hide_banner \"{videoFileName}\"",
    //         //     workingDirectory,
    //         //     cancellationToken,
    //         //     1
    //         // );

    //         var result = await _ffmpeg.FfprobeAsync(videoFilePath, video.WorkingDirectory, cancellationToken);

    //         if (result.stdErr.ToLower().Contains(audio))
    //         {
    //             continue;
    //         }

    //         string audioFilePath = narrationFiles.Where(
    //                 x => x.Contains(Path.GetFileNameWithoutExtension(videoFilePath))
    //             )
    //             .Single();

    //         string outputFileName = $"{Path.GetFileNameWithoutExtension(videoFilePath)}.tmp{FileExtension.Mp4}"
    //             .Replace(narration, string.Empty)
    //             .Replace(narrative, string.Empty);

    //         // await RunCommandAsync(
    //         //     ProgramPaths.Ffmpeg,
    //         //     $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -i \"{audioFile}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
    //         //     workingDirectory,
    //         //     cancellationToken,
    //         //     10);

    //         // await _ffmpeg.FfmpegAsync(
    //         //     $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFilePath)}\" -i \"{audioFilePath}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
    //         //     video.WorkingDirectory,
    //         //     cancellationToken
    //         // );
    //         await _ffmpeg.AddAudioToVideoAsync(
    //             videoFilePath, audioFilePath, video.OutputFilePath, cancellationToken);

    //         _fileSystem.DeleteFile(videoFilePath);
    //         _fileSystem.MoveFile(Path.Combine(video.WorkingDirectory, outputFileName), videoFilePath);
    //     }

    //     _fileSystem.DeleteFiles(narrationFiles);
    // }

    // private async Task ConvertVideoFilesToCommonFormatAsync(TechnologyVideo video, CancellationToken cancellationToken)
    // {
    //     var videoFiles = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
    //         .Where(x => x.EndsWith(FileExtension.Mkv) || x.EndsWith(FileExtension.Mp4))
    //         .OrderBy(x => x)
    //         .ToArray();

    //     foreach (var videoFileName in videoFiles)
    //     {
    //         //     var result = await _ffmpeg.FfprobeAsync(videoFileName, video.WorkingDirectory, cancellationToken);

    //         //     string scaledFile = 
    //         //         $"{Path.GetFileNameWithoutExtension(videoFileName)}.{video.xResolution}x{video.yResolution}{FileExtension.Mp4}";

    //         //     if (result.stdErr.Contains($"{video.xResolution}x{video.yResolution}") &&
    //         //         result.stdErr.Contains($"{video.audioBitRate} Hz") &&
    //         //         result.stdErr.Contains($"196 kb/s") &&
    //         //         videoFileName.EndsWith(FileExtension.Mp4))
    //         //     {
    //         //         _fileSystem.MoveFile(
    //         //             Path.Combine(video.WorkingDirectory, videoFileName),
    //         //             Path.Combine(video.WorkingDirectory, scaledFile),
    //         //             false);
    //         //         continue;
    //         //     }

    //         // await _ffmpeg.FfmpegAsync(
    //         //     $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -r 30 -vf \"scale_vaapi=w={handyTechVideo.xResolution}:h={handyTechVideo.yResolution}\" -vcodec h264_vaapi -ar {handyTechVideo.audioSampleRate} -b:a {handyTechVideo.audioBitRate} \"{scaledFile}\"",
    //         //     handyTechVideo.WorkingDirectory,
    //         //     cancellationToken);

    //         await _ffmpeg.ConvertVideoFileToTsFormatAsync(
    //             videoFileName, video.OutputFilePath, cancellationToken);

    //         // _fileSystem.DeleteFile(Path.Combine(video.WorkingDirectory, videoFileName));

    //         // string outputFileName = Path.GetFileNameWithoutExtension(videoFileName) + FileExtension.Mp4;
    //     }
    // }

}