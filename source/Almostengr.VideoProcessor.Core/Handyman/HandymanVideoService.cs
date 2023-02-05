using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music;
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
        ReviewingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Reviewing);
        ReviewWorkDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.ReviewWork);
        UploadingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Uploading);
        _loggerService = loggerService;
        _srtService = srtSubtitleFileService;
    }

    public async Task ConvertGzToXzAsync(CancellationToken cancellationToken)
    {
        var tarGzFiles = _fileSystemService.GetFilesInDirectory(ArchiveDirectory)
            .Where(f => f.EndsWith(FileExtension.TarGz.Value));

        foreach (var file in tarGzFiles)
        {
            await _compressionService.DecompressFileAsync(file, cancellationToken);

            await _compressionService.CompressFileAsync(
                file.Replace(FileExtension.TarGz.Value, FileExtension.Tar.Value), cancellationToken);
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

    private async Task AddMusicToTimelapseVideoAsync(CancellationToken cancellationToken)
    {
        var workingDirVideos = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
            .Where(f => f.ToLower().EndsWith(FileExtension.Mp4.Value));

        foreach (var videoFilePath in workingDirVideos)
        {
            HandymanVideoFile video = new HandymanVideoFile(videoFilePath);

            var result = await _ffmpegService.FfprobeAsync($"\"{video.FilePath}\"", IncomingWorkDirectory, cancellationToken);

            if (result.stdErr.ToLower().Contains("audio"))
            {
                continue;
            }

            string audioFilePath =
                _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                    .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3.Value))
                    .SingleOrDefault() ?? _musicService.GetRandomNonMixTrack();
            video.SetAudioFile(audioFilePath);

            string tempOutputFileName = Path.GetFileNameWithoutExtension(video.FilePath) + FileExtension.TmpMp4.Value;

            await _ffmpegService.AddAudioToVideoAsync(
                video.FilePath, video.AudioFile.FilePath, tempOutputFileName, cancellationToken);

            _fileSystemService.DeleteFile(video.FilePath);
            _fileSystemService.MoveFile(tempOutputFileName, video.FilePath);
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

            _srtService.WriteFile(Path.Combine(UploadingDirectory, subtitle.FileName), subtitle.Subtitles);

            _fileSystemService.SaveFileContents(
                Path.Combine(UploadingDirectory, subtitle.BlogFileName()), subtitle.BlogPostText());

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

    public override async Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken)
    {
        HandymanIncomingTarballFile? tarball = null;

        try
        {
            string selectedTarballFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.Tar);

            tarball = new HandymanIncomingTarballFile(selectedTarballFilePath);

            _loggerService.LogInformation($"Processing {selectedTarballFilePath}");

            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);
            _fileSystemService.CreateDirectory(IncomingWorkDirectory);

            await _tarballService.ExtractTarballContentsAsync(tarball.FilePath, IncomingWorkDirectory, cancellationToken);

            StopProcessingIfKdenliveFileExists(IncomingWorkDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(IncomingWorkDirectory);

            _fileSystemService.PrepareAllFilesInDirectory(IncomingWorkDirectory);

            // add audio to video files with dedicated audio

            var audioFiles = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp3.Value))
                .Select(f => new AudioFile(f))
                .ToList();

            foreach (var audioFile in audioFiles)
            {
                var video = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                    .Where(f => f.StartsWith(audioFile.FileName.Replace(FileExtension.Mp3.Value, string.Empty)))
                    .Select(f => new HandymanVideoFile(f))
                    .Single();

                video.SetAudioFile(audioFile);

                string tsOutputFilePath = Path.Combine(
                    IncomingWorkDirectory,
                    Path.GetFileNameWithoutExtension(video.FileName) + FileExtension.Ts.Value);

                await _ffmpegService.AddAccAudioToVideoAsync(
                    video.FilePath, video.AudioFile.FilePath, tsOutputFilePath, cancellationToken);

                _fileSystemService.DeleteFile(video.FilePath);
            }

            // add music to video files that have no audio

            var mp4MkvVideoFiles = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4.Value) || f.EndsWith(FileExtension.Mkv.Value))
                .Select(f => new HandymanVideoFile(f))
                .ToList();

            foreach (var video in mp4MkvVideoFiles)
            {
                var result = await _ffmpegService.FfprobeAsync($"\"{video.FilePath}\"", IncomingWorkDirectory, cancellationToken);

                if (result.stdErr.ToLower().Contains("audio"))
                {
                    await _ffmpegService.ConvertVideoFileToTsFormatAsync(
                        video.FilePath, video.FilePath.Replace(FileExtension.Mp4.Value, FileExtension.Ts.Value), cancellationToken);
                }

                video.SetAudioFile(_musicService.GetRandomMixTrack());

                string tempOutputFileName = Path.GetFileNameWithoutExtension(video.FilePath) + FileExtension.Ts.Value;

                await _ffmpegService.AddAccAudioToVideoAsync(
                    video.FilePath, video.AudioFile.FilePath, tempOutputFileName, cancellationToken);

                _fileSystemService.DeleteFile(video.FilePath);
            }

            string? ffmpegInputFilePath = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                .Where(f => f.EndsWith(FileExtension.FfmpegInput.Value))
                .SingleOrDefault();

            if (string.IsNullOrEmpty(ffmpegInputFilePath))
            {
                var videoFiles = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                    .Where(f => f.EndsWith(FileExtension.Ts.Value))
                    .OrderBy(f => f)
                    .ToList();

                ffmpegInputFilePath = Path.Combine(IncomingWorkDirectory, "videos" + FileExtension.FfmpegInput.Value);
                CreateFfmpegInputFile(videoFiles.ToArray(), ffmpegInputFilePath);
            }

            string outputFilePath = Path.Combine(IncomingWorkDirectory, tarball.VideoFileName);

            await _ffmpegService.ConcatTsFilesToMp4FileAsync(
                ffmpegInputFilePath,
                outputFilePath,
                string.Empty,
                cancellationToken);

            _fileSystemService.MoveFile(tarball.FilePath, Path.Combine(ArchiveDirectory, tarball.FileName));
            _fileSystemService.MoveFile(outputFilePath, Path.Combine(ReviewingDirectory, tarball.VideoFileName));
            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);

            _loggerService.LogInformation($"Completed processing {selectedTarballFilePath}");
        }
        catch (NoFilesMatchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _fileSystemService.MoveFile(tarball.FilePath, tarball.FilePath + FileExtension.Err.Value);
            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);
        }
    }

    public override Task ProcessReviewedFilesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}