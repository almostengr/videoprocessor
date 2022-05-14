using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Common;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Constants;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.VideoRender
{
    public abstract class BaseVideoRenderService : BaseService, IBaseVideoRenderService
    {
        private readonly ILogger<BaseVideoRenderService> _logger;
        private readonly AppSettings _appSettings;
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

        public BaseVideoRenderService(ILogger<BaseVideoRenderService> logger, AppSettings appSettings) : base(logger, appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;

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

        public async Task ArchiveDirectoryContentsAsync(string directoryToArchive, string archiveName, string archiveDestination, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Archiving directory contents: {directoryToArchive}");

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ProgramPaths.BashShell,
                    Arguments = $"-c \"cd {directoryToArchive} && tar -cvJf \\\"{Path.Combine(archiveDestination, archiveName)}\\\" *\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);

            _logger.LogInformation(process.StandardOutput.ReadToEnd());
            if (process.StandardError.ReadToEnd().Length > 0)
            {
                _logger.LogError(process.StandardError.ReadToEnd());
            }
        }

        public string[] GetVideoArchivesInDirectory(string directory)
        {
            return base.GetDirectoryContents(directory, $"*{FileExtension.Tar}*");
        }

        public string GetSubtitlesFilter(string workingDirectory)
        {
            string subtitleFile = Path.Combine(workingDirectory, VideoRenderFiles.SubtitlesFile);

            if (File.Exists(subtitleFile))
            {
                return $", subtitles='{subtitleFile}'";
            }

            return string.Empty;
        }

        public async Task ExtractTarFileAsync(string tarFile, string workingDirectory, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Extracting tar file: {tarFile}");

            Process process = new();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = ProgramPaths.TarBinary,
                ArgumentList = {
                    "-xvf",
                    tarFile,
                    "-C",
                    workingDirectory
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
        }

        public virtual async Task RenderVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken)
        {
            if (_appSettings.DoRenderVideos == false)
            {
                return;
            }

            _logger.LogInformation($"Rendering video: {videoProperties.SourceTarFilePath}");

            Process process = new();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = ProgramPaths.FfmpegBinary,
                ArgumentList = {
                    "-hide_banner",
                    "-safe",
                    "0",
                    "-loglevel",
                    FfMpegLogLevel.Error,
                    "-y",
                    "-f",
                    "concat",
                    "-i",
                    videoProperties.FfmpegInputFilePath,
                    "-vf",
                    videoProperties.VideoFilter,
                    videoProperties.OutputVideoFilePath
                },
                WorkingDirectory = videoProperties.WorkingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
        }

        public async Task ConvertVideoFilesToMp4Async(string directory, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Converting video files to mp4: {directory}");

            var nonMp4VideoFiles = Directory.GetFiles(directory)
                .Where(x => x.ToLower().EndsWith(FileExtension.Mkv) || x.ToLower().EndsWith(FileExtension.Mov))
                .OrderBy(x => x).ToArray();

            foreach (var videoFile in nonMp4VideoFiles)
            {
                string outputFilename = Path.GetFileNameWithoutExtension(videoFile) + FileExtension.Mp4;
                _logger.LogInformation($"Converting {videoFile} to {outputFilename}");

                Process process = new();
                process.StartInfo = new ProcessStartInfo()
                {
                    FileName = ProgramPaths.FfmpegBinary,
                    ArgumentList = {
                        "-hide_banner",
                        "-i",
                        Path.Combine(directory, videoFile),
                        Path.Combine(directory, outputFilename)
                    },
                    WorkingDirectory = directory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                process.Start();
                await process.WaitForExitAsync(cancellationToken);

                base.DeleteDirectory(Path.Combine(directory, videoFile));

                _logger.LogInformation($"Done converting {videoFile} to {outputFilename}");
            }
        }

        public void PrepareFileNamesInDirectory(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                File.Move(
                    file,
                    Path.Combine(directory, Path.GetFileName(file).ToLower().Replace(" ", "_"))
                );
            }
        }

        public void CheckOrCreateFfmpegInputFile(string workingDirectory)
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

            for (int sceneChangePct = 80; sceneChangePct > 0; sceneChangePct -= 5)
            {
                Process process = new();
                process.StartInfo = new ProcessStartInfo()
                {
                    FileName = ProgramPaths.FfmpegBinary,
                    ArgumentList = {
                        "-y",
                        "-hide_banner",
                        "-loglevel",
                        FfMpegLogLevel.Warning,
                        "-i",
                        videoProperties.OutputVideoFilePath,
                        "-vf",
                        $"select=gt(scene\\,0.{sceneChangePct})",
                        "-frames:v",
                        _appSettings.ThumbnailFrames.ToString(),
                        "-vsync",
                        "vfr",
                        $"{videoProperties.VideoTitle}-%03d.jpg"
                    },
                    WorkingDirectory = videoProperties.UploadDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                process.Start();
                await process.WaitForExitAsync(cancellationToken);

                _logger.LogInformation(process.StandardOutput.ReadToEnd());
                _logger.LogError(process.StandardError.ReadToEnd());

                int thumbnailsCreated = base.GetDirectoryContents(videoProperties.UploadDirectory, $"{videoProperties.VideoTitle}*{FileExtension.Jpg}").Count();

                if (thumbnailsCreated >= _appSettings.ThumbnailFrames)
                {
                    _logger.LogInformation($"Done creating thumbnails for {videoProperties.OutputVideoFilePath}");
                    break;
                }
            } // end for
        }

        public void CleanUpBeforeArchiving(string workingDirectory)
        {
            _logger.LogInformation($"Cleaning up before archiving: {workingDirectory}");
            base.DeleteFile(Path.Combine(workingDirectory, "title.txt"));
            base.DeleteFile(Path.Combine(workingDirectory, "dashcam.txt"));
            base.DeleteFile(Path.Combine(workingDirectory, "services.txt"));
        }

    }
}
