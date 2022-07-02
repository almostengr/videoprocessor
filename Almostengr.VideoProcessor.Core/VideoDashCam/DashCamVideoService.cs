using Almostengr.VideoProcessor.Core.Configuration;
using Almostengr.VideoProcessor.Core.Music;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Almostengr.VideoProcessor.Core.Status;
using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.VideoCommon;

namespace Almostengr.VideoProcessor.Core.VideoDashCam
{
    public sealed class DashCamVideoService : VideoService, IDashCamVideoService
    {
        private readonly string _streetSignTextSubfilter;
        private readonly string _streetSignBoxFilter;
        private readonly ILogger<DashCamVideoService> _logger;
        private readonly IMusicService _musicService;
        private readonly IStatusService _statusService;
        private readonly AppSettings _appSettings;
        private readonly IBaseService _BaseService;
        private readonly string _channelBranding;

        // essential files
        private const string DESTINATION_FILE = "destination.txt";
        private const string MAJOR_ROADS_FILE = "majorroads.txt";

        private readonly string _incomingDirectory;
        private readonly string _archiveDirectory;
        private readonly string _uploadDirectory;
        private readonly string _workingDirectory;

        public DashCamVideoService(ILogger<DashCamVideoService> logger, AppSettings appSettings,
           IServiceScopeFactory factory, IStatusService statusService, IMusicService musicService) :
          base(logger, appSettings)
        {
            _streetSignBoxFilter = $", drawbox=x=0:y=in_h-200:w=in_w:h=200:color={FfMpegColors.Green}:t=fill";
            _streetSignTextSubfilter = $"fontcolor=white:fontsize={LARGE_FONT}:{_lowerCenter}";

            _channelBranding = GetBrandingText();

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

        public override async Task ExecuteServiceAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string videoArchivePath = GetRandomVideoArchiveInDirectory(_incomingDirectory);
                bool isDiskSpaceAvailable = _BaseService.IsDiskSpaceAvailable(_incomingDirectory, _appSettings.DiskSpaceThreshold);

                if (string.IsNullOrEmpty(videoArchivePath) || isDiskSpaceAvailable == false)
                {
                    await WorkerIdleAsync(cancellationToken);
                    continue;
                }

                try
                {
                    _logger.LogInformation($"Processing {videoArchivePath}");

                    _BaseService.DeleteDirectory(_workingDirectory);
                    _BaseService.CreateDirectory(_workingDirectory);

                    await _BaseService.ConfirmFileTransferCompleteAsync(videoArchivePath);

                    await ExtractTarFileAsync(videoArchivePath, _workingDirectory, cancellationToken);

                    PrepareFileNamesInDirectory(_workingDirectory);

                    await ConvertVideoFilesToMp4Async(_workingDirectory, cancellationToken);

                    CheckOrCreateFfmpegInputFile(_workingDirectory);

                    string videoTitle = GetVideoTitleFromArchiveName(videoArchivePath);

                    string videoFilter = GetFfmpegVideoFilters(videoTitle);
                    videoFilter += GetDestinationFilter();
                    videoFilter += GetMajorRoadsFilter();

                    string videoOutputPath = GetVideoOutputPath(_uploadDirectory, videoTitle);
                    await RenderVideoAsync(videoTitle, videoOutputPath, videoFilter, cancellationToken);

                    await CreateThumbnailsFromFinalVideoAsync(
                        videoOutputPath, _uploadDirectory, videoTitle, cancellationToken);

                    await CleanUpBeforeArchivingAsync(_workingDirectory);

                    string archiveTarFile = GetArchiveTarFileName(videoTitle);

                    await ArchiveDirectoryContentsAsync(
                      _workingDirectory, archiveTarFile, _archiveDirectory, cancellationToken);

                    _BaseService.DeleteFile(videoArchivePath);

                    _BaseService.DeleteDirectory(_workingDirectory);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.InnerException, ex.Message);
                }

                _logger.LogInformation($"Finished processing {videoArchivePath}");
            }
        }

        protected override string GetFfmpegVideoFilters(string videoTitle)
        {
            int randomDuration = _random.Next(5, 16);
            string textColor = GetTextColor(videoTitle);

            // solid text - channel name
            string videoFilter = string.Empty;
            videoFilter += $"drawtext=textfile:'{_channelBranding}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={SMALL_FONT}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={DASHCAM_BORDER_WIDTH}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}:";
            videoFilter += $"enable='between(t,0,{randomDuration})', ";

            // dimmed text - channel name
            videoFilter += $"drawtext=textfile:'{_channelBranding}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={SMALL_FONT}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={DASHCAM_BORDER_WIDTH}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}@{DIM_BACKGROUND}:";
            videoFilter += $"enable='gt(t,{randomDuration})', ";

            // solid text - video title
            videoFilter += $"drawtext=textfile:'{videoTitle.Split(";")[0]}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={SMALL_FONT}:";
            videoFilter += $"{_upperLeft}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={DASHCAM_BORDER_WIDTH}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}:";
            videoFilter += $"enable='between(t,0,{randomDuration})', ";

            // dimmed text - video title
            videoFilter += $"drawtext=textfile:'{videoTitle.Split(";")[0]}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={SMALL_FONT}:";
            videoFilter += $"{_upperLeft}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={DASHCAM_BORDER_WIDTH}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}@{DIM_BACKGROUND}:";
            videoFilter += $"enable='gt(t,{randomDuration})'";

            return videoFilter;
        }

        private string GetTextColor(string videoTitle)
        {
            if (videoTitle.ToLower().Contains("night"))
            {
                return FfMpegColors.Orange;
            }

            return FfMpegColors.White;
        }

        private string GetDestinationFilter()
        {
            if (File.Exists(Path.Combine(_workingDirectory, DESTINATION_FILE)))
            {
                return $"{_streetSignBoxFilter}, drawtext=textfile={DESTINATION_FILE}:${_streetSignTextSubfilter}:enable='between(t,2,12)'";
            }

            return string.Empty;
        }

        private string GetMajorRoadsFilter()
        {
            if (File.Exists(Path.Combine(_workingDirectory, MAJOR_ROADS_FILE)))
            {
                return $"{_streetSignBoxFilter}, drawtext=textfile={MAJOR_ROADS_FILE}:${_streetSignTextSubfilter}:enable='between(t,12,22)'";
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
              $"-hide_banner -y -safe 0 {LOG_ERRORS} -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i \"{FFMPEG_INPUT_FILE}\" -i \"{_musicService.GetRandomMixTrack()}\" -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{videoOutputPath}\"",
              _workingDirectory, // videoProperties.WorkingDirectory,
              cancellationToken,
              240);
        }

        protected override void CheckOrCreateFfmpegInputFile()
        {
            string ffmpegInputFile = Path.Combine(_workingDirectory, FFMPEG_INPUT_FILE);
            _BaseService.DeleteFile(ffmpegInputFile);

            using (StreamWriter writer = new StreamWriter(ffmpegInputFile))
            {
                _logger.LogInformation("Creating FFMPEG input file");
                string[] mp4Files = _BaseService.GetFilesInDirectory(_workingDirectory)
                  .Where(x => x.EndsWith(FileExtension.Mov))
                  .OrderBy(x => x)
                  .ToArray();

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

        protected override async Task ConvertVideoFilesToMp4Async(string directory, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.DashStatus, StatusValues.ConvertingToMp4);
            await _statusService.SaveChangesAsync();
            await base.ConvertVideoFilesToMp4Async(directory, cancellationToken);
        }

        protected override async Task ExtractTarFileAsync(string tarFile, string workingDirectory, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.DashStatus, StatusValues.Extracting);
            await _statusService.UpsertAsync(StatusKeys.RhtFile, tarFile);
            await _statusService.SaveChangesAsync();
            await base.ExtractTarFileAsync(tarFile, workingDirectory, cancellationToken);
        }

        protected override async Task WorkerIdleAsync(CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.DashStatus, StatusValues.Idle);
            await _statusService.UpsertAsync(StatusKeys.DashFile, string.Empty);
            await _statusService.SaveChangesAsync();
            await Task.Delay(TimeSpan.FromMinutes(_appSettings.WorkerIdleInterval), cancellationToken);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _BaseService.CreateDirectory(_incomingDirectory);
            _BaseService.CreateDirectory(_archiveDirectory);
            _BaseService.CreateDirectory(_uploadDirectory);
            _BaseService.CreateDirectory(_workingDirectory);
            await Task.CompletedTask;
        }

        protected override Task ConvertVideoFilesToCommonFormatAsync(string directory, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
