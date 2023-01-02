using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Interfaces;
using Almostengr.VideoProcessor.Domain.Common.Videos;
using Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Domain.Music.Services;

namespace Almostengr.VideoProcessor.Domain.Videos.Handyman;

public sealed class HandymanVideoService : BaseVideoService, IHandymanVideoService
{
    private readonly IFileSystem _fileSystem;
    private readonly IFfmpeg _ffmpeg;
    private readonly ITarball _tarball;
    private readonly IMusicService _musicService;
    private readonly ILoggerService<HandymanVideoService> _logger;
    private readonly AppSettings _appSettings;

    public HandymanVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarballService, IMusicService musicService, ILoggerService<HandymanVideoService> logger,
         ITarball tarball, AppSettings appSettings
    ) : base(fileSystemService, ffmpegService, tarball, appSettings)
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
                HandymanVideo video = new HandymanVideo(_appSettings.HandymanDirectory);

                CreateVideoDirectories(video);
                DeleteFilesOlderThanSpecifiedDays(video.UploadDirectory);

                _fileSystem.IsDiskSpaceAvailable(video.BaseDirectory);

                await CreateTarballsFromDirectoriesAsync(video.IncomingDirectory, stoppingToken);

                video.SetTarballFilePath(_fileSystem.GetRandomTarballFromDirectory(video.IncomingDirectory));

                _fileSystem.DeleteDirectory(video.WorkingDirectory);
                _fileSystem.CreateDirectory(video.WorkingDirectory);

                await _tarball.ExtractTarballContentsAsync(
                    video.TarballFilePath, video.WorkingDirectory, stoppingToken);

                _fileSystem.PrepareAllFilesInDirectory(video.WorkingDirectory);

                await AddAudioToTimelapseAsync(video, stoppingToken);

                await ConvertVideoFilesToCommonFormatAsync(video, stoppingToken);

                _fileSystem.PrepareAllFilesInDirectory(video.WorkingDirectory);

                CreateFfmpegInputFile(video);

                string videoFilter = DrawTextVideoFilter(video);

                await _ffmpeg.RenderVideoAsync(
                    video.FfmpegInputFilePath, videoFilter, video.OutputFilePath, stoppingToken);

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

            await _ffmpeg.AddAudioToVideoAsync(
                videoFilePath, audioFilePath, outputFileName, cancellationToken);

            _fileSystem.DeleteFile(videoFilePath);
            _fileSystem.MoveFile(Path.Combine(video.WorkingDirectory, outputFileName), videoFilePath);
        }

        _fileSystem.DeleteFiles(narrationFiles);
    }
}