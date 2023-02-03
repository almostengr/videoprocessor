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
        ArchiveDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Archive);
        UploadDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Upload);
        WorkingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Working);
        DraftDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Draft);
        _loggerService = loggerService;
        _ffmpegInputFilePath = Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME);
        _xzFileService = xzFileService;
        _gzFileService = gzFileService;
    }

    public override async Task ConvertGzToXzAsync(CancellationToken cancellationToken)
    {
        var tarGzFiles = _fileSystemService.GetFilesInDirectory(ArchiveDirectory)
            .Where(f => f.EndsWith(FileExtension.TarGz.ToString()));

        foreach (var file in tarGzFiles)
        {
            await _gzFileService.DecompressFileAsync(file, cancellationToken);

            await _xzFileService.CompressFileAsync(
                file.Replace(FileExtension.TarGz.ToString(), FileExtension.Tar.ToString()), cancellationToken);
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

    public override async Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken)
    {
        DashCamVideoFile? video = null;

        try
        {
            string incomingTarball = _fileSystemService.GetRandomFileByExtensionFromDirectory(IncomingDirectory, FileExtension.Srt);
            _loggerService.LogInformation($"Processing ${incomingTarball}");

            video = new DashCamVideoFile(incomingTarball);

            string incomingFilePath = Path.Combine(IncomingDirectory, video.TarballFileName);
            string archiveFilePath = Path.Combine(ArchiveDirectory, video.TarballFileName);
            string outputFilePath = Path.Combine(UploadDirectory, video.TarballFileName);
            string draftFilePath = Path.Combine(DraftDirectory, video.TarballFileName);
            string ffmpegInputFilePath = Path.Combine(WorkingDirectory, video.TarballFileName);

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                incomingFilePath, WorkingDirectory, cancellationToken);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            if (DoesKdenliveFileExist(IncomingDirectory))
            {
                throw new KdenliveFileExistsException("Archive has Kdenlive project file");
            }

            // if (_fileSystemService.GetFilesInDirectory(WorkingDirectory))

            CreateFfmpegInputFile(video);

            if (video.IsDraft)
            {
                await _ffmpegService.RenderVideoAsCopyAsync(
                    ffmpegInputFilePath, archiveFilePath, cancellationToken);
                _fileSystemService.MoveFile(
                    incomingTarball, draftFilePath);// Path.Combine(DraftDirectory, video.FileName));
                _fileSystemService.SaveFileContents(
                    Path.Combine(ArchiveDirectory, video.TarballFileName + FileExtension.GraphicsAss),
                    string.Empty);
                _fileSystemService.DeleteDirectory(WorkingDirectory);
                return;
            }

            // brand video
            // video.AddDrawTextVideoFilter(
            //     RandomChannelBrandingText(video.BrandingTextOptions()),
            //     video.DrawTextFilterTextColor(),
            //     Opacity.Full,
            //     FfmpegFontSize.Medium,
            //     DrawTextPosition.UpperRight,
            //     video.DrawTextFilterBackgroundColor(),
            //     Opacity.Light);

            // video.AddDrawTextVideoFilter(
            //     video.Title,
            //     video.DrawTextFilterTextColor(),
            //     Opacity.Full,
            //     FfmpegFontSize.Small,
            //     DrawTextPosition.UpperLeft,
            //     video.DrawTextFilterBackgroundColor(),
            //     Opacity.Light);

            // var graphicsSubtitle = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
            //     .Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString()))
            //     .Single();

            // video.AddDrawTextVideoFilterFromSubtitles(_assSubtitleFileService.ReadFile(graphicsSubtitle));
            
            string? graphicsSubtitleFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString()))
                .Single();

            video.SetGraphicsSubtitleFile(graphicsSubtitleFile);
            video.GraphicsSubtitleFile.SetSubtitles(
                _assSubtitleFileService.ReadFile(video.GraphicsSubtitleFile.FilePath));

            // video.SetGraphicsSubtitleFileName(graphicsSubtitle);

            // video.AddSubscribeVideoFilter(_randomService.SubscribeLikeDuration());
            
            video.SetBrandingText(RandomChannelBrandingText(video.BrandingTextOptions()));
            var videoFilters = video.VideoFilters() + Constant.CommaSpace + video.TitleDrawTextFilter();

            await _ffmpegService.RenderVideoWithMixTrackAsync(
                ffmpegInputFilePath, // video.FfmpegInputFilePath(),
                _musicService.GetRandomMixTrack(),
                // video.VideoFilters(),
                videoFilters,
                outputFilePath, // video.OutputVideoFilePath(),
                cancellationToken);

            _fileSystemService.MoveFile(incomingFilePath, archiveFilePath);
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
                _fileSystemService.MoveFile(
                    Path.Combine(IncomingDirectory, video.TarballFileName),
                    Path.Combine(ArchiveDirectory, video.TarballFileName)
                );
                _fileSystemService.SaveFileContents(
                    Path.Combine(ArchiveDirectory, video.TarballFileName + FileExtension.Log),
                    ex.Message);
            }
        }
    }

    private void CreateFfmpegInputFile(DashCamVideoFile video)
    {
        _fileSystemService.DeleteFile(Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME));

        string[] videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
            .Where(f => f.EndsWith(FileExtension.Mov.ToString()))
            .OrderBy(f => f)
            .ToArray();
        CreateFfmpegInputFile(videoFiles, Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME));
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