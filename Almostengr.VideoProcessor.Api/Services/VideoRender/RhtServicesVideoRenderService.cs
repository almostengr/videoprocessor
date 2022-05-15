using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services.ExternalProcess;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.VideoRender
{
    public class RhtServicesVideoRenderService : BaseVideoRenderService, IRhtServicesVideoRenderService
    {
        private readonly ILogger<RhtServicesVideoRenderService> _logger;
        private readonly AppSettings _appSettings;
        private readonly IExternalProcessService _externalProcess;

        public RhtServicesVideoRenderService(ILogger<RhtServicesVideoRenderService> logger, AppSettings appSettings, 
            IExternalProcessService externalProcess) : 
            base(logger, appSettings, externalProcess)
        {
            _logger = logger;
            _appSettings = appSettings;
            _externalProcess = externalProcess;
        }

        public override async Task RenderVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken)
        {
            if (_appSettings.DoRenderVideos == false)
            {
                return;
            }

            _logger.LogInformation($"Rendering {videoProperties.SourceTarFilePath}");

            await _externalProcess.RunProcessAsync(
                ProgramPaths.FfmpegBinary,
                $"-hide_banner -y -safe 0 -loglevel {FfMpegLogLevel.Error} -f concat -i {videoProperties.FfmpegInputFilePath} -vf {videoProperties.VideoFilter} -c:a copy {videoProperties.OutputVideoFilePath}",
                videoProperties.WorkingDirectory,
                cancellationToken,
                240);

            _logger.LogInformation($"Done rendering {videoProperties.SourceTarFilePath}");
        }

        public override string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties)
        {
            List<string> socialMediaOptions = new List<string> {
                "Robinson Handy and Technology Services",
                "rhtservices.net",
                "IG @rhtservicesllc",
                "instagram.com/rhtservicesllc",
                "facebook.com/rhtservicesllc",
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
    }
}
