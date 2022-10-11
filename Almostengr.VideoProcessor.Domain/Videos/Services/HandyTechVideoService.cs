using System.Text;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;

namespace Almostengr.VideoProcessor.Domain.Videos.Services;

public sealed class HandyTechVideoService : BaseVideoService, IHandyTechVideoService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IFfmpegService _ffmpegService;
    private readonly ITarballService _tarballService;
    private readonly IMusicService _musicService;
    private const int RHT_BORDER_WIDTH = 7;

    public HandyTechVideoService(IFileSystemService fileSystemService, IFfmpegService ffmpegService,
        ITarballService tarballService, IMusicService musicService) :
        base(fileSystemService, ffmpegService)
    {
        _fileSystemService = fileSystemService;
        _ffmpegService = ffmpegService;
        _tarballService = tarballService;
        _musicService = musicService;
    }

    public override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                HandyTechVideo video = new HandyTechVideo();

                _fileSystemService.IsDiskSpaceAvailable(video.BaseDirectory);

                string? tarBallFilePath = _fileSystemService.GetRandomTarballFromDirectory(video.BaseDirectory);

                video.SetTarballFilePath(tarBallFilePath);

                _fileSystemService.DeleteDirectory(video.WorkingDirectory);
                _fileSystemService.CreateDirectory(video.WorkingDirectory);

                await _tarballService.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                _fileSystemService.PrepareAllFilesInDirectory(video.WorkingDirectory); // lowercase all file names

                await AddAudioToTimelapseAsync(video, stoppingToken); // add audio to timelapse videos

                _fileSystemService.CopyFile(video.ShowIntroFilePath, video.WorkingDirectory);

                await ConvertVideoFilesToCommonFormatAsync(video, stoppingToken);

                CreateFfmpegInputFile(video);

                string videoFilter = FfmpegVideoFilter(video);

                // await _ffmpegService.RenderVideoAsync(video.WorkingDirectory, videoFilter);
                await _ffmpegService.FfmpegAsync(
                    $"-y {LOG_ERRORS} -safe 0 -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i \"{FFMPEG_INPUT_FILE}\" -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -c:v h264_vaapi -bsf:a aac_adtstoasc \"{video.OutputFilePath}\"", //string.Empty,
                    video.WorkingDirectory,
                    stoppingToken);

                _fileSystemService.MoveFile(video.TarballFilePath, video.ArchiveDirectory);

                _fileSystemService.DeleteDirectory(video.WorkingDirectory);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void CreateFfmpegInputFile(HandyTechVideo video)
    {
        _fileSystemService.DeleteFile(video.FfmpegInputFilePath);

        using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
        {
            var filesInDirectory = _fileSystemService.GetFilesInDirectory(video.WorkingDirectory)
                .Where(x => x.EndsWith(FileExtension.Mp4))
                .OrderBy(x => x)
                .ToArray();

            const string rhtservicesintro = "rhtservicesintro.1920x1080.mp4";
            const string file = "file";
            for (int i = 0; i < filesInDirectory.Length; i++)
            {
                if (filesInDirectory[i].Contains(rhtservicesintro))
                {
                    continue;
                }

                if (i == 1 && _fileSystemService.DoesFileExist(video.NoIntroFilePath()) == false)
                {
                    writer.WriteLine($"{file} '{rhtservicesintro}'");
                }

                writer.WriteLine($"{file} '{Path.GetFileName(filesInDirectory[i])}'");
            }
        }
    }

    internal override string FfmpegVideoFilter<HandyTechVideo>(HandyTechVideo handyTechVideo)
    {
        StringBuilder videoFilter = new();

        videoFilter.Append($"drawtext=textfile:'{handyTechVideo.ChannelBannerText()}':");
        videoFilter.Append($"fontcolor={handyTechVideo.TextColor()}@{DIM_TEXT}:");
        videoFilter.Append($"fontsize={SMALL_FONT}:");
        videoFilter.Append($"{_upperRight}:");
        videoFilter.Append($"box=1:");
        videoFilter.Append($"boxborderw={RHT_BORDER_WIDTH.ToString()}:");
        videoFilter.Append($"boxcolor={handyTechVideo.BoxColor()}@{DIM_BACKGROUND}");

        return videoFilter.ToString();
    }


    private async Task AddAudioToTimelapseAsync(HandyTechVideo video, CancellationToken cancellationToken)
    {
        const string narration = "narration";
        const string narrative = "narrative";

        var videoFiles = _fileSystemService.GetFilesInDirectory(video.WorkingDirectory)
            .Where(x => !x.Contains(narration) || !x.Contains(narrative))
            .Where(x => x.EndsWith(FileExtension.Mp4));

        var narrationFiles = _fileSystemService.GetFilesInDirectory(video.WorkingDirectory)
            .Where(x => x.Contains(narration) || x.Contains(narrative))
            .Where(x => x.EndsWith(FileExtension.Mp4) || x.EndsWith(FileExtension.Mkv))
            .ToArray();

        foreach (var videoFilePath in videoFiles)
        {
            // var result = await RunCommandAsync(
            //     ProgramPaths.Ffprobe,
            //     $"-hide_banner \"{videoFileName}\"",
            //     workingDirectory,
            //     cancellationToken,
            //     1
            // );

            var result = await _ffmpegService.FfprobeAsync(
                $"-hide_banner \"{videoFilePath}\"",
                video.WorkingDirectory,
                cancellationToken
            );

            if (result.stdErr.ToLower().Contains("audio"))
            {
                continue;
            }

            string? audioFilePath = narrationFiles.Where(
                    x => x.Contains(Path.GetFileNameWithoutExtension(videoFilePath))
                )
                .SingleOrDefault();

            if (string.IsNullOrEmpty(audioFilePath))
            {
                audioFilePath = _musicService.GetRandomNonMixTrack();
            }

            string outputFileName = $"{Path.GetFileNameWithoutExtension(videoFilePath)}.tmp{FileExtension.Mp4}";

            outputFileName = outputFileName.Replace(narration, string.Empty)
                .Replace(narrative, string.Empty);

            // await RunCommandAsync(
            //     ProgramPaths.Ffmpeg,
            //     $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -i \"{audioFile}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
            //     workingDirectory,
            //     cancellationToken,
            //     10);

            await _ffmpegService.FfmpegAsync(
                $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFilePath)}\" -i \"{audioFilePath}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
                video.WorkingDirectory,
                cancellationToken
            );

            _fileSystemService.DeleteFile(videoFilePath);
            _fileSystemService.MoveFile(Path.Combine(video.WorkingDirectory, outputFileName), videoFilePath);
        }

        _fileSystemService.DeleteFiles(narrationFiles);
    }


    private async Task ConvertVideoFilesToCommonFormatAsync(HandyTechVideo handyTechVideo, CancellationToken cancellationToken)
    {
        var videoFiles = _fileSystemService.GetFilesInDirectory(handyTechVideo.WorkingDirectory)
            .Where(x => x.EndsWith(FileExtension.Mkv) || x.EndsWith(FileExtension.Mov) || x.EndsWith(FileExtension.Mp4))
            .OrderBy(x => x)
            .ToArray();

        foreach (var videoFileName in videoFiles)
        {
            var result = await _ffmpegService.FfprobeAsync(videoFileName, handyTechVideo.WorkingDirectory, cancellationToken);

            string scaledFile = $"{Path.GetFileNameWithoutExtension(videoFileName)}.{handyTechVideo.xResolution}x{handyTechVideo.yResolution}{FileExtension.Mp4}";

            if (result.stdErr.Contains($"{handyTechVideo.xResolution}x{handyTechVideo.yResolution}") &&
                result.stdErr.Contains($"{handyTechVideo.audioBitRate} Hz") &&
                result.stdErr.Contains($"196 kb/s") &&
                videoFileName.EndsWith(FileExtension.Mp4))
            {
                _fileSystemService.MoveFile(
                    Path.Combine(handyTechVideo.WorkingDirectory, videoFileName),
                    Path.Combine(handyTechVideo.WorkingDirectory, scaledFile),
                    false);
                continue;
            }

            await _ffmpegService.FfmpegAsync(
                $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -r 30 -vf \"scale_vaapi=w={handyTechVideo.xResolution}:h={handyTechVideo.yResolution}\" -vcodec h264_vaapi -ar {handyTechVideo.audioSampleRate} -b:a {handyTechVideo.audioBitRate} \"{scaledFile}\"",
                handyTechVideo.WorkingDirectory,
                cancellationToken);

            _fileSystemService.DeleteFile(Path.Combine(handyTechVideo.WorkingDirectory, videoFileName));

            string outputFileName = Path.GetFileNameWithoutExtension(videoFileName) + FileExtension.Mp4;
        }
    }

}