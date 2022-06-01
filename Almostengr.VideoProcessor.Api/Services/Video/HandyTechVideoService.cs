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
            string videoFilter = $"drawtext=textfile:'{GetBrandingText()}':";
            videoFilter += $"fontcolor={FfMpegColors.White}@{DIM_TEXT}:";
            videoFilter += $"fontsize={SMALL_FONT}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={RHT_BORDER_WIDTH.ToString()}:";
            videoFilter += $"boxcolor={GetBoxColor(videoProperties.VideoTitle)}@{DIM_BACKGROUND}";

            return videoFilter;
        }

        private string GetBrandingText()
        {
            string[] socialMediaOptions =  {
                "facebook.com/rhtservicesllc",
                "instagram.com/rhtservicesllc",
                "rhtservices.net",
                "Robinson Handy and Technology Services",
                "youtube.com/c/robinsonhandyandtechnologyservices",
            };

            return socialMediaOptions[_random.Next(0, socialMediaOptions.Length)];
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
            _fileSystem.DeleteFile(ffmpegInputFile);

            using (StreamWriter writer = new StreamWriter(ffmpegInputFile))
            {
                _logger.LogInformation("Creating FFMPEG input file");
                // var filesInDirectory = _fileSystem.GetFilesInDirectory(workingDirectory);
                var filesInDirectory = _fileSystem.GetFilesInDirectory(workingDirectory)
                    .Where(x => x.EndsWith(FileExtension.Ts)).OrderBy(x => x).ToArray();

                int showIntroPosition = _random.Next(1, 3);

                // foreach (string file in filesInDirectory.Where(x => x.EndsWith(FileExtension.Ts)).OrderBy(x => x).ToArray())
                // {
                //     writer.WriteLine($"file '{Path.GetFileName(file)}'"); // add video files
                // }

                // string[] files = filesInDirectory.Where(x => x.EndsWith(FileExtension.Ts)).OrderBy(x => x).ToArray();
                for (int i = 1; i < filesInDirectory.Length; i++)
                {
                    writer.WriteLine($"file '{Path.GetFileName(filesInDirectory[i])}'"); // add video files

                    if (i == showIntroPosition)
                    {
                        string introClip = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/RHT Services/ytvideostructure/rhtservicesintro.ts";
                        writer.WriteLine($"file '{Path.GetFileName(introClip)}'"); // add video files
                    }
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
            var narrationFiles = _fileSystem.GetFilesInDirectory(workingDirectory)
                .Where(x => x.Contains("narration") && (x.EndsWith(FileExtension.Mp4) || x.EndsWith(FileExtension.Mkv)))
                .ToArray();

            foreach (var narrationFile in narrationFiles)
            {
                string mp3FileName = Path.GetFileNameWithoutExtension(narrationFile).Replace("narration", string.Empty) + FileExtension.Mp3;
                _logger.LogInformation($"Converting narration file {narrationFile} to {mp3FileName}");

                await _externalProcess.RunCommandAsync(
                    ProgramPaths.FfmpegBinary,
                    $"{LOG_ERRORS} -hide_banner -y -i \"{narrationFile}\" -vn \"{mp3FileName}\"",
                    workingDirectory,
                    cancellationToken,
                    5);

                _fileSystem.DeleteFile(narrationFile);
            }

            var audioFiles = _fileSystem.GetFilesInDirectory(workingDirectory)
                .Where(x => x.EndsWith(FileExtension.Mp3))
                .ToArray();

            foreach (var audioFile in audioFiles)
            {
                string outputFileName = $"{Path.GetFileNameWithoutExtension(audioFile)}.tmp{FileExtension.Mp4}";
                string videoFileName = _fileSystem.GetFilesInDirectory(workingDirectory)
                    .Where(x => x.Contains(Path.GetFileNameWithoutExtension(audioFile)) && x.EndsWith(FileExtension.Mp3) == false)
                    .SingleOrDefault();

                if (File.Exists(Path.Combine(workingDirectory, videoFileName)) == false)
                {
                    throw new ArgumentNullException($"Video file for {audioFile} was not found");
                }

                await _externalProcess.RunCommandAsync(
                    ProgramPaths.FfmpegBinary,
                    $"{LOG_ERRORS} {HW_OPTIONS} -i {Path.GetFileName(videoFileName)} -i {Path.GetFileName(audioFile)} -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 {outputFileName}",
                    workingDirectory,
                    cancellationToken,
                    10);

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

                _fileSystem.DeleteFile(videoFileName);
            }
        }

    }
}
