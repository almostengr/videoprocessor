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
        IncomingWorkDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.IncomingWork);
        ArchiveDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Archive);
        UploadingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Uploading);
        ReviewingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Reviewing);
        ReviewWorkDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.ReviewWork);
        // WorkingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Working);
        _loggerService = loggerService;
        // _ffmpegInputFilePath = Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME);
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

    // public override async Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken)
    public override async Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken)
    {
        // ToastmastersVideoFile? video = null;
        ToastmastersIncomingTarballFile? tarball = null;

        try
        {
            string selectedTarballFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.Tar);

            // video = new ToastmastersVideoFile(incomingTarball);
            tarball = new ToastmastersIncomingTarballFile(selectedTarballFilePath);

            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);
            _fileSystemService.CreateDirectory(IncomingWorkDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                tarball.FilePath, IncomingWorkDirectory, cancellationToken);

            // _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            // _fileSystemService.DeleteFile(_ffmpegInputFilePath);

            string[] videoFiles = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4.ToString()))
                .OrderBy(f => f)
                .ToArray();

            string ffmpegInputFilePath = Path.Combine(IncomingWorkDirectory, "videos" + FileExtension.FfmpegInput.Value);
            CreateFfmpegInputFile(videoFiles, ffmpegInputFilePath);

            string outputFilePath = Path.Combine(IncomingWorkDirectory, tarball.VideoFileName);

            await _ffmpegService.RenderVideoAsCopyAsync(
                ffmpegInputFilePath, outputFilePath, cancellationToken);

            // await _ffmpegService.ConvertVideoFileToMp3FileAsync(
            //     outputFilePath, outputFilePath.Replace(FileExtension.Mp4.Value, FileExtension.Mp3.Value),
            //     ReviewingDirectory, cancellationToken);

            _fileSystemService.MoveFile(tarball.FilePath, Path.Combine(ArchiveDirectory, tarball.FileName));
            _fileSystemService.MoveFile(outputFilePath, Path.Combine(ReviewingDirectory, tarball.FileName));
            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);

            _loggerService.LogInformation($"Completed processing {selectedTarballFilePath}");
        }
        catch (NoFilesMatchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _fileSystemService.MoveFile(tarball.FilePath, tarball.FilePath + FileExtension.Err.Value);
            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);
        }
    }

    public override async Task ProcessReviewedFilesAsync(CancellationToken cancellationToken)
    {
        // AssSubtitleFile? subtitleFile = null;
        // try
        // {
        //     video.SetBrandingText(RandomChannelBrandingText(video.BrandingTextOptions()));

        //     await _ffmpegService.RenderVideoAsync(
        //         _ffmpegInputFilePath,
        //         video.VideoFilters() + Constant.CommaSpace + video.MeetingFilter(),
        //         Path.Combine(UploadingDirectory, video.OutputVideoFileName),
        //         cancellationToken);

        //     _fileSystemService.MoveFile(video.TarballFilePath, Path.Combine(ArchiveDirectory, video.TarballFileName));
        //     _fileSystemService.DeleteDirectory(WorkingDirectory);
        //     _loggerService.LogInformation($"Completed processing {incomingTarball}");

        AssSubtitleFile? subtitleFile = null;

        try
        {
            string subtitleFilePath =
                _fileSystemService.GetRandomFileByExtensionFromDirectory(ReviewingDirectory, FileExtension.GraphicsAss);

            subtitleFile = new AssSubtitleFile(subtitleFilePath);
            
            _loggerService.LogInformation($"Processing {subtitleFilePath}");

            string videoFilePath = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
                .Where(f => f.StartsWith(subtitleFile.FileName) && f.ToLower().EndsWith(FileExtension.Mp4.Value))
                .Single();

            ToastmastersVideoFile video = new ToastmastersVideoFile(videoFilePath);
            video.SetGraphicsSubtitleFile(subtitleFilePath);
            video.GraphicsSubtitleFile.SetSubtitles(_assSubtitleFileService.ReadFile(video.GraphicsSubtitleFile.FilePath));

            _fileSystemService.DeleteDirectory(ReviewWorkDirectory);
            _fileSystemService.CreateDirectory(ReviewWorkDirectory);

            video.SetBrandingText(RandomChannelBrandingText(video.BrandingTextOptions()));
            string videoFilters = video.VideoFilters();

            // string audioFilePath = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
            //     .Where(f => f.StartsWith(subtitle.FileName) && f.ToLower().EndsWith(FileExtension.Mp3.Value))
            //     .Single();
            // video.SetAudioFile(_musicService.GetRandomMixTrack());

            /// render video
            // await _ffmpegService.RenderVideoAsync
            string outputFilePath = Path.Combine(ReviewWorkDirectory, video.FileName);

            await _ffmpegService.RenderVideoAsync(
                video.FilePath, video.VideoFilters(), outputFilePath, ReviewWorkDirectory, cancellationToken);

            _fileSystemService.MoveFile(
                video.GraphicsSubtitleFile.FilePath,
                Path.Combine(ArchiveDirectory, video.GraphicsSubtitleFile.FileName));
            _fileSystemService.MoveFile(outputFilePath, Path.Combine(UploadingDirectory, video.FileName));
            _fileSystemService.DeleteDirectory(ReviewWorkDirectory);

            _loggerService.LogInformation($"Completed processing {subtitleFilePath}");
        }
        catch (NoFilesMatchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _fileSystemService.MoveFile(subtitleFile.FilePath, subtitleFile.FilePath + FileExtension.Err.Value);

            // if (video != null)
            // {
            //     _fileSystemService.MoveFile(video.TarballFilePath, Path.Combine(ArchiveDirectory, video.TarballFileName));
            //     _fileSystemService.SaveFileContents(
            //         Path.Combine(ArchiveDirectory, video.TarballFileName + FileExtension.Log),
            //         ex.Message);
            // }
        }
    }

}
