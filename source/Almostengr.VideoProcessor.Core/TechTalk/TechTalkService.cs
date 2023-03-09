using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music;
using Almostengr.VideoProcessor.Core.Music.Services;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed class TechTalkService : BaseVideoService, ITechTalkVideoService, ITechTalkTranscriptionService
{
    private readonly ILoggerService<TechTalkService> _loggerService;
    private readonly ISrtSubtitleFileService _srtService;
    private readonly IGzFileCompressionService _gzFileService;
    private readonly IXzFileCompressionService _xzFileService;
    private readonly IThumbnailService _thumbnailService;
    private readonly ISrtSubtitleFileService _srtSubtitleService;

    public TechTalkService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<TechTalkService> loggerService, IMusicService musicService,
        ISrtSubtitleFileService srtSubtitleFileService, IAssSubtitleFileService assSubtitleFileService,
        IXzFileCompressionService xzFileService, IGzFileCompressionService gzFileService,
        IThumbnailService thumbnailService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService, assSubtitleFileService)
    {
        IncomingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Incoming);
        WorkingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Working);
        ArchiveDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Archive);
        UploadingDirectory = Path.Combine(_appSettings.TechnologyDirectory, DirectoryName.Uploading);
        _loggerService = loggerService;
        _srtService = srtSubtitleFileService;
        _xzFileService = xzFileService;
        _gzFileService = gzFileService;
        _thumbnailService = thumbnailService;
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

    public override async Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken)
    {
        TechTalkVideoFile? archiveFile = null;

        try
        {
            string selectedTarballFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                IncomingDirectory, FileExtension.Tar);

            archiveFile = new TechTalkVideoFile(new VideoProjectArchiveFile(selectedTarballFilePath));

            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(archiveFile.FilePath, WorkingDirectory, cancellationToken);

            StopProcessingIfKdenliveFileExists(WorkingDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(WorkingDirectory);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            // todo normailze audio files

            var audioFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp3.Value, StringComparison.OrdinalIgnoreCase))
                .Select(f => new AudioFile(f))
                .ToList();

            foreach (var audioFile in audioFiles)
            {
                var video = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                    .Where(f => f.StartsWith(audioFile.FilePath.Replace(FileExtension.Mp3.Value, string.Empty)) && !f.Equals(audioFile.FilePath))
                    .Select(f => new TechTalkVideoFile(f))
                    .Single();

                video.SetAudioFile(audioFile);

                string tsOutputFilePath = Path.Combine(
                    WorkingDirectory,
                    Path.GetFileNameWithoutExtension(video.FileName()) + FileExtension.Ts.Value);

                await _ffmpegService.AddAccAudioToVideoAsync(
                    video.FilePath, video.AudioFilePath(), tsOutputFilePath, cancellationToken);

                _fileSystemService.DeleteFile(video.FilePath);
            }

            var mp4MkvVideoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4.Value, StringComparison.OrdinalIgnoreCase) || f.EndsWith(FileExtension.Mkv.Value, StringComparison.OrdinalIgnoreCase))
                .Select(f => new TechTalkVideoFile(f))
                .ToList();

            foreach (var video in mp4MkvVideoFiles)
            {
                var result = await _ffmpegService.FfprobeAsync($"\"{video.FilePath}\"", WorkingDirectory, cancellationToken);

                if (result.stdErr.ToLower().Contains(Constant.Audio))
                {
                    await _ffmpegService.ConvertMp4VideoFileToTsFormatAsync(
                        video.FilePath,
                        video.FilePath.Replace(FileExtension.Mp4.Value, FileExtension.Ts.Value).Replace(FileExtension.Mkv.Value, FileExtension.Ts.Value),
                        cancellationToken);
                    continue;
                }

                video.SetAudioFile(_musicService.GetRandomMixTrack());

                string tsOutputFilePath = Path.Combine(WorkingDirectory, video.TsOutputFileName());

                await _ffmpegService.AddAccAudioToVideoAsync(
                    video.FilePath, video.AudioFilePath(), tsOutputFilePath, cancellationToken);

                _fileSystemService.DeleteFile(video.FilePath);
            }

            string? ffmpegInputFilePath = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.FfmpegInput.Value))
                .SingleOrDefault();

            if (string.IsNullOrEmpty(ffmpegInputFilePath))
            {
                var tsVideoFiles = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                    .Where(f => f.EndsWith(FileExtension.Ts.Value, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f);

                ffmpegInputFilePath = Path.Combine(WorkingDirectory, "videos" + FileExtension.FfmpegInput.Value);
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
        catch (NoFilesMatchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);

            if (archiveFile != null)
            {
                _loggerService.LogError(ex, $"Error when processing {archiveFile.FilePath}");
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

    public async Task ProcessSrtSubtitlesAsync(CancellationToken cancellationToken)
    {
        SrtSubtitleFile srtFile = null;

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
                _loggerService.LogError(ex, $"Error when processing {srtFile.FilePath}");
                _fileSystemService.MoveFile(srtFile.FilePath, srtFile.FilePath + FileExtension.Err.Value);
            }

            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
    }

}
