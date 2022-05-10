using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Common;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services
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
                "rhtservices.net",
                "IG @rhtservicesllc",
                "instagram.com/rhtservicesllc",
                "facebook.com/rhtservicesllc",
            };

            Random random = new();
            string socialText = socialMediaOptions[random.Next(0, socialMediaOptions.Count)];
            
            string channelText = "Robinson Handy and Technology Services";
            string textColor = FfMpegColors.White;

            string videoFilter = $"drawtext=textfile:'{channelText}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSize}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:boxborderw={FfMpegConstants.RhtBorderWidth.ToString()}:boxcolor={FfMpegColors.Black}";
            videoFilter += $"@{FfMpegConstants.DimmedBackground}";
            videoFilter += $":enable='gt(t,0)'";

            videoFilter += $", drawtext=textfile:'{socialText}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSize}:";
            videoFilter += $"{_lowerRight}:";
            videoFilter += $"box=1:boxborderw={FfMpegConstants.RhtBorderWidth.ToString()}:boxcolor={FfMpegColors.Black}";
            videoFilter += $"@{FfMpegConstants.DimmedBackground}";
            videoFilter += $":enable='gt(t,0)'";

            return videoFilter;
        }
    }
}
