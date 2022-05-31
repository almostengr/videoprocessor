using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Enums;
using Almostengr.VideoProcessor.Api.Services.Data;
using Almostengr.VideoProcessor.Api.Services.ExternalProcess;
using Almostengr.VideoProcessor.Api.Services.FileSystem;
using Almostengr.VideoProcessor.Api.Services.MusicService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.Video
{
    public class DashCamVideoService : VideoService, IDashCamVideoService
    {
        private readonly string _streetSignTextSubfilter;
        private readonly string _streetSignBoxFilter;
        private readonly ILogger<DashCamVideoService> _logger;
        private readonly IMusicService _musicService;
        private readonly IStatusService _statusService;
        private readonly AppSettings _appSettings;
        private readonly IExternalProcessService _externalProcess;
        private const string _channelBranding = "Kenny Ram Dash Cam";

        // essential files
        internal const string DESTINATION_FILE = "destination.txt";
        internal const string MAJOR_ROADS_FILE = "majorroads.txt";

        public DashCamVideoService(ILogger<DashCamVideoService> logger, AppSettings appSettings,
            IExternalProcessService externalProcess, IFileSystemService fileSystem, IServiceScopeFactory factory) :
            base(logger, appSettings, externalProcess, fileSystem)
        {
            _streetSignBoxFilter = $", drawbox=x=0:y=in_h-200:w=in_w:h=200:color={FfMpegColors.Green}:t=fill";
            _streetSignTextSubfilter = $"fontcolor=white:fontsize={LARGE_FONT}:{_lowerCenter}";
            _logger = logger;
            _musicService = factory.CreateScope().ServiceProvider.GetRequiredService<IMusicService>();
            _statusService = factory.CreateScope().ServiceProvider.GetRequiredService<IStatusService>();
            _externalProcess = externalProcess;
            _appSettings = appSettings;
        }

        public override string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties)
        {
            Random random = new();
            int randomDuration = random.Next(5, 16);
            string textColor = GetTextColor(videoProperties.VideoTitle);

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
            videoFilter += $"drawtext=textfile:'{videoProperties.VideoTitle.Split(";")[0]}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={SMALL_FONT}:";
            videoFilter += $"{_upperLeft}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={DASHCAM_BORDER_WIDTH}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}:";
            videoFilter += $"enable='between(t,0,{randomDuration})', ";

            // dimmed text - video title
            videoFilter += $"drawtext=textfile:'{videoProperties.VideoTitle.Split(";")[0]}':";
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
            videoTitle = videoTitle.ToLower();

            if (videoTitle.Contains("night"))
            {
                return FfMpegColors.Orange;
            }

            return FfMpegColors.White;
        }

        public string GetDestinationFilter(string workingDirectory)
        {
            if (File.Exists(Path.Combine(workingDirectory, DESTINATION_FILE)))
            {
                return $"{_streetSignBoxFilter}, drawtext=textfile={DESTINATION_FILE}:${_streetSignTextSubfilter}:enable='between(t,2,12)'";
            }

            return string.Empty;
        }

        public string GetMajorRoadsFilter(string workingDirectory)
        {
            if (File.Exists(Path.Combine(workingDirectory, MAJOR_ROADS_FILE)))
            {
                return $"{_streetSignBoxFilter}, drawtext=textfile={MAJOR_ROADS_FILE}:${_streetSignTextSubfilter}:enable='between(t,12,22)'";
            }

            return string.Empty;
        }

        public override async Task RenderVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken)
        {
            if (_appSettings.DoRenderVideos == false)
            {
                return;
            }

            _logger.LogInformation($"Rendering video: {videoProperties.SourceTarFilePath}");
            await _statusService.UpsertAsync(StatusKeys.DashStatus, StatusValues.Rendering);
            await _statusService.SaveChangesAsync();

            await _externalProcess.RunCommandAsync(
                ProgramPaths.FfmpegBinary,
                $"-hide_banner -y -safe 0 -loglevel {LOG_ERRORS} -hwaccel vaapi -hwaccel_output_format vaapi -f concat -i {FFMPEG_INPUT_FILE} -i {_musicService.GetRandomMixTrack()} -vf {videoProperties.VideoFilter} -vcodec h264_vaapi -b:v 5M -shortest -map 0:v:0 -map 1:a:0 {videoProperties.OutputVideoFilePath}",
                videoProperties.WorkingDirectory,
                cancellationToken,
                240);
        }

        public override async Task ArchiveDirectoryContentsAsync(string directoryToArchive, string archiveName, string archiveDestination, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.DashStatus, StatusValues.Archiving);
            await _statusService.SaveChangesAsync();
            await base.ArchiveDirectoryContentsAsync(directoryToArchive, archiveName, archiveDestination, cancellationToken);
        }

        public override async Task CleanUpBeforeArchivingAsync(string workingDirectory)
        {
            await _statusService.UpsertAsync(StatusKeys.DashStatus, StatusValues.Archiving);
            await _statusService.SaveChangesAsync();
            await base.CleanUpBeforeArchivingAsync(workingDirectory);
        }

        public override async Task ConvertVideoFilesToMp4Async(string directory, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.DashStatus, StatusValues.ConvertingToMp4);
            await _statusService.SaveChangesAsync();
            await base.ConvertVideoFilesToMp4Async(directory, cancellationToken);
        }

        public override async Task ExtractTarFileAsync(string tarFile, string workingDirectory, CancellationToken cancellationToken)
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
    }
}
