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
        ArchiveDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Archive);
        UploadDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Upload);
        WorkingDirectory = Path.Combine(_appSettings.ToastmastersDirectory, DirectoryName.Working);
        _loggerService = loggerService;
        _ffmpegInputFilePath = Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME);
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

        foreach (var file in tarGzFiles)
        {
            await _compressionService.DecompressFileAsync(file, cancellationToken);

            await _compressionService.CompressFileAsync(
                file.Replace(FileExtension.TarGz.ToString(), FileExtension.Tar.ToString()), cancellationToken);
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

    public override async Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken)
    {
        ToastmastersVideoFile? video = null;

        try
        {
            string incomingTarball = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.Tar);

            video = new ToastmastersVideoFile(incomingTarball);

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                video.TarballFilePath, WorkingDirectory, cancellationToken);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            // create input file
            _fileSystemService.DeleteFile(_ffmpegInputFilePath);

            string[] videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4.ToString()))
                .OrderBy(f => f)
                .ToArray();

            CreateFfmpegInputFile(videoFiles, _ffmpegInputFilePath);

            // brand video
            // video.AddDrawTextVideoFilter(
            //     RandomChannelBrandingText(video.BrandingTextOptions()),
            //     video.DrawTextFilterTextColor(),
            //     Opacity.Full,
            //     FfmpegFontSize.Large,
            //     DrawTextPosition.UpperRight,
            //     video.DrawTextFilterBackgroundColor(),
            //     Opacity.Medium);
            video.SetBrandingText(RandomChannelBrandingText(video.BrandingTextOptions()));

            // video.AddLikeVideoFilter(_randomService.SubscribeLikeDuration());

            await _ffmpegService.RenderVideoAsync(
                _ffmpegInputFilePath,
                video.VideoFilters() + Constant.CommaSpace + video.MeetingFilter(),
                Path.Combine(UploadDirectory, video.OutputVideoFileName),
                cancellationToken);

            _fileSystemService.MoveFile(video.TarballFilePath, Path.Combine(ArchiveDirectory, video.TarballFileName));
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
                _fileSystemService.MoveFile(video.TarballFilePath, Path.Combine(ArchiveDirectory, video.TarballFileName));
                _fileSystemService.SaveFileContents(
                    Path.Combine(ArchiveDirectory, video.TarballFileName + FileExtension.Log),
                    ex.Message);
            }
        }
    }

}
