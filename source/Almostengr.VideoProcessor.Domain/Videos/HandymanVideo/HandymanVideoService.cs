using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Exceptions;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;

namespace Almostengr.VideoProcessor.Domain.Videos.HandymanVideo;

public sealed class HandymanVideoService : BaseVideoService, IHandymanVideoService
{
    private readonly IFileSystem _fileSystem;
    private readonly IFfmpeg _ffmpeg;
    private readonly ITarball _tarball;
    private readonly IMusicService _musicService;
    private readonly ILoggerService<HandymanVideoService> _logger;

    public HandymanVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarballService, IMusicService musicService, ILoggerService<HandymanVideoService> logger,
         ITarball tarball
        ) : base(fileSystemService, ffmpegService, tarball)
    {
        _fileSystem = fileSystemService;
        _ffmpeg = ffmpegService;
        _tarball = tarballService;
        _musicService = musicService;
        _logger = logger;
    }

    public override async Task ProcessVideosAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                HandymanVideo video = new HandymanVideo(Constants.HandymanBaseDirectory);
                
                CreateVideoDirectories(video);
                DeleteFilesOlderThanSpecifiedDays(video.UploadDirectory);

                _fileSystem.IsDiskSpaceAvailable(video.BaseDirectory);

                string tarBallFilePath = _fileSystem.GetRandomTarballFromDirectory(video.BaseDirectory);

                video.SetTarballFilePath(tarBallFilePath);

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
                _fileSystem.CreateDirectory(video.WorkingDirectory);

                await _tarball.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                _fileSystem.PrepareAllFilesInDirectory(video.WorkingDirectory); // lowercase all file names

                await AddAudioToTimelapseAsync(video, stoppingToken); // add audio to timelapse videos

                // foreach (var videoFile in _fileSystem.GetFilesInDirectory(video.WorkingDirectory))
                // {
                await _ffmpeg.CreateThumbnailsFromVideoFilesAsync(video, stoppingToken);
                //         video.Title, video.OutputFilePath, video.WorkingDirectory, stoppingToken);
                // }

                _fileSystem.CopyFile(video.ShowIntroFilePath, video.WorkingDirectory);

                await ConvertVideoFilesToCommonFormatAsync(video, stoppingToken);

                CreateFfmpegInputFile(video);

                string videoFilter = DrawTextVideoFilter(video);

                // await _ffmpeg.RenderVideoAsync(video.WorkingDirectory, videoFilter);
                // await _ffmpeg.FfmpegAsync(
                //     $"-y {LOG_ERRORS} -safe 0 -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i \"{video.FfmpegInputFilePath}\" -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -c:v h264_vaapi -bsf:a aac_adtstoasc \"{video.OutputFilePath}\"", //string.Empty,
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

    internal override void CreateFfmpegInputFile<HandymanVideo>(HandymanVideo video)
    {
        base.RhtCreateFfmpegInputFile<HandymanVideo>(video);
    }

    private async Task AddAudioToTimelapseAsync(HandymanVideo video, CancellationToken cancellationToken)
    {
        var videoFiles = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
            .Where(x => !x.Contains(NARRATION) || !x.Contains(NARRATIVE))
            .Where(x => x.EndsWith(FileExtension.Mp4));

        var narrationFiles = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
            .Where(x => x.Contains(NARRATION) || x.Contains(NARRATIVE))
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

            var result = await _ffmpeg.FfprobeAsync(
                $"\"{videoFilePath}\"", video.WorkingDirectory, cancellationToken);

            if (result.stdErr.ToLower().Contains(AUDIO))
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

            string outputFileName = $"{Path.GetFileNameWithoutExtension(videoFilePath)}.tmp{FileExtension.Mp4}"
                .Replace(NARRATION, string.Empty)
                .Replace(NARRATIVE, string.Empty);

            // await RunCommandAsync(
            //     ProgramPaths.Ffmpeg,
            //     $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -i \"{audioFile}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
            //     workingDirectory,
            //     cancellationToken,
            //     10);

            // await _ffmpeg.FfmpegAsync(
            //     $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFilePath)}\" -i \"{audioFilePath}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
            //     video.WorkingDirectory,
            //     cancellationToken
            // );

            await _ffmpeg.AddAudioToVideoAsync(
                videoFilePath, audioFilePath, outputFileName, cancellationToken);

            _fileSystem.DeleteFile(videoFilePath);
            _fileSystem.MoveFile(Path.Combine(video.WorkingDirectory, outputFileName), videoFilePath);
        }

        _fileSystem.DeleteFiles(narrationFiles);
    }
}