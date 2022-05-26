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

namespace Almostengr.VideoProcessor.Api.Services.VideoRender
{
    public abstract class VideoRenderService : IVideoRenderService
    {
        private readonly ILogger<VideoRenderService> _logger;
        private readonly AppSettings _appSettings;
        private readonly IExternalProcessService _externalProcess;
        private readonly IFileSystemService _fileSystem;
        private const int PADDING = 30;
        internal readonly string _subscribeFilter;
        internal readonly string _subscribeScrollingFilter;
        internal readonly string _upperLeft;
        internal readonly string _upperCenter;
        internal readonly string _upperRight;
        internal readonly string _centered;
        internal readonly string _lowerLeft;
        internal readonly string _lowerCenter;
        internal readonly string _lowerRight;

        public VideoRenderService(ILogger<VideoRenderService> logger, AppSettings appSettings,
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

            _subscribeFilter = $"drawtext=text:'SUBSCRIBE':fontcolor={FfMpegColors.White}:fontsize={FfMpegConstants.FontSizeSmall}:{_lowerLeft}:boxcolor={FfMpegColors.Red}:box=1:boxborderw=10";
            _subscribeScrollingFilter = $"drawtext=text:'SUBSCRIBE':fontcolor={FfMpegColors.White}:fontsize={FfMpegConstants.FontSizeSmall}:x=w+(100*t):y=h-th-{PADDING}:boxcolor={FfMpegColors.Red}:box=1:boxborderw=10";
        }

        public abstract string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties);

        public virtual async Task ArchiveDirectoryContentsAsync(string directoryToArchive, string archiveName, string archiveDestination, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Archiving directory contents: {directoryToArchive}");

            await _externalProcess.RunProcessAsync(
                ProgramPaths.BashShell,
                $"-c \"cd \\\"{directoryToArchive}\\\" && tar -cvJf \\\"{Path.Combine(archiveDestination, archiveName)}\\\" *\"",
                directoryToArchive,
                cancellationToken,
                10);
        }

        public virtual string[] GetVideoArchivesInDirectory(string directory)
        {
            return _fileSystem.GetDirectoryContents(directory, $"*{FileExtension.Tar}*");
        }

        public virtual string GetSubtitlesFilter(string workingDirectory)
        {
            string subtitleFile = Path.Combine(workingDirectory, VideoRenderFiles.SubtitlesFile);

            if (File.Exists(subtitleFile))
            {
                return $", subtitles='{subtitleFile}'";
            }

            return string.Empty;
        }

        public virtual async Task ExtractTarFileAsync(string tarFile, string workingDirectory, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Extracting tar file: {tarFile}");

            await _externalProcess.RunProcessAsync(
                ProgramPaths.TarBinary,
                $"-xvf \"{tarFile}\" -C \"{workingDirectory}\"",
                Path.GetDirectoryName(tarFile),
                cancellationToken,
                10);
        }

        public abstract Task RenderVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken);

        public virtual async Task ConvertVideoFilesToMp4Async(string directory, CancellationToken cancellationToken)
        {
            var nonMp4VideoFiles = Directory.GetFiles(directory)
                .Where(x => x.ToLower().EndsWith(FileExtension.Mkv) || x.ToLower().EndsWith(FileExtension.Mov))
                .OrderBy(x => x).ToArray();

            foreach (var videoFile in nonMp4VideoFiles)
            {
                string outputFilename = Path.GetFileNameWithoutExtension(videoFile) + FileExtension.Mp4;
                _logger.LogInformation($"Converting {videoFile} to {outputFilename}");

                await _externalProcess.RunProcessAsync(
                    ProgramPaths.FfmpegBinary,
                    $"-loglevel {FfMpegLogLevel.Error} -hide_banner -y -hwaccel vaapi -hwaccel_output_format vaapi -i {Path.GetFileName(videoFile)} -f mp4 -vcodec h264_vaapi -b:v 5M {Path.GetFileName(outputFilename)}",
                    directory,
                    cancellationToken,
                    20);

                _fileSystem.DeleteFile(videoFile);
            }
        }

        public virtual void PrepareFileNamesInDirectory(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
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
            string ffmpegInputFile = Path.Combine(workingDirectory, VideoRenderFiles.InputFile);
            string inputFile = Path.Combine(workingDirectory, "input.txt");

            if (File.Exists(inputFile))
            {
                File.Move(inputFile, ffmpegInputFile);
            }

            if (File.Exists(ffmpegInputFile) == false)
            {
                using (StreamWriter writer = new StreamWriter(ffmpegInputFile))
                {
                    foreach (string file in Directory.GetFiles(workingDirectory, $"*{FileExtension.Mp4}").OrderBy(x => x))
                    {
                        writer.WriteLine($"file '{Path.GetFileName(file)}'");
                    }
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

                await _externalProcess.RunProcessAsync(
                    ProgramPaths.FfmpegBinary,
                    $"-hide_banner -y -loglevel {FfMpegLogLevel.Warning} -i {videoProperties.OutputVideoFilePath} -vf select=gt(scene\\,0.{sceneChangePct}) -frames:v {_appSettings.ThumbnailFrames.ToString()} -vsync vfr {videoProperties.VideoTitle}-%03d.jpg",
                    videoProperties.UploadDirectory,
                    cancellationToken,
                    240);

                int thumbnailsCreated = _fileSystem.GetDirectoryContents(videoProperties.UploadDirectory, $"{videoProperties.VideoTitle}*{FileExtension.Jpg}").Count();

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

            string[] files = Directory.GetFiles(workingDirectory)
                .Where(f => f.EndsWith(FileExtension.Gif) || 
                    f.EndsWith(FileExtension.Kdenlive) || 
                    f.EndsWith(FileExtension.Mp3))
                .ToArray();

            foreach (var file in files)
            {
                _fileSystem.DeleteFile(Path.Combine(workingDirectory, file));
            }

            string[] directories = Directory.GetDirectories(workingDirectory);
            foreach (var directory in directories)
            {
                _fileSystem.DeleteDirectory(directory);
            }

            await Task.CompletedTask;
        }

        public abstract Task StandByModeAsync(CancellationToken cancellationToken);
    }
}
