using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;

namespace Almostengr.VideoProcessor.Core.Toastmasters;

public sealed class ToastmastersService : BaseVideoService, IToastmastersVideoService
{
    private readonly ILoggerService<ToastmastersService> _loggerService;

    public ToastmastersService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService compressionService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        IGzFileCompressionService gzipService, IAssSubtitleFileService assSubtitleFileService,
        ILoggerService<ToastmastersService> loggerService, IMusicService musicService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService, assSubtitleFileService)
    {
        IncomingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Incoming);
        WorkingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Working);
        ArchiveDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Archive);
        UploadingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Uploading);
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

    public override async Task ProcessVideoProjectAsync(CancellationToken cancellationToken)
    {
        string? readyFile = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
            .Where(f => f.EndsWithIgnoringCase(FileExtension.Ready.Value))
            .FirstOrDefault();

        if (readyFile == null)
        {
            return;
        }

        string projectFileName =
            Path.GetFileName(readyFile.ReplaceIgnoringCase(FileExtension.Ready.Value, FileExtension.Tar.Value));
        ToastmastersVideoProject? project = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
           .Where(f => f.ContainsIgnoringCase(projectFileName))
           .Select(f => new ToastmastersVideoProject(f))
           .SingleOrDefault();

        if (project == null)
        {
            return;
        }

        try
        {
            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                project.FilePath, WorkingDirectory, cancellationToken);

            StopProcessingIfDetailsTxtFileExists(WorkingDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(WorkingDirectory);
            StopProcessingIfKdenliveFileExists(WorkingDirectory);

            var videoClips = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWithIgnoringCase(FileExtension.Mp4.Value))
                .OrderBy(f => f);

            string ffmpegInputFilePath = Path.Combine(WorkingDirectory, Constant.FfmpegInputFileName);
            CreateFfmpegInputFile(videoClips, ffmpegInputFilePath);

            var textOptions = project.BrandingTextOptions().ToList();
            string brandingText = textOptions[_randomService.Next(0, textOptions.Count)];
            string outputFilePath = Path.Combine(WorkingDirectory, project.VideoFileName());

            await _ffmpegService.RenderVideoWithInputFileAndFiltersAsync(
                ffmpegInputFilePath, project.ChannelBrandDrawTextFilter(brandingText), outputFilePath, cancellationToken);

            _fileSystemService.MoveFile(outputFilePath, Path.Combine(UploadingDirectory, project.VideoFileName()));
            _fileSystemService.MoveFile(
                project.FilePath, Path.Combine(ArchiveDirectory, project.FileName()));
            _fileSystemService.DeleteFile(readyFile);
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _loggerService.LogErrorProcessingFile(project.FilePath, ex);
            _fileSystemService.MoveFile(readyFile, readyFile + FileExtension.Err.Value);
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
    }

}
