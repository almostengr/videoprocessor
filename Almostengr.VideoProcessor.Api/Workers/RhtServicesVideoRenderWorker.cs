using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Common;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Workers
{
    public class RhtServicesVideoRenderWorker : BackgroundService
    {
        private readonly IRhtServicesVideoRenderService _videoRenderService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<RhtServicesVideoRenderWorker> _logger;
        private readonly string _incomingDirectory;
        private readonly string _archiveDirectory;
        private readonly string _uploadDirectory;
        private readonly string _workingDirectory;
        private readonly string _ffmpegInputFilePath;

        public RhtServicesVideoRenderWorker(ILogger<RhtServicesVideoRenderWorker> logger, IServiceScopeFactory factory)
        {
            _videoRenderService = factory.CreateScope().ServiceProvider.GetRequiredService<IRhtServicesVideoRenderService>();
            _appSettings = factory.CreateScope().ServiceProvider.GetRequiredService<AppSettings>();
            _logger = logger;
            _incomingDirectory = Path.Combine(_appSettings.Directories.BaseDirectory, "rhtvideos/incoming");
            _archiveDirectory = Path.Combine(_appSettings.Directories.BaseDirectory, "rhtvideos/archive");
            _uploadDirectory = Path.Combine(_appSettings.Directories.BaseDirectory, "rhtvideos/upload");
            _workingDirectory = Path.Combine(_appSettings.Directories.BaseDirectory, "rhtvideos/working");
            _ffmpegInputFilePath = Path.Combine(_workingDirectory, VideoRenderFiles.InputFile);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool isDiskSpaceAvailable = false;
            while (!stoppingToken.IsCancellationRequested)
            {
                string[] videoArchives = _videoRenderService.GetVideoArchivesInDirectory(_incomingDirectory);

                foreach (var videoArchive in videoArchives)
                {
                    isDiskSpaceAvailable = _videoRenderService.IsDiskSpaceAvailable(_incomingDirectory);
                    if (isDiskSpaceAvailable == false)
                    {
                        break;
                    }

                    _logger.LogInformation($"Processing {videoArchive}");

                    _videoRenderService.DeleteDirectory(_workingDirectory);
                    _videoRenderService.CreateDirectory(_workingDirectory);

                    VideoPropertiesDto videoProperties = new();
                    videoProperties.SourceTarFilePath = videoArchive;
                    videoProperties.FfmpegInputFilePath = _ffmpegInputFilePath;
                    videoProperties.WorkingDirectory = _workingDirectory;
                    videoProperties.UploadDirectory = _uploadDirectory;
                    videoProperties.ArchiveDirectory = _archiveDirectory;

                    await _videoRenderService.ExtractTarFileAsync(
                        videoArchive,
                        _workingDirectory,
                        stoppingToken);

                    _videoRenderService.PrepareFileNamesInDirectory(_workingDirectory);

                    await _videoRenderService.ConvertVideoFilesToMp4Async(_workingDirectory, stoppingToken);

                    _videoRenderService.CheckOrCreateFfmpegInputFile(_workingDirectory);

                    videoProperties.VideoFilter = _videoRenderService.GetFfmpegVideoFilters(videoProperties);

                    await _videoRenderService.RenderVideoAsync(videoProperties, stoppingToken); // TODO enable after testing

                    await _videoRenderService.CreateThumbnailsFromFinalVideoAsync(videoProperties, stoppingToken);

                    _videoRenderService.CleanUpBeforeArchiving(_workingDirectory);

                    await _videoRenderService.ArchiveDirectoryContentsAsync(
                        _workingDirectory,
                        videoProperties.ArchiveTarFile,
                        _archiveDirectory,
                        stoppingToken);

                    _videoRenderService.DeleteFile(videoProperties.SourceTarFilePath);

                    _videoRenderService.DeleteDirectory(_workingDirectory);

                    _logger.LogInformation($"Finished processing {videoArchive}");
                }

                if (videoArchives.Length == 0 || isDiskSpaceAvailable == false)
                {
                    await Task.Delay(_appSettings.WorkerServiceInterval, stoppingToken);
                }
            }
        } // end of ExecuteAsync

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _videoRenderService.CreateDirectory(_incomingDirectory);
            _videoRenderService.CreateDirectory(_archiveDirectory);
            _videoRenderService.CreateDirectory(_uploadDirectory);
            _videoRenderService.CreateDirectory(_workingDirectory);

            return base.StartAsync(cancellationToken);
        }

    }
}
