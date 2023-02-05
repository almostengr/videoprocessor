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

    public TechTalkVideoService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<TechTalkVideoService> loggerService, IMusicService musicService,
        ISrtSubtitleFileService srtSubtitleFileService, IAssSubtitleFileService assSubtitleFileService,
        IXzFileCompressionService xzFileService, IGzFileCompressionService gzFileService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService, assSubtitleFileService)
    {
        IncomingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Incoming);
        IncomingWorkDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.IncomingWork);
        ArchiveDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Archive);
        ReviewingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Reviewing);
        ReviewWorkDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.ReviewWork);
        UploadingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Uploading);
        // WorkingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Working);
        // DraftDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Draft);
        _loggerService = loggerService;
        _srtService = srtSubtitleFileService;
        // _ffmpegInputFilePath = Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME);
        _xzFileService = xzFileService;
        _gzFileService = gzFileService;
    }

    public async Task ConvertGzToXzAsync(CancellationToken cancellationToken)
    {
        var tarGzFiles = _fileSystemService.GetFilesInDirectory(ArchiveDirectory)
            .Where(f => f.EndsWith(FileExtension.TarGz.ToString()));

        foreach (var file in tarGzFiles)
        {
            await _gzFileService.DecompressFileAsync(file, cancellationToken);

            await _xzFileService.CompressFileAsync(
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

    // public override async Task ProcessIncomingTarballFilesAsyncOld(CancellationToken cancellationToken)
    // {
    //     TechTalkVideoFile? video = null;

    //     try
    //     {
    //         string incomingTarball = _fileSystemService.GetRandomFileByExtensionFromDirectory(
    //             IncomingDirectory, FileExtension.Tar);
    //         _loggerService.LogInformation($"Processing ${incomingTarball}");

    //         video = new TechTalkVideoFile(incomingTarball);

    //         _fileSystemService.DeleteDirectory(IncomingWorkDirectory);
    //         _fileSystemService.CreateDirectory(IncomingWorkDirectory);

    //         await _tarballService.ExtractTarballContentsAsync(
    //             video.TarballFilePath, WorkingDirectory, cancellationToken);

    //         _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

    //         StopProcessingIfKdenliveFileExists(WorkingDirectory);

    //         // if (DoesKdenliveFileExist(IncomingDirectory))
    //         // {
    //         //     throw new KdenliveFileExistsException("Archive has Kdenlive project file");
    //         // }

    //         _fileSystemService.DeleteFile(Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME));

    //         var videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
    //             .Where(f => f.EndsWith(FileExtension.Mp4.ToString()) || f.EndsWith(FileExtension.Mkv.ToString()))
    //             .OrderBy(f => f)
    //             .ToList();

    //         await MergeVideoAndAudioFilesAsync(cancellationToken);

    //         foreach (var file in videoFiles)
    //         {
    //             await _ffmpegService.ConvertVideoFileToTsFormatAsync(
    //                 file, file + FileExtension.Ts, cancellationToken);
    //         }

    //         videoFiles.Clear();

    //         videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
    //             .Where(f => f.EndsWith(FileExtension.Ts.ToString()))
    //             .OrderBy(f => f)
    //             .ToList();

    //         CreateFfmpegInputFile(videoFiles.ToArray(), Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME));

    //         if (video.IsDraft)
    //         {
    //             await _ffmpegService.ConcatTsFilesToMp4FileAsync(
    //                 _ffmpegInputFilePath,
    //                 Path.Combine(UploadingDirectory, video.OutputVideoFileName),
    //                 string.Empty,
    //                 cancellationToken);

    //             _fileSystemService.MoveFile(video.TarballFilePath, Path.Combine(ArchiveDirectory, video.TarballFileName));
    //             _fileSystemService.DeleteDirectory(WorkingDirectory);
    //             return;
    //         }

    //         // brand video
    //         // video.AddDrawTextVideoFilter(
    //         //     RandomChannelBrandingText(video.BrandingTextOptions()),
    //         //     video.DrawTextFilterTextColor(),
    //         //     Opacity.Full,
    //         //     FfmpegFontSize.Large,
    //         //     DrawTextPosition.ChannelBrand,
    //         //     video.DrawTextFilterBackgroundColor(),
    //         //     Opacity.Medium,
    //         //     10);

    //         // CheckAndAddGraphicsSubtitle(video);
    //         string? graphicsSubtitleFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
    //             .Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString()))
    //             .SingleOrDefault();

    //         if (!string.IsNullOrEmpty(graphicsSubtitleFile))
    //         {
    //             video.SetGraphicsSubtitleFile(graphicsSubtitleFile);
    //             video.GraphicsSubtitleFile.SetSubtitles(
    //                 _assSubtitleFileService.ReadFile(video.GraphicsSubtitleFile.FilePath));
    //         }

    //         await _ffmpegService.ConcatTsFilesToMp4FileAsync(
    //             _ffmpegInputFilePath,
    //             Path.Combine(UploadingDirectory, video.OutputVideoFileName),
    //             video.VideoFilters(),
    //             cancellationToken);

    //         _fileSystemService.MoveFile(
    //             video.TarballFilePath, Path.Combine(ArchiveDirectory, video.TarballFileName));
    //         _fileSystemService.DeleteDirectory(WorkingDirectory);
    //     }
    //     catch (NoFilesMatchException)
    //     {
    //         throw;
    //     }
    //     catch (Exception ex)
    //     {
    //         _loggerService.LogError(ex, ex.Message);

    //         if (video != null)
    //         {
    //             _fileSystemService.MoveFile(
    //                 video.TarballFilePath,
    //                 Path.Combine(ArchiveDirectory, video.TarballFileName));
    //             _fileSystemService.SaveFileContents(
    //                 Path.Combine(ArchiveDirectory, video.TarballFileName + FileExtension.Log),
    //                 ex.Message);
    //         }
    //     }
    // }

    // private async Task MergeVideoAndAudioFilesAsync(CancellationToken cancellationToken)
    // {
    //     var workingDirVideos = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
    //         .Where(f => f.EndsWith(FileExtension.Mp4.ToString()) || f.EndsWith(FileExtension.Mkv.ToString()));

    //     foreach (var videoFilePath in workingDirVideos)
    //     {
    //         string? audioFilePath = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
    //             .Where(f => f.Contains(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3.ToString()))
    //             .SingleOrDefault();

    //         if (string.IsNullOrWhiteSpace(audioFilePath))
    //         {
    //             continue;
    //         }

    //         string tempOutputFilePath = Path.Combine(WorkingDirectory,
    //             Path.GetFileNameWithoutExtension(videoFilePath) + ".tmp" + Path.GetExtension(videoFilePath));

    //         await _ffmpegService.AddAccAudioToVideoAsync(
    //             videoFilePath, audioFilePath, tempOutputFilePath, cancellationToken);

    //         _fileSystemService.DeleteFile(videoFilePath);
    //         _fileSystemService.MoveFile(tempOutputFilePath, videoFilePath);
    //     }
    // }

    public override Task ProcessIncomingSubtitlesAsync(CancellationToken cancellationToken)
    {
        try
        {
            string filePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(IncomingDirectory, FileExtension.Srt);

            TechTalkSrtSubtitleFile subtitle = new(filePath);

            var subtitles = _srtService.ReadFile(subtitle.FilePath);
            subtitle.SetSubtitles(subtitles);

            _srtService.WriteFile(Path.Combine(UploadingDirectory, subtitle.FileName), subtitle.Subtitles);

            _fileSystemService.SaveFileContents(
                Path.Combine(UploadingDirectory, subtitle.BlogFileName()), subtitle.BlogPostText());

            _fileSystemService.MoveFile(
                subtitle.FilePath, Path.Combine(ArchiveDirectory, subtitle.FileName), false);
        }
        catch (NoFilesMatchException)
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

    public override async Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken)
    {
        TechTalkIncomingTarballFile? tarball = null;

        try
        {
            string selectedTarballFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.Tar);

            tarball = new TechTalkIncomingTarballFile(selectedTarballFilePath);

            _loggerService.LogInformation($"Processing {selectedTarballFilePath}");

            _fileSystemService.DeleteDirectory(IncomingWorkDirectory);
            _fileSystemService.CreateDirectory(IncomingWorkDirectory);

            await _tarballService.ExtractTarballContentsAsync(tarball.FilePath, IncomingWorkDirectory, cancellationToken);

            StopProcessingIfKdenliveFileExists(IncomingWorkDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(IncomingWorkDirectory);

            _fileSystemService.PrepareAllFilesInDirectory(IncomingWorkDirectory);

            // var videoFiles = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
            //     .Where(f => f.EndsWith(FileExtension.Mp4.ToString()) || f.EndsWith(FileExtension.Mkv.ToString()))
            //     .OrderBy(f => f)
            //     .ToList();

            // await MergeVideoAndAudioFilesAsync(cancellationToken);

            // var videoFiles = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
            //     .Where(f => f.EndsWith(FileExtension.Mp4.Value) || f.EndsWith(FileExtension.Mkv.Value))
            //     .Select(f => new TechTalkVideoFile(f));

            // foreach (var videoFile in videoFiles)
            // {
            //     var result = await _ffmpegService.FfprobeAsync($"\"{videoFile.FilePath}\"", IncomingWorkDirectory, cancellationToken);

            //     if (result.stdErr.ToLower().Contains("audio"))
            //     {
            //         continue;
            //     }

            //     AudioFile audioFile = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
            //         .Where(f => f.EndsWith(FileExtension.Mp3.Value) && f.StartsWith(Path.GetFileNameWithoutExtension(videoFile.FileName)))
            //         .Select(f => new AudioFile(f))
            //         .SingleOrDefault() ?? new AudioFile(_musicService.GetRandomMixTrack());

            //     // if (audioFile == null)
            //     // {
            //     //     // audioFile = _musicService.GetRandomMixTrack();
            //     //     audioFile = new AudioFile(_musicService.GetRandomMixTrack());
            //     // }

            //     videoFile.SetAudioFile(audioFile);

            //     string tsFileName =
            //         Path.Combine(ReviewWorkDirectory, Path.GetFileNameWithoutExtension(videoFile.FilePath) + FileExtension.Ts.Value);

            //     await _ffmpegService.AddAudioToVideoAsync(
            //     // videoFilePath, audioFilePath, tempOutputFileName, cancellationToken);
            //         videoFile.FilePath, videoFile.AudioFile.FilePath, tsFileName, cancellationToken);
            // }


            // check video files for audio 

            // if video file does not have audio, then mix track and add to video
            // await AddMusicToTimelapseVideoAsync(cancellationToken);




            // foreach (var file in videoFiles)
            // {
            //     await _ffmpegService.ConvertVideoFileToTsFormatAsync(
            //         file, file + FileExtension.Ts, cancellationToken);
            // }


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
                .Select(f => new TechTalkVideoFile(f))
                .ToList();

            foreach (var video in mp4MkvVideoFiles)
            {
                var result = await _ffmpegService.FfprobeAsync($"\"{video.FilePath}\"", IncomingWorkDirectory, cancellationToken);

                if (result.stdErr.ToLower().Contains("audio"))
                {
                    // continue;
                    await _ffmpegService.ConvertVideoFileToTsFormatAsync(
                        video.FilePath, video.FilePath.Replace(FileExtension.Mp4.Value, FileExtension.Ts.Value), cancellationToken);
                }

                video.SetAudioFile(_musicService.GetRandomMixTrack());

                string tempOutputFileName = Path.GetFileNameWithoutExtension(video.FilePath) + FileExtension.Ts.Value;

                await _ffmpegService.AddAccAudioToVideoAsync(
                    video.FilePath, video.AudioFile.FilePath, tempOutputFileName, cancellationToken);

                _fileSystemService.DeleteFile(video.FilePath);
                // _fileSystemService.MoveFile(tempOutputFileName, video.FilePath);
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


            // await _ffmpegService.RenderVideoAsCopyAsync(
            //     ffmpegInputFilePath, outputFilePath, cancellationToken);

            // await _ffmpegService.ConvertVideoFileToMp3FileAsync(
            //     outputFilePath, outputFilePath.Replace(FileExtension.Mp4.Value, FileExtension.Mp3.Value),
            //     ReviewingDirectory, cancellationToken);

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

    // private async Task AddMusicToTimelapseVideoAsync(CancellationToken cancellationToken)
    // {
    //     var workingDirVideos = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
    //         .Where(f => f.ToLower().EndsWith(FileExtension.Mp4.ToString()));

    //     foreach (var videoFilePath in workingDirVideos)
    //     {
    //         TechTalkVideoFile video = new TechTalkVideoFile(videoFilePath);

    //         var result = await _ffmpegService.FfprobeAsync($"\"{video.FilePath}\"", IncomingWorkDirectory, cancellationToken);

    //         if (result.stdErr.ToLower().Contains("audio"))
    //         {
    //             continue;
    //         }

    //         // string audioFilePath = _fileSystemService.GetFilesInDirectory(AssemblyDirectory)
    //         //     .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3.ToString()))
    //         //     .SingleOrDefault() ?? _musicService.GetRandomNonMixTrack();

    //         string audioFilePath =
    //             _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
    //                 .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3.ToString()))
    //                 .SingleOrDefault() ?? _musicService.GetRandomMixTrack();
    //         video.SetAudioFile(audioFilePath);

    //         // string tempOutputFileName = Path.GetFileNameWithoutExtension(videoFilePath) + FileExtension.TmpMp4;
    //         // string tempOutputFileName = Path.GetFileNameWithoutExtension(video.FilePath) + FileExtension.TmpMp4.Value;
    //         string tempOutputFileName = Path.GetFileNameWithoutExtension(video.FilePath) + FileExtension.Ts.Value;

    //         await _ffmpegService.AddAudioToVideoAsync(
    //             // videoFilePath, audioFilePath, tempOutputFileName, cancellationToken);
    //             video.FilePath, video.AudioFile.FilePath, tempOutputFileName, cancellationToken);

    //         _fileSystemService.DeleteFile(video.FilePath);
    //         // _fileSystemService.MoveFile(tempOutputFileName, videoFilePath);
    //         // _fileSystemService.MoveFile(tempOutputFileName, video.FilePath);
    //     }
    // }

    public override async Task ProcessReviewedFilesAsync(CancellationToken cancellationToken)
    {
        AssSubtitleFile? subtitleFile = null;

        try
        {
            string subtitleFilePath =
                _fileSystemService.GetRandomFileByExtensionFromDirectory(ReviewingDirectory, FileExtension.GraphicsAss);

            subtitleFile = new AssSubtitleFile(subtitleFilePath);

            _loggerService.LogInformation($"Processing {subtitleFilePath}");

            string videoFilePath = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
                .Where(f => f.StartsWith(subtitleFile.FileName) && f.ToLower().EndsWith(FileExtension.Mp4.Value))
                .Single();

            TechTalkVideoFile video = new TechTalkVideoFile(videoFilePath);
            video.SetGraphicsSubtitleFile(subtitleFilePath);
            video.GraphicsSubtitleFile.SetSubtitles(_assSubtitleFileService.ReadFile(video.GraphicsSubtitleFile.FilePath));

            _fileSystemService.DeleteDirectory(ReviewWorkDirectory);
            _fileSystemService.CreateDirectory(ReviewWorkDirectory);

            video.SetBrandingText(RandomChannelBrandingText(video.BrandingTextOptions()));
            string videoFilters = video.VideoFilters();

            // string audioFilePath = _fileSystemService.GetFilesInDirectory(ReviewingDirectory)
            //     .Where(f => f.StartsWith(subtitle.FileName) && f.ToLower().EndsWith(FileExtension.Mp3.Value))
            //     .Single();
            video.SetAudioFile(_musicService.GetRandomMixTrack());

            /// render video
            // await _ffmpegService.RenderVideoAsync
            string outputFilePath = Path.Combine(ReviewWorkDirectory, video.FileName);

            await _ffmpegService.RenderVideoAsync(
                video.FilePath, video.VideoFilters(), outputFilePath, ReviewWorkDirectory, cancellationToken, video.AudioFile.FilePath
            );

            _fileSystemService.MoveFile(
                video.GraphicsSubtitleFile.FilePath,
                Path.Combine(ArchiveDirectory, video.GraphicsSubtitleFile.FileName));
            _fileSystemService.MoveFile(outputFilePath, Path.Combine(UploadingDirectory, video.FileName));
            _fileSystemService.DeleteDirectory(ReviewWorkDirectory);

            _loggerService.LogInformation($"Completed processing {subtitleFilePath}");
        }
        catch (NoFilesMatchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _fileSystemService.MoveFile(subtitleFile.FilePath, subtitleFile.FilePath + FileExtension.Err.Value);
        }
    }
}