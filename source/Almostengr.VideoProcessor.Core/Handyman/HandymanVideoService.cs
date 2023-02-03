using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;

namespace Almostengr.VideoProcessor.Core.Handyman;

public sealed class HandymanVideoService : BaseVideoService, IHandymanVideoService
{
    private readonly ILoggerService<HandymanVideoService> _loggerService;
    private readonly ISrtSubtitleFileService _srtService;

    public HandymanVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        IMusicService musicService, ILoggerService<HandymanVideoService> loggerService,
        IAssSubtitleFileService assSubtitleFileService, ISrtSubtitleFileService srtSubtitleFileService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService, assSubtitleFileService)
    {
        IncomingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Incoming);
        ArchiveDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Archive);
        UploadDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Upload);
        WorkingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Working);
        DraftDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Draft);
        _loggerService = loggerService;
        _srtService = srtSubtitleFileService;
        _ffmpegInputFilePath = Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME);
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
        HandymanVideoFile? video = null;
        try
        {
            string incomingTarball = _fileSystemService.GetRandomFileByExtensionFromDirectory(IncomingDirectory, FileExtension.Tar);

            video = new HandymanVideoFile(Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                video.TarballFilePath, WorkingDirectory, cancellationToken);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            if (DoesKdenliveFileExist(IncomingDirectory))
            {
                throw new KdenliveFileExistsException();
            }

            await ConvertVideoAudioFilesToAudioOnly(WorkingDirectory, cancellationToken);

            await MergeVideoAndAudioFilesAsync(cancellationToken);

            CreateFfmpegInputFile(video);

            // video.AddDrawTextVideoFilter(
            //     RandomChannelBrandingText(video.BrandingTextOptions()),
            //     video.DrawTextFilterTextColor(),
            //     Opacity.Full,
            //     FfmpegFontSize.Large,
            //     DrawTextPosition.UpperRight,
            //     video.DrawTextFilterBackgroundColor(),
            //     Opacity.Medium);

            // if (_fileSystemService.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString())).Any())
            // {
            // video.AddSubtitleVideoFilter(
            //     _fileSystemService.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString())).Single(),
            //     "&H00006400",
            //     "&H00FFFFFF");
            // }

            string? graphicsSubtitleFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString()))
                .SingleOrDefault();

            if (!string.IsNullOrEmpty(graphicsSubtitleFile))
            {
                video.SetGraphicsSubtitleFile(graphicsSubtitleFile);
                video.GraphicsSubtitleFile.SetSubtitles(
                    _assSubtitleFileService.ReadFile(video.GraphicsSubtitleFile.FilePath));
            }

            await _ffmpegService.RenderVideoAsync(
                Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME),
                video.VideoFilters(),
                Path.Combine(UploadDirectory, video.OutputVideoFileName),
                cancellationToken);

            _fileSystemService.MoveFile(video.TarballFilePath, Path.Combine(ArchiveDirectory, video.TarballFileName));
            _fileSystemService.DeleteDirectory(WorkingDirectory);
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

    private void CreateFfmpegInputFile(HandymanVideoFile handymanVideo)
    {
        _fileSystemService.DeleteFile(Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME));

        var videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
            .Where(f => f.EndsWith(FileExtension.Mp4.ToString()) || f.EndsWith(FileExtension.Mkv.ToString()))
            .OrderBy(f => f)
            .ToList();

        base.CreateFfmpegInputFile(videoFiles.ToArray(), Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME));
    }

    private async Task MergeVideoAndAudioFilesAsync(CancellationToken cancellationToken)
    {
        var workingDirVideos = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                    .Where(f => f.EndsWith(FileExtension.Mp4.ToString()) || f.EndsWith(FileExtension.Mkv.ToString()));

        foreach (var videoFilePath in workingDirVideos)
        {
            var result = await _ffmpegService.FfprobeAsync($"\"{videoFilePath}\"", WorkingDirectory, cancellationToken);

            if (result.stdErr.ToLower().Contains("audio"))
            {
                continue;
            }

            string? audioFilePath = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3.ToString()))
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(audioFilePath))
            {
                audioFilePath = _musicService.GetRandomNonMixTrack();
            }

            string tempOutputFileName = Path.GetFileNameWithoutExtension(videoFilePath) + FileExtension.TmpMp4;

            await _ffmpegService.AddAudioToVideoAsync(
                videoFilePath, audioFilePath, tempOutputFileName, cancellationToken);

            _fileSystemService.DeleteFile(videoFilePath);
            _fileSystemService.MoveFile(tempOutputFileName, videoFilePath);
        }
    }

    public override Task ProcessIncomingSubtitlesAsync(CancellationToken cancellationToken)
    {
        try
        {
            string filePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(IncomingDirectory, FileExtension.Srt);

            HandymanSrtSubtitleFile subtitle = new(filePath);

            var subtitles = _srtService.ReadFile(subtitle.FilePath);
            subtitle.SetSubtitles(subtitles);

            _srtService.WriteFile(Path.Combine(UploadDirectory, subtitle.FileName), subtitle.Subtitles);

            _fileSystemService.SaveFileContents(
                Path.Combine(UploadDirectory, subtitle.BlogFileName()), subtitle.BlogPostText());

            _fileSystemService.MoveFile(
                subtitle.FilePath, Path.Combine(ArchiveDirectory, subtitle.FileName), false);
        }
        catch (NoFilesMatchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }

        return Task.CompletedTask;
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