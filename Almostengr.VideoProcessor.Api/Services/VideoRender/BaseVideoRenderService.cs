using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services.ExternalProcess;
using Almostengr.VideoProcessor.Constants;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.VideoRender
{
    public abstract class BaseVideoRenderService : BaseService, IBaseVideoRenderService
    {
        private readonly ILogger<BaseVideoRenderService> _logger;
        private readonly AppSettings _appSettings;
        private readonly IExternalProcessService _externalProcess;
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

        public BaseVideoRenderService(ILogger<BaseVideoRenderService> logger, AppSettings appSettings,
            IExternalProcessService externalProcess) :
            base(logger, appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;
            _externalProcess = externalProcess;

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

            await _externalProcess.RunProcessAsync(
                ProgramPaths.BashShell,
                $"-c \"cd \\\"{directoryToArchive}\\\" && tar -cvJf \\\"{Path.Combine(archiveDestination, archiveName)}\\\" *\"",
                directoryToArchive,
                cancellationToken,
                10);

            _logger.LogInformation($"Done archiving directory contents: {directoryToArchive}");
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

            await _externalProcess.RunProcessAsync(
                ProgramPaths.TarBinary,
                $"-xvf \"{tarFile}\" -C \"{workingDirectory}\"",
                Path.GetDirectoryName(tarFile),
                cancellationToken,
                10);

            _logger.LogInformation($"Done extracting tar file: {tarFile}");
        }

        public virtual async Task RenderVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken)
        {
            if (_appSettings.DoRenderVideos == false)
            {
                return;
            }

            _logger.LogInformation($"Rendering video: {videoProperties.SourceTarFilePath}");

            await _externalProcess.RunProcessAsync(
                ProgramPaths.FfmpegBinary,
                $"-hide_banner -y -safe 0 -loglevel {FfMpegLogLevel.Error} -f concat -i {videoProperties.FfmpegInputFilePath} -vf {videoProperties.VideoFilter} {videoProperties.OutputVideoFilePath}",
                videoProperties.WorkingDirectory,
                cancellationToken,
                240);

            _logger.LogInformation($"Done rendering video: {videoProperties.SourceTarFilePath}");
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

                await _externalProcess.RunProcessAsync(
                    ProgramPaths.FfmpegBinary,
                    $"-hide_banner -y -loglevel {FfMpegLogLevel.Error} -i {Path.GetFileName(videoFile)} {Path.GetFileName(outputFilename)}",
                    directory,
                    cancellationToken,
                    20);

                base.DeleteFile(videoFile);

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

            for (int sceneChangePct = 90; sceneChangePct > 0; sceneChangePct -= 10)
            {
                await _externalProcess.RunProcessAsync(
                    ProgramPaths.FfmpegBinary,
                    $"-hide_banner -y -loglevel {FfMpegLogLevel.Warning} -i {videoProperties.OutputVideoFilePath} -vf select=gt(scene\\,0.{sceneChangePct}) -frames:v {_appSettings.ThumbnailFrames.ToString()} -vsync vfr {videoProperties.VideoTitle}-%03d.jpg",
                    videoProperties.UploadDirectory,
                    cancellationToken,
                    240);

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

            base.DeleteFile(Path.Combine(workingDirectory, "ae_intro.mp4"));
            base.DeleteFile(Path.Combine(workingDirectory, "ae_outtro2.mp4"));
            base.DeleteFile(Path.Combine(workingDirectory, "dashcam.txt"));
            base.DeleteFile(Path.Combine(workingDirectory, "services.txt"));
            base.DeleteFile(Path.Combine(workingDirectory, "subscriber_click.mp4"));
            base.DeleteFile(Path.Combine(workingDirectory, "title.txt"));

            string[] files = Directory.GetFiles(workingDirectory);
            foreach (var file in files)
            {
                if (
                    file.EndsWith($".{FileExtension.Gif}") ||
                    file.EndsWith($".{FileExtension.Kdenlive}") || 
                    file.EndsWith($".{FileExtension.Mp3}")
                    )
                {
                    base.DeleteFile(Path.Combine(workingDirectory, file));
                }
            }

            base.DeleteDirectory(Path.Combine(workingDirectory, "images"));
            base.DeleteDirectory(Path.Combine(workingDirectory, "sounds"));
            base.DeleteDirectory(Path.Combine(workingDirectory, "videos"));
        }

    }
}
