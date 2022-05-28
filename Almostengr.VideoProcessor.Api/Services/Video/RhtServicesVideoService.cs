using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Enums;
using Almostengr.VideoProcessor.Api.Services.Data;
using Almostengr.VideoProcessor.Api.Services.ExternalProcess;
using Almostengr.VideoProcessor.Api.Services.FileSystem;
using Almostengr.VideoProcessor.Constants;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.Video
{
    public class RhtServicesVideoService : VideoService, IRhtServicesVideoService
    {
        private readonly ILogger<RhtServicesVideoService> _logger;
        private readonly IFileSystemService _fileSystem;
        private readonly AppSettings _appSettings;
        private readonly IExternalProcessService _externalProcess;
        private readonly IStatusService _statusService;

        public RhtServicesVideoService(ILogger<RhtServicesVideoService> logger, AppSettings appSettings,
            IExternalProcessService externalProcess, IFileSystemService fileSystem,
            IStatusService statusService) :
             base(logger, appSettings, externalProcess, fileSystem)
        {
            _appSettings = appSettings;
            _fileSystem = fileSystem;
            _externalProcess = externalProcess;
            _logger = logger;
            _statusService = statusService;
        }

        public override async Task RenderVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken)
        {
            if (_appSettings.DoRenderVideos == false)
            {
                return;
            }

            _logger.LogInformation($"Rendering {videoProperties.SourceTarFilePath}");
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, nameof(StatusValues.Rendering));

            await _externalProcess.RunCommandAsync(
                ProgramPaths.FfmpegBinary,
                // $"{HW_OPTIONS} -f concat -i {FFMPEG_INPUT_FILE} -filter_complex \"[0:v]{videoProperties.VideoFilter}[v]\" -map \"[v]\" -map 0:a -c:a copy {videoProperties.OutputVideoFilePath}",
                $"-safe 0 {LOG_ERRORS} {HW_OPTIONS} -f concat -i {FFMPEG_INPUT_FILE} -c copy -bsf:a aac_adtstoasc {videoProperties.OutputVideoFilePath}",
                videoProperties.WorkingDirectory,
                cancellationToken,
                240
            );

            string brandedOutputVideo = $"{Path.GetFileNameWithoutExtension(videoProperties.OutputVideoFilePath)}.branded.mp4";

            await _externalProcess.RunCommandAsync(
                ProgramPaths.FfmpegBinary,
                $"{LOG_ERRORS} {HW_OPTIONS} -i {videoProperties.OutputVideoFilePath} -vf {videoProperties.VideoFilter} {brandedOutputVideo}",
                videoProperties.WorkingDirectory,
                cancellationToken,
                240
            );
        }

        public override string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties)
        {
            List<string> socialMediaOptions = new List<string> {
                "facebook.com/rhtservicesllc",
                "instagram.com/rhtservicesllc",
                "rhtservices.net",
                "rhtservices.net/facebook",
                "rhtservices.net/instagram",
                "rhtservices.net/nextdoor",
                "rhtservices.net/youtube",
                "Robinson Handy and Technology Services",
            };

            List<string> positionOptions = new List<string> {
                _lowerRight,
                _upperRight
            };

            Random random = new();

            string videoFilter = $"drawtext=textfile:'{socialMediaOptions[random.Next(0, socialMediaOptions.Count)]}':";
            videoFilter += $"fontcolor={FfMpegColors.White}@{DIM_BACKGROUND}:";
            videoFilter += $"fontsize={SMALL_FONT}:";
            videoFilter += $"{positionOptions[random.Next(0, positionOptions.Count)]}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={RHT_BORDER_WIDTH.ToString()}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}@{DIM_BACKGROUND}";

            return videoFilter;
        }

        public override void CheckOrCreateFfmpegInputFile(string workingDirectory)
        {
            string ffmpegInputFile = Path.Combine(workingDirectory, FFMPEG_INPUT_FILE);
            string inputFile = Path.Combine(workingDirectory, "input.txt");

            if (File.Exists(inputFile))
            {
                File.Move(inputFile, ffmpegInputFile);
            }

            if (File.Exists(ffmpegInputFile))
            {
                return;
            }

            using (StreamWriter writer = new StreamWriter(ffmpegInputFile))
            {
                _logger.LogInformation("Creating FFMPEG input file");
                foreach (string file in Directory.GetFiles(workingDirectory, $"*{FileExtension.Ts}").OrderBy(x => x))
                {
                    writer.WriteLine($"file '{Path.GetFileName(file)}'");
                }
            }
        }

        public override async Task ArchiveDirectoryContentsAsync(string directoryToArchive, string archiveName, string archiveDestination, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.Archiving);
            await base.ArchiveDirectoryContentsAsync(
                directoryToArchive, archiveDestination, archiveDestination, cancellationToken);
        }

        public override async Task CleanUpBeforeArchivingAsync(string workingDirectory)
        {
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.Archiving);
            await base.CleanUpBeforeArchivingAsync(workingDirectory);
        }

        public override async Task ConvertVideoFilesToMp4Async(string directory, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.ConvertingToMp4);
            await base.ConvertVideoFilesToMp4Async(directory, cancellationToken);
        }

        public override async Task ExtractTarFileAsync(string tarFile, string workingDirectory, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.Extracting);
            await _statusService.UpsertAsync(StatusKeys.RhtFile, tarFile);
            await base.ExtractTarFileAsync(tarFile, workingDirectory, cancellationToken);
        }

        public override async Task WorkerIdleAsync(CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.Idle);
            await _statusService.UpsertAsync(StatusKeys.RhtFile, string.Empty);
            await Task.Delay(TimeSpan.FromMinutes(_appSettings.WorkerServiceInterval), cancellationToken);
        }

        public async Task AddAudioToTimelapseAsync(string workingDirectory, CancellationToken cancellationToken)
        {
            var goProFiles = _fileSystem.GetDirectoryContents(workingDirectory, "")
                .Where(x => x.StartsWith("gh") && x.EndsWith(FileExtension.Mp4))
                .ToArray();

            foreach (var videoFileName in goProFiles)
            {
                string outputFileName = $"{Path.GetFileNameWithoutExtension(videoFileName)}.tmp{FileExtension.Mp4}";
                string audioFileName = $"{Path.GetFileNameWithoutExtension(videoFileName)}{FileExtension.Mp3}";

                if (File.Exists(Path.Combine(workingDirectory, audioFileName)) == false)
                {
                    throw new ArgumentNullException($"Audio file for {audioFileName} was not found");
                }

                var result = await _externalProcess.RunCommandAsync(
                    ProgramPaths.FfmpegBinary,
                    $"-loglevel error -hide_banner -y -hwaccel vaapi -hwaccel_output_format vaapi -i {videoFileName} -i {audioFileName} -vcodec h264_vaapi -map 0:v:0 -map 1:a:0 {outputFileName}",
                    workingDirectory,
                    cancellationToken,
                    10);

                if (result.stdErr.Length > 0)
                {
                    throw new ApplicationException("Errors occurred when attempting to merge audio and video");
                }

                _fileSystem.MoveFile(outputFileName, videoFileName);
            }
        }

        public async Task ConvertVideoFilesToTsAsync(string directory, CancellationToken stoppingToken)
        {
            var videoFiles = Directory.GetFiles(directory)
                .Where(x =>
                    x.EndsWith(FileExtension.Mkv) ||
                    x.EndsWith(FileExtension.Mov) ||
                    x.EndsWith(FileExtension.Mp4)
                )
                .OrderBy(x => x).ToArray();

            foreach (var videoFileName in videoFiles)
            {
                string outputFileName = Path.GetFileNameWithoutExtension(videoFileName) + FileExtension.Ts;
                _logger.LogInformation($"Converting {videoFileName} to {outputFileName}");

                var result = await _externalProcess.RunCommandAsync(
                    ProgramPaths.FfmpegBinary,
                    $"{LOG_ERRORS} {HW_OPTIONS} -i {videoFileName} -c copy -bsf:v h264_mp4toannexb -f mpegts {outputFileName}",
                    directory,
                    stoppingToken,
                    10
                );

                if (result.stdErr.Length > 0)
                {
                    throw new ApplicationException("Error occurred when converting to TS format");
                }

                _fileSystem.DeleteFile(videoFileName);
            }
        }

    }
}
