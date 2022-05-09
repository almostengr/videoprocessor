using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Common;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services
{
    public class DashCamVideoRenderService : BaseVideoRenderService, IDashCamVideoRenderService
    {
        private readonly string _streetSignFilter;
        private readonly ILogger<DashCamVideoRenderService> _logger;
        private readonly AppSettings _appSettings;

        public DashCamVideoRenderService(ILogger<DashCamVideoRenderService> logger, AppSettings appSettings) : base(logger, appSettings)
        {
            _streetSignFilter = $"fontcolor=white:fontsize={FfMpegConstants.FontSize}:box=1:boxborderw={FfMpegConstants.DashCamBorderWidth}:boxcolor=green:{_lowerCenter}";
            _logger = logger;
            _appSettings = appSettings;
        }

        public override string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties)
        {
            Random random = new();
            int randomDuration = random.Next(5, 26);

            string textColor = FfMpegColors.White;
            if (videoProperties.VideoTitle.ToLower().Contains("night"))
            {
                textColor = FfMpegColors.Orange;
            }

            var positionOptions = new List<string> {
                _upperRight,
                _lowerRight
            };

            string channelBranding = "Kenny Ram Dash Cam";
            string textPosition = positionOptions[random.Next(0, positionOptions.Count)];

            // solid text - channel name
            string videoFilter = $"drawtext=textfile:'" + channelBranding + "':";
            videoFilter += $"fontcolor=" + textColor + ":";
            videoFilter += $"fontsize={FfMpegConstants.FontSize}:";
            videoFilter += $"{textPosition}:";
            videoFilter += $"box=1:boxborderw={FfMpegConstants.DashCamBorderWidth}:boxcolor={FfMpegColors.Black}";
            videoFilter += $":enable='between(t,0,{randomDuration})', ";

            // dimmed text - channel name
            videoFilter += $"drawtext=textfile:'{channelBranding}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSize}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:boxborderw={FfMpegConstants.DashCamBorderWidth}:boxcolor={FfMpegColors.Black}";
            videoFilter += $":@{FfMpegConstants.DimmedBackground}:";
            videoFilter += $"enable='gt(t,{randomDuration})', ";

            // solid text - video title
            videoFilter += $"drawtext=textfile:'{videoProperties.VideoTitle}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSize}:";
            videoFilter += $"{_upperLeft}:";
            videoFilter += $"box=1:boxborderw={FfMpegConstants.DashCamBorderWidth}:boxcolor={FfMpegColors.Black}";
            videoFilter += $":enable='between(t,0,{randomDuration})', ";

            // dimmed text - video title
            videoFilter += $"drawtext=textfile:'{videoProperties.VideoTitle}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSize}:";
            videoFilter += $"{_upperLeft}:";
            videoFilter += $"box=1:boxborderw={FfMpegConstants.DashCamBorderWidth}:boxcolor={FfMpegColors.Black}";
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
                return $", drawtext=textfile=majorroads.txt:${_streetSignFilter}:enable='between(t,12,19)'";
            }

            return string.Empty;
        }

        public override async Task RenderVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Rendering video: {videoProperties.SourceTarFilePath}");

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
                    "-i",
                    PickRandomMusicTrack(),
                    "-shortest",
                    "-a",
                    videoProperties.OutputVideoFile
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


    }
}
