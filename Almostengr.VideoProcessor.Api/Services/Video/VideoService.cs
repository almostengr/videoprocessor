using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services.ExternalProcess;
using Almostengr.VideoProcessor.Api.Services.FileSystem;
using Almostengr.VideoProcessor.Constants;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.Video
{
    public abstract class VideoService : IVideoService
    {
        private readonly ILogger<VideoService> _logger;
        private readonly AppSettings _appSettings;
        private readonly IExternalProcessService _externalProcess;
        private readonly IFileSystemService _fileSystem;
        private const int PADDING = 30;
        internal readonly string _subscribeFilter;
        internal readonly string _subscribeScrollingFilter;

        // ffmpeg positions
        internal readonly string _upperLeft;
        internal readonly string _upperCenter;
        internal readonly string _upperRight;
        internal readonly string _centered;
        internal readonly string _lowerLeft;
        internal readonly string _lowerCenter;
        internal readonly string _lowerRight;

        // essential files
        internal const string FFMPEG_INPUT_FILE = "ffmpeginput.txt";
        internal const string SUBTITLES_FILE = "subtitles.ass";

        // ffmpeg options
        internal const string LOG_ERRORS = "-loglevel error";
        internal const string LOG_WARNINGS = "-loglevel warning";
        internal const string HW_OPTIONS = "-hide_banner -y -hwaccel vaapi -hwaccel_output_format vaapi";
        internal const string HW_VCODEC = "-vcodec h264_vaapi -b:v 5M";

        // ffmpeg filter attributes
        internal const int DASHCAM_BORDER_WIDTH = 10;
        internal const string DIM_TEXT = "0.7";
        internal const string DIM_BACKGROUND = "0.3";
        internal const string LARGE_FONT = "h/20";
        internal const string SMALL_FONT = "h/35";
        internal const int RHT_BORDER_WIDTH = 7;

        public VideoService(ILogger<VideoService> logger, AppSettings appSettings,
            IExternalProcessService externalProcess, IFileSystemService fileSystem)
        {
            _logger = logger;
            _appSettings = appSettings;
            _externalProcess = externalProcess;
            _fileSystem = fileSystem;

            _upperLeft = $"x={PADDING}:y={PADDING}";
            _upperCenter = $"x=(w-tw)/2:y={PADDING}";
            _upperRight = $"x=w-tw-{PADDING}:y={PADDING}";
            _centered = $"x=(w-tw)/2:y=(h-th)/2";
            _lowerLeft = $"x={PADDING}:y=h-th-{PADDING}";
            _lowerCenter = $"x=(w-tw)/2:y=h-th-{PADDING}";
            _lowerRight = $"x=w-tw-{PADDING}:y=h-th-{PADDING}";

            _subscribeFilter = $"drawtext=text:'SUBSCRIBE':fontcolor={FfMpegColors.White}:fontsize={SMALL_FONT}:{_lowerLeft}:boxcolor={FfMpegColors.Red}:box=1:boxborderw=10";
            _subscribeScrollingFilter = $"drawtext=text:'SUBSCRIBE':fontcolor={FfMpegColors.White}:fontsize={SMALL_FONT}:x=w+(100*t):y=h-th-{PADDING}:boxcolor={FfMpegColors.Red}:box=1:boxborderw=10";
        }

        public abstract string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties);

        public virtual async Task ArchiveDirectoryContentsAsync(string directoryToArchive, string archiveName, string archiveDestination, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Archiving directory contents: {directoryToArchive}");

            await _externalProcess.RunCommandAsync(
                ProgramPaths.BashShell,
                $"-c \"cd \\\"{directoryToArchive}\\\" && tar -cvJf \\\"{Path.Combine(archiveDestination, archiveName)}\\\" *\"",
                directoryToArchive,
                cancellationToken,
                15
            );
        }

        public virtual string[] GetVideoArchivesInDirectory(string directory)
        {
            return _fileSystem.GetFilesInDirectory(directory)
                .Where(x => x.Contains(FileExtension.Tar))
                .ToArray();
        }

        public virtual string GetSubtitlesFilter(string workingDirectory)
        {
            string subtitleFile = Path.Combine(workingDirectory, SUBTITLES_FILE);

            if (File.Exists(subtitleFile))
            {
                return $", subtitles='{subtitleFile}'";
            }

            return string.Empty;
        }

        public virtual async Task ExtractTarFileAsync(string tarFile, string workingDirectory, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Extracting tar file: {tarFile}");

            await _externalProcess.RunCommandAsync(
                ProgramPaths.TarBinary,
                $"-xvf \"{tarFile}\" -C \"{workingDirectory}\"",
                Path.GetDirectoryName(tarFile),
                cancellationToken,
                10);
        }

        public abstract Task RenderVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken);

        public virtual async Task ConvertVideoFilesToMp4Async(string directory, CancellationToken cancellationToken)
        {
            var nonMp4VideoFiles = _fileSystem.GetFilesInDirectory(directory)
                .Where(x => x.ToLower().EndsWith(FileExtension.Mkv) || x.ToLower().EndsWith(FileExtension.Mov))
                .OrderBy(x => x)
                .ToArray();

            foreach (var videoFile in nonMp4VideoFiles)
            {
                string outputFilename = Path.GetFileNameWithoutExtension(videoFile) + FileExtension.Mp4;
                _logger.LogInformation($"Converting {videoFile} to {outputFilename}");

                await _externalProcess.RunCommandAsync(
                    ProgramPaths.FfmpegBinary,
                    $"{LOG_ERRORS} {HW_OPTIONS} -i {Path.GetFileName(videoFile)} -f mp4 {HW_VCODEC} {Path.GetFileName(outputFilename)}",
                    directory,
                    cancellationToken,
                    20);

                _fileSystem.DeleteFile(videoFile);
            }
        }

        public virtual void PrepareFileNamesInDirectory(string directory)
        {
            foreach (string file in _fileSystem.GetFilesInDirectory(directory))
            {
                File.Move(
                    file,
                    Path.Combine(
                            directory,
                            Path.GetFileName(file)
                                .ToLower()
                                .Replace(";", "_")
                                .Replace(" ", "_")
                                .Replace("__", "_"))
                );
            }
        }

        public virtual void CheckOrCreateFfmpegInputFile(string workingDirectory)
        {
            string ffmpegInputFile = Path.Combine(workingDirectory, FFMPEG_INPUT_FILE);
            _fileSystem.DeleteFile(ffmpegInputFile);

            using (StreamWriter writer = new StreamWriter(ffmpegInputFile))
            {
                _logger.LogInformation("Creating FFMPEG input file");
                string[] mp4Files = _fileSystem.GetFilesInDirectory(workingDirectory)
                    .Where(x => x.EndsWith(FileExtension.Mp4))
                    .OrderBy(x => x)
                    .ToArray();

                foreach (string file in mp4Files)
                {
                    writer.WriteLine($"file '{Path.GetFileName(file)}'");
                }
            }
        }

        public virtual async Task CreateThumbnailsFromFinalVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken)
        {
            if (_appSettings.DoRenderVideos == false)
            {
                return;
            }

            _logger.LogInformation($"Creating thumbnails for {videoProperties.OutputVideoFilePath}");

            for (int sceneChangePct = 90; sceneChangePct > 0; sceneChangePct -= 10)
            {
                _logger.LogInformation($"Scene change percent {sceneChangePct}%");

                await _externalProcess.RunCommandAsync(
                    ProgramPaths.FfmpegBinary,
                    $"-hide_banner -y {LOG_WARNINGS} -i \"{videoProperties.OutputVideoFilePath}\" -vf select=gt(scene\\,0.{sceneChangePct}) -frames:v {_appSettings.ThumbnailFrames.ToString()} -vsync vfr \"{videoProperties.VideoTitle}-%03d.jpg\"",
                    videoProperties.UploadDirectory,
                    cancellationToken,
                    240);

                int thumbnailsCreated = _fileSystem.GetFilesInDirectory(videoProperties.UploadDirectory)
                    .Where(x => x.Contains(videoProperties.VideoTitle) && x.EndsWith(FileExtension.Jpg))
                    .ToArray()
                    .Count();

                if (thumbnailsCreated >= _appSettings.ThumbnailFrames)
                {
                    break;
                }
            } // end for
        }

        public virtual async Task CleanUpBeforeArchivingAsync(string workingDirectory)
        {
            _logger.LogInformation($"Cleaning up before archiving {workingDirectory}");

            _fileSystem.DeleteFile(Path.Combine(workingDirectory, "ae_intro.mp4"));
            _fileSystem.DeleteFile(Path.Combine(workingDirectory, "ae_outtro2.mp4"));
            _fileSystem.DeleteFile(Path.Combine(workingDirectory, "dashcam.txt"));
            _fileSystem.DeleteFile(Path.Combine(workingDirectory, "dash_cam_opening.mp4"));
            _fileSystem.DeleteFile(Path.Combine(workingDirectory, "services.txt"));
            _fileSystem.DeleteFile(Path.Combine(workingDirectory, "subscriber_click.mp4"));
            _fileSystem.DeleteFile(Path.Combine(workingDirectory, "title.txt"));
            _fileSystem.DeleteFile(Path.Combine(workingDirectory, FFMPEG_INPUT_FILE));

            string[] files = _fileSystem.GetFilesInDirectory(workingDirectory)
                .Where(f => f.EndsWith(FileExtension.Gif) ||
                    f.EndsWith(FileExtension.Kdenlive) ||
                    f.EndsWith(FileExtension.Mp3))
                .ToArray();

            foreach (var file in files)
            {
                _fileSystem.DeleteFile(Path.Combine(workingDirectory, file));
            }

            string[] directories = _fileSystem.GetDirectoriesInDirectory(workingDirectory);
            foreach (var directory in directories)
            {
                _fileSystem.DeleteDirectory(directory);
            }

            await Task.CompletedTask;
        }

        public abstract Task WorkerIdleAsync(CancellationToken cancellationToken);
    }
}
