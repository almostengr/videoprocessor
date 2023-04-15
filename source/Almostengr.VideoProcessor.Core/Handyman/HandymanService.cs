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

    public void CreateThumbnails()
    {
        try
        {
            var thumbnailFiles = GetThumbnailFiles(IncomingDirectory)
                .Select(f => new HandymanThumbnailFile(f));

            _thumbnailService.GenerateThumbnails<HandymanThumbnailFile>(UploadingDirectory, thumbnailFiles);

            foreach (var thumbnailFile in thumbnailFiles)
            {
                _fileSystemService.MoveFile(
                    thumbnailFile.ThumbTxtFilePath,
                    Path.Combine(ArchiveDirectory, thumbnailFile.ThumbTxtFileName()));
            }
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }

    public void ProcessSrtSubtitleFile()
    {
        SrtSubtitleFile? srtFile = null;

        try
        {
            string? srtFilePath = _fileSystemService.GetRandomFileByExtensionFromDirectory(
                WorkingDirectory, FileExtension.Srt);

            if (string.IsNullOrEmpty(srtFilePath))
            {
                return;
            }

            srtFile = new HandymanSrtSubtitleFile(srtFilePath);

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

            var videoClips = _fileSystemService.GetFilesInDirectoryWithFileInfo(WorkingDirectory)
                .Where(f => f.FullName.EndsWith(FileExtension.Mov.Value, StringComparison.OrdinalIgnoreCase) ||
                    f.FullName.EndsWith(FileExtension.Mkv.Value, StringComparison.OrdinalIgnoreCase) ||
                    f.FullName.EndsWith(FileExtension.Mp4.Value, StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.Name);



            // todo
            
            // for each video file
            // check if it has audio
            // if no audio, then check for separate audio file, 
            // if separate audio, then copy mix track with same name as video; continue to next file
            // check the max volume of audio
            // if audio has not already been normalized, then normalize it by splitting it from video
            
            // merge each audio and video pair into a ts file

            // combine the ts files into a single output file

            // move final output to upload directory
            // move project tarball to archive directory
            // delete working directory

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

            string outputVideoFilePath = Path.Combine(WorkingDirectory, project.VideoFileName());

            await _ffmpegService.RenderVideoWithInputFileAndFiltersAsync(
                ffmpegInputFilePath, project.VideoFilters(), outputVideoFilePath, cancellationToken);

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
}