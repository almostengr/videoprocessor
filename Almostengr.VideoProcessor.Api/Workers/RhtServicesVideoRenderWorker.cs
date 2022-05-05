using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services;
using Almostengr.VideoProcessor.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Workers
{
    public class RhtServicesVideoRenderWorker : BackgroundService
    {
        private readonly IRhtServicesVideoRenderService _videoRenderService;
        private readonly ILogger<RhtServicesVideoRenderWorker> _logger;
        private readonly string _incomingDirectory;
        private readonly string _archiveDirectory;
        private readonly string _uploadDirectory;
        private readonly string _workingDirectory;
        private readonly string _ffmpegInputFile;
        private readonly string _subtitlesFile;
        private const string DEFAULT_VIDEO_DESCRIPTION = "Visit https://rhtservices.net for more information.";

        public RhtServicesVideoRenderWorker(ILogger<RhtServicesVideoRenderWorker> logger, IServiceScopeFactory factory)
        {
            _videoRenderService = factory.CreateScope().ServiceProvider.GetRequiredService<IRhtServicesVideoRenderService>();
            _logger = logger;
            _incomingDirectory = Path.Combine(Directories.BaseDirectory, "rhtvideos/incoming");
            _archiveDirectory = Path.Combine(Directories.BaseDirectory, "rhtvideos/archive");
            _uploadDirectory = Path.Combine(Directories.BaseDirectory, "rhtvideos/upload");
            _workingDirectory = Path.Combine(Directories.BaseDirectory, "rhtvideos/working");
            _ffmpegInputFile = Path.Combine(_workingDirectory, VideoRenderFiles.INPUT_FILE);
            _subtitlesFile = Path.Combine(_workingDirectory, VideoRenderFiles.SUBTITLES_FILE);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string[] videoArchives = _videoRenderService.GetVideoArchivesInDirectory(_incomingDirectory);

                foreach (var videoArchive in videoArchives)
                {
                    bool IsDiskSpaceAvailable = _videoRenderService.IsDiskSpaceAvailable(_incomingDirectory);

                    _logger.LogInformation($"Processing {videoArchive}");

                    _videoRenderService.CleanDirectory(_workingDirectory);

                    VideoPropertiesDto videoProperties = new();
                    videoProperties.ArchiveFileName = videoArchive;
                    videoProperties.VideoDescription = DEFAULT_VIDEO_DESCRIPTION;
                    videoProperties.IncomingArchiveFileName = Path.Combine(_incomingDirectory, videoArchive);
                    videoProperties.InputFile = _ffmpegInputFile;

                    await _videoRenderService.ExtractTarFileToWorkingDirectoryAsync(videoArchive, _workingDirectory);

                    await _videoRenderService.ConvertVideoFilesToMp4Async(_workingDirectory);

                    _videoRenderService.LowerCaseFileNamesInDirectory(_workingDirectory);

                    _videoRenderService.CheckOrCreateFfmpegInputFile(_workingDirectory);

                    string ffmpegVideoFilter = _videoRenderService.GetFfmpegVideoFilters(videoProperties);

                    await _videoRenderService.RenderVideoToUploadDirectoryAsync(_workingDirectory, _uploadDirectory);

                    _videoRenderService.SaveVideoMetaData(videoProperties);

                    // move finalized video to the upload directory

                    await _videoRenderService.ArchiveWorkingDirectoryContentsAsync(_workingDirectory, _archiveDirectory);

                    _videoRenderService.MoveProcessedVideoArchiveToArchive(_workingDirectory, _archiveDirectory);

                    _videoRenderService.RemoveFile(videoProperties.IncomingArchiveFileName);

                    _videoRenderService.CleanDirectory(_workingDirectory);

                    _logger.LogInformation($"Finished processing {videoArchive}");
                }

                if (videoArchives.Length == 0)
                {
                    _logger.LogInformation("No videos to process");
                    await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
                }
            }
        } // end of ExecuteAsync

    }
}
