using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Common;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services.MusicService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.VideoRender
{
    public class DashCamVideoRenderService : BaseVideoRenderService, IDashCamVideoRenderService
    {
        private readonly string _streetSignFilter;
        private readonly ILogger<DashCamVideoRenderService> _logger;
        private readonly IMusicService _musicService;
        private readonly AppSettings _appSettings;

        public DashCamVideoRenderService(ILogger<DashCamVideoRenderService> logger, AppSettings appSettings,
            IServiceScopeFactory factory) : base(logger, appSettings)
        {
            _streetSignFilter = $"fontcolor=white:fontsize={FfMpegConstants.FontSize}:box=1:boxborderw={FfMpegConstants.DashCamBorderWidth}:boxcolor=green:{_lowerCenter}";
            _logger = logger;
            _musicService = factory.CreateScope().ServiceProvider.GetRequiredService<IMusicService>();
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

            // solid text - channel name
            string videoFilter = $"drawtext=textfile:'{channelBranding}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSize}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={FfMpegConstants.DashCamBorderWidth}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}:";
            videoFilter += $"enable='between(t,0,{randomDuration})', ";

            // dimmed text - channel name
            videoFilter += $"drawtext=textfile:'{channelBranding}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSize}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={FfMpegConstants.DashCamBorderWidth}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}@{FfMpegConstants.DimmedBackground}:";
            videoFilter += $"enable='gt(t,{randomDuration})', ";

            // solid text - video title
            videoFilter += $"drawtext=textfile:'{videoProperties.VideoTitle.Split(";")[0]}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSize}:";
            videoFilter += $"{_upperLeft}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={FfMpegConstants.DashCamBorderWidth}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}:";
            videoFilter += $"enable='between(t,0,{randomDuration})', ";

            // dimmed text - video title
            videoFilter += $"drawtext=textfile:'{videoProperties.VideoTitle.Split(";")[0]}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSize}:";
            videoFilter += $"{_upperLeft}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={FfMpegConstants.DashCamBorderWidth}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}@{FfMpegConstants.DimmedBackground}:";
            videoFilter += $"enable='gt(t,{randomDuration})'";

            return videoFilter;
        }

        public string GetDestinationFilter(string destinationFile)
        {
            if (File.Exists(destinationFile))
            {
                return $", drawtext=textfile={VideoRenderFiles.DestinationFile}:${_streetSignFilter}:enable='between(t,5,12)'";
            }

            return string.Empty;
        }

        public string GetMajorRoadsFilter(string majorRoadsFile)
        {
            if (File.Exists(majorRoadsFile))
            {
                return $", drawtext=textfile={VideoRenderFiles.MajorRoadsFile}:${_streetSignFilter}:enable='between(t,12,19)'";
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
                    "-i",
                    _musicService.PickRandomMusicTrack(),
                    "-vf",
                    videoProperties.VideoFilter,
                    "-shortest",
                    "-map",
                    "0:v:0",
                    "-map",
                    "1:a:0",
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

    }
}
