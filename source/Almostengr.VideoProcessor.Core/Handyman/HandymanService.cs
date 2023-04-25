using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;
using Almostengr.VideoProcessor.Core.TechTalk;

namespace Almostengr.VideoProcessor.Core.Handyman;

public sealed class HandymanService : BaseVideoService, IHandymanVideoService, IHandymanTranscriptionService
{
    private readonly ILoggerService<HandymanService> _loggerService;
    private readonly ISrtSubtitleFileService _srtSubtitleService;
    private readonly IThumbnailService _thumbnailService;

    public HandymanService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        IMusicService musicService, ILoggerService<HandymanService> loggerService,
        IAssSubtitleFileService assSubtitleFileService, ISrtSubtitleFileService srtSubtitleFileService,
        IThumbnailService thumbnailService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService, assSubtitleFileService)
    {
        IncomingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Incoming);
        ArchiveDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Archive);
        WorkingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Working);
        UploadingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Uploading);
        _loggerService = loggerService;
        _srtSubtitleService = srtSubtitleFileService;
        _thumbnailService = thumbnailService;

        _fileSystemService.CreateDirectory(IncomingDirectory);
        _fileSystemService.CreateDirectory(UploadingDirectory);
        _fileSystemService.CreateDirectory(ArchiveDirectory);
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

    public void CreateThumbnails()
    {
        IEnumerable<TechTalkThumbnailFile>? thumbnailFiles = GetThumbnailFiles(IncomingDirectory)
            .Select(f => new TechTalkThumbnailFile(f));

        if (thumbnailFiles.Any())
        {
            return;
        }

        foreach (TechTalkThumbnailFile thumbnailFile in thumbnailFiles)
        {
            try
            {
                _thumbnailService.GenerateThumbnail<TechTalkThumbnailFile>(
                    UploadingDirectory, thumbnailFile);

                _fileSystemService.MoveFile(
                    thumbnailFile.ThumbTxtFilePath,
                    Path.Combine(ArchiveDirectory, Path.GetFileName(thumbnailFile.ThumbTxtFileName())));
            }
            catch (Exception ex)
            {
                _loggerService.LogError(ex, ex.Message);
                _fileSystemService.MoveFile(
                    thumbnailFile.ThumbTxtFilePath, thumbnailFile.ThumbTxtFilePath + FileExtension.Err.Value);
            }
        }
    }

    public void ProcessSrtSubtitleFile()
    {
        HandymanSrtSubtitleFile? srtFile = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
            .Where(f => f.EndsWithIgnoringCase(FileExtension.Srt.Value))
            .Select(f => new HandymanSrtSubtitleFile(f))
            .FirstOrDefault();

        if (srtFile == null)
        {
            return;
        }

        try
        {
            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            srtFile.SetSubtitles(_srtSubtitleService.ReadFile(srtFile.FileName()));

            _srtSubtitleService.WriteFile(
                Path.Combine(WorkingDirectory, srtFile.FileName()), srtFile.Subtitles);

            _srtSubtitleService.WriteFile(
                Path.Combine(WorkingDirectory, srtFile.BlogFileName()), srtFile.Subtitles);

            _fileSystemService.MoveFile(
                Path.Combine(WorkingDirectory, srtFile.FileName()),
                Path.Combine(UploadingDirectory, srtFile.BlogFileName()));

            _fileSystemService.MoveFile(
                Path.Combine(WorkingDirectory, srtFile.FileName()),
                Path.Combine(UploadingDirectory, srtFile.FileName()));

            _fileSystemService.DeleteDirectory(WorkingDirectory);

            _fileSystemService.MoveFile(
                srtFile.FilePath, Path.Combine(UploadingDirectory, srtFile.FileName()));
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _loggerService.LogErrorProcessingFile(srtFile.FilePath, ex);
            _fileSystemService.MoveFile(srtFile.FilePath, srtFile.FilePath + FileExtension.Err.Value);
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
    }

    public override async Task ProcessVideoProjectAsync(CancellationToken cancellationToken)
    {
        HandymanVideoProject? project = _fileSystemService.GetTarballFilesInDirectory(IncomingDirectory)
            .Select(f => new HandymanVideoProject(f))
            .FirstOrDefault();

        if (project == null)
        {
            return;
        }

        try
        {
            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                project.FilePath, WorkingDirectory, cancellationToken);

            StopProcessingIfKdenliveFileExists(WorkingDirectory);
            StopProcessingIfDetailsTxtFileExists(WorkingDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(WorkingDirectory);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            var videoClips = _fileSystemService.GetVideoFilesInDirectoryWithFileInfo(WorkingDirectory)
                .OrderBy(f => f.Name);

            foreach (var videoClip in videoClips)
            {
                string audioClipFilePath =
                    Path.Combine(WorkingDirectory, Path.GetFileNameWithoutExtension(videoClip.Name) + FileExtension.Mp3.Value);

                if (File.Exists(audioClipFilePath))
                {
                    await AnalyzeAndNormalizeAudioAsync(audioClipFilePath, cancellationToken);
                    goto RenderTs;
                }

                var result = await _ffmpegService.FfprobeAsync($"\"{videoClip.FullName}\"", WorkingDirectory, cancellationToken);

                if (result.stdErr.DoesNotContainIgnoringCase(Constant.Audio))
                {
                    // throw new NoAudioTrackException("The video track does not have audio nor an audio file");
                    _loggerService.LogWarning(
                        $"Video clip {videoClip.Name} in project {project.FileName()} does not contain audio");
                    audioClipFilePath  = _musicService.GetRandomMixTrack().FilePath;
                    goto RenderTs;
                }

                var audioConversionResult = await _ffmpegService.ConvertVideoFileToMp3FileAsync(
                    videoClip.FullName, audioClipFilePath, WorkingDirectory, cancellationToken);

                await AnalyzeAndNormalizeAudioAsync(audioClipFilePath, cancellationToken);

            RenderTs:
                string tsFilePath =
                    Path.Combine(WorkingDirectory, Path.GetFileNameWithoutExtension(videoClip.Name) + FileExtension.Ts.Value);

                await _ffmpegService.AddAccAudioToVideoAsync(
                    videoClip.FullName, audioClipFilePath, tsFilePath, cancellationToken);
            }

            var tsVideoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWithIgnoringCase(FileExtension.Ts.Value))
                .OrderBy(f => f);

            string ffmpegInputFilePath = Path.Combine(WorkingDirectory, Constant.FfmpegInputFileName);
            CreateFfmpegInputFile(tsVideoFiles.ToArray(), ffmpegInputFilePath);

            string outputVideoFilePath = Path.Combine(WorkingDirectory, project.VideoFileName());

            var textOptions = project.BrandingTextOptions().ToList();
            string brandingText = textOptions[_randomService.Next(0, textOptions.Count)];
            await _ffmpegService.RenderVideoWithInputFileAndFiltersAsync(
                ffmpegInputFilePath, project.ChannelBrandDrawTextFilter(brandingText), outputVideoFilePath, cancellationToken);

            _fileSystemService.SaveFileContents(
                Path.Combine(IncomingDirectory, project.ThumbnailFileName()), project.Title());

            _fileSystemService.MoveFile(project.FilePath, Path.Combine(ArchiveDirectory, project.FileName()));
            _fileSystemService.MoveFile(
                outputVideoFilePath, Path.Combine(UploadingDirectory, project.VideoFileName()));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _loggerService.LogErrorProcessingFile(project.FilePath, ex);
            _fileSystemService.MoveFile(project.FilePath, project.FilePath + FileExtension.Err.Value);
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
    }

    public async Task ProcessReviewedFilesAsync(CancellationToken cancellationToken)
    {
        HandymanGraphicsFile? graphicsFile = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
            .Where(f => f.EndsWithIgnoringCase(FileExtension.GraphicsAss.Value))
            .Select(f => new HandymanGraphicsFile(f))
            .FirstOrDefault();

        if (graphicsFile == null)
        {
            return;
        }

        try
        {
            var videoFilePath = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
                .Where(f => f.StartsWith(graphicsFile.FilePath.Replace(FileExtension.GraphicsAss.Value, string.Empty)) &&
                    f.EndsWithIgnoringCase(FileExtension.Mp4.Value))
                .Single();

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            string outputFilePath = Path.Combine(WorkingDirectory, graphicsFile.VideoFileName());

            await _ffmpegService.RenderVideoWithFiltersAsync(
                videoFilePath, string.Empty, outputFilePath, cancellationToken);

            _fileSystemService.MoveFile(
                outputFilePath, Path.Combine(UploadingDirectory, graphicsFile.VideoFileName()));

            _fileSystemService.MoveFile(
                graphicsFile.FilePath, Path.Combine(ArchiveDirectory, graphicsFile.FileName));

            _fileSystemService.DeleteFile(videoFilePath);
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _loggerService.LogErrorProcessingFile(graphicsFile.FilePath, ex);
            _fileSystemService.MoveFile(graphicsFile.FilePath, graphicsFile.FilePath + FileExtension.Err.Value);
        }
    }
}