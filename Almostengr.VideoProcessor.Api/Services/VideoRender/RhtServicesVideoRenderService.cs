using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Enums;
using Almostengr.VideoProcessor.Api.Services.Data;
using Almostengr.VideoProcessor.Api.Services.ExternalProcess;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.VideoRender
{
    public class RhtServicesVideoRenderService : VideoRenderService, IRhtServicesVideoRenderService
    {
        private readonly ILogger<RhtServicesVideoRenderService> _logger;
        private readonly AppSettings _appSettings;
        private readonly IExternalProcessService _externalProcess;
        private readonly IStatusService _statusService;

        public RhtServicesVideoRenderService(ILogger<RhtServicesVideoRenderService> logger, AppSettings appSettings,
            IExternalProcessService externalProcess, IStatusService statusService) :
            base(logger, appSettings, externalProcess)
        {
            _logger = logger;
            _appSettings = appSettings;
            _externalProcess = externalProcess;
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

            await _externalProcess.RunProcessAsync(
                ProgramPaths.FfmpegBinary,
                $"-hide_banner -y -safe 0 -loglevel {FfMpegLogLevel.Error} -hwaccel vaapi -hwaccel_output_format vaapi -f concat -i {videoProperties.FfmpegInputFilePath} -vf {videoProperties.VideoFilter} -vcodec h264_vaapi -b:v 5M -c:a copy {videoProperties.OutputVideoFilePath}",
                videoProperties.WorkingDirectory,
                cancellationToken,
                240);
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
            videoFilter += $"fontcolor={FfMpegColors.White}@{FfMpegConstants.DimmedBackground}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSizeSmall}:";
            videoFilter += $"{positionOptions[random.Next(0, positionOptions.Count)]}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={FfMpegConstants.RhtBorderWidth.ToString()}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}@{FfMpegConstants.DimmedBackground}";

            return videoFilter;
        }

        public override async Task ArchiveDirectoryContentsAsync(string directoryToArchive, string archiveName, string archiveDestination, CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.Archiving);
            await base.ArchiveDirectoryContentsAsync(directoryToArchive, archiveName, archiveDestination, cancellationToken);
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

        public override async Task StandByModeAsync(CancellationToken cancellationToken)
        {
            await _statusService.UpsertAsync(StatusKeys.RhtStatus, StatusValues.Idle);
            await _statusService.UpsertAsync(StatusKeys.RhtFile, string.Empty);
            await Task.Delay(TimeSpan.FromMinutes(_appSettings.WorkerServiceInterval), cancellationToken);
        }
    }
}
