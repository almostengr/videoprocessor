using System;
using System.Collections.Generic;
using System.IO;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services
{
    public class DashCamVideoRenderService : BaseVideoRenderService, IDashCamVideoRenderService
    {
        private readonly string _streetSignFilter;

        public DashCamVideoRenderService(ILogger<DashCamVideoRenderService> logger) : base(logger)
        {
            _streetSignFilter = $"fontcolor=white:fontsize={FfMpegConstants.FontSize}:box=1:boxborderw=7:boxcolor=green:{_lowerCenter}";
        }

        public override string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties)
        {
            Random random = new();
            string channelBranding = "Kenny Ram Dash Cam";
            int randomDuration = random.Next(5, 20);

            string textColor = FfMpegColors.White;
            if (videoProperties.VideoTitle.ToLower().Contains("night"))
            {
                textColor = FfMpegColors.Orange;
            }

            var positionOptions = new List<string> {
                _upperRight,
                _lowerRight
            };

            string textPosition = positionOptions[random.Next(0, positionOptions.Count - 1)];

            // solid text - channel name
            string videoFilter = "drawtext=textfile:'" + channelBranding + "':";
            videoFilter += "fontcolor=" + textColor + ":";
            videoFilter += "fontsize=" + FfMpegConstants.FontSize + ":";
            videoFilter += $"{textPosition}:";
            videoFilter += "box=1:boxborderw=10:boxcolor=" + FfMpegColors.Black;
            videoFilter += $":enable='between(t,0,{randomDuration})', ";

            // dimmed text - channel name
            videoFilter += "drawtext=textfile:'" + channelBranding + "':";
            videoFilter += "fontcolor=" + textColor + ":";
            videoFilter += "fontsize=" + FfMpegConstants.FontSize + ":";
            videoFilter += $"{_upperRight}:";
            videoFilter += "box=1:boxborderw=10:boxcolor=" + FfMpegColors.Black;
            videoFilter += $":@{FfMpegConstants.DimmedBackground}:";
            videoFilter += $"enable='gt(t,{randomDuration})', ";

            // solid text - video title
            videoFilter += "drawtext=textfile:'" + videoProperties.VideoTitle + "':";
            videoFilter += "fontcolor=" + textColor + ":";
            videoFilter += "fontsize=" + FfMpegConstants.FontSize + ":";
            videoFilter += $"{_upperLeft}:";
            videoFilter += "box=1:boxborderw=10:boxcolor=" + FfMpegColors.Black;
            videoFilter += $":enable='between(t,0,{randomDuration})', ";

            // dimmed text - video title
            videoFilter += "drawtext=textfile:'" + videoProperties.VideoTitle + "':";
            videoFilter += "fontcolor=" + textColor + ":";
            videoFilter += "fontsize=" + FfMpegConstants.FontSize + ":";
            videoFilter += $"{_upperLeft}:";
            videoFilter += "box=1:boxborderw=10:boxcolor=" + FfMpegColors.Black;
            videoFilter += $":@{FfMpegConstants.DimmedBackground}:";
            videoFilter += $"enable='gt(t,{randomDuration})', ";

            return videoFilter;
        }

        public string GetDestinationFilter(string destinationFile)
        {
            if (File.Exists(destinationFile))
            {
                return $", drawtext=textfile=destination.txt:${_streetSignFilter}:enable='between(t,5,12)'";
            }

            return string.Empty;
        }

        public string GetMajorRoadsFilter(string majorRoadsFile)
        {
            if (File.Exists(majorRoadsFile))
            {
                return $", drawtext=textfile=majorroads.txt:${_streetSignFilter}:enable='between(t,5,12)'";
            }

            return string.Empty;
        }

    }
}
