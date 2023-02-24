using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music;
using Almostengr.VideoProcessor.Core.Music.Services;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed class TechTalkVideoService : BaseVideoService, ITechTalkVideoService
{
    private readonly ILoggerService<TechTalkVideoService> _loggerService;
    private readonly ISrtSubtitleFileService _srtService;
    private readonly IGzFileCompressionService _gzFileService;
    private readonly IXzFileCompressionService _xzFileService;
    private readonly IThumbnailService _thumbnailService;

    public TechTalkVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<TechTalkVideoService> loggerService, IMusicService musicService,
        ISrtSubtitleFileService srtSubtitleFileService, IAssSubtitleFileService assSubtitleFileService,
        IXzFileCompressionService xzFileService, IGzFileCompressionService gzFileService,
        IThumbnailService thumbnailService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService, assSubtitleFileService)
    {
        IncomingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Incoming);
        IncomingWorkDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.IncomingWork);
        ArchiveDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Archive);
        // ReviewingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Reviewing);
        // ReviewWorkDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.ReviewWork);
        UploadingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Uploading);
        _loggerService = loggerService;
        _srtService = srtSubtitleFileService;
        _xzFileService = xzFileService;
        _gzFileService = gzFileService;
        _thumbnailService = thumbnailService;

        _fileSystemService.CreateDirectory(IncomingDirectory);
        _fileSystemService.CreateDirectory(UploadingDirectory);
        _fileSystemService.CreateDirectory(ArchiveDirectory);
        // _fileSystemService.CreateDirectory(ReviewingDirectory);
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

    // public override Task ProcessIncomingSubtitlesAsync(CancellationToken cancellationToken)
    // {
    //     try
    //     {
    //         string filePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(IncomingDirectory, FileExtension.Srt);

    //         TechTalkSrtSubtitleFile subtitle = new(filePath);

    //         var subtitles = _srtService.ReadFile(subtitle.FilePath);
    //         subtitle.SetSubtitles(subtitles);

    //         _srtService.WriteFile(Path.Combine(UploadingDirectory, subtitle.FileName), subtitle.Subtitles);

    //         _fileSystemService.SaveFileContents(
    //             Path.Combine(UploadingDirectory, subtitle.BlogFileName()), subtitle.BlogPostText());

    //         _fileSystemService.MoveFile(
    //             subtitle.FilePath, Path.Combine(ArchiveDirectory, subtitle.FileName));
    //     }
    //     catch (NoFilesMatchException)
    //     { }
    //     catch (Exception ex)
    //     {
    //         _loggerService.LogError(ex, ex.Message);
    //     }

    //     return Task.CompletedTask;
    // }

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

    public async Task NewMethod(CancellationToken cancellationToken)
    {

        TechTalkVideoProjectArchive? archive = null;

        try
        {
            string selectedTarballFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.Tar);

            archive = new TechTalkVideoProjectArchive(selectedTarballFilePath);

            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);
            _fileSystemService.CreateDirectory(IncomingWorkDirectory);

            await _tarballService.ExtractTarballContentsAsync(archive.FilePath, IncomingWorkDirectory, cancellationToken);

            StopProcessingIfKdenliveFileExists(IncomingWorkDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(IncomingWorkDirectory);

            _fileSystemService.PrepareAllFilesInDirectory(IncomingWorkDirectory);

            // todo normailze audio files

            // add audio to video files with dedicated audio

            var audioFiles = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp3.Value))
                .Select(f => new AudioFile(f))
                .ToList();

            foreach (var audioFile in audioFiles)
            {
                var videoFilePath = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                    .Where(f => f.StartsWith(audioFile.FileName.Replace(FileExtension.Mp3.Value, string.Empty)))
                    // .Select(f => new TechTalkVideoFile(f))
                    .Single();

                // video.SetAudioFile(audioFile);

                string tsOutputFilePath = Path.Combine(
                    IncomingWorkDirectory,
                    // Path.GetFileNameWithoutExtension(video.FileName) + FileExtension.Ts.Value);
                    Path.GetFileNameWithoutExtension(videoFilePath) + FileExtension.Ts.Value);

                // await _ffmpegService.AddAccAudioToVideoAsync(
                //     videoFilePath.FilePath, videoFilePath.AudioFile.FilePath, tsOutputFilePath, cancellationToken);

                // _fileSystemService.DeleteFile(videoFilePath.FilePath);

                await _ffmpegService.AddAccAudioToVideoAsync(
                    videoFilePath, audioFile.FilePath, tsOutputFilePath, cancellationToken);

                _fileSystemService.DeleteFile(videoFilePath);
            }

        }
        catch (NoFilesMatchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);

            if (archive != null)
            {
                _fileSystemService.MoveFile(archive.FilePath, archive.FilePath + FileExtension.Err.Value);
            }

            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);
        }
    }

    public override async Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken)
    {
        TechTalkVideoProjectArchive? archive = null;

        try
        {
            string selectedTarballFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.Tar);

            archive = new TechTalkVideoProjectArchive(selectedTarballFilePath);

            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);
            _fileSystemService.CreateDirectory(IncomingWorkDirectory);

            await _tarballService.ExtractTarballContentsAsync(archive.FilePath, IncomingWorkDirectory, cancellationToken);

            StopProcessingIfKdenliveFileExists(IncomingWorkDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(IncomingWorkDirectory);

            _fileSystemService.PrepareAllFilesInDirectory(IncomingWorkDirectory);

            // todo normailze audio files

            // add audio to video files with dedicated audio

            var audioFiles = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp3.Value))
                .Select(f => new AudioFile(f))
                .ToList();

            foreach (var audioFile in audioFiles)
            {
                var video = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                    .Where(f => f.StartsWith(audioFile.FileName.Replace(FileExtension.Mp3.Value, string.Empty)))
                    .Select(f => new TechTalkVideoFile(f))
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
                .Select(f => new TechTalkVideoFile(f));

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

            string outputVideoFilePath = Path.Combine(IncomingWorkDirectory, archive.FileName());
            // string outputVideoFilePath = Path.Combine(IncomingWorkDirectory, archive.VideoFileName);

            await _ffmpegService.ConcatTsFilesToMp4FileAsync(
                ffmpegInputFilePath, outputVideoFilePath, cancellationToken);

            _fileSystemService.MoveFile(outputVideoFilePath, Path.Combine(ReviewingDirectory, archive.FileName()));
            // _fileSystemService.MoveFile(outputVideoFilePath, Path.Combine(ReviewingDirectory, archive.VideoFileName));

            // var graphicsFile = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
            //     .Where(f => f.ToLower().EndsWith(FileExtension.GraphicsAss.Value))
            //     .SingleOrDefault();

            // if (graphicsFile != null)
            // {
            //     _fileSystemService.MoveFile(
            //         graphicsFile, Path.Combine(ReviewingDirectory, archive.GraphicsFileName));
            // }

            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);
            // _fileSystemService.MoveFile(archive.FilePath, Path.Combine(ArchiveDirectory, archive.FileName));
            _fileSystemService.MoveFile(archive.FilePath, Path.Combine(ArchiveDirectory, archive.FileName()));

            _loggerService.LogInformation($"Completed processing {selectedTarballFilePath}");
        }
        catch (NoFilesMatchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);

            if (archive != null)
            {
                _fileSystemService.MoveFile(archive.FilePath, archive.FilePath + FileExtension.Err.Value);
            }

            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);
        }
    }

    // public override async Task ProcessReviewedFilesAsync(CancellationToken cancellationToken)
    // {
    //     AudioFile? audio = null;

    //     try
    //     {
    //         string audioFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(ReviewingDirectory, FileExtension.Mp3);
    //         audio = new AudioFile(audioFilePath);

    //         _loggerService.LogInformation($"Processing {audio.FilePath}");

    //         TechTalkVideoFile video = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
    //             .Where(f => f.StartsWith(audio.FilePath.Replace(FileExtension.Mp3.Value, string.Empty)) && f.ToLower().EndsWith(FileExtension.Mp4.Value))
    //             .Select(f => new TechTalkVideoFile(f))
    //             .Single();

    //         video.SetAudioFile(audio);

    //         AssSubtitleFile? graphicsSubtitle = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
    //             .Where(f => f.StartsWith(audio.FilePath.Replace(FileExtension.Mp3.Value, string.Empty)) && f.ToLower().EndsWith(FileExtension.GraphicsAss.Value))
    //             .Select(f => new AssSubtitleFile(f))
    //             .SingleOrDefault();

    //         if (graphicsSubtitle != null)
    //         {
    //             video.SetGraphicsSubtitleFile(graphicsSubtitle.FilePath);
    //             video.GraphicsSubtitleFile.SetSubtitles(_assSubtitleFileService.ReadFile(video.GraphicsSubtitleFile.FilePath));
    //         }

    //         _fileSystemService.DeleteDirectory(ReviewWorkDirectory);
    //         _fileSystemService.CreateDirectory(ReviewWorkDirectory);

    //         video.SetBrandingText(RandomChannelBrandingText(video.BrandingTextOptions()));

    //         /// render video
    //         string outputFilePath = Path.Combine(ReviewWorkDirectory, video.FileName);

    //         await _ffmpegService.RenderVideoWithAudioAndFiltersAsync(
    //             video.FilePath, video.AudioFile.FilePath, video.VideoFilters(), outputFilePath, cancellationToken);

    //         _thumbnailService.GenerateThumbnail(
    //             ThumbnailType.TechTalk, UploadingDirectory, video.ThumbnailFileName, video.Title);

    //         _fileSystemService.MoveFile(
    //             video.GraphicsSubtitleFile.FilePath,
    //             Path.Combine(ArchiveDirectory, video.GraphicsSubtitleFile.FileName));
    //         _fileSystemService.MoveFile(outputFilePath, Path.Combine(UploadingDirectory, video.FileName));
    //         _fileSystemService.DeleteDirectory(ReviewWorkDirectory);
    //         _fileSystemService.DeleteFile(video.FilePath);
    //         _fileSystemService.DeleteFile(video.AudioFile.FilePath);

    //         _loggerService.LogInformation($"Completed processing {audio.FilePath}");
    //     }
    //     catch (NoFilesMatchException)
    //     {
    //         throw;
    //     }
    //     catch (Exception ex)
    //     {
    //         _loggerService.LogError(ex, ex.Message);

    //         if (audio != null)
    //         {
    //             _fileSystemService.MoveFile(audio.FilePath, audio.FilePath + FileExtension.Err.Value);
    //         }
    //     }
    // }

    public void CreateThumbnails()
    {
        try
        {
            var thumbnailFiles = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
                .Where(f => f.ToLower().EndsWith(FileExtension.ThumbTxt.Value));

            if (thumbnailFiles.Count() == 0)
            {
                return;
            }

            foreach (var thumbnailFile in thumbnailFiles)
            {
                _thumbnailService.GenerateThumbnail(
                    ThumbnailType.TechTalk,
                    UploadingDirectory,
                    Path.GetFileNameWithoutExtension(thumbnailFile) + FileExtension.Jpg.Value,
                    Path.GetFileNameWithoutExtension(thumbnailFile));

                _fileSystemService.MoveFile(
                    thumbnailFile,
                    Path.Combine(ArchiveDirectory, Path.GetFileName(thumbnailFile)));
            }
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }
}