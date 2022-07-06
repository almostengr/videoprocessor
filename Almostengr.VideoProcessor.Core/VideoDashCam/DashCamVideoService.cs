using Almostengr.VideoProcessor.Core.Configuration;
using Almostengr.VideoProcessor.Core.Music;
using Microsoft.Extensions.Logging;
using Almostengr.VideoProcessor.Core.Status;
using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.VideoCommon;

namespace Almostengr.VideoProcessor.Core.VideoDashCam
{
    public sealed class DashCamVideoService : VideoService, IDashCamVideoService
    {
        private readonly string _streetSignTextSubfilter;
        private readonly ILogger<DashCamVideoService> _logger;
        private readonly IMusicService _musicService;
        private readonly IStatusService _statusService;
        private readonly AppSettings _appSettings;

        // essential files
        private const string DESTINATION_FILE = "destination.txt";
        private const string MAJOR_ROADS_FILE = "majorroads.txt";

        private readonly string _incomingDirectory;
        private readonly string _archiveDirectory;
        private readonly string _uploadDirectory;
        private readonly string _workingDirectory;

        public DashCamVideoService(ILogger<DashCamVideoService> logger, AppSettings appSettings,
           IStatusService statusService, IMusicService musicService) :
          base(logger, appSettings)
        {
            _streetSignTextSubfilter = $"fontcolor=white:fontsize={LARGE_FONT}:{_lowerCenter}:box=1:boxborderw={DASHCAM_BORDER_WIDTH}:boxcolor={FfMpegColors.Green}";

            _logger = logger;
            _musicService = musicService;
            _statusService = statusService;
            _appSettings = appSettings;

            _incomingDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "incoming");
            _archiveDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "archive");
            _uploadDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "upload");
            _workingDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "working");
        }

        protected override string GetBrandingText()
        {
            return "Kenny Ram Dash Cam";
        }

        public override async Task ExecuteServiceAsync(string videoArchivePath, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Processing {videoArchivePath}");

                DeleteDirectory(_workingDirectory);
                CreateDirectory(_workingDirectory);

                await ConfirmFileTransferCompleteAsync(videoArchivePath);

                await ExtractTarFileAsync(videoArchivePath, _workingDirectory, cancellationToken);

                PrepareFileNamesInDirectory(_workingDirectory);

                await ConvertVideoFilesToCommonFormatAsync(_workingDirectory, cancellationToken);

                CheckOrCreateFfmpegInputFile(_workingDirectory);

                string videoTitle = GetVideoTitleFromArchiveName(videoArchivePath);

                string videoFilter = GetFfmpegVideoFilters(videoTitle);
                videoFilter += GetDestinationFilter();
                videoFilter += GetMajorRoadsFilter();

                string videoOutputPath = GetVideoOutputPath(_uploadDirectory, videoTitle);
                await RenderVideoAsync(videoTitle, videoOutputPath, videoFilter, cancellationToken);

                await CleanUpBeforeArchivingAsync(_workingDirectory);

                string archiveTarFile = GetArchiveTarFileName(videoTitle);

                await ArchiveDirectoryContentsAsync(
                    _workingDirectory, archiveTarFile, _archiveDirectory, cancellationToken);

                DeleteFile(videoArchivePath);

                DeleteDirectory(_workingDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException, ex.Message);
            }
        }

        protected override string GetFfmpegVideoFilters(string videoTitle)
        {
            int randomDuration = _random.Next(5, 16);
            string textColor = GetTextColor(videoTitle);
            string brandingText = GetBrandingText();

            // solid text - channel name
            string videoFilter = string.Empty;
            // videoFilter += $"drawtext=textfile:'{brandingText}':";
            // videoFilter += $"fontcolor={textColor}:";
            // videoFilter += $"fontsize={SMALL_FONT}:";
            // videoFilter += $"{_upperRight}:";
            // videoFilter += $"box=1:";
            // videoFilter += $"boxborderw={DASHCAM_BORDER_WIDTH}:";
            // videoFilter += $"boxcolor={FfMpegColors.Black}:";
            // videoFilter += $"enable='between(t,0,{randomDuration})', ";

            // dimmed text - channel name
            videoFilter += $"drawtext=textfile:'{brandingText}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={SMALL_FONT}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={DASHCAM_BORDER_WIDTH}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}@{DIM_BACKGROUND}:";
            videoFilter += $"enable='gt(t,0)', ";
            // videoFilter += $"enable='gt(t,{randomDuration})', ";

            // solid text - video title
            // videoFilter += $"drawtext=textfile:'{videoTitle.Split(";")[0]}':";
            // videoFilter += $"fontcolor={textColor}:";
            // videoFilter += $"fontsize={SMALL_FONT}:";
            // videoFilter += $"{_upperLeft}:";
            // videoFilter += $"box=1:";
            // videoFilter += $"boxborderw={DASHCAM_BORDER_WIDTH}:";
            // videoFilter += $"boxcolor={FfMpegColors.Black}:";
            // videoFilter += $"enable='between(t,0,{randomDuration})', ";

            // dimmed text - video title
            videoFilter += $"drawtext=textfile:'{videoTitle.Split(";")[0]}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={SMALL_FONT}:";
            videoFilter += $"{_upperLeft}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={DASHCAM_BORDER_WIDTH}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}@{DIM_BACKGROUND}:";
            videoFilter += $"enable='gt(t,0)'";
            // videoFilter += $"enable='gt(t,{randomDuration})'";

            return videoFilter;
        }

        private string GetTextColor(string videoTitle)
        {
            videoTitle = videoTitle.ToLower();
            if (videoTitle.Contains("night") || videoTitle.Contains("fireworks"))
            {
                return FfMpegColors.Orange;
            }

            return FfMpegColors.White;
        }

        private string GetDestinationFilter()
        {
            string destinationFile = Path.Combine(_workingDirectory, DESTINATION_FILE);

            if (File.Exists(destinationFile))
            {
                string fileContents = File.ReadAllText(destinationFile).Trim();
                return $", drawtext=textfile:{fileContents}:{_streetSignTextSubfilter}:enable='between(t,2,12)'";
            }

            return string.Empty;
        }

        private string GetMajorRoadsFilter()
        {
            string majorRoadsFile = Path.Combine(_workingDirectory, MAJOR_ROADS_FILE);
            if (File.Exists(majorRoadsFile))
            {
                string fileContents = File.ReadAllText(majorRoadsFile).Trim();
                return $", drawtext=textfile:'{fileContents}':{_streetSignTextSubfilter}:enable='between(t,12,22)'";
            }

            return string.Empty;
        }

        protected override async Task RenderVideoAsync(string videoTitle, string videoOutputPath, string videoFilter, CancellationToken cancellationToken)
        {
            if (_appSettings.DoRenderVideos == false)
            {
                return;
            }

            _logger.LogInformation($"Rendering video: {videoTitle}");
            await _statusService.UpsertAsync(StatusKeys.DashStatus, StatusValues.Rendering);
            await _statusService.SaveChangesAsync();

            await RunCommandAsync(
                ProgramPaths.FfmpegBinary,
                $"-y {LOG_ERRORS} -safe 0 -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i \"{FFMPEG_INPUT_FILE}\" -i \"{_musicService.GetRandomMixTrack()}\" -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi -shortest -bsf:a aac_adtstoasc -map 0:v:0 -map 1:a:0 \"{videoOutputPath}\"",
                _workingDirectory,
                cancellationToken,
                240);
        }

        protected override void CheckOrCreateFfmpegInputFile()
        {
            string ffmpegInputFile = Path.Combine(_workingDirectory, FFMPEG_INPUT_FILE);
            DeleteFile(ffmpegInputFile);

            using (StreamWriter writer = new StreamWriter(ffmpegInputFile))
            {
                _logger.LogInformation("Creating FFMPEG input file");
                var mp4Files = GetFilesInDirectory(_workingDirectory)
                  .Where(x => x.EndsWith(FileExtension.Mov))
                  .OrderBy(x => x);

                foreach (string file in mp4Files)
                {
                    writer.WriteLine($"file '{Path.GetFileName(file)}'");
                }
            }
        }

        protected override async Task ArchiveDirectoryContentsAsync(string directoryToArchive, string archiveName, string archiveDestination, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.DashStatus, StatusValues.Archiving);
            await _statusService.SaveChangesAsync();
            await base.ArchiveDirectoryContentsAsync(directoryToArchive, archiveName, archiveDestination, cancellationToken);
        }

        protected override async Task CleanUpBeforeArchivingAsync(string workingDirectory)
        {
            await _statusService.UpsertAsync(StatusKeys.DashStatus, StatusValues.Archiving);
            await _statusService.SaveChangesAsync();
            await base.CleanUpBeforeArchivingAsync(workingDirectory);
        }

        protected override async Task ExtractTarFileAsync(string tarFile, string workingDirectory, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.DashStatus, StatusValues.Extracting);
            await _statusService.UpsertAsync(StatusKeys.RhtFile, tarFile);
            await _statusService.SaveChangesAsync();
            await base.ExtractTarFileAsync(tarFile, workingDirectory, cancellationToken);
        }

        public override async Task WorkerIdleAsync(CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.DashStatus, StatusValues.Idle);
            await _statusService.UpsertAsync(StatusKeys.DashFile, string.Empty);
            await _statusService.SaveChangesAsync();
            await Task.Delay(TimeSpan.FromMinutes(_appSettings.WorkerIdleInterval), cancellationToken);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            CreateDirectory(_incomingDirectory);
            CreateDirectory(_archiveDirectory);
            CreateDirectory(_uploadDirectory);
            CreateDirectory(_workingDirectory);
            await Task.CompletedTask;
        }

        protected override async Task ConvertVideoFilesToCommonFormatAsync(string directory, CancellationToken cancellationToken)
        {
            var videoFiles = GetFilesInDirectory(directory)
                .Where(x => x.EndsWith(FileExtension.Mov))
                .OrderBy(x => x);

            foreach (var videoFileName in videoFiles)
            {
                _logger.LogInformation($"Converting video {videoFileName} to common format");

                var result = await RunCommandAsync(
                    ProgramPaths.FfprobeBinary,
                    $"-hide_banner \"{videoFileName}\"",
                    directory,
                    cancellationToken,
                    1
                );

                string xResolution = "1920";
                string yResolution = "1080";

                if (result.stdErr.Contains("1280x720"))
                {
                    xResolution = "1280";
                    yResolution = "720";
                }

                string frameRate = "30";

                if (result.stdErr.Contains("60 fps"))
                {
                    frameRate = "60";
                }

                string scaledFile = $"{Path.GetFileNameWithoutExtension(videoFileName)}.{xResolution}x{yResolution}{FileExtension.Mp4}";
                string videoFilters = $"scale_vaapi=w={xResolution}:h={yResolution}";

                if (DoesFileExist(Path.Combine(directory, Path.GetFileNameWithoutExtension(videoFileName) + ".vflip" + FileExtension.Txt)) ||
                    DoesFileExist(Path.Combine(directory, "vflip" + FileExtension.Txt)))
                {
                    videoFilters += ":vflip";
                }

                if (DoesFileExist(Path.Combine(directory, Path.GetFileNameWithoutExtension(videoFileName) + ".hflip" + FileExtension.Txt)) ||
                    DoesFileExist(Path.Combine(directory, "hflip" + FileExtension.Txt)))
                {
                    videoFilters += ":hflip";
                }

                await RunCommandAsync(
                    ProgramPaths.FfmpegBinary,
                    $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -r {frameRate} -vf \"{videoFilters}\" -vcodec h264_vaapi -an \"{scaledFile}\"",
                    directory,
                    cancellationToken,
                    10
                );

                DeleteFile(Path.Combine(directory, videoFileName));
                MoveFile(Path.Combine(directory, scaledFile), Path.Combine(directory, videoFileName.Replace(FileExtension.Mov, FileExtension.Mp4)));
            }
        }

    } // end class
}
