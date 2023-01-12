using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Videos.Handyman;

public sealed class HandymanVideoService : BaseVideoService, IHandymanVideoService
{
    private readonly AppSettings _appSettings;

    public string IncomingDirectory { get; }
    public string ArchiveDirectory { get; }
    public string ErrorDirectory { get; }
    public string UploadDirectory { get; }
    public string WorkingDirectory { get; }

    private readonly IFileSystemService _fileSystem;
    private readonly ITarballService _tarball;
    private readonly IRandomService _random;
    private readonly IFfmpegService _ffmpeg;
    private readonly IMusicService _musicService;

    public HandymanVideoService(AppSettings appSettings, IFileSystemService fileSystem,
        ITarballService tarball, IRandomService random, IFfmpegService ffmpeg, IMusicService music)
    {
        _appSettings = appSettings;
        IncomingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Incoming);
        ArchiveDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Archive);
        ErrorDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Error);
        UploadDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Upload);
        WorkingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Working);
        _fileSystem = fileSystem;
        _tarball = tarball;
        _random = random;
        _ffmpeg = ffmpeg;
        _musicService = music;
    }

    public override async Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken)
    {
        string incomingTarball = _fileSystem.GetRandomTarballFromDirectory(IncomingDirectory);

        HandymanVideo handymanVideo = new HandymanVideo(
            _appSettings.DashCamDirectory, Path.GetFileName(incomingTarball));

        _fileSystem.DeleteDirectory(WorkingDirectory);

        await _tarball.ExtractTarballContentsAsync(
            handymanVideo.IncomingTarballFilePath(), WorkingDirectory, cancellationToken);

        _fileSystem.PrepareAllFilesInDirectory(WorkingDirectory);

        await ConvertVideoAudioFilesToAudioOnly(cancellationToken);

        await MergeVideoAndAudioFiles(cancellationToken);

        CreateFfmpegInputFile(handymanVideo);

        handymanVideo.AddDrawTextVideoFilter(
            GetChannelBrandingText(handymanVideo.BrandingTextOptions()),
            handymanVideo.DrawTextFilterTextColor(),
            Opacity.Full,
            FfmpegFontSize.Large,
            DrawTextPosition.UpperRight,
            handymanVideo.DrawTextFilterBackgroundColor(),
            Opacity.Light);

        if (_fileSystem.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss)).Any())
        {
            handymanVideo.AddSubtitleVideoFilter(
                _fileSystem.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss)).Single());
        }

        await _ffmpeg.RenderVideoAsync(
            handymanVideo.FfmpegInputFilePath(), handymanVideo.VideoFilter, handymanVideo.OutputFileName(), cancellationToken);

        _fileSystem.MoveFile(handymanVideo.IncomingTarballFilePath(), handymanVideo.ArchiveTarballFilePath());
        _fileSystem.DeleteDirectory(WorkingDirectory);
    }

    private void CreateFfmpegInputFile(HandymanVideo handymanVideo)
    {
        _fileSystem.DeleteFile(handymanVideo.FfmpegInputFilePath());
        string[] videoFiles = _fileSystem.GetFilesInDirectory(WorkingDirectory)
            .Where(f => f.EndsWith(FileExtension.Mp4) || f.EndsWith(FileExtension.Mkv))
            .OrderBy(f => f)
            .ToArray();
        string ffmpegInput = FfmpegInputFileText(videoFiles, handymanVideo.FfmpegInputFilePath());

        _fileSystem.SaveFileContents(handymanVideo.FfmpegInputFilePath(), ffmpegInput);
    }

    private async Task MergeVideoAndAudioFiles(CancellationToken cancellationToken)
    {
        var workingDirVideos = _fileSystem.GetFilesInDirectory(WorkingDirectory)
                    .Where(f => f.EndsWith(FileExtension.Mp4) || f.EndsWith(FileExtension.Mkv));

        foreach (var videoFilePath in workingDirVideos)
        {
            var result = await _ffmpeg.FfprobeAsync($"\"{videoFilePath}\"", WorkingDirectory, cancellationToken);

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

            string? audioFilePath = _fileSystem.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3))
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(audioFilePath))
            {
                audioFilePath = _musicService.GetRandomNonMixTrack();
            }

            string tempOutputFileName = Path.GetFileNameWithoutExtension(videoFilePath) + FileExtension.TmpMp4;

            await _ffmpeg.AddAudioToVideoAsync(
                videoFilePath, audioFilePath, tempOutputFileName, cancellationToken);

            _fileSystem.DeleteFile(videoFilePath);
            _fileSystem.MoveFile(tempOutputFileName, videoFilePath);
        }
    }

    private async Task ConvertVideoAudioFilesToAudioOnly(CancellationToken cancellationToken)
    {
        var audioAsVideoFiles = _fileSystem.GetFilesInDirectory(WorkingDirectory)
            .Where(f => f.EndsWith(FileExtension.AudioMkv) || f.EndsWith(FileExtension.AudioMp4));

        foreach (var file in audioAsVideoFiles)
        {
            string outputFilePath = Path.Combine(WorkingDirectory,
                file.Replace(FileExtension.AudioMkv, FileExtension.Mp3)
                    .Replace(FileExtension.AudioMp4, FileExtension.Mp3));

            await _ffmpeg.ConvertVideoToMp3AudioAsync(
                file, outputFilePath, WorkingDirectory, cancellationToken);
        }
    }

    internal override string GetChannelBrandingText(string[] options)
    {
        return options.ElementAt(_random.Next(0, options.Count()));
    }
}