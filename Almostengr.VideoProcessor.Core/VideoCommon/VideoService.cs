using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Configuration;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Core.VideoCommon
{
    public abstract class VideoService : BaseService, IVideoService
    {
        private readonly ILogger<VideoService> _logger;
        private readonly AppSettings _appSettings;
        private const int PADDING = 30;
        protected readonly string _subscribeFilter;
        protected readonly Random _random;
        protected readonly string _subscribeScrollingFilter;

        // ffmpeg positions
        protected readonly string _upperLeft;
        protected readonly string _upperCenter;
        protected readonly string _upperRight;
        protected readonly string _centered;
        protected readonly string _lowerLeft;
        protected readonly string _lowerCenter;
        protected readonly string _lowerRight;

        // essential files
        protected const string FFMPEG_INPUT_FILE = "ffmpeginput.txt";
        protected const string SUBTITLES_FILE = "subtitles.ass";

        // ffmpeg options
        protected const string LOG_ERRORS = "-loglevel error";
        protected const string LOG_WARNINGS = "-loglevel warning";
        protected const string HW_OPTIONS = "-hide_banner -y -hwaccel vaapi -hwaccel_output_format vaapi";
        protected const string HW_VCODEC = "-vcodec h264_vaapi -b:v 5M";

        // ffmpeg filter attributes
        protected const int DASHCAM_BORDER_WIDTH = 10;
        protected const string DIM_TEXT = "0.8";
        protected const string DIM_BACKGROUND = "0.3";
        protected const string LARGE_FONT = "h/20";
        protected const string SMALL_FONT = "h/35";
        protected const int RHT_BORDER_WIDTH = 7;

        public VideoService(ILogger<VideoService> logger, AppSettings appSettings) : base(logger)
        {
            _logger = logger;
            _appSettings = appSettings;
            _random = new();

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

        protected abstract Task RenderVideoAsync(string videoTitle, string videoOutputPath, string videoFilter, CancellationToken cancellationToken);
        public abstract Task WorkerIdleAsync(CancellationToken cancellationToken);
        protected abstract string GetFfmpegVideoFilters(string videoTitle);
        protected abstract Task ConvertVideoFilesToCommonFormatAsync(string directory, CancellationToken cancellationToken);
        protected abstract string GetBrandingText();
        protected abstract void CheckOrCreateFfmpegInputFile();

        public abstract Task ExecuteServiceAsync(string videoArchive, CancellationToken cancellationToken);
        public abstract Task StartAsync(CancellationToken cancellationToken);

        protected virtual string GetVideoOutputPath(string uploadDirectory, string videoTitle)
        {
            return Path.Combine(uploadDirectory, videoTitle + FileExtension.Mp4);
        }

        protected virtual async Task ArchiveDirectoryContentsAsync(string directoryToArchive, string archiveName, string archiveDestination, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Archiving directory contents: {directoryToArchive}");

            await RunCommandAsync(
                ProgramPaths.BashShell,
                $"-c \"cd \\\"{directoryToArchive}\\\" && tar -cvJf \\\"{Path.Combine(archiveDestination, archiveName)}\\\" *\"",
                directoryToArchive,
                cancellationToken,
                15
            );
        }

        public virtual string GetRandomVideoArchiveInDirectory(string directory)
        {
            return GetFilesInDirectory(directory)
                .Where(x => x.Contains(FileExtension.Tar))
                .Where(x => x.StartsWith(".") == false)
                .OrderBy(x => _random.Next()).Take(1)
                .FirstOrDefault();
        }

        protected virtual string GetSubtitlesFilter(string workingDirectory)
        {
            string subtitleFile = Path.Combine(workingDirectory, SUBTITLES_FILE);

            if (File.Exists(subtitleFile))
            {
                return $", subtitles='{subtitleFile}'";
            }

            return string.Empty;
        }

        protected string GetArchiveTarFileName(string videoTitle)
        {
            return $"{videoTitle.Replace(FileExtension.Mp4, string.Empty).Replace(FileExtension.Mkv, string.Empty)}.{DateTime.Now.ToString("yyyyMMdd")}.{DateTime.Now.ToString("HHmmss")}{FileExtension.TarXz}";
        }

        protected virtual string GetVideoTitleFromArchiveName(string archiveFilePath)
        {
            return Path.GetFileNameWithoutExtension(archiveFilePath).Replace(FileExtension.Tar, string.Empty);
        }

        protected virtual async Task ExtractTarFileAsync(string tarFile, string workingDirectory, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Extracting tar file: {tarFile}");

            await RunCommandAsync(
                ProgramPaths.TarBinary,
                $"-xvf \"{tarFile}\" -C \"{workingDirectory}\"",
                Path.GetDirectoryName(tarFile),
                cancellationToken,
                10);
        }

        protected virtual async Task ConvertVideoFilesToMp4Async(string directory, CancellationToken cancellationToken)
        {
            var nonMp4VideoFiles = GetFilesInDirectory(directory)
                .Where(x => x.ToLower().EndsWith(FileExtension.Mkv) || x.ToLower().EndsWith(FileExtension.Mov))
                .OrderBy(x => x);

            foreach (var videoFile in nonMp4VideoFiles)
            {
                string outputFilename = Path.GetFileNameWithoutExtension(videoFile) + FileExtension.Mp4;
                _logger.LogInformation($"Converting {videoFile} to {outputFilename}");

                await RunCommandAsync(
                    ProgramPaths.FfmpegBinary,
                    $"{LOG_ERRORS} {HW_OPTIONS} -i {Path.GetFileName(videoFile)} -f mp4 {HW_VCODEC} {Path.GetFileName(outputFilename)}",
                    directory,
                    cancellationToken,
                    20);

                DeleteFile(videoFile);
            }
        }

        protected virtual string GetSubscribeLikeFollowFilter()
        {
            return "";
        }

        protected virtual void PrepareFileNamesInDirectory(string directory)
        {
            foreach (string childDirectory in GetDirectoriesInDirectory(directory))
            {
                foreach (string childFile in GetFilesInDirectory(childDirectory))
                {
                    MoveFile(
                        Path.Combine(childDirectory, childFile),
                        Path.Combine(directory, Path.GetFileName(childFile))
                    );
                }
            }

            foreach (string file in GetFilesInDirectory(directory))
            {
                File.Move(
                    file,
                    Path.Combine(
                            directory,
                            Path.GetFileName(file)
                                .ToLower()
                                .Replace(";", "_")
                                .Replace(" ", "_")
                                .Replace("__", "_")
                                .Replace("\"", string.Empty)
                                .Replace("\'", string.Empty))
                );
            }
        }

        protected virtual void CheckOrCreateFfmpegInputFile(string workingDirectory)
        {
            string ffmpegInputFile = Path.Combine(workingDirectory, FFMPEG_INPUT_FILE);
            DeleteFile(ffmpegInputFile);

            using (StreamWriter writer = new StreamWriter(ffmpegInputFile))
            {
                _logger.LogInformation("Creating FFMPEG input file");
                var mp4Files = GetFilesInDirectory(workingDirectory)
                    .Where(x => x.EndsWith(FileExtension.Mp4))
                    .OrderBy(x => x);

                foreach (string file in mp4Files)
                {
                    writer.WriteLine($"file '{Path.GetFileName(file)}'");
                }
            }
        }

        protected virtual async Task CreateThumbnailsFromFinalVideoAsync(
            string outputVideoPath, string uploadDirectory, string videoTitle, CancellationToken cancellationToken)
        {
            if (_appSettings.DoRenderVideos == false)
            {
                return;
            }

            _logger.LogInformation($"Creating thumbnails for {outputVideoPath}");

            for (int sceneChangePct = 90; sceneChangePct > 0; sceneChangePct -= 10)
            {
                _logger.LogInformation($"Scene change percent {sceneChangePct}%");

                await RunCommandAsync(
                    ProgramPaths.FfmpegBinary,
                    $"-hide_banner -y {LOG_WARNINGS} -i \"{outputVideoPath}\" -vf select=gt(scene\\,0.{sceneChangePct}) -frames:v {_appSettings.ThumbnailFrames.ToString()} -vsync vfr \"{videoTitle}-%03d.jpg\"",
                    uploadDirectory,
                    cancellationToken,
                    240);

                int thumbnailsCreated = GetFilesInDirectory(uploadDirectory)
                    .Where(x => x.Contains(videoTitle) && x.EndsWith(FileExtension.Jpg))
                    .ToArray()
                    .Count();

                if (thumbnailsCreated >= _appSettings.ThumbnailFrames)
                {
                    break;
                }
            } // end for
        }

        protected virtual async Task CleanUpBeforeArchivingAsync(string workingDirectory)
        {
            _logger.LogInformation($"Cleaning up before archiving {workingDirectory}");

            DeleteFile(Path.Combine(workingDirectory, "ae_intro.mp4"));
            DeleteFile(Path.Combine(workingDirectory, "ae_outtro2.mp4"));
            DeleteFile(Path.Combine(workingDirectory, "dashcam.txt"));
            DeleteFile(Path.Combine(workingDirectory, "dash_cam_opening.mp4"));
            DeleteFile(Path.Combine(workingDirectory, "services.txt"));
            DeleteFile(Path.Combine(workingDirectory, "subscriber_click.mp4"));
            DeleteFile(Path.Combine(workingDirectory, "title.txt"));
            DeleteFile(Path.Combine(workingDirectory, FFMPEG_INPUT_FILE));

            string[] files = GetFilesInDirectory(workingDirectory)
                .Where(f => f.EndsWith(FileExtension.Gif) ||
                    f.EndsWith(FileExtension.Kdenlive) ||
                    f.EndsWith(FileExtension.Mp3))
                .ToArray();

            foreach (var file in files)
            {
                DeleteFile(Path.Combine(workingDirectory, file));
            }

            string[] directories = GetDirectoriesInDirectory(workingDirectory);
            foreach (var directory in directories)
            {
                DeleteDirectory(directory);
            }

            await Task.CompletedTask;
        }

    }
}
