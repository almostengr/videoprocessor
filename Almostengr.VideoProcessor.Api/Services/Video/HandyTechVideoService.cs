using System;
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
    public class HandyTechVideoService : VideoService, IHandyTechVideoService
    {
        private readonly ILogger<HandyTechVideoService> _logger;
        private readonly IFileSystemService _fileSystem;
        private readonly AppSettings _appSettings;
        private readonly IExternalProcessService _externalProcess;
        private readonly IStatusService _statusService;
        private readonly Random _random = new();

        public HandyTechVideoService(ILogger<HandyTechVideoService> logger, AppSettings appSettings,
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
            await _statusService.SaveChangesAsync();

            string outputTsFile = Path.GetFileNameWithoutExtension(videoProperties.OutputVideoFilePath) + FileExtension.Ts;

            await _externalProcess.RunCommandAsync(
                ProgramPaths.FfmpegBinary,
                $"-y {LOG_ERRORS} -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i {FFMPEG_INPUT_FILE} -filter_hw_device foo -vf \"{videoProperties.VideoFilter}, format=vaapi|nv12,hwupload\" -c:v h264_vaapi -bsf:a aac_adtstoasc \"{videoProperties.OutputVideoFilePath}\"",
                videoProperties.WorkingDirectory,
                cancellationToken,
                240
            );
        }

        public override string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties)
        {
            string[] socialMediaOptions =  {
                "facebook.com/rhtservicesllc",
                "instagram.com/rhtservicesllc",
                "rhtservices.net",
                "Robinson Handy and Technology Services",
                "youtube.com/c/robinsonhandyandtechnologyservices",
            };

            string boxColor = GetBoxColor(videoProperties.VideoTitle);

            string videoFilter = $"drawtext=textfile:'{socialMediaOptions[_random.Next(0, socialMediaOptions.Length)]}':";
            videoFilter += $"fontcolor={FfMpegColors.White}@{DIM_TEXT}:";
            videoFilter += $"fontsize={SMALL_FONT}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={RHT_BORDER_WIDTH.ToString()}:";
            videoFilter += $"boxcolor={boxColor}@{DIM_BACKGROUND}";

            return videoFilter;
        }

        private string GetBoxColor(string videoTitle)
        {
            videoTitle = videoTitle.ToLower();
            bool randomBool = _random.Next(0, 50) >= 25;

            if (videoTitle.Contains("christmas"))
            {
                return randomBool ? FfMpegColors.Green : FfMpegColors.Maroon;
            }

            return randomBool ? FfMpegColors.Black : FfMpegColors.RhtYellow;
        }

        public override void CheckOrCreateFfmpegInputFile(string workingDirectory)
        {
            string ffmpegInputFile = Path.Combine(workingDirectory, FFMPEG_INPUT_FILE);
            string inputFile = Path.Combine(workingDirectory, "input.txt");

            if (_fileSystem.DoesFileExist(inputFile))
            {
                _fileSystem.MoveFile(inputFile, ffmpegInputFile);
            }

            if (_fileSystem.DoesFileExist(ffmpegInputFile))
            {
                return;
            }

            using (StreamWriter writer = new StreamWriter(ffmpegInputFile))
            {
                _logger.LogInformation("Creating FFMPEG input file");
                var filesInDirectory = _fileSystem.GetFilesInDirectory(workingDirectory);

                foreach (string file in filesInDirectory.Where(x => x.EndsWith(FileExtension.Ts)).OrderBy(x => x).ToArray())
                {
                    writer.WriteLine($"file '{Path.GetFileName(file)}'"); // add video files
                }

                foreach (string file in filesInDirectory.Where(x => x.EndsWith(FileExtension.Jpg)).OrderBy(x => x).ToArray())
                {
                    writer.WriteLine($"file '{Path.GetFileName(file)}'"); // add images
                }
            }
        }

        public override async Task ArchiveDirectoryContentsAsync(
            string directoryToArchive, string archiveName, string archiveDestination, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.Archiving);
            await _statusService.SaveChangesAsync();

            await base.ArchiveDirectoryContentsAsync(
                directoryToArchive, archiveName, archiveDestination, cancellationToken);
        }

        public override async Task CleanUpBeforeArchivingAsync(string workingDirectory)
        {
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.Archiving);
            await _statusService.SaveChangesAsync();

            string[] filesToRemove = _fileSystem.GetFilesInDirectory(workingDirectory)
                .Where(x =>
                    x.EndsWith(FileExtension.Ts) == false &&
                    x.EndsWith(FileExtension.Jpg) == false &&
                    x.EndsWith(FFMPEG_INPUT_FILE) == false
                )
                .ToArray();

            _fileSystem.DeleteFiles(filesToRemove);

            string[] directoriesToRemove = _fileSystem.GetDirectoriesInDirectory(workingDirectory);
            _fileSystem.DeleteDirectories(directoriesToRemove);
        }

        public override async Task ExtractTarFileAsync(
            string tarFile, string workingDirectory, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.Extracting);
            await _statusService.UpsertAsync(StatusKeys.RhtFile, tarFile);
            await _statusService.SaveChangesAsync();
            await base.ExtractTarFileAsync(tarFile, workingDirectory, cancellationToken);
        }

        public override async Task WorkerIdleAsync(CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.Idle);
            await _statusService.UpsertAsync(StatusKeys.RhtFile, string.Empty);
            await _statusService.SaveChangesAsync();
            await Task.Delay(TimeSpan.FromMinutes(_appSettings.WorkerIdleInterval), cancellationToken);
        }

        public async Task AddAudioToTimelapseAsync(string workingDirectory, CancellationToken cancellationToken)
        {
            var audioFiles = _fileSystem.GetFilesInDirectory(workingDirectory)
                .Where(x => x.EndsWith(FileExtension.Mp3))
                .ToArray();

            foreach (var file in audioFiles)
            {
                string outputFileName = $"{Path.GetFileNameWithoutExtension(file)}.tmp{FileExtension.Mp4}";
                string videoFileName = _fileSystem.GetFilesInDirectory(workingDirectory)
                    .Where(x => x.Contains(Path.GetFileNameWithoutExtension(file)) && x.EndsWith(FileExtension.Mp3) == false)
                    .SingleOrDefault();

                if (File.Exists(Path.Combine(workingDirectory, videoFileName)) == false)
                {
                    throw new ArgumentNullException($"Video file for {file} was not found");
                }

                var result = await _externalProcess.RunCommandAsync(
                    ProgramPaths.FfmpegBinary,
                    $"-loglevel error -hide_banner -y -hwaccel vaapi -hwaccel_output_format vaapi -i {Path.GetFileName(videoFileName)} -i {Path.GetFileName(file)} -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 {outputFileName}",
                    workingDirectory,
                    cancellationToken,
                    10);

                if (result.stdErr.Length > 0)
                {
                    throw new ApplicationException("Errors occurred when attempting to merge audio and video");
                }

                _fileSystem.DeleteFile(videoFileName);
                _fileSystem.MoveFile(Path.Combine(workingDirectory, outputFileName), videoFileName);
            }
        }

        public async Task ConvertVideoFilesToTsAsync(string directory, CancellationToken stoppingToken)
        {
            var videoFiles = _fileSystem.GetFilesInDirectory(directory)
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
                    $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -c copy -bsf:v h264_mp4toannexb -f mpegts \"{outputFileName}\"",
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
