using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services.MusicService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.VideoRender
{
    public class DashCamVideoRenderService : BaseVideoRenderService, IDashCamVideoRenderService
    {
        private readonly string _streetSignTextSubfilter;
        private readonly string _streetSignBoxFilter;
        private readonly ILogger<DashCamVideoRenderService> _logger;
        private readonly IMusicService _musicService;
        private readonly AppSettings _appSettings;
        private const string _channelBranding = "Kenny Ram Dash Cam";

        public DashCamVideoRenderService(ILogger<DashCamVideoRenderService> logger, AppSettings appSettings,
            IServiceScopeFactory factory) : base(logger, appSettings)
        {
            _streetSignBoxFilter = $", drawbox=x=0:y=in_h-200:w=in_w:h=200:color={FfMpegColors.Green}:t=fill";
            _streetSignTextSubfilter = $"fontcolor=white:fontsize={FfMpegConstants.FontSizeLarge}:{_lowerCenter}";
            _logger = logger;
            _musicService = factory.CreateScope().ServiceProvider.GetRequiredService<IMusicService>();
            _appSettings = appSettings;
        }

        public override string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties)
        {
            Random random = new();
            int randomDuration = random.Next(5, 16);

            string textColor = FfMpegColors.White;
            if (videoProperties.VideoTitle.ToLower().Contains("night"))
            {
                textColor = FfMpegColors.Orange;
            }

            // solid text - channel name
            string videoFilter = string.Empty;
            videoFilter += $"drawtext=textfile:'{_channelBranding}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSizeSmall}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={FfMpegConstants.DashCamBorderWidth}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}:";
            videoFilter += $"enable='between(t,0,{randomDuration})', ";

            // dimmed text - channel name
            videoFilter += $"drawtext=textfile:'{_channelBranding}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSizeSmall}:";
            videoFilter += $"{_upperRight}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={FfMpegConstants.DashCamBorderWidth}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}@{FfMpegConstants.DimmedBackground}:";
            videoFilter += $"enable='gt(t,{randomDuration})', ";

            // solid text - video title
            videoFilter += $"drawtext=textfile:'{videoProperties.VideoTitle.Split(";")[0]}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSizeSmall}:";
            videoFilter += $"{_upperLeft}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={FfMpegConstants.DashCamBorderWidth}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}:";
            videoFilter += $"enable='between(t,0,{randomDuration})', ";

            // dimmed text - video title
            videoFilter += $"drawtext=textfile:'{videoProperties.VideoTitle.Split(";")[0]}':";
            videoFilter += $"fontcolor={textColor}:";
            videoFilter += $"fontsize={FfMpegConstants.FontSizeSmall}:";
            videoFilter += $"{_upperLeft}:";
            videoFilter += $"box=1:";
            videoFilter += $"boxborderw={FfMpegConstants.DashCamBorderWidth}:";
            videoFilter += $"boxcolor={FfMpegColors.Black}@{FfMpegConstants.DimmedBackground}:";
            videoFilter += $"enable='gt(t,{randomDuration})'";

            return videoFilter;
        }

        public string GetDestinationFilter(string workingDirectory)
        {
            if (File.Exists(Path.Combine(workingDirectory, VideoRenderFiles.DestinationFile)))
            {
                return $"{_streetSignBoxFilter}, drawtext=textfile={VideoRenderFiles.DestinationFile}:${_streetSignTextSubfilter}:enable='between(t,2,12)'";
            }

            return string.Empty;
        }

        public string GetMajorRoadsFilter(string workingDirectory)
        {
            if (File.Exists(Path.Combine(workingDirectory, VideoRenderFiles.MajorRoadsFile)))
            {
                return $"{_streetSignBoxFilter}, drawtext=textfile={VideoRenderFiles.MajorRoadsFile}:${_streetSignTextSubfilter}:enable='between(t,12,22)'";
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
