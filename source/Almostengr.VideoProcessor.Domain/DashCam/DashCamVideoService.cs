using Almostengr.VideoProcessor.Domain.Common.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Videos;
using Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Domain.Common.Exceptions;

namespace Almostengr.VideoProcessor.Domain.DashCam;

public sealed class DashCamVideoService : BaseVideoService, IDashCamVideoService
{
    private readonly IFfmpeg _ffmpeg;
    private readonly IFileSystem _fileSystem;
    private readonly ITarball _tarball;
    private readonly IMusicService _musicService;
    private readonly ILoggerService<DashCamVideoService> _logger;
    private readonly AppSettings _appSettings;
    private readonly IGzipService _gzipService;

    public DashCamVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService,
        ITarball tarball, IMusicService musicService, ILoggerService<DashCamVideoService> logger,
        AppSettings appSettings, IGzipService gzipService
    ) : base(fileSystemService, ffmpegService, tarball, appSettings)
    {
        _fileSystem = fileSystemService;
        _ffmpeg = ffmpegService;
        _tarball = tarball;
        _musicService = musicService;
        _logger = logger;
        _appSettings = appSettings;
        _gzipService = gzipService;
    }

    public async Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken)
    {
        DashCamVideo video = new DashCamVideo(_appSettings.DashCamDirectory);
        await CreateTarballsFromDirectoriesAsync(video.IncomingDirectory, cancellationToken);
    }

    public override async Task<bool> ProcessVideoAsync(CancellationToken cancellationToken)
    {
        DashCamVideo video = new DashCamVideo(_appSettings.DashCamDirectory);

        try
        {
            CreateVideoDirectories(video);
            DeleteFilesOlderThanSpecifiedDays(video.UploadDirectory);

            _fileSystem.IsDiskSpaceAvailable(video.BaseDirectory);

            await CreateTarballsFromDirectoriesAsync(video.IncomingDirectory, cancellationToken);

            video.SetTarballFilePath(_fileSystem.GetRandomTarballFromDirectory(video.IncomingDirectory));

            if (_fileSystem.DoesFileExist(video.IncomingDetailsFilePath()))
            {
                if (video.TarballFilePath.ToLower().EndsWith(FileExtension.TarGz))
                {
                    await _gzipService.DecompressFileAsync(video.TarballFilePath, cancellationToken);
                    video.SetTarballFilePath(video.TarballFilePath.Replace(FileExtension.TarGz, FileExtension.Tar));
                }

                if (await _tarball.DoesTarballContainFileAsync(video.TarballFilePath, "details.txt", cancellationToken) == false)
                {
                    await _tarball.AddFileToTarballAsync(
                        video.TarballFilePath, video.IncomingDetailsFilePath(), video.IncomingDirectory, cancellationToken);
                }

                _fileSystem.DeleteFile(video.IncomingDetailsFilePath());
            }

            _fileSystem.DeleteDirectory(video.WorkingDirectory);
            _fileSystem.CreateDirectory(video.WorkingDirectory);

            await _tarball.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, cancellationToken);

            _fileSystem.PrepareAllFilesInDirectory(video.WorkingDirectory);

            CreateFfmpegInputFile(video);

            if (video.TarballFileName.ToLower().Contains(".draft"))
            {
                await _ffmpeg.RenderVideoAsCopyAsync(
                    video.FfmpegInputFilePath, video.OutputFilePath.Replace(FileExtension.Mp4, FileExtension.Mov), cancellationToken);

                _fileSystem.MoveFile(video.TarballFilePath, video.TarballArchiveFilePath, false);
                _fileSystem.SaveFileContents(
                    Path.Combine(video.ArchiveDirectory, video.Title + ".details.txt"),
                    string.Empty);
                _fileSystem.DeleteDirectory(video.WorkingDirectory);
                return false;
            }

            video.SetChannelBannerText(SelectChannelBannerText());

            string? detailFile = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
                .Where(f => f.EndsWith(video.DetailsFileSuffix()))
                .SingleOrDefault();
            video.SetDetailsFileName(detailFile);

            if (video.GetDetailsFileName() != string.Empty)
            {
                string fileContents = _fileSystem.GetFileContents(video.DetailsFilePath());
                video.AddDetailsContentToVideoFilter(fileContents.Split(Environment.NewLine));
            }

            video.AddDrawTextFilter(
                video.StrippedTitle(), video.BannerTextColor(),
                Opacity.Full,
                FfmpegFontSize.Small,
                DrawTextPosition.UpperLeft,
                video.BannerBackgroundColor(),
                Opacity.Light,
                Constant.BorderBoxWidthSmall); // video title filter

            if (_fileSystem.DoesFileExist(video.NoMusicFilePath()))
            {
                await _ffmpeg.RenderVideoAsync(
                    video.FfmpegInputFilePath, video.VideoFilter, video.OutputFilePath, cancellationToken);
            }
            else
            {
                await _ffmpeg.RenderVideoWithMixTrackAsync(
                    video.FfmpegInputFilePath, _musicService.GetRandomMixTrack(), video.VideoFilter, video.OutputFilePath, cancellationToken);
            }

            _fileSystem.MoveFile(video.TarballFilePath, video.TarballArchiveFilePath, false);
            _fileSystem.DeleteDirectory(video.WorkingDirectory);
            _fileSystem.DeleteFile(video.IncomingDetailsFilePath());
        }
        catch (NoTarballsPresentException)
        {
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            _fileSystem.MoveFile(video.TarballFilePath, video.TarballErrorFilePath, false);

            if (_fileSystem.DoesFileExist(video.IncomingDetailsFilePath()))
            {
                _fileSystem.MoveFile(video.IncomingDetailsFilePath(), Path.Combine(video.ErrorDirectory, Path.GetFileName(video.IncomingDetailsFilePath())), false);
            }

            _fileSystem.SaveFileContents(video.ErrorLogFilePath, ex.Message);
        }

        return false;
    }

    public async Task CompressTarballsInArchiveFolderAsync(CancellationToken stoppingToken)
    {
        DashCamVideo video = new DashCamVideo(_appSettings.DashCamDirectory);

        foreach (var uncompressedTarball in _fileSystem.GetFilesInDirectory(video.ArchiveDirectory)
            .Where(f => f.EndsWith(FileExtension.Tar) && !f.ToLower().Contains("draft")))
        {
            try
            {
                await _gzipService.CompressFileAsync(uncompressedTarball, stoppingToken);
            }
            catch (UnableToCompressFileException ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }

    internal override void CreateFfmpegInputFile<DashCamVideo>(DashCamVideo video)
    {
        _fileSystem.DeleteFile(video.FfmpegInputFilePath);

        using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
        {
            var filesInDirectory = (new DirectoryInfo(video.WorkingDirectory)).GetFiles()
                .OrderBy(f => f.CreationTimeUtc)
                .ToArray();

            foreach (var file in filesInDirectory)
            {
                if (file.Name.EndsWith(FileExtension.Md) ||
                    file.Name.EndsWith(FileExtension.Srt) ||
                    file.Name.EndsWith(FileExtension.Txt))
                {
                    continue;
                }

                writer.WriteLine($"{FILE} '{file}'");
            }
        }
    }

    internal override string SelectChannelBannerText()
    {
        return "Kenny Ram Dash Cam";
    }
}