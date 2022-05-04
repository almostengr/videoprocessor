using System;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Workers
{
    public class VideoRenderWorker : BaseWorker
    {
        private readonly IVideoRenderService _videoRenderService;
        private readonly ILogger<VideoRenderWorker> _logger;

        public VideoRenderWorker(ILogger<VideoRenderWorker> logger, IServiceScopeFactory factory) : base(logger)
        {
            _videoRenderService = factory.CreateScope().ServiceProvider.GetRequiredService<IVideoRenderService>();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string[] videoArchives = _videoRenderService.GetVideosFromInputDirectory();

                foreach (var videoArchive in videoArchives)
                {
                    bool IsDiskSpaceAvailable = _videoRenderService.IsDiskSpaceAvailable();

                    _logger.LogInformation($"Processing {videoArchive}");

                    _videoRenderService.CleanWorkingDirectory();

                    // check if channel file (dash or services) exists
                    if (_videoRenderService.DoesChannelFileExist() == false)
                    {
                        _logger.LogError($"Channel file does not exist in archive. Skipping");
                        continue;
                    }

                    VideoPropertiesDto videoProperties = new(); // set the channel and video properties

                    await _videoRenderService.ExtractTarFileToWorkingDirectoryAsync(videoArchive);

                    await _videoRenderService.ConvertVideoFilesToMp4Async(); // convert non mp4 videos to mp4 format

                    // set the branding color based on video title
                    string brandingTextColor = 
                        _videoRenderService.GetBrandingTextColor(
                            videoProperties.ChannelProperties.Name,
                            videoProperties.VideoTitle);

                    // set the branding text

                    // set the channel branding text

                    // define the destination video filter

                    _videoRenderService.GetMajorRoadFilter();

                    _videoRenderService.GetSubtitlesFilter();

                    // check for input file for ffmpeg; if non exists, then create it

                    await _videoRenderService.RenderVideoToUploadDirectoryAsync();

                    // save meta data file to upload directory

                    // archive working directory contents

                    // move archive tar file to archive directory

                    // remove archive tar file from input directory

                    _videoRenderService.CleanWorkingDirectory();

                    _logger.LogInformation($"Finished processing {videoArchive}");
                }

                if (videoArchives.Length == 0)
                {
                    _logger.LogInformation("No videos to process");
                    await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
                }

            }
        }

    }
}
