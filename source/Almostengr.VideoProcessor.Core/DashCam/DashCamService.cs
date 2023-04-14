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

    public override async Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken)
    {
        DashCamVideoFile? archiveFile = null;

        try
        {
            string? selectedTarballFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.Tar);

            if (string.IsNullOrEmpty(selectedTarballFilePath))
            {
                return;
            }

            archiveFile = new DashCamVideoFile(new VideoProjectArchiveFile(selectedTarballFilePath));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                archiveFile.FilePath, WorkingDirectory, cancellationToken);

            StopProcessingIfKdenliveFileExists(WorkingDirectory);
            StopProcessingIfDetailsTxtFileExists(WorkingDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(WorkingDirectory);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            foreach (var filePath in _fileSystemService.GetFilesInDirectory(WorkingDirectory))
            {
                string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileName(filePath).ToLower());

                if (filePath == newFilePath)
                {
                    continue;
                }

                _fileSystemService.MoveFile(filePath, newFilePath);
            }

            var videoFiles = _fileSystemService.GetFilesInDirectoryWithFileInfo(WorkingDirectory)
                .Where(f => f.FullName.EndsWith(FileExtension.Mov.Value, StringComparison.OrdinalIgnoreCase) || 
                    f.FullName.EndsWith(FileExtension.Mkv.Value, StringComparison.OrdinalIgnoreCase) || 
                    f.FullName.EndsWith(FileExtension.Mp4.Value, StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.Name)
                .Select(f => new DashCamVideoFile(f.FullName))
                .ToList();

            foreach (var video in videoFiles)
            {
                string outFilePath = video.FilePath
                    .Replace(FileExtension.Mov.Value, string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace(FileExtension.Mkv.Value, string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace(FileExtension.Mp4.Value, string.Empty, StringComparison.OrdinalIgnoreCase) + FileExtension.Ts.Value;

                await _ffmpegService.ConvertVideoFileToTsFormatAsync(
                    video.FilePath, outFilePath, cancellationToken);
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
                CreateFfmpegInputFile(tsVideoFiles.ToArray(), ffmpegInputFilePath);
            }

            string outputFilePath = Path.Combine(WorkingDirectory, archiveFile.OutputFileName());

            await _ffmpegService.RenderVideoAsync(ffmpegInputFilePath, outputFilePath, cancellationToken);

            _fileSystemService.SaveFileContents(
                Path.Combine(IncomingDirectory, archiveFile.Title() + FileExtension.ThumbTxt.Value),
                archiveFile.Title());

            _fileSystemService.MoveFile(archiveFile.FilePath, Path.Combine(ArchiveDirectory, archiveFile.FileName()));
            _fileSystemService.MoveFile(outputFilePath, Path.Combine(IncomingDirectory, archiveFile.OutputFileName()));

            var graphicsFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
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

            if (archiveFile != null)
            {
                // _loggerService.LogError(ex, $"Error when processing {archiveFile.FilePath}");
                _loggerService.LogErrorProcessingFile(archiveFile.FilePath, ex);
                _fileSystemService.MoveFile(archiveFile.FilePath, archiveFile.FilePath + FileExtension.Err.Value);
            }

            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
    }

    public async Task ProcessIncomingVideosWithGraphicsAsync(CancellationToken cancellationToken)
    {
        AssSubtitleFile? graphicsFile = null;

        try
        {
            string? selectGraphicsFile = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.GraphicsAss);
            
            if (string.IsNullOrEmpty(selectGraphicsFile))
            {
                return;
            }

            graphicsFile = new AssSubtitleFile(selectGraphicsFile);

            var video = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
                .Where(f => f.StartsWith(graphicsFile.FilePath.Replace(FileExtension.GraphicsAss.Value, string.Empty)) && 
                    f.EndsWith(FileExtension.Mp4.Value, StringComparison.OrdinalIgnoreCase))
                .Select(f => new DashCamVideoFile(f))
                .Single();

            video.SetGraphicsSubtitleFile(graphicsFile);
            video.SetSubtitles(_assSubtitleFileService.ReadFile(video.GraphicsFilePath()));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            string videoFilters = video.VideoFilters();

            string outputFilePath = Path.Combine(WorkingDirectory, video.FileName());

            if (video.SubType != DashCamVideoFile.DashCamVideoType.CarRepair)
            {
                videoFilters += Constant.CommaSpace + video.TitleDrawTextFilter();
                video.SetAudioFile(_musicService.GetRandomMixTrack());

                await _ffmpegService.RenderVideoWithAudioAndFiltersAsync(
                    video.FilePath, video.AudioFilePath(), videoFilters, outputFilePath, cancellationToken);
            }
            else
            {
                await _ffmpegService.RenderVideoWithFiltersAsync(
                    video.FilePath, videoFilters, outputFilePath, cancellationToken);
            }

            _fileSystemService.MoveFile(outputFilePath, Path.Combine(UploadingDirectory, video.FileName()));
            _fileSystemService.DeleteFile(video.FilePath);
            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.MoveFile(
                video.GraphicsFilePath(), Path.Combine(ArchiveDirectory, Path.GetFileName(video.GraphicsFilePath())));
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);

            if (graphicsFile != null)
            {
                // _loggerService.LogError(ex, $"Error when processing {graphicsFile.FilePath}");
                _loggerService.LogErrorProcessingFile(graphicsFile.FilePath, ex);
                _fileSystemService.MoveFile(graphicsFile.FilePath, graphicsFile.FilePath + FileExtension.Err.Value);
            }
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
}