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
    private readonly string _incomingDirectory;
    private readonly string _archiveDirectory;
    private readonly string _uploadDirectory;
    private readonly string _workingDirectory;

    public HandymanVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
         IMusicService musicService, ILoggerService<HandymanVideoService> loggerService,
         ISrtSubtitleFileService srtSubtitleFileService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService)
    {
        _incomingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Incoming);
        _archiveDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Archive);
        _uploadDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Upload);
        _workingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Working);
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
        HandymanVideo? video = null;
        try
        {
            // string incomingTarball = _fileSystemService.GetRandomTarballFromDirectory(IncomingDirectory);
            string incomingTarball = _fileSystemService.GetRandomFileByExtensionFromDirectory(_incomingDirectory, FileExtension.Tar);

            video = new HandymanVideo(_appSettings.HandymanDirectory, Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(_workingDirectory);
            _fileSystemService.CreateDirectory(_workingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                video.IncomingTarballFilePath(), _workingDirectory, cancellationToken);

            // _fileSystemService.CopyFile(
            //     video.EndScreenFilePath(),
            //     Path.Combine(_workingDirectory, video.EndScreenFileName()));

            _fileSystemService.PrepareAllFilesInDirectory(_workingDirectory);

            if (DoesKdenliveFileExist(_incomingDirectory))
            {
                throw new KdenliveFileExistsException();
            }

            // if (video.ArchiveFileName.ToLower().StartsWith(END_SCREEN))
            // {
            //     await CreateEndScreenVideoAsync(_workingDirectory, cancellationToken);
            //     _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
            //     _fileSystemService.DeleteDirectory(_workingDirectory);
            //     return;
            // }

            await ConvertVideoAudioFilesToAudioOnly(_workingDirectory, cancellationToken);

            await MergeVideoAndAudioFilesAsync(cancellationToken);

            CreateFfmpegInputFile(video);

            video.AddDrawTextVideoFilter(
                RandomChannelBrandingText(video.BrandingTextOptions()),
                video.DrawTextFilterTextColor(),
                Opacity.Full,
                FfmpegFontSize.Large,
                DrawTextPosition.UpperRight,
                video.DrawTextFilterBackgroundColor(),
                Opacity.Medium);

            if (_fileSystemService.GetFilesInDirectory(_workingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString())).Any())
            {
                video.AddSubtitleVideoFilter(
                    // _fileSystemService.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss)).Single());
                    _fileSystemService.GetFilesInDirectory(_workingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString())).Single(),
                    "&H00006400",
                    "&H00FFFFFF");
            }

            await _ffmpegService.RenderVideoAsync(
                video.FfmpegInputFilePath(), video.VideoFilter, video.OutputVideoFileName(), cancellationToken);

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

    private void CreateFfmpegInputFile(HandymanVideo handymanVideo)
    {
        _fileSystemService.DeleteFile(handymanVideo.FfmpegInputFilePath());

        var videoFiles = _fileSystemService.GetFilesInDirectory(_workingDirectory)
            .Where(f => f.EndsWith(FileExtension.Mp4.ToString()) || f.EndsWith(FileExtension.Mkv.ToString()))
            .OrderBy(f => f)
            // .ToArray();
            .ToList();

        // videoFiles.Add(handymanVideo.EndScreenFilePath());
        base.CreateFfmpegInputFile(videoFiles.ToArray(), handymanVideo.FfmpegInputFilePath());
    }

    private async Task MergeVideoAndAudioFilesAsync(CancellationToken cancellationToken)
    {
        var workingDirVideos = _fileSystemService.GetFilesInDirectory(_workingDirectory)
                    .Where(f => f.EndsWith(FileExtension.Mp4.ToString()) || f.EndsWith(FileExtension.Mkv.ToString()));

        foreach (var videoFilePath in workingDirVideos)
        {
            var result = await _ffmpegService.FfprobeAsync($"\"{videoFilePath}\"", _workingDirectory, cancellationToken);

            if (result.stdErr.ToLower().Contains("audio"))
            {
                continue;
            }

            string? audioFilePath = _fileSystemService.GetFilesInDirectory(_workingDirectory)
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
            // string filePath = _fileSystemService.GetRandomSrtFileFromDirectory(IncomingDirectory);
            string filePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(_incomingDirectory, FileExtension.Srt);

            HandymanSrtSubtitleFile subtitle = new(filePath);

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
            await base.CreateTarballsFromDirectoriesAsync(_incomingDirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }
}