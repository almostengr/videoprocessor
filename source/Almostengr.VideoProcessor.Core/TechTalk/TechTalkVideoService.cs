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

    // private readonly string _incomingDirectory;
    // private readonly string _archiveDirectory;
    // private readonly string _uploadDirectory;
    // private readonly string _workingDirectory;

    public TechTalkVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<TechTalkVideoService> loggerService, IMusicService musicService,
        ISrtSubtitleFileService srtSubtitleFileService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService)
    {
        IncomingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Incoming);
        ArchiveDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Archive);
        UploadDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Upload);
        WorkingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Working);
        DraftDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Draft);
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
        TechTalkVideoFile? video = null;

        try
        {
            // string incomingTarball = _fileSystemService.GetRandomTarballFromDirectory(IncomingDirectory);
            string incomingTarball = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.Tar);
            _loggerService.LogInformation($"Processing ${incomingTarball}");

            video = new TechTalkVideoFile(incomingTarball);
            // video = new TechTalkVideoFile(_appSettings.TechnologyDirectory, Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                // video.IncomingTarballFilePath(), WorkingDirectory, cancellationToken);
                video.TarballFilePath, WorkingDirectory, cancellationToken);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            if (DoesKdenliveFileExist(IncomingDirectory))
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

            // _fileSystemService.DeleteFile(video.FfmpegInputFilePath());
            _fileSystemService.DeleteFile(Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME));

            var videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
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

            videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Ts.ToString()))
                .OrderBy(f => f)
                .ToList();

            // if (video.SubType == TechTalkVideoSubType.TechTalk && video.IsDraft() == false)
            // {
            //     videoFiles.Add(video.EndScreenFilePath());
            // }

            // CreateFfmpegInputFile(videoFiles.ToArray(), video.FfmpegInputFilePath());
            CreateFfmpegInputFile(videoFiles.ToArray(), Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME));

            if (video.IsDraft)
            {
                await _ffmpegService.ConcatTsFilesToMp4FileAsync(
                    // _ffmpegInputFilePath, string.Empty, video.OutputVideoFilePath(), cancellationToken);
                    _ffmpegInputFilePath,
                    Path.Combine(UploadDirectory, video.OutputVideoFileName),
                    video.VideoFilter,
                    cancellationToken);

                _fileSystemService.MoveFile(video.TarballFilePath, Path.Combine(ArchiveDirectory, video.TarballFileName));
                _fileSystemService.DeleteDirectory(WorkingDirectory);
                return;
            }

            // // if (_fileSystemService.DoesFileExist(video.ChristmasLightMetaFile()))
            // if (_fileSystemService.DoesFileExist(Path.Combine(WorkingDirectory, video.ChristmasLightFileName())))
            // {
            //     video.ConfirmChristmasLightVideo();
            // }

            // if (_fileSystemService.DoesFileExist(Path.Combine(WorkingDirectory, video.IndependenceDayFileName())))
            // // if (_fileSystemService.DoesFileExist(video.IndependenceDayMetaFile()))
            // {
            //     video.ConfirmIndependenceDayVideo();
            // }

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

            string? graphicsSubtitleFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString()))
                .SingleOrDefault();

            if (graphicsSubtitleFile != null)
            {
                video.AddSubtitleVideoFilter(graphicsSubtitleFile,"&H00006400","&H00FFFFFF", 26);
            }

            // var graphicsSubtitle = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
            //     .Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString()))
            //     .SingleOrDefault();
            // video.SetGraphicsSubtitleFileName(graphicsSubtitle);

            await _ffmpegService.ConcatTsFilesToMp4FileAsync(
                _ffmpegInputFilePath,
                Path.Combine(UploadDirectory, video.OutputVideoFileName),
                video.VideoFilter,
                cancellationToken);
            // video.FfmpegInputFilePath(), video.OutputVideoFilePath(), video.VideoFilter, cancellationToken);

            // _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
            _fileSystemService.MoveFile(
                video.TarballFilePath, Path.Combine(ArchiveDirectory, video.TarballFileName));
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
                    // video.IncomingTarballFilePath(), 
                    video.TarballFilePath,
                    Path.Combine(ArchiveDirectory, video.TarballFileName));
                _fileSystemService.SaveFileContents(
                    Path.Combine(ArchiveDirectory, video.TarballFileName + FileExtension.Log),
                    ex.Message);
            }
        }
    }

    private async Task MergeVideoAndAudioFilesAsync(CancellationToken cancellationToken)
    {
        var workingDirVideos = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
            .Where(f => f.EndsWith(FileExtension.Mp4.ToString()) || f.EndsWith(FileExtension.Mkv.ToString()));

        foreach (var videoFilePath in workingDirVideos)
        {
            string? audioFilePath = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.Contains(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3.ToString()))
                // .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3))
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(audioFilePath))
            {
                continue;
            }

            string tempOutputFilePath = Path.Combine(WorkingDirectory,
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
            string filePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(IncomingDirectory, FileExtension.Srt);
            // string filePath = _fileSystemService.GetRandomSrtFileFromDirectory(IncomingDirectory);

            TechTalkSrtSubtitleFile subtitle = new(filePath);

            // subtitle.SetSubtitleText(_fileSystemService.GetFileContents(subtitle.IncomingFilePath()));

            // _fileSystemService.SaveFileContents(subtitle.SubtitleOutputFilePath(), subtitle.SubtitleText());
            // _fileSystemService.SaveFileContents(subtitle.BlogOutputFilePath(), subtitle.BlogPostText());
            // _fileSystemService.MoveFile(subtitle.IncomingFilePath(), subtitle.ArchiveFilePath());

            var subtitles = _srtService.ReadFile(subtitle.FilePath);
            subtitle.SetSubtitles(subtitles);

            _srtService.WriteFile(Path.Combine(UploadDirectory, subtitle.FileName()), subtitle.Subtitles);

            _fileSystemService.SaveFileContents(
                Path.Combine(UploadDirectory, subtitle.BlogFileName()), subtitle.BlogPostText());

            _fileSystemService.MoveFile(
                subtitle.FilePath, Path.Combine(ArchiveDirectory, subtitle.FileName()), false);
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
            await base.CreateTarballsFromDirectoriesAsync(IncomingDirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }
}