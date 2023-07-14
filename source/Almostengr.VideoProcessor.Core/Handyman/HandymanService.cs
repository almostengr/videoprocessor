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

    public HandymanService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        IMusicService musicService, ILoggerService<HandymanService> loggerService,
        IAssSubtitleFileService assSubtitleFileService, ISrtSubtitleFileService srtSubtitleFileService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService, assSubtitleFileService)
    {
        IncomingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Incoming);
        ArchiveDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Archive);
        WorkingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Working);
        UploadingDirectory = Path.Combine(_appSettings.HandymanDirectory, DirectoryName.Uploading);
        _loggerService = loggerService;
        _srtSubtitleService = srtSubtitleFileService;

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

            srtFile.SetSubtitles(_srtSubtitleService.ReadFile(srtFile.FilePath));

            string workingSrtFilePath = Path.Combine(WorkingDirectory, srtFile.FileName());
            _srtSubtitleService.WriteFile(workingSrtFilePath, srtFile.Subtitles);

            string workingBlogFilePath = Path.Combine(WorkingDirectory, srtFile.BlogFileName());
            // _srtSubtitleService.WriteFile(workingBlogFilePath, srtFile.BlogPostText());
            _fileSystemService.SaveFileContents(workingBlogFilePath, srtFile.BlogPostText());

            string uploadSrtFilePath = Path.Combine(UploadingDirectory, srtFile.FileName());
            string uploadBlogFilePath = Path.Combine(UploadingDirectory, srtFile.BlogFileName());

            _fileSystemService.MoveFile(workingSrtFilePath, uploadSrtFilePath);
            _fileSystemService.MoveFile(workingBlogFilePath, uploadBlogFilePath);

            _fileSystemService.DeleteDirectory(WorkingDirectory);

            _fileSystemService.MoveFile(
                srtFile.FilePath, Path.Combine(ArchiveDirectory, srtFile.FileName()));
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
        string? readyFile = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
            .Where(f => f.EndsWithIgnoringCase(FileExtension.Ready.Value))
            .FirstOrDefault();

        if (readyFile == null)
        {
            return;
        }

        string projectFileName =
            Path.GetFileName(readyFile.ReplaceIgnoringCase(FileExtension.Ready.Value, FileExtension.Tar.Value));
        HandymanVideoProject? project = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
           .Where(f => f.ContainsIgnoringCase(projectFileName))
           .Select(f => new HandymanVideoProject(f))
           .SingleOrDefault();

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

            var imageFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWithIgnoringCase(FileExtension.Jpg.Value))
                .ToList();

            foreach (string imageFile in imageFiles)
            {
                string outputFilePath = Path.Combine(
                    WorkingDirectory, Path.GetFileNameWithoutExtension(imageFile) + FileExtension.Mp4.Value);
                await _ffmpegService.ConvertImageFileToVideoAsync(imageFile, outputFilePath, cancellationToken);
            }

            var videoClips = _fileSystemService.GetVideoFilesInDirectoryWithFileInfo(WorkingDirectory)
                .OrderBy(f => f.Name);

            foreach (var videoClip in videoClips)
            {
                string audioClipFilePath =
                    Path.Combine(WorkingDirectory, Path.GetFileNameWithoutExtension(videoClip.Name) + FileExtension.Mp3.Value);

                bool normalizeAudio = true;
                if (!File.Exists(audioClipFilePath))
                {
                    var result = await _ffmpegService.FfprobeAsync($"\"{videoClip.FullName}\"", WorkingDirectory, cancellationToken);

                    if (result.stdErr.DoesNotContainIgnoringCase(Constant.Audio))
                    {
                        _loggerService.LogWarning($"{videoClip.FullName} does not contain any audio.");
                        normalizeAudio = false;
                    }
                    else
                    {
                        var audioConversionResult = await _ffmpegService.ConvertVideoFileToMp3FileAsync(
                            videoClip.FullName, audioClipFilePath, WorkingDirectory, cancellationToken);
                    }
                }

                if (normalizeAudio)
                {
                    await AnalyzeAndNormalizeAudioAsync(audioClipFilePath, cancellationToken);
                }

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
            _fileSystemService.DeleteFile(readyFile);
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _loggerService.LogErrorProcessingFile(project.FilePath, ex);
            _fileSystemService.MoveFile(readyFile, readyFile + FileExtension.Err.Value);
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
    }
}