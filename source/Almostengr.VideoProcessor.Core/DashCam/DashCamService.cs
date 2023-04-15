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

    public async Task ProcessIncomingVideoWithGraphicsAsync(CancellationToken cancellationToken)
    {
        AssSubtitleFile? graphicsFile = null;

        graphicsFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
            .Where(f => f.EndsWith(FileExtension.GraphicsAss.Value))
            .Select(f => new AssSubtitleFile(f))
            .SingleOrDefault();

        if (graphicsFile == null)
        {
            return;
        }

        try
        {
            var videoFilePath = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
                .Where(f => f.StartsWith(graphicsFile.FilePath.Replace(FileExtension.GraphicsAss.Value, string.Empty)) &&
                    f.EndsWith(FileExtension.Mp4.Value, StringComparison.OrdinalIgnoreCase))
                .Single();

            string videoFileName = Path.GetFileName(videoFilePath);

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            string outputFilePath = Path.Combine(WorkingDirectory, videoFileName);

            if (videoFilePath.Contains(Constant.GmcSierra, StringComparison.OrdinalIgnoreCase) ||
                videoFilePath.Contains(Constant.NissanAltima, StringComparison.OrdinalIgnoreCase))
            {
                await _ffmpegService.RenderVideoWithFiltersAsync(
                    videoFilePath, string.Empty, outputFilePath, cancellationToken);
            }
            else
            {
                var audioFile = _musicService.GetRandomMixTrack();
                await _ffmpegService.RenderVideoWithAudioAndFiltersAsync(
                    videoFilePath, audioFile.FilePath, string.Empty, outputFilePath, cancellationToken);
            }

            _fileSystemService.MoveFile(
                outputFilePath, Path.Combine(UploadingDirectory, videoFileName));

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
                .Where(f => f.FullName.EndsWith(FileExtension.Mov.Value, StringComparison.OrdinalIgnoreCase) ||
                    f.FullName.EndsWith(FileExtension.Mkv.Value, StringComparison.OrdinalIgnoreCase) ||
                    f.FullName.EndsWith(FileExtension.Mp4.Value, StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.Name);

            foreach (var video in videoClips)
            {
                string outFilePath = video.FullName
                    .Replace(FileExtension.Mov.Value, string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace(FileExtension.Mkv.Value, string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace(FileExtension.Mp4.Value, string.Empty, StringComparison.OrdinalIgnoreCase)
                    + FileExtension.Ts.Value;

                await _ffmpegService.ConvertVideoFileToTsFormatAsync(
                    video.FullName, outFilePath, cancellationToken);
            }

            string? ffmpegInputFilePath = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.FfmpegInput.Value))
                .SingleOrDefault();

            if (string.IsNullOrEmpty(ffmpegInputFilePath))
            {
                var tsVideoFiles = _fileSystemService.GetFilesInDirectoryWithFileInfo(WorkingDirectory)
                    .Where(f => f.Name.EndsWith(FileExtension.Ts.Value, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f.Name)
                    .Select(f => f.FullName);

                ffmpegInputFilePath = Path.Combine(WorkingDirectory, Constant.FfmpegInputFileName);
                CreateFfmpegInputFile(tsVideoFiles, ffmpegInputFilePath);
            }

            string outputFilePath = Path.Combine(WorkingDirectory, project.VideoFileName());

            await _ffmpegService.RenderVideoAsync(ffmpegInputFilePath, outputFilePath, cancellationToken);

            _fileSystemService.SaveFileContents(
                Path.Combine(IncomingDirectory, project.ThumbnailFileName()), project.Title());

            _fileSystemService.MoveFile(project.FilePath, Path.Combine(ArchiveDirectory, project.FileName()));
            _fileSystemService.MoveFile(outputFilePath, Path.Combine(IncomingDirectory, project.VideoFileName()));

            string? graphicsFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.GraphicsAss.Value, StringComparison.OrdinalIgnoreCase))
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