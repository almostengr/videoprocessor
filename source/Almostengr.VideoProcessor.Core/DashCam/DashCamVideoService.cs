using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed class DashCamVideoService : BaseVideoService, IDashCamVideoService
{
    private readonly ILoggerService<DashCamVideoService> _loggerService;
    private readonly IGzFileCompressionService _gzFileService;
    private readonly IXzFileCompressionService _xzFileService;

    public DashCamVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<DashCamVideoService> loggerService, IMusicService musicService,
        IAssSubtitleFileService assSubtitleFileService,
        IXzFileCompressionService xzFileService, IGzFileCompressionService gzFileService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService, assSubtitleFileService)
    {
        IncomingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Incoming);
        WorkingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Working);
        // ReviewWorkDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.ReviewWork);
        // ReviewingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Reviewing);
        ArchiveDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Archive);
        UploadingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Uploading);

        _loggerService = loggerService;
        _xzFileService = xzFileService;
        _gzFileService = gzFileService;
    }

    // public override async Task ProcessIncomingSubtitlesAsync(CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }

    public override async Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken)
    {
        DashCamVideoFile? archiveFile = null;

        try
        {
            string selectedTarballFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.Tar);

            archiveFile = new DashCamVideoFile(new VideoProjectArchiveFile(selectedTarballFilePath));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(archiveFile.FilePath, WorkingDirectory, cancellationToken);

            StopProcessingIfKdenliveFileExists(WorkingDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(WorkingDirectory);
            StopProcessingIfDetailsTxtFileExists(WorkingDirectory);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            string? ffmpegInputFilePath = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.FfmpegInput.Value))
                .SingleOrDefault();

            if (string.IsNullOrEmpty(ffmpegInputFilePath))
            {
                // todo should be ordering by date
                var videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                    .Where(f => f.EndsWith(FileExtension.Mov.Value))
                    .OrderBy(f => f);

                ffmpegInputFilePath = Path.Combine(WorkingDirectory, "videos" + FileExtension.FfmpegInput.Value);
                CreateFfmpegInputFile(videoFiles.ToArray(), ffmpegInputFilePath);
            }

            string outputFilePath = Path.Combine(WorkingDirectory, archiveFile.OutputFileName());

            _fileSystemService.SaveFileContents(
                Path.Combine(IncomingDirectory, archiveFile.Title() + FileExtension.ThumbTxt.Value),
                archiveFile.Title());

            await _ffmpegService.RenderVideoAsCopyAsync(
                ffmpegInputFilePath, outputFilePath, cancellationToken);

            _fileSystemService.MoveFile(archiveFile.FilePath, Path.Combine(ArchiveDirectory, archiveFile.FileName()));
            _fileSystemService.MoveFile(outputFilePath, Path.Combine(IncomingDirectory, archiveFile.OutputFileName()));

            var graphicsFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.ToLower().EndsWith(FileExtension.GraphicsAss.Value))
                .SingleOrDefault();

            if (graphicsFile != null)
            {
                _fileSystemService.MoveFile(
                    graphicsFile, Path.Combine(IncomingDirectory, Path.GetFileName(graphicsFile)));
            }

            _fileSystemService.DeleteDirectory(WorkingDirectory);
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

    public async Task ProcessIncomingVideosWithGraphicsAsync(CancellationToken cancellationToken)
    {
        AssSubtitleFile? graphicsFile = null;

        try
        {
            string selectGraphicsFile = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.GraphicsAss);

            graphicsFile = new AssSubtitleFile(selectGraphicsFile);

            var video = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
                .Where(f => f.StartsWith(graphicsFile.FilePath.Replace(FileExtension.GraphicsAss.Value, string.Empty)) && f.ToLower().EndsWith(FileExtension.Mp4.Value))
                .Select(f => new DashCamVideoFile(f))
                .Single();

            video.SetGraphicsSubtitleFile(graphicsFile);
            video.SetSubtitles(_assSubtitleFileService.ReadFile(video.GraphicsFilePath()));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            // video.SetBrandingText(RandomChannelBrandingText(video.BrandingTextOptions()));
            string videoFilters = video.VideoFilters() + Constant.CommaSpace + video.TitleDrawTextFilter();

            string outputFilePath = Path.Combine(WorkingDirectory, video.FileName());

            if (video.SubType != DashCamVideoFile.DashCamVideoType.CarRepair)
            {
                video.SetAudioFile(_musicService.GetRandomMixTrack());

                await _ffmpegService.RenderVideoWithAudioAndFiltersAsync(
                    video.FilePath, video.AudioFilePath(), videoFilters, outputFilePath, cancellationToken);
                // video.FilePath, video.AudioFile.FilePath, video.VideoFilters(), outputFilePath, cancellationToken);
            }
            else
            {
                await _ffmpegService.RenderVideoWithFiltersAsync(
                    video.FilePath, videoFilters, outputFilePath, cancellationToken);
                // video.FilePath, video.VideoFilters(), outputFilePath, cancellationToken);
            }

            _fileSystemService.MoveFile(
                video.GraphicsFilePath(),
                Path.Combine(ArchiveDirectory, video.GraphicsFilePath()));
            _fileSystemService.MoveFile(outputFilePath, Path.Combine(UploadingDirectory, video.FileName()));
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
        catch (NoFilesMatchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);

            if (graphicsFile != null)
            {
                _loggerService.LogError(ex, $"Error when processing {graphicsFile.FilePath}");
                _fileSystemService.MoveFile(graphicsFile.FilePath, graphicsFile.FilePath + FileExtension.Err.Value);
            }
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

    //         var video = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
    //             .Where(f => f.StartsWith(subtitleFile.FilePath.Replace(FileExtension.GraphicsAss.Value, string.Empty)) && f.ToLower().EndsWith(FileExtension.Mp4.Value))
    //             .Select(f => new VideoFile(f))
    //             .Single();

    //         video.SetGraphicsSubtitleFile(subtitleFilePath);
    //         video.GraphicsSubtitleFile.SetSubtitles(_assSubtitleFileService.ReadFile(video.GraphicsSubtitleFile.FilePath));

    //         _fileSystemService.DeleteDirectory(ReviewWorkDirectory);
    //         _fileSystemService.CreateDirectory(ReviewWorkDirectory);

    //         video.SetBrandingText(RandomChannelBrandingText(video.BrandingTextOptions()));
    //         string videoFilters = video.VideoFilters() + Constant.CommaSpace + video.TitleDrawTextFilter();

    //         string outputFilePath = Path.Combine(ReviewWorkDirectory, video.FileName);

    //         if (video.SubType != DashCamVideoFile.DashCamVideoType.CarRepair)
    //         {
    //             video.SetAudioFile(_musicService.GetRandomMixTrack());

    //             await _ffmpegService.RenderVideoWithAudioAndFiltersAsync(
    //                 video.FilePath, video.AudioFile.FilePath, video.VideoFilters(), outputFilePath, cancellationToken);
    //         }
    //         else
    //         {
    //             await _ffmpegService.RenderVideoWithFiltersAsync(
    //                 video.FilePath, video.VideoFilters(), outputFilePath, cancellationToken);
    //         }

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