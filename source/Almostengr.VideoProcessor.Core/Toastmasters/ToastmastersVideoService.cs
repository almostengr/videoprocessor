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

    public ToastmastersVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService compressionService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        IGzFileCompressionService gzipService, IAssSubtitleFileService assSubtitleFileService,
        ILoggerService<ToastmastersVideoService> loggerService, IMusicService musicService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService, assSubtitleFileService)
    {
        IncomingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Incoming);
        WorkingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Working);
        ArchiveDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Archive);
        UploadingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Uploading);
        // ReviewingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Reviewing);
        // ReviewWorkDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.ReviewWork);
        _loggerService = loggerService;
    }

    // public override async Task ProcessIncomingSubtitlesAsync(CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }

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

    public override async Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await base.CreateTarballsFromDirectoriesAsync(IncomingDirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }

    public override async Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken)
    {
        // ToastmastersIncomingTarballFile? tarball = null;
        ToastmastersVideoFile? archiveFile = null;

        try
        {
            string selectedTarballFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.Tar);

            // tarball = new ToastmastersIncomingTarballFile(selectedTarballFilePath);
            archiveFile = new ToastmastersVideoFile(new VideoProjectArchiveFile(selectedTarballFilePath));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                archiveFile.FilePath, WorkingDirectory, cancellationToken);
            // tarball.FilePath, IncomingWorkDirectory, cancellationToken);

            string[] videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4.Value))
                .OrderBy(f => f)
                .ToArray();

            string ffmpegInputFilePath = Path.Combine(WorkingDirectory, "videos" + FileExtension.FfmpegInput.Value);
            CreateFfmpegInputFile(videoFiles, ffmpegInputFilePath);

            string outputFilePath = Path.Combine(WorkingDirectory, archiveFile.OutputFileName());
            // string outputFilePath = Path.Combine(IncomingWorkDirectory, tarball.VideoFileName);

            // await _ffmpegService.RenderVideoAsCopyAsync(
            //     ffmpegInputFilePath, outputFilePath, cancellationToken);

            await _ffmpegService.RenderVideoWithFiltersAsync(
                ffmpegInputFilePath, archiveFile.VideoFilters(), outputFilePath, cancellationToken);

            _fileSystemService.MoveFile(outputFilePath, Path.Combine(UploadingDirectory, archiveFile.OutputFileName()));
            // _fileSystemService.MoveFile(outputFilePath, Path.Combine(ReviewingDirectory, tarball.FileName));
            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.MoveFile(
                archiveFile.FilePath, Path.Combine(ArchiveDirectory, archiveFile.FileName()));
            // _fileSystemService.MoveFile(tarball.FilePath, Path.Combine(ArchiveDirectory, tarball.FileName));
        }
        catch (NoFilesMatchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);

            if (archiveFile != null)
            {
                _loggerService.LogError(ex, $"Error when processing {archiveFile.FilePath}");
                _fileSystemService.MoveFile(archiveFile.FilePath, archiveFile.FilePath + FileExtension.Err.Value);
            }

            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
    }

    // public override async Task ProcessReviewedFilesAsync(CancellationToken cancellationToken)
    // {
    //     AssSubtitleFile? subtitleFile = null;

    //     try
    //     {
    //         string subtitleFilePath =
    //             _fileSystemService.GetRandomFileByExtensionFromDirectory(ReviewingDirectory, FileExtension.GraphicsAss);

    //         subtitleFile = new AssSubtitleFile(subtitleFilePath);

    //         _loggerService.LogInformation($"Processing {subtitleFilePath}");

    //         string archiveFilePath = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
    //             .Where(f => f.StartsWith(subtitleFile.FileName) && f.ToLower().EndsWith(FileExtension.Mp4.Value))
    //             .Single();

    //         // ToastmastersVideoFile video = new ToastmastersVideoFile(videoFilePath);
    //         ToastmastersVideoProjectArchive archive = new ToastmastersVideoProjectArchive(archiveFilePath);
    //         video.SetGraphicsSubtitleFile(subtitleFilePath);
    //         video.GraphicsSubtitleFile.SetSubtitles(_assSubtitleFileService.ReadFile(video.GraphicsSubtitleFile.FilePath));

    //         _fileSystemService.DeleteDirectory(ReviewWorkDirectory);
    //         _fileSystemService.CreateDirectory(ReviewWorkDirectory);

    //         video.SetBrandingText(RandomChannelBrandingText(video.BrandingTextOptions()));
    //         string videoFilters = video.VideoFilters();

    //         /// render video
    //         string outputFilePath = Path.Combine(ReviewWorkDirectory, video.FileName);

    //         await _ffmpegService.RenderVideoWithFiltersAsync(
    //             video.FilePath, video.VideoFilters(), outputFilePath, cancellationToken);

    //         _fileSystemService.MoveFile(
    //             video.GraphicsSubtitleFile.FilePath,
    //             Path.Combine(ArchiveDirectory, video.GraphicsSubtitleFile.FileName));
    //         _fileSystemService.MoveFile(outputFilePath, Path.Combine(UploadingDirectory, video.FileName));
    //         _fileSystemService.DeleteDirectory(ReviewWorkDirectory);

    //         _loggerService.LogInformation($"Completed processing {subtitleFilePath}");
    //     }
    //     catch (NoFilesMatchException)
    //     {
    //         throw;
    //     }
    //     catch (Exception ex)
    //     {
    //         _loggerService.LogError(ex, ex.Message);
    //         _fileSystemService.MoveFile(subtitleFile.FilePath, subtitleFile.FilePath + FileExtension.Err.Value);
    //     }
    // }

}
