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
                subtitle.FilePath, Path.Combine(ArchiveDirectory, subtitle.FileName));
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
                .Select(f => new HandymanVideoFile(f));

            foreach (var video in mp4MkvVideoFiles)
            {
                var result = await _ffmpegService.FfprobeAsync($"\"{video.FilePath}\"", IncomingWorkDirectory, cancellationToken);

                if (result.stdErr.ToLower().Contains("audio"))
                {
                    await _ffmpegService.ConvertVideoFileToTsFormatAsync(
                        video.FilePath,
                        video.FilePath.Replace(FileExtension.Mp4.Value, FileExtension.Ts.Value).Replace(FileExtension.Mkv.Value, FileExtension.Ts.Value),
                        cancellationToken);
                    continue;
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
                    .OrderBy(f => f);

                ffmpegInputFilePath = Path.Combine(IncomingWorkDirectory, "videos" + FileExtension.FfmpegInput.Value);
                CreateFfmpegInputFile(videoFiles.ToArray(), ffmpegInputFilePath);
            }

            string outputVideoFilePath = Path.Combine(IncomingWorkDirectory, tarball.VideoFileName);
            // string outputAudioFilePath = Path.Combine(IncomingWorkDirectory, tarball.AudioFileName);

            await _ffmpegService.ConcatTsFilesToMp4FileAsync(
                ffmpegInputFilePath, outputVideoFilePath, cancellationToken);

            // await _ffmpegService.ConvertVideoFileToMp3FileAsync(
            //     outputVideoFilePath, outputAudioFilePath, IncomingWorkDirectory, cancellationToken);

            _fileSystemService.MoveFile(outputVideoFilePath, Path.Combine(ReviewingDirectory, tarball.VideoFileName));
            // _fileSystemService.MoveFile(outputAudioFilePath, Path.Combine(ReviewingDirectory, tarball.AudioFileName));

            var graphicsFile = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                .Where(f => f.ToLower().EndsWith(FileExtension.GraphicsAss.Value))
                .SingleOrDefault();

            if (graphicsFile != null)
            {
                _fileSystemService.MoveFile(
                    graphicsFile, Path.Combine(ReviewingDirectory, tarball.GraphicsFileName));
            }

            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);
            _fileSystemService.MoveFile(tarball.FilePath, Path.Combine(ArchiveDirectory, tarball.FileName));

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

    public override async Task ProcessReviewedFilesAsync(CancellationToken cancellationToken)
    {
        // AssSubtitleFile? subtitleFile = null;
        AudioFile? audio = null;

        try
        {
            audio = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
                .Where(f => f.ToLower().EndsWith(FileExtension.Mp3.Value))
                .Select(f => new AudioFile(f))
                .Take(1)
                .Single();

            _loggerService.LogInformation($"Processing {audio.FilePath}");

            // string subtitleFilePath =
            //     _fileSystemService.GetRandomFileByExtensionFromDirectory(ReviewingDirectory, FileExtension.GraphicsAss);

            // subtitleFile = new AssSubtitleFile(subtitleFilePath);

            // _loggerService.LogInformation($"Processing {subtitleFilePath}");

            HandymanVideoFile video = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
                .Where(f => f.StartsWith(audio.FilePath.Replace(FileExtension.Mp3.Value, string.Empty)) && f.ToLower().EndsWith(FileExtension.Mp4.Value))
                // .Where(f => f.StartsWith(subtitleFile.FilePath.Replace(FileExtension.GraphicsAss.Value, string.Empty)) && f.ToLower().EndsWith(FileExtension.Mp4.Value))
                .Select(f => new HandymanVideoFile(f))
                .Single();

            video.SetAudioFile(audio);

            // AudioFile audioFile = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
            //     .Where(f => f.StartsWith(subtitleFile.FilePath.Replace(FileExtension.GraphicsAss.Value, string.Empty)) && f.ToLower().EndsWith(FileExtension.Mp3.Value))
            //     .Select(f => new AudioFile(f))
            //     .Single();

            // video.SetGraphicsSubtitleFile(subtitleFilePath);
            // video.GraphicsSubtitleFile.SetSubtitles(_assSubtitleFileService.ReadFile(video.GraphicsSubtitleFile.FilePath));
            // video.SetAudioFile(audioFile);

            AssSubtitleFile? graphicsSubtitle = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
                .Where(f => f.StartsWith(audio.FilePath.Replace(FileExtension.Mp3.Value, string.Empty)) && f.ToLower().EndsWith(FileExtension.GraphicsAss.Value))
                .Select(f => new AssSubtitleFile(f))
                .SingleOrDefault();

            if (graphicsSubtitle != null)
            {
                video.SetGraphicsSubtitleFile(graphicsSubtitle.FilePath);
                video.GraphicsSubtitleFile.SetSubtitles(_assSubtitleFileService.ReadFile(video.GraphicsSubtitleFile.FilePath));
            }

            _fileSystemService.DeleteDirectory(ReviewWorkDirectory);
            _fileSystemService.CreateDirectory(ReviewWorkDirectory);

            video.SetBrandingText(RandomChannelBrandingText(video.BrandingTextOptions()));

            /// render video
            string outputFilePath = Path.Combine(ReviewWorkDirectory, video.FileName);

            await _ffmpegService.RenderVideoWithAudioAndFiltersAsync(
                video.FilePath, video.AudioFile.FilePath, video.VideoFilters(), outputFilePath, cancellationToken);

            _fileSystemService.MoveFile(
                video.GraphicsSubtitleFile.FilePath,
                Path.Combine(ArchiveDirectory, video.GraphicsSubtitleFile.FileName));
            _fileSystemService.MoveFile(outputFilePath, Path.Combine(UploadingDirectory, video.FileName));
            _fileSystemService.DeleteDirectory(ReviewWorkDirectory);
            _fileSystemService.DeleteFile(video.FilePath);
            _fileSystemService.DeleteFile(video.AudioFile.FilePath);

            // _loggerService.LogInformation($"Completed processing {subtitleFilePath}");
            _loggerService.LogInformation($"Completed processing {audio.FilePath}");
        }
        catch (NoFilesMatchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            // _fileSystemService.MoveFile(subtitleFile.FilePath, subtitleFile.FilePath + FileExtension.Err.Value);
            _fileSystemService.MoveFile(audio.FilePath, audio.FilePath + FileExtension.Err.Value);
        }
    }
}