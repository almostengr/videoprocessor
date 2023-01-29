using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;

namespace Almostengr.VideoProcessor.Core.Toastmasters;

public sealed class ToastmastersVideoService : BaseVideoService, IToastmastersVideoService
{
    private readonly ILoggerService<ToastmastersVideoService> _loggerService;
    
    // private readonly string _incomingDirectory;
    // private readonly string _archiveDirectory;
    // private readonly string _uploadDirectory;
    // private readonly string _workingDirectory;

    public ToastmastersVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService compressionService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<ToastmastersVideoService> loggerService, IMusicService musicService) :
        base(appSettings, ffmpegService, compressionService, tarballService, fileSystemService, randomService, musicService)
    {
        Incomingdirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Incoming);
        ArchiveDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Archive);
        UploadDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Upload);
        WorkingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Working);
        _loggerService = loggerService;
    }

    public override async Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken)
    {
        try
        {
            await base.CompressTarballsInArchiveFolderAsync(ArchiveDirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }

    public override async Task ConvertGzToXzAsync(CancellationToken cancellationToken)
    {
        var tarGzFiles = _fileSystemService.GetFilesInDirectory(ArchiveDirectory)
            .Where(f => f.EndsWith(FileExtension.TarGz.ToString()));

        foreach(var file in tarGzFiles)
        {
            await _compressionService.DecompressFileAsync(file, cancellationToken);

            await _compressionService.CompressFileAsync(
                file.Replace(FileExtension.TarGz.ToString(), FileExtension.Tar.ToString()), cancellationToken);
        }
    }

    public void CreateDirectories()
    {
        _fileSystemService.CreateDirectory(Incomingdirectory);
        _fileSystemService.CreateDirectory(ArchiveDirectory);
        _fileSystemService.CreateDirectory(UploadDirectory);
        _fileSystemService.CreateDirectory(WorkingDirectory);
    }

    public override async Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await base.CreateTarballsFromDirectoriesAsync(Incomingdirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }

    public override async Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken)
    {
        ToastmastersVideoFile? video = null;

        try
        {
            // string incomingTarball = _fileSystemService.GetRandomTarballFromDirectory(IncomingDirectory);
            string incomingTarball = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                Incomingdirectory, FileExtension.Tar);

            video = new ToastmastersVideoFile(_appSettings.ToastmastersDirectory, Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                video.IncomingTarballFilePath(), WorkingDirectory, cancellationToken);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            // create input file
            _fileSystemService.DeleteFile(video.FfmpegInputFilePath());

            string[] videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4.ToString()))
                .OrderBy(f => f)
                .ToArray();

            CreateFfmpegInputFile(videoFiles, video.FfmpegInputFilePath());

            // brand video
            video.AddDrawTextVideoFilter(
                RandomChannelBrandingText(video.BrandingTextOptions()),
                video.DrawTextFilterTextColor(),
                Opacity.Full,
                FfmpegFontSize.Large,
                DrawTextPosition.UpperRight,
                video.DrawTextFilterBackgroundColor(),
                Opacity.Medium);

            video.AddLikeVideoFilter(_randomService.SubscribeLikeDuration());

            await _ffmpegService.RenderVideoAsync(
                video.FfmpegInputFilePath(), video.VideoFilter, video.OutputVideoFilePath(), cancellationToken);

            _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _loggerService.LogInformation($"Completed processing {incomingTarball}");
        }
        catch (NoFilesMatchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);

            if (video != null)
            {
                _fileSystemService.MoveFile(video.IncomingTarballFilePath(), Path.Combine(ArchiveDirectory, video.ArchiveFileName));
                _fileSystemService.SaveFileContents(
                    Path.Combine(ArchiveDirectory, video.ArchiveFileName + FileExtension.Log),
                    ex.Message);
            }
        }
    }
}
