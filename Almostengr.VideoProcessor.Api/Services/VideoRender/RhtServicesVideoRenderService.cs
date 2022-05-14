using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.VideoRender
{
    public class RhtServicesVideoRenderService : BaseVideoRenderService, IRhtServicesVideoRenderService
    {
        private readonly ILogger<RhtServicesVideoRenderService> _logger;
        private readonly AppSettings _appSettings;

        public RhtServicesVideoRenderService(ILogger<RhtServicesVideoRenderService> logger, AppSettings appSettings) : base(logger, appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;
        }

        public override async Task RenderVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken)
        {
            if (_appSettings.DoRenderVideos == false)
            {
                return;
            }

            _logger.LogInformation($"Rendering {videoProperties.SourceTarFilePath}");
            
            Process process = new();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = ProgramPaths.FfmpegBinary,
                ArgumentList = {
                    "-hide_banner",
                    "-safe",
                    "0",
                    "-loglevel",
                    FfMpegLogLevel.Error,
                    "-y",
                    "-f",
                    "concat",
                    "-i",
                    videoProperties.FfmpegInputFilePath,
                    "-vf",
                    videoProperties.VideoFilter,
                    "-c:a",
                    "copy",
                    videoProperties.OutputVideoFilePath
                },
                WorkingDirectory = videoProperties.WorkingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
            _logger.LogInformation(process.StandardOutput.ReadToEnd());
            _logger.LogError(process.StandardError.ReadToEnd());
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
