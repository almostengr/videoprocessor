using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Interfaces;
using Almostengr.VideoProcessor.Domain.Common.Videos;
using Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Domain.Toastmasters;

public sealed class ToastmastersVideoService : BaseVideoService, IToastmastersVideoService
{
    private readonly IFfmpeg _ffmpegSerivce;
    private readonly IFileSystem _fileSystem;
    private readonly ITarball _tarball;
    private readonly ILoggerService<ToastmastersVideoService> _logger;
    private readonly AppSettings _appSettings;

    public ToastmastersVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarballService, ILoggerService<ToastmastersVideoService> logger, ITarball tarball,
        AppSettings appSettings
    ) : base(fileSystemService, ffmpegService, tarball, appSettings)
    {
        _fileSystem = fileSystemService;
        _ffmpegSerivce = ffmpegService;
        _tarball = tarballService;
        _logger = logger;
        _appSettings = appSettings;
    }

    public override async Task<bool> ProcessVideosAsync(CancellationToken stoppingToken)
    {
        ToastmastersVideo video = new(_appSettings.ToastmastersDirectory);

        try
        {
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

            CreateFfmpegInputFile(video);

            video.AddChannelBannerTextFilter();

            await _ffmpegSerivce.RenderVideoAsync(
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

    internal override void CreateFfmpegInputFile<ToastmastersVideo>(ToastmastersVideo video)
    {
        _fileSystem.DeleteFile(video.FfmpegInputFilePath);

        using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
        {
            var filesInDirectory = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4))
                .OrderBy(f => f)
                .ToArray();

            foreach (var file in filesInDirectory)
            {
                writer.WriteLine($"{FILE} '{file}'");
            }
        }
    }
}