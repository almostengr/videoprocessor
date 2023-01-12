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
    // private readonly AppSettings _appSettings;

    public string IncomingDirectory { get; }
    public string ArchiveDirectory { get; }
    public string ErrorDirectory { get; }
    public string UploadDirectory { get; }
    public string WorkingDirectory { get; }

    private readonly ILoggerService<HandymanVideoService> _loggerService;

    // private readonly IFileSystemService _fileSystemService;
    // private readonly ITarballService _tarballService;
    // private readonly IRandomService _randomService;
    // private readonly IFfmpegService _ffmpegService;
    // private readonly IMusicService _musicService;

    // public HandymanVideoService(AppSettings appSettings, IFileSystemService fileSystem,
    //     ITarballService tarball, IRandomService random, IFfmpegService ffmpeg, IMusicService music)
    // {
    //     _appSettings = appSettings;
    //     IncomingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Incoming);
    //     ArchiveDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Archive);
    //     ErrorDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Error);
    //     UploadDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Upload);
    //     WorkingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Working);
    //     _fileSystemService = fileSystem;
    //     _tarballService = tarball;
    //     _randomService = random;
    //     _ffmpegService = ffmpeg;
    //     _musicService = music;
    // }

    public HandymanVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IGzipService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
         IMusicService musicService, ILoggerService<HandymanVideoService> loggerService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService)
    {
        IncomingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Incoming);
        ArchiveDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Archive);
        ErrorDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Error);
        UploadDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Upload);
        WorkingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Working);
        _loggerService = loggerService;
    }

    public override async Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken)
    {
        await base.CompressTarballsInArchiveFolderAsync(ArchiveDirectory, cancellationToken);
    }

    public override async Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken)
    {
        HandymanVideo? video = null;
        try
        {
            string incomingTarball = _fileSystemService.GetRandomTarballFromDirectory(IncomingDirectory);

            video = new HandymanVideo(
                _appSettings.DashCamDirectory, Path.GetFileName(incomingTarball));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                video.IncomingTarballFilePath(), WorkingDirectory, cancellationToken);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            await ConvertVideoAudioFilesToAudioOnly(WorkingDirectory, cancellationToken);

            await MergeVideoAndAudioFiles(cancellationToken);

            CreateFfmpegInputFile(video);

            video.AddDrawTextVideoFilter(
                GetChannelBrandingText(video.BrandingTextOptions()),
                video.DrawTextFilterTextColor(),
                Opacity.Full,
                FfmpegFontSize.Large,
                DrawTextPosition.UpperRight,
                video.DrawTextFilterBackgroundColor(),
                Opacity.Light);

            if (_fileSystemService.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss)).Any())
            {
                video.AddSubtitleVideoFilter(
                    _fileSystemService.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss)).Single());
            }

            await _ffmpegService.RenderVideoAsync(
                video.FfmpegInputFilePath(), video.VideoFilter, video.OutputFileName(), cancellationToken);

            _fileSystemService.MoveFile(video.IncomingTarballFilePath(), video.ArchiveTarballFilePath());
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
        catch (NoTarballsPresentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);

            if (video != null)
            {
                _fileSystemService.MoveFile(video.IncomingTarballFilePath(), Path.Combine(ErrorDirectory, video.ArchiveFileName));
                _fileSystemService.SaveFileContents(
                    Path.Combine(ErrorDirectory, video.ArchiveFileName + FileExtension.Log),
                    ex.Message);
            }
        }
    }

    private void CreateFfmpegInputFile(HandymanVideo handymanVideo)
    {
        _fileSystemService.DeleteFile(handymanVideo.FfmpegInputFilePath());
        string[] videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
            .Where(f => f.EndsWith(FileExtension.Mp4) || f.EndsWith(FileExtension.Mkv))
            .OrderBy(f => f)
            .ToArray();
        string ffmpegInput = FfmpegInputFileText(videoFiles, handymanVideo.FfmpegInputFilePath());

        _fileSystemService.SaveFileContents(handymanVideo.FfmpegInputFilePath(), ffmpegInput);
    }

    private async Task MergeVideoAndAudioFiles(CancellationToken cancellationToken)
    {
        var workingDirVideos = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                    .Where(f => f.EndsWith(FileExtension.Mp4) || f.EndsWith(FileExtension.Mkv));

        foreach (var videoFilePath in workingDirVideos)
        {
            var result = await _ffmpegService.FfprobeAsync($"\"{videoFilePath}\"", WorkingDirectory, cancellationToken);

            if (result.stdErr.ToLower().Contains("audio"))
            {
                continue;
            }

            // string audioFileName = 
            // workingDirVideos
            //     .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)))
            //     .Single() 
            // string audioFilePath = videoFilePath
            //     .Replace(FileExtension.Mp4, string.Empty)
            //     .Replace(FileExtension.Mkv, string.Empty)
            //     + FileExtension.Mp3;

            string? audioFilePath = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3))
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

    internal override string GetChannelBrandingText(string[] options)
    {
        return options.ElementAt(_randomService.Next(0, options.Count()));
    }

    public override Task ProcessIncomingSubtitlesAsync(CancellationToken cancellationToken)
    {
        try
        {
            string filePath = _fileSystemService.GetRandomSrtFileFromDirectory(IncomingDirectory);

            HandymanSrtSubtitle subtitle = new(_appSettings.TechnologyDirectory, filePath);

            subtitle.SetSubtitleText(_fileSystemService.GetFileContents(subtitle.IncomingFilePath()));

            _fileSystemService.SaveFileContents(subtitle.SubtitleOutputFilePath(), subtitle.SubtitleText());
            _fileSystemService.SaveFileContents(subtitle.BlogOutputFilePath(), subtitle.BlogPostText());
            _fileSystemService.MoveFile(subtitle.IncomingFilePath(), subtitle.ArchiveFilePath());
        }
        catch (NoSubtitleFilesPresentException)
        { }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }

        return Task.CompletedTask;
    }

    public override async Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken)
    {
        await base.CreateTarballsFromDirectoriesAsync(IncomingDirectory, cancellationToken);
    }
}