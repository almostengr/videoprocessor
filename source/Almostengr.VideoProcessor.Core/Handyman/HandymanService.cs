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

    public override async Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken)
    {
        HandymanVideoFile? archiveFile = null;

        try
        {
            string? selectedTarballFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.Tar);

            if (string.IsNullOrEmpty(selectedTarballFilePath))
            {
                return;
            }

            archiveFile = new HandymanVideoFile(new VideoProjectArchiveFile(selectedTarballFilePath));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(archiveFile.FilePath, WorkingDirectory, cancellationToken);

            StopProcessingIfKdenliveFileExists(WorkingDirectory);
            StopProcessingIfDetailsTxtFileExists(WorkingDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(WorkingDirectory);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            var audioFiles = GetAudioFilesInDirectory(WorkingDirectory);
                
            // normalize audio

            foreach (var audioFile in audioFiles)
            {
                var video = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                    .Where(f => f.StartsWith(audioFile.FilePath.Replace(FileExtension.Mp3.Value, string.Empty)))
                    .Select(f => new HandymanVideoFile(f))
                    .Single();

                video.SetAudioFile(audioFile);

                string tsOutputFilePath = Path.Combine(WorkingDirectory, video.TsOutputFileName());

                await _ffmpegService.AddAccAudioToVideoAsync(
                    video.FilePath, video.AudioFilePath(), tsOutputFilePath, cancellationToken);

                _fileSystemService.DeleteFile(video.FilePath);
            }

            var videoSegmentFiles = GetVideoFilesInDirectory(WorkingDirectory)
                .Select(f => new HandymanVideoFile(f));

            foreach (var video in videoSegmentFiles)
            {
                var result = await _ffmpegService.FfprobeAsync($"\"{video.FilePath}\"", WorkingDirectory, cancellationToken);

                if (result.stdErr.ToLower().Contains(Constant.Audio))
                {
                    await _ffmpegService.ConvertMp4VideoFileToTsFormatAsync(
                        video.FilePath,
                        Path.Combine(WorkingDirectory, video.TsOutputFileName()),
                        cancellationToken);
                    continue;
                }

                video.SetAudioFile(_musicService.GetRandomMixTrack());

                string tempOutputFileName = Path.GetFileNameWithoutExtension(video.FilePath) + FileExtension.Ts.Value;

                await _ffmpegService.AddAccAudioToVideoAsync(
                    video.FilePath, video.AudioFilePath(), tempOutputFileName, cancellationToken);

                _fileSystemService.DeleteFile(video.FilePath);
            }

            string? ffmpegInputFilePath = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.FfmpegInput.Value, StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();

            if (string.IsNullOrEmpty(ffmpegInputFilePath))
            {
                var tsVideoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                    .Where(f => f.EndsWith(FileExtension.Ts.Value, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f);

                ffmpegInputFilePath = Path.Combine(WorkingDirectory, Constant.FfmpegInputFileName);
                CreateFfmpegInputFile(tsVideoFiles.ToArray(), ffmpegInputFilePath);
            }

            string outputVideoFilePath = Path.Combine(WorkingDirectory, archiveFile.OutputFileName());

            await _ffmpegService.RenderVideoWithInputFileAndFiltersAsync(
                ffmpegInputFilePath, archiveFile.VideoFilters(), outputVideoFilePath, cancellationToken);

            _fileSystemService.SaveFileContents(
                Path.Combine(IncomingDirectory, archiveFile.ThumbnailFileName()), archiveFile.Title());

            _fileSystemService.MoveFile(archiveFile.FilePath, Path.Combine(ArchiveDirectory, archiveFile.FileName()));
            _fileSystemService.MoveFile(
                outputVideoFilePath, Path.Combine(UploadingDirectory, archiveFile.OutputFileName()));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);

            if (archiveFile != null)
            {
                _loggerService.LogErrorProcessingFile(archiveFile.FilePath, ex);
                _fileSystemService.MoveFile(archiveFile.FilePath, archiveFile.FilePath + FileExtension.Err.Value);
            }

            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
    }

    public void CreateThumbnails()
    {
        try
        {
            var thumbnailFiles = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
                .Where(f => f.EndsWith(FileExtension.ThumbTxt.Value, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var thumbnailFile in thumbnailFiles)
            {
                _thumbnailService.GenerateThumbnail(
                    ThumbnailType.Handyman,
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

    public void ProcessSrtSubtitles(CancellationToken cancellationToken)
    {
        SrtSubtitleFile? srtFile = null;

        try
        {
            srtFile = new SrtSubtitleFile(_fileSystemService.GetRandomFileByExtensionFromDirectory(
                WorkingDirectory, FileExtension.Srt));

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

            if (srtFile != null)
            {
                _loggerService.LogErrorProcessingFile(srtFile.FilePath, ex);
                _fileSystemService.MoveFile(srtFile.FilePath, srtFile.FilePath + FileExtension.Err.Value);
            }

            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
    }
}