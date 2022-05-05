using System;
using System.Collections.Generic;
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
        
        
        public override async Task RenderVideoToUploadDirectoryAsync(string workingDirectory, string uploadDirectory)
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
                    FfMpegLogLevel.ERROR,
                    "-y",
                    "-f",
                    "concat",
                    "-i",
                    Path.Combine(workingDirectory, VideoRenderFiles.INPUT_FILE),
                    "-vf",
                    videoProperties.VideoFilter,
                    "-c:a",
                    "copy",
                    videoProperties.OutputFilename
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync();
        }

        public override string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties)
        {
            Random random = new();

            var channelBrandingText = new List<string> {
                "Robinson Handy and Technology Services",
                "rhtservices.net",
                "instagram.com/rhtservicesllc",
                "facebook.com/rhtservicesllc",
                "Subscribe to our YouTube channel!",
            };

            string textColor = FfMpegColors.WHITE;

            string channelBranding = channelBrandingText[random.Next(0, channelBrandingText.Count - 1)];

            string videoFilter = "drawtext=textfile:'" + channelBranding + "':";
            videoFilter += "fontcolor:" + textColor + ":";
            videoFilter += "fontsize:" + FfMpegConstants.FONT_SIZE + ":";
            videoFilter += $"{_upperRight}:";
            videoFilter += "box=1:boxborderw=10:boxcolor:" + FfMpegColors.BLACK;
            videoFilter += ":enable='gt(t,0)'";

            return videoFilter;
        }
    }
}
