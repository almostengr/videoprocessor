using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;
using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed class DashCamService : BaseVideoService, IDashCamVideoService
{
    private readonly ILoggerService<DashCamService> _loggerService;

    public DashCamService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<DashCamService> loggerService, IMusicService musicService,
        IAssSubtitleFileService assSubtitleFileService,
        IXzFileCompressionService xzFileService, IGzFileCompressionService gzFileService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService, assSubtitleFileService)
    {
        IncomingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Incoming);
        WorkingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Working);
        ArchiveDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Archive);
        UploadingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Uploading);
        _loggerService = loggerService;
    }

    public async Task ProcessReviewedFilesAsync(CancellationToken cancellationToken)
    {
        DashCamGraphicsFile? graphicsFile = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
            .Where(f => f.EndsWithIgnoringCase(FileExtension.GraphicsAss.Value))
            .Select(f => new DashCamGraphicsFile(f))
            .FirstOrDefault();

        if (graphicsFile == null)
        {
            return;
        }

        try
        {
            var videoFilePath = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
                .Where(f => f.StartsWith(graphicsFile.FilePath.ReplaceIgnoringCase(FileExtension.GraphicsAss.Value, string.Empty)) &&
                    f.EndsWithIgnoringCase(FileExtension.Mp4.Value))
                .Single();

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            string outputFilePath = Path.Combine(WorkingDirectory, graphicsFile.VideoFileName());

            var audioFile = _musicService.GetRandomMixTrack();
            await _ffmpegService.RenderVideoWithAudioAndFiltersAsync(
                videoFilePath, audioFile.FilePath, string.Empty, outputFilePath, cancellationToken);

            _fileSystemService.MoveFile(
                outputFilePath, Path.Combine(UploadingDirectory, graphicsFile.VideoFileName()));

            _fileSystemService.MoveFile(
                graphicsFile.FilePath, Path.Combine(ArchiveDirectory, graphicsFile.FileName));

            _fileSystemService.DeleteFile(videoFilePath);
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _loggerService.LogErrorProcessingFile(graphicsFile.FilePath, ex);
            _fileSystemService.MoveFile(graphicsFile.FilePath, graphicsFile.FilePath + FileExtension.Err.Value);
        }
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
        DashCamVideoProject? project = _fileSystemService.GetTarballFilesInDirectory(IncomingDirectory)
            .Select(f => new DashCamVideoProject(f))
            .FirstOrDefault();

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

            StopProcessingIfKdenliveFileExists(WorkingDirectory);
            StopProcessingIfDetailsTxtFileExists(WorkingDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(WorkingDirectory);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            var videoClips = _fileSystemService.GetFilesInDirectoryWithFileInfo(WorkingDirectory)
                .Where(f => f.FullName.IsVideoFile())
                .OrderBy(f => f.Name);

            foreach (var video in videoClips)
            {
                string outFilePath = video.FullName
                    .ReplaceIgnoringCase(FileExtension.Mov.Value, string.Empty)
                    .ReplaceIgnoringCase(FileExtension.Mkv.Value, string.Empty)
                    .ReplaceIgnoringCase(FileExtension.Mp4.Value, string.Empty)
                    + FileExtension.Ts.Value;

                await _ffmpegService.ConvertVideoFileToTsFormatAsync(
                    video.FullName, outFilePath, cancellationToken);
            }

            string? ffmpegInputFilePath = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWithIgnoringCase(FileExtension.FfmpegInput.Value))
                .SingleOrDefault();

            if (string.IsNullOrEmpty(ffmpegInputFilePath))
            {
                var tsVideoFiles = _fileSystemService.GetFilesInDirectoryWithFileInfo(WorkingDirectory)
                    .Where(f => f.Name.EndsWithIgnoringCase(FileExtension.Ts.Value))
                    .OrderBy(f => f.Name)
                    .Select(f => f.FullName);

                ffmpegInputFilePath = Path.Combine(WorkingDirectory, Constant.FfmpegInputFileName);
                CreateFfmpegInputFile(tsVideoFiles, ffmpegInputFilePath);
            }

            string outputFilePath = Path.Combine(WorkingDirectory, project.VideoFileName());

            var textOptions = project.BrandingTextOptions().ToList();
            string brandingText = textOptions[_randomService.Next(0, textOptions.Count)];
            await _ffmpegService.RenderVideoWithInputFileAndFiltersAsync(
                ffmpegInputFilePath, project.ChannelBrandDrawTextFilter(brandingText), outputFilePath, cancellationToken);

            _fileSystemService.SaveFileContents(
                Path.Combine(IncomingDirectory, project.ThumbnailFileName()), project.Title());

            _fileSystemService.MoveFile(project.FilePath, Path.Combine(ArchiveDirectory, project.FileName()));

            string destinationDir = IncomingDirectory;
            if (project.SubType == DashCamVideoProject.DashCamVideoType.CarRepair)
            {
                destinationDir = UploadingDirectory;
            }

            _fileSystemService.MoveFile(outputFilePath, Path.Combine(destinationDir, project.VideoFileName()));

            string? graphicsFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWithIgnoringCase(FileExtension.GraphicsAss.Value))
                .SingleOrDefault();

            if (graphicsFile != null)
            {
                _fileSystemService.MoveFile(
                    graphicsFile, Path.Combine(IncomingDirectory, Path.GetFileName(graphicsFile)));
            }

            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _loggerService.LogErrorProcessingFile(project.FilePath, ex);
            _fileSystemService.MoveFile(project.FilePath, project.FilePath + FileExtension.Err.Value);
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }

    }
}