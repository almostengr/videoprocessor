using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Configuration;
using Almostengr.VideoProcessor.Core.Music;
using Almostengr.VideoProcessor.Core.Status;
using Almostengr.VideoProcessor.Core.VideoCommon;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Core.VideoHandyTech
{
    public sealed class HandyTechVideoService : VideoService, IHandyTechVideoService
    {
        private readonly ILogger<HandyTechVideoService> _logger;
        private readonly string _incomingDirectory;
        private readonly string _archiveDirectory;
        private readonly string _uploadDirectory;
        private readonly string _workingDirectory;
        private readonly AppSettings _appSettings;
        private readonly IStatusService _statusService;
        private readonly IMusicService _musicService;
        private const string _xResolution = "1920";
        private const string _yResolution = "1080";
        private const string _audioBitRate = "196000";
        private const string _audioSampleRate = "48000";
        private const string NO_INTRO_FILE = "nointro.txt";
        private const string SHOW_INTRO_FILENAME_MP4 = "rhtservicesintro.mp4";

        public HandyTechVideoService(ILogger<HandyTechVideoService> logger, AppSettings appSettings,
            IStatusService statusService, IMusicService musicService) :
            base(logger, appSettings)
        {
            _appSettings = appSettings;
            _logger = logger;
            _statusService = statusService;
            _musicService = musicService;

            _incomingDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "incoming");
            _archiveDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "archive");
            _uploadDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "upload");
            _workingDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "working");
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            CreateDirectory(_incomingDirectory);
            CreateDirectory(_archiveDirectory);
            CreateDirectory(_uploadDirectory);
            CreateDirectory(_workingDirectory);
            await Task.CompletedTask;
        }

        public override async Task ExecuteServiceAsync(string videoArchive, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Processing {videoArchive}");

                DeleteDirectory(_workingDirectory);
                CreateDirectory(_workingDirectory);

                await ConfirmFileTransferCompleteAsync(videoArchive);

                await ExtractTarFileAsync(videoArchive, _workingDirectory, cancellationToken);

                PrepareFileNamesInDirectory(_workingDirectory);

                await AddAudioToTimelapseAsync(_workingDirectory, cancellationToken); // add audio to gopro timelapse files

                CopyShowIntroToWorkingDirectory(_appSettings.Directories.IntroVideoPath, _workingDirectory);

                await ConvertVideoFilesToCommonFormatAsync(_workingDirectory, cancellationToken);

                CheckOrCreateFfmpegInputFile();

                string videoTitle = GetVideoTitleFromArchiveName(videoArchive);
                string videoFilter = GetFfmpegVideoFilters(videoTitle);

                string videoOutputPath = GetVideoOutputPath(_uploadDirectory, videoTitle);
                await RenderVideoAsync(videoTitle, videoOutputPath, videoFilter, cancellationToken);

                await CreateThumbnailsFromFinalVideoAsync(
                    videoOutputPath, _uploadDirectory, videoTitle, cancellationToken);

                await CleanUpBeforeArchivingAsync(_workingDirectory);

                string archiveTarFile = GetArchiveTarFileName(videoTitle);

                await ArchiveDirectoryContentsAsync(
                    _workingDirectory, archiveTarFile, _archiveDirectory, cancellationToken);

                DeleteFile(videoArchive);

                DeleteDirectory(_workingDirectory);

                _logger.LogInformation($"Finished processing {videoArchive}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException, ex.Message);
            }
        }

        protected override async Task RenderVideoAsync(string videoTitle, string videoOutputPath, string videoFilter, CancellationToken cancellationToken)
        {
            if (_appSettings.DoRenderVideos == false)
            {
                return;
            }

            _logger.LogInformation($"Rendering {videoOutputPath}");
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, nameof(StatusValues.Rendering));
            await _statusService.SaveChangesAsync();

            string outputTsFile = Path.GetFileNameWithoutExtension(videoOutputPath) + FileExtension.Ts;

            await RunCommandAsync(
                ProgramPaths.FfmpegBinary,
                $"-y {LOG_ERRORS} -safe 0 -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i {FFMPEG_INPUT_FILE} -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -c:v h264_vaapi -bsf:a aac_adtstoasc \"{videoOutputPath}\"",
                _workingDirectory,
                cancellationToken,
                240
            );
        }

        protected override string GetFfmpegVideoFilters(string videoTitle)
        {
            string videoFilter = $"drawtext=textfile:'{GetBrandingText()}':";
            videoFilter += $"fontcolor={FfMpegColors.White}@{DIM_TEXT}:";
            videoFilter += $"fontsize={SMALL_FONT}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={RHT_BORDER_WIDTH.ToString()}:";
            videoFilter += $"boxcolor={GetBoxColor(videoTitle)}@{DIM_BACKGROUND}";

            return videoFilter;
        }

        protected override string GetBrandingText()
        {
            string[] socialMediaOptions =  {
                "Robinson Handy and Technology Services",
                "rhtservices.net",
                "rhtservices.net/facebook",
                "rhtservices.net/instagram",
                "rhtservices.net/recommended-tools",
                "rhtservices.net/services",
                "rhtservices.net/youtube",
            };

            return socialMediaOptions[_random.Next(0, socialMediaOptions.Length)];
        }

        private string GetBoxColor(string videoTitle)
        {
            videoTitle = videoTitle.ToLower();

            if (videoTitle.Contains("christmas"))
            {
                return _random.Next(0, 50) >= 25 ? FfMpegColors.Green : FfMpegColors.Maroon;
            }

            return FfMpegColors.Black;
        }

        protected override void CheckOrCreateFfmpegInputFile()
        {
            string ffmpegInputFile = Path.Combine(_workingDirectory, FFMPEG_INPUT_FILE);
            DeleteFile(ffmpegInputFile);

            using (StreamWriter writer = new StreamWriter(ffmpegInputFile))
            {
                _logger.LogInformation("Creating FFMPEG input file");
                var filesInDirectory = GetFilesInDirectory(_workingDirectory)
                    .Where(x => x.EndsWith(FileExtension.Mp4))
                    .OrderBy(x => x)
                    .ToArray();

                string rhtservicesintro = "rhtservicesintro.1920x1080.mp4";
                for (int i = 0; i < filesInDirectory.Length; i++)
                {
                    if (filesInDirectory[i].Contains(rhtservicesintro))
                    {
                        continue;
                    }

                    if (i == 1 && DoesFileExist(Path.Combine(_workingDirectory, NO_INTRO_FILE)) == false)
                    {
                        writer.WriteLine($"file '{rhtservicesintro}'"); // add video files
                    }

                    writer.WriteLine($"file '{Path.GetFileName(filesInDirectory[i])}'"); // add video files
                }
            }
        }

        protected override async Task ArchiveDirectoryContentsAsync(
            string directoryToArchive, string archiveName, string archiveDestination, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.Archiving);
            await _statusService.SaveChangesAsync();

            await base.ArchiveDirectoryContentsAsync(
                directoryToArchive, archiveName, archiveDestination, cancellationToken);
        }

        protected override async Task CleanUpBeforeArchivingAsync(string workingDirectory)
        {
            _logger.LogInformation($"Cleaning up before archiving {workingDirectory}");

            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.Archiving);
            await _statusService.SaveChangesAsync();

            DeleteFile(Path.Combine(workingDirectory, "dash_cam_opening.mp4"));
            DeleteFile(Path.Combine(workingDirectory, "subscriber_click.mp4"));

            string[] filesToRemove = GetFilesInDirectory(workingDirectory)
                .Where(x => !x.EndsWith(FileExtension.Mp4) ||
                    x.Contains("rhtservicesintro"))
                .ToArray();

            DeleteFiles(filesToRemove);
            DeleteDirectories(GetDirectoriesInDirectory(workingDirectory));

            await Task.CompletedTask;
        }

        protected override async Task ExtractTarFileAsync(
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

        protected async Task AddAudioToTimelapseAsync(string workingDirectory, CancellationToken cancellationToken)
        {
            var videoFiles = GetFilesInDirectory(workingDirectory)
                .Where(x => !x.Contains("narration") || !x.Contains("narrative"))
                .Where(x => x.EndsWith(FileExtension.Mp4));

            var narrationFiles = GetFilesInDirectory(workingDirectory)
                .Where(x => x.Contains("narration") || x.Contains("narrative"))
                .Where(x => x.EndsWith(FileExtension.Mp4) || x.EndsWith(FileExtension.Mkv))
                .ToArray();

            foreach (var videoFileName in videoFiles)
            {
                var result = await RunCommandAsync(
                    ProgramPaths.FfprobeBinary,
                    $"-hide_banner \"{videoFileName}\"",
                    workingDirectory,
                    cancellationToken,
                    1
                );

                if (result.stdErr.ToLower().Contains("audio"))
                {
                    continue;
                }

                string audioFile = narrationFiles.Where(
                        x => x.Contains(Path.GetFileNameWithoutExtension(videoFileName))
                    )
                    .SingleOrDefault();

                if (string.IsNullOrEmpty(audioFile))
                {
                    audioFile = _musicService.GetRandomNonMixTrack();
                }

                string outputFileName = $"{Path.GetFileNameWithoutExtension(videoFileName)}.tmp{FileExtension.Mp4}";

                outputFileName = outputFileName.Replace("narration", string.Empty)
                    .Replace("narrative", string.Empty);

                await RunCommandAsync(
                    ProgramPaths.FfmpegBinary,
                    $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -i \"{audioFile}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
                    workingDirectory,
                    cancellationToken,
                    10);

                DeleteFile(videoFileName);
                MoveFile(Path.Combine(workingDirectory, outputFileName), videoFileName);
            }

            DeleteFiles(narrationFiles);
        }

        protected override async Task ConvertVideoFilesToCommonFormatAsync(string directory, CancellationToken cancellationToken)
        {
            var videoFiles = GetFilesInDirectory(directory)
                .Where(x => x.EndsWith(FileExtension.Mkv) || x.EndsWith(FileExtension.Mov) || x.EndsWith(FileExtension.Mp4))
                .OrderBy(x => x)
                .ToArray();

            foreach (var videoFileName in videoFiles)
            {
                _logger.LogInformation($"Checking resolution of {videoFileName}");

                var result = await RunCommandAsync(
                    ProgramPaths.FfprobeBinary,
                    $"-hide_banner \"{videoFileName}\"",
                    directory,
                    cancellationToken,
                    1
                );

                string scaledFile = $"{Path.GetFileNameWithoutExtension(videoFileName)}.{_xResolution}x{_yResolution}{FileExtension.Mp4}";

                if (result.stdErr.Contains($"{_xResolution}x{_yResolution}") &&
                    result.stdErr.Contains($"{_audioBitRate} Hz") &&
                    result.stdErr.Contains($"196 kb/s") &&
                    videoFileName.EndsWith(FileExtension.Mp4))
                {
                    MoveFile(Path.Combine(directory, videoFileName), Path.Combine(directory, scaledFile));
                    continue;
                }

                _logger.LogInformation($"Converting video {videoFileName} to common format");

                await RunCommandAsync(
                    ProgramPaths.FfmpegBinary,
                    $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -r 30 -vf \"scale_vaapi=w={_xResolution}:h={_yResolution}\" -vcodec h264_vaapi -ar {_audioSampleRate} -b:a {_audioBitRate} \"{scaledFile}\"",
                    directory,
                    cancellationToken,
                    10
                );

                DeleteFile(Path.Combine(directory, videoFileName));

                string outputFileName = Path.GetFileNameWithoutExtension(videoFileName) + FileExtension.Mp4;
            }
        }

        private void CopyShowIntroToWorkingDirectory(string introVideoPath, string workingDirectory)
        {
            CopyFile(introVideoPath, Path.Combine(workingDirectory, SHOW_INTRO_FILENAME_MP4));
        }

    }
}