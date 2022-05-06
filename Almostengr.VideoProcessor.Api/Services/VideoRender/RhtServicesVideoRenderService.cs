using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services
{
    public class RhtServicesVideoRenderService : BaseVideoRenderService, IRhtServicesVideoRenderService
    {
        public RhtServicesVideoRenderService(ILogger<RhtServicesVideoRenderService> logger) : base(logger)
        {
        }

        public override Process RenderVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken)
        {
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
                    videoProperties.InputFile,
                    "-vf",
                    videoProperties.VideoFilter,
                    "-c:a",
                    "copy",
                    videoProperties.OutputFile
                },
                WorkingDirectory = videoProperties.WorkingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
        }

        public override string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties)
        {
            Random random = new();

            string channelText = "Robinson Handy and Technology Services";

            var socialMediaOptions = new List<string> {
                "rhtservices.net",
                "IG: @rhtservicesllc",
                "instagram.com/rhtservicesllc",
                "facebook.com/rhtservicesllc",
                "Subscribe to our YouTube channel!",
            };

            string socialText = socialMediaOptions[random.Next(0, socialMediaOptions.Count - 1)];
            
            string textColor = FfMpegColors.White;

            string videoFilter = $"drawtext=textfile:'{channelText}':";
            videoFilter += $"fontcolor:{textColor}:";
            videoFilter += $"fontsize:{FfMpegConstants.FontSize}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:boxborderw=10:boxcolor:{FfMpegColors.Black}";
            videoFilter += $"@{FfMpegConstants.DimmedBackground}";
            videoFilter += $":enable='gt(t,0)'";

            videoFilter += $", drawtext=textfile:'{socialText}':";
            videoFilter += $"fontcolor:{textColor}:";
            videoFilter += $"fontsize:{FfMpegConstants.FontSize}:";
            videoFilter += $"{_lowerRight}:";
            videoFilter += $"box=1:boxborderw=10:boxcolor:{FfMpegColors.Black}";
            videoFilter += $"@{FfMpegConstants.DimmedBackground}";
            videoFilter += $":enable='gt(t,0)'";

            return videoFilter;
        }
    }
}
