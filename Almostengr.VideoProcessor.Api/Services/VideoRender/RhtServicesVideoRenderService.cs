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