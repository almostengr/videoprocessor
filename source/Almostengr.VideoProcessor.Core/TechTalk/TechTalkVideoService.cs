using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed class TechTalkVideoService : BaseVideoService, ITechTalkVideoService
{
    private readonly ILoggerService<TechTalkVideoService> _loggerService;
    private readonly ISrtSubtitleFileService _srtService;

    private readonly string _incomingDirectory;
    private readonly string _archiveDirectory;
    private readonly string _uploadDirectory;
    private readonly string _workingDirectory;

    public TechTalkVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<TechTalkVideoService> loggerService, IMusicService musicService,
        ISrtSubtitleFileService srtSubtitleFileService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService)
    {
        _incomingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Incoming);
        _archiveDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Archive);
        _uploadDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Upload);
        _workingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Working);
        _loggerService = loggerService;
        _srtService = srtSubtitleFileService;
    }

    public override async Task ConvertGzToXzAsync(CancellationToken cancellationToken)
    {
        var tarGzFiles = _fileSystemService.GetFilesInDirectory(_archiveDirectory)
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
            await base.CompressTarballsInArchiveFolderAsync(_archiveDirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }

    public override async Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken)
    {
        TechTalkVideo? video = null;

        try
        {
            // string incomingTarball = _fileSystemService.GetRandomTarballFromDirectory(IncomingDirectory);
            string incomingTarball = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                _incomingDirectory, FileExtension.Tar);
            _loggerService.LogInformation($"Processing ${incomingTarball}");

            video = new TechTalkVideo(_appSettings.TechnologyDirectory, Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(_workingDirectory);
            _fileSystemService.CreateDirectory(_workingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                video.IncomingTarballFilePath(), _workingDirectory, cancellationToken);

            _fileSystemService.PrepareAllFilesInDirectory(_workingDirectory);

            if (DoesKdenliveFileExist(_incomingDirectory))
            {
                throw new KdenliveFileExistsException("Archive has Kdenlive project file");
            }

            // if (video.ArchiveFileName.ToLower().StartsWith(END_SCREEN))
            // {
            //     await CreateEndScreenVideoAsync(_workingDirectory, cancellationToken);
            //     _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
            //     _fileSystemService.DeleteDirectory(_workingDirectory);
            //     return;
            // }

            _fileSystemService.DeleteFile(video.FfmpegInputFilePath());

            var videoFiles = _fileSystemService.GetFilesInDirectory(_workingDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4.ToString()) || f.EndsWith(FileExtension.Mkv.ToString()))
                .OrderBy(f => f)
                .ToList();

            await MergeVideoAndAudioFilesAsync(cancellationToken);

            foreach (var file in videoFiles)
            {
                await _ffmpegService.ConvertVideoFileToTsFormatAsync(
                    file, file + FileExtension.Ts, cancellationToken);
            }

            videoFiles.Clear();

            videoFiles = _fileSystemService.GetFilesInDirectory(_workingDirectory)
                .Where(f => f.EndsWith(FileExtension.Ts.ToString()))
                .OrderBy(f => f)
                .ToList();

            // if (video.SubType == TechTalkVideoSubType.TechTalk && video.IsDraft() == false)
            // {
            //     videoFiles.Add(video.EndScreenFilePath());
            // }

            CreateFfmpegInputFile(videoFiles.ToArray(), video.FfmpegInputFilePath());

            if (video.IsDraft)
            {
                await _ffmpegService.ConcatTsFilesToMp4FileAsync(
                    video.FfmpegInputFilePath(), string.Empty, video.OutputVideoFilePath(), cancellationToken);
                _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
                _fileSystemService.DeleteDirectory(_workingDirectory);
                return;
            }

            if (_fileSystemService.DoesFileExist(video.ChristmasLightMetaFile()))
            {
                video.ConfirmChristmasLightVideo();
            }

            if (_fileSystemService.DoesFileExist(video.IndependenceDayMetaFile()))
            {
                video.ConfirmIndependenceDayVideo();
            }

            // brand video
            video.AddDrawTextVideoFilter(
                RandomChannelBrandingText(video.BrandingTextOptions()),
                video.DrawTextFilterTextColor(),
                Opacity.Full,
                FfmpegFontSize.Large,
                DrawTextPosition.UpperRight,
                video.DrawTextFilterBackgroundColor(),
                Opacity.Light,
                10);

            if (_fileSystemService.GetFilesInDirectory(_workingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString())).Any())
            {
                video.AddSubtitleVideoFilter(
                    _fileSystemService.GetFilesInDirectory(_workingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString())).Single(),
                    "&H00006400",
                    "&H00FFFFFF");
            }

            var graphicsSubtitle = _fileSystemService.GetFilesInDirectory(_workingDirectory)
                .Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString()))
                .SingleOrDefault();
            video.SetGraphicsSubtitleFileName(graphicsSubtitle);

            await _ffmpegService.ConcatTsFilesToMp4FileAsync(
                video.FfmpegInputFilePath(), video.OutputVideoFilePath(), video.VideoFilter, cancellationToken);

            _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
            _fileSystemService.DeleteDirectory(_workingDirectory);
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
                _fileSystemService.MoveFile(video.IncomingTarballFilePath(), Path.Combine(_archiveDirectory, video.ArchiveFileName));
                _fileSystemService.SaveFileContents(
                    Path.Combine(_archiveDirectory, video.ArchiveFileName + FileExtension.Log),
                    ex.Message);
            }
        }
    }

    private async Task MergeVideoAndAudioFilesAsync(CancellationToken cancellationToken)
    {
        var workingDirVideos = _fileSystemService.GetFilesInDirectory(_workingDirectory)
            .Where(f => f.EndsWith(FileExtension.Mp4.ToString()) || f.EndsWith(FileExtension.Mkv.ToString()));

        foreach (var videoFilePath in workingDirVideos)
        {
            string? audioFilePath = _fileSystemService.GetFilesInDirectory(_workingDirectory)
                .Where(f => f.Contains(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3.ToString()))
                // .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3))
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(audioFilePath))
            {
                continue;
            }

            string tempOutputFilePath = Path.Combine(_workingDirectory,
                Path.GetFileNameWithoutExtension(videoFilePath) + ".tmp" + Path.GetExtension(videoFilePath));

            await _ffmpegService.AddAccAudioToVideoAsync(
                videoFilePath, audioFilePath, tempOutputFilePath, cancellationToken);

            _fileSystemService.DeleteFile(videoFilePath);
            _fileSystemService.MoveFile(tempOutputFilePath, videoFilePath);
        }
    }

    public override Task ProcessIncomingSubtitlesAsync(CancellationToken cancellationToken)
    {
        try
        {
            string filePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(_incomingDirectory, FileExtension.Srt);
            // string filePath = _fileSystemService.GetRandomSrtFileFromDirectory(IncomingDirectory);

            TechTalkSrtSubtitleFile subtitle = new(filePath);

            // subtitle.SetSubtitleText(_fileSystemService.GetFileContents(subtitle.IncomingFilePath()));

            // _fileSystemService.SaveFileContents(subtitle.SubtitleOutputFilePath(), subtitle.SubtitleText());
            // _fileSystemService.SaveFileContents(subtitle.BlogOutputFilePath(), subtitle.BlogPostText());
            // _fileSystemService.MoveFile(subtitle.IncomingFilePath(), subtitle.ArchiveFilePath());

            var subtitles = _srtService.ReadFile(subtitle.FilePath);
            subtitle.SetSubtitles(subtitles);

            _srtService.WriteFile(Path.Combine(_uploadDirectory, subtitle.FileName()), subtitle.Subtitles);

            _fileSystemService.SaveFileContents(
                Path.Combine(_uploadDirectory, subtitle.BlogFileName()), subtitle.BlogPostText());

            _fileSystemService.MoveFile(
                subtitle.FilePath, Path.Combine(_archiveDirectory, subtitle.FileName()), false);
        }
        catch (NoFilesMatchException) // NoSubtitleFilesPresentException)
        { }
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
            await base.CreateTarballsFromDirectoriesAsync(_incomingDirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }
}