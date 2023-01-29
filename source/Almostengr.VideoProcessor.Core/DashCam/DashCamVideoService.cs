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

    // private readonly string _incomingDirectory;
    // private readonly string _archiveDirectory;
    // private readonly string _uploadDirectory;
    // private readonly string _workingDirectory;

    public DashCamVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<DashCamVideoService> loggerService, IMusicService musicService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService)
    {
        Incomingdirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Incoming);
        ArchiveDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Archive);
        UploadDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Upload);
        WorkingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Working);
        DraftDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Draft);
        _loggerService = loggerService;
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
            // string incomingTarball = _fileSystemService.GetRandomTarballFromDirectory(_incomingDirectory);
            string incomingTarball = _fileSystemService.GetRandomFileByExtensionFromDirectory(Incomingdirectory, FileExtension.Srt);
            _loggerService.LogInformation($"Processing ${incomingTarball}");

            video = new DashCamVideoFile(_appSettings.DashCamDirectory, Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                video.IncomingTarballFilePath(), WorkingDirectory, cancellationToken);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            if (DoesKdenliveFileExist(Incomingdirectory))
            {
                throw new KdenliveFileExistsException("Archive has Kdenlive project file");    
            }

            CreateFfmpegInputFile(video);

            if (video.IsDraft)
            {
                await _ffmpegService.RenderVideoAsCopyAsync(
                    video.FfmpegInputFilePath(),
                    Path.Combine(ArchiveDirectory, video.OutputVideoFileName()),
                    cancellationToken);
                _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.DraftTarballFilePath());
                _fileSystemService.SaveFileContents(
                    Path.Combine(ArchiveDirectory, video.ArchiveFileName + FileExtension.GraphicsAss),
                    string.Empty);
                _fileSystemService.DeleteDirectory(WorkingDirectory);
                return;
            }

            // brand video
            video.AddDrawTextVideoFilter(
                RandomChannelBrandingText(video.BrandingTextOptions()),
                video.DrawTextFilterTextColor(),
                Opacity.Full,
                FfmpegFontSize.Medium,
                DrawTextPosition.UpperRight,
                video.DrawTextFilterBackgroundColor(),
                Opacity.Light);

            video.AddDrawTextVideoFilter(
                video.Title,
                video.DrawTextFilterTextColor(),
                Opacity.Full,
                FfmpegFontSize.Small,
                DrawTextPosition.UpperLeft,
                video.DrawTextFilterBackgroundColor(),
                Opacity.Light);

            var graphicsSubtitle = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString()))
                .Single();

            video.SetGraphicsSubtitleFileName(graphicsSubtitle);

            video.AddSubscribeVideoFilter(_randomService.SubscribeLikeDuration());

            await _ffmpegService.RenderVideoWithMixTrackAsync(
                video.FfmpegInputFilePath(),
                _musicService.GetRandomMixTrack(),
                video.VideoFilter,
                video.OutputVideoFilePath(),
                cancellationToken);

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

    private void CreateFfmpegInputFile(DashCamVideoFile video)
    {
        _fileSystemService.DeleteFile(video.FfmpegInputFilePath());

        string[] videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
            .Where(f => f.EndsWith(FileExtension.Mov.ToString()))
            .OrderBy(f => f)
            .ToArray();
        CreateFfmpegInputFile(videoFiles, video.FfmpegInputFilePath());
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
}