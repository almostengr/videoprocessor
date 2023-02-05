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
        // WorkingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Working);
        // DraftDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Draft);
        _loggerService = loggerService;
        _srtService = srtSubtitleFileService;
        // _ffmpegInputFilePath = Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME);
    }

    public async Task ConvertGzToXzAsync(CancellationToken cancellationToken)
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

    // public override async Task ReceivedToStitchedAsync(CancellationToken cancellationToken)
    // {
    //     HandymanIncomingTarballFile? tarball = null;

    //     try
    //     {
    //         string selectedTarballFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(
    //             IncomingDirectory, FileExtension.Tar);

    //         tarball = new HandymanIncomingTarballFile(selectedTarballFilePath);

    //         _fileSystemService.DeleteDirectory(StitchingDirectory);
    //         _fileSystemService.CreateDirectory(StitchingDirectory);

    //         await _tarballService.ExtractTarballContentsAsync(tarball.FilePath, StitchingDirectory, cancellationToken);

    //         StopProcessingIfKdenliveFileExists(StitchingDirectory);
    //         StopProcessingIfFfmpegInputTxtFileExists(StitchingDirectory);

    //         // check each video file for audio track 
    //         // files that do not have audio track, will have track added
    //         await AddMusicToTimelapseVideoAsync(cancellationToken);

    //         string? ffmpegInputFilePath = _fileSystemService.GetFilesInDirectory(StitchingDirectory)
    //             .Where(f => f.EndsWith(FileExtension.FfmpegInput.Value))
    //             .SingleOrDefault();

    //         if (string.IsNullOrEmpty(ffmpegInputFilePath))
    //         {
    //             string[] videoFiles = _fileSystemService.GetFilesInDirectory(StitchingDirectory)
    //                 .Where(f => f.ToLower().EndsWith(FileExtension.Mkv.Value) ||
    //                     f.ToLower().EndsWith(FileExtension.Mp4.Value))
    //                 .OrderBy(f => f)
    //                 .ToArray();

    //             ffmpegInputFilePath = Path.Combine(StitchingDirectory, "handyman" + FileExtension.FfmpegInput.Value);
    //             CreateFfmpegInputFile(videoFiles, ffmpegInputFilePath);
    //         }

    //         string outputFilePath = Path.Combine(StitchingDirectory, tarball.VideoFileName);

    //         await _ffmpegService.RenderVideoAsCopyAsync(
    //             ffmpegInputFilePath, outputFilePath, cancellationToken);

    //         await _ffmpegService.ConvertVideoFileToMp3FileAsync(
    //             outputFilePath, outputFilePath.Replace(FileExtension.Mp4.Value, FileExtension.Mp3.Value),
    //             AnimatingDirectory, cancellationToken);

    //         _fileSystemService.MoveFile(tarball.FilePath, Path.Combine(ArchiveDirectory, tarball.FileName));
    //         _fileSystemService.MoveFile(outputFilePath, Path.Combine(AnimatingDirectory, tarball.FileName));
    //         _fileSystemService.DeleteDirectory(StitchingDirectory);
    //     }
    //     catch (NoFilesMatchException)
    //     {
    //         throw;
    //     }
    //     catch (Exception ex)
    //     {
    //         _loggerService.LogError(ex, ex.Message);
    //         _fileSystemService.MoveFile(tarball.FilePath, tarball.FilePath + FileExtension.Err.Value);
    //         _fileSystemService.DeleteDirectory(StitchingDirectory);
    //     }
    // }

    // public override async Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken)
    // {
    //     HandymanVideoFile? video = null;
    //     try
    //     {
    //         string incomingTarball = _fileSystemService.GetRandomFileByExtensionFromDirectory(IncomingDirectory, FileExtension.Tar);

    //         video = new HandymanVideoFile(Path.GetFileName(incomingTarball));

    //         _fileSystemService.DeleteDirectory(WorkingDirectory);
    //         _fileSystemService.CreateDirectory(WorkingDirectory);

    //         await _tarballService.ExtractTarballContentsAsync(
    //             video.TarballFilePath, WorkingDirectory, cancellationToken);

    //         _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

    //         // if (DoesKdenliveFileExist(IncomingDirectory))
    //         // {
    //         //     throw new KdenliveFileExistsException();
    //         // }

    //         StopProcessingIfKdenliveFileExists(WorkingDirectory);

    //         await ConvertVideoAudioFilesToAudioOnly(WorkingDirectory, cancellationToken);

    //         await MergeVideoAndAudioFilesAsync(cancellationToken);

    //         CreateFfmpegInputFile(video);

    //         // video.AddDrawTextVideoFilter(
    //         //     RandomChannelBrandingText(video.BrandingTextOptions()),
    //         //     video.DrawTextFilterTextColor(),
    //         //     Opacity.Full,
    //         //     FfmpegFontSize.Large,
    //         //     DrawTextPosition.UpperRight,
    //         //     video.DrawTextFilterBackgroundColor(),
    //         //     Opacity.Medium);

    //         // if (_fileSystemService.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString())).Any())
    //         // {
    //         // video.AddSubtitleVideoFilter(
    //         //     _fileSystemService.GetFilesInDirectory(WorkingDirectory).Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString())).Single(),
    //         //     "&H00006400",
    //         //     "&H00FFFFFF");
    //         // }

    //         string? graphicsSubtitleFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
    //             .Where(f => f.EndsWith(FileExtension.GraphicsAss.ToString()))
    //             .SingleOrDefault();

    //         if (!string.IsNullOrEmpty(graphicsSubtitleFile))
    //         {
    //             video.SetGraphicsSubtitleFile(graphicsSubtitleFile);
    //             video.GraphicsSubtitleFile.SetSubtitles(
    //                 _assSubtitleFileService.ReadFile(video.GraphicsSubtitleFile.FilePath));
    //         }

    //         await _ffmpegService.RenderVideoAsync(
    //             Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME),
    //             video.VideoFilters(),
    //             Path.Combine(UploadDirectory, video.OutputVideoFileName),
    //             cancellationToken);

    //         _fileSystemService.MoveFile(video.TarballFilePath, Path.Combine(ArchiveDirectory, video.TarballFileName));
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
    //                 Path.Combine(IncomingDirectory, video.TarballFileName),
    //                 Path.Combine(ArchiveDirectory, video.TarballFileName)
    //             );
    //             _fileSystemService.SaveFileContents(
    //                 Path.Combine(ArchiveDirectory, video.TarballFileName + FileExtension.Log),
    //                 ex.Message);
    //         }
    //     }
    // }

    // private void CreateFfmpegInputFile(HandymanVideoFile handymanVideo)
    // {
    //     _fileSystemService.DeleteFile(Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME));

    //     var videoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
    //         .Where(f => f.EndsWith(FileExtension.Mp4.ToString()) || f.EndsWith(FileExtension.Mkv.ToString()))
    //         .OrderBy(f => f)
    //         .ToList();

    //     base.CreateFfmpegInputFile(videoFiles.ToArray(), Path.Combine(WorkingDirectory, FFMPEG_FILE_NAME));
    // }

    private async Task AddMusicToTimelapseVideoAsync(CancellationToken cancellationToken)
    {
        var workingDirVideos = _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
            .Where(f => f.ToLower().EndsWith(FileExtension.Mp4.ToString()));

        foreach (var videoFilePath in workingDirVideos)
        {
            HandymanVideoFile video = new HandymanVideoFile(videoFilePath);

            var result = await _ffmpegService.FfprobeAsync($"\"{video.FilePath}\"", IncomingWorkDirectory, cancellationToken);

            if (result.stdErr.ToLower().Contains("audio"))
            {
                continue;
            }

            // string audioFilePath = _fileSystemService.GetFilesInDirectory(AssemblyDirectory)
            //     .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3.ToString()))
            //     .SingleOrDefault() ?? _musicService.GetRandomNonMixTrack();

            string audioFilePath =
                _fileSystemService.GetFilesInDirectory(IncomingWorkDirectory)
                    .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3.ToString()))
                    .SingleOrDefault() ?? _musicService.GetRandomNonMixTrack();
            video.SetAudioFile(audioFilePath);

            // string tempOutputFileName = Path.GetFileNameWithoutExtension(videoFilePath) + FileExtension.TmpMp4;
            string tempOutputFileName = Path.GetFileNameWithoutExtension(video.FilePath) + FileExtension.TmpMp4.Value;

            await _ffmpegService.AddAudioToVideoAsync(
                // videoFilePath, audioFilePath, tempOutputFileName, cancellationToken);
                video.FilePath, video.AudioFile.FilePath, tempOutputFileName, cancellationToken);

            _fileSystemService.DeleteFile(video.FilePath);
            // _fileSystemService.MoveFile(tempOutputFileName, videoFilePath);
            _fileSystemService.MoveFile(tempOutputFileName, video.FilePath);
        }
    }

    // private async Task MergeVideoAndAudioFilesAsync(CancellationToken cancellationToken)
    // {
    //     var workingDirVideos = _fileSystemService.GetFilesInDirectory(AssemblyDirectory)
    //                 .Where(f => f.EndsWith(FileExtension.Mp4.ToString()) || f.EndsWith(FileExtension.Mkv.ToString()));

    //     foreach (var videoFilePath in workingDirVideos)
    //     {
    //         var result = await _ffmpegService.FfprobeAsync($"\"{videoFilePath}\"", AssemblyDirectory, cancellationToken);

    //         if (result.stdErr.ToLower().Contains("audio"))
    //         {
    //             continue;
    //         }

    //         string? audioFilePath = _fileSystemService.GetFilesInDirectory(AssemblyDirectory)
    //             .Where(f => f.StartsWith(Path.GetFileNameWithoutExtension(videoFilePath)) && f.EndsWith(FileExtension.Mp3.ToString()))
    //             .SingleOrDefault();

    //         if (string.IsNullOrWhiteSpace(audioFilePath))
    //         {
    //             audioFilePath = _musicService.GetRandomNonMixTrack();
    //         }

    //         string tempOutputFileName = Path.GetFileNameWithoutExtension(videoFilePath) + FileExtension.TmpMp4;

    //         await _ffmpegService.AddAudioToVideoAsync(
    //             videoFilePath, audioFilePath, tempOutputFileName, cancellationToken);

    //         _fileSystemService.DeleteFile(videoFilePath);
    //         _fileSystemService.MoveFile(tempOutputFileName, videoFilePath);
    //     }
    // }

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

    public override Task ProcessReviewedFilesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}