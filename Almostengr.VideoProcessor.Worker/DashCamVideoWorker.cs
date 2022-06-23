using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services.FileSystem;
using Almostengr.VideoProcessor.Api.Services.Video;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Worker
{
    public class DashCamVideoWorker : BackgroundService
    {
        private readonly IDashCamVideoService _videoService;
        private readonly AppSettings _appSettings;
        private readonly IFileSystemService _fileSystemService;
        private readonly ILogger<DashCamVideoWorker> _logger;
        private readonly string _incomingDirectory;
        private readonly string _archiveDirectory;
        private readonly string _uploadDirectory;
        private readonly string _workingDirectory;

        public DashCamVideoWorker(ILogger<DashCamVideoWorker> logger, IServiceScopeFactory factory)
        {
            _videoService = factory.CreateScope().ServiceProvider.GetRequiredService<IDashCamVideoService>();
            _appSettings = factory.CreateScope().ServiceProvider.GetRequiredService<AppSettings>();
            _fileSystemService = factory.CreateScope().ServiceProvider.GetRequiredService<IFileSystemService>();
            _logger = logger;
            _incomingDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "incoming");
            _archiveDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "archive");
            _uploadDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "upload");
            _workingDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "working");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Random random = new();
            while (!stoppingToken.IsCancellationRequested)
            {
                string videoArchive = _videoService.GetVideoArchivesInDirectory(_incomingDirectory)
                    .Where(x => x.StartsWith(".") == false)
                    .OrderBy(x => random.Next()).Take(1).FirstOrDefault();
                bool isDiskSpaceAvailable = _fileSystemService.IsDiskSpaceAvailable(_incomingDirectory, _appSettings.DiskSpaceThreshold);

                if (string.IsNullOrEmpty(videoArchive) || isDiskSpaceAvailable == false)
                {
                    await _videoService.WorkerIdleAsync(stoppingToken);
                    continue;
                }

                try
                {
                    _logger.LogInformation($"Processing {videoArchive}");

                    _fileSystemService.DeleteDirectory(_workingDirectory);
                    _fileSystemService.CreateDirectory(_workingDirectory);

                    await _fileSystemService.ConfirmFileTransferCompleteAsync(videoArchive);

                    VideoPropertiesDto videoProperties = new VideoPropertiesDto(
                        videoArchive, _workingDirectory, _uploadDirectory, _archiveDirectory);

                    await _videoService.ExtractTarFileAsync(videoArchive, _workingDirectory, stoppingToken);

                    _videoService.PrepareFileNamesInDirectory(_workingDirectory);

                    // await _VideoService.ConvertVideoFilesToMp4Async(_workingDirectory, stoppingToken);

                    _videoService.CheckOrCreateFfmpegInputFile(_workingDirectory);

                    videoProperties.VideoFilter = _videoService.GetFfmpegVideoFilters(videoProperties);
                    videoProperties.VideoFilter += _videoService.GetDestinationFilter(videoProperties.WorkingDirectory);
                    videoProperties.VideoFilter += _videoService.GetMajorRoadsFilter(videoProperties.WorkingDirectory);

                    await _videoService.RenderVideoAsync(videoProperties, stoppingToken);

                    // await _videoService.CreateThumbnailsFromFinalVideoAsync(videoProperties, stoppingToken);

                    await _videoService.CleanUpBeforeArchivingAsync(_workingDirectory);

                    await _videoService.ArchiveDirectoryContentsAsync(
                        _workingDirectory, videoProperties.ArchiveTarFile, _archiveDirectory, stoppingToken);

                    _fileSystemService.DeleteFile(videoProperties.SourceTarFilePath);

                    _fileSystemService.DeleteDirectory(_workingDirectory);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.InnerException, ex.Message);
                }

                _logger.LogInformation($"Finished processing {videoArchive}");
            }
        } // end of ExecuteAsync

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _fileSystemService.CreateDirectory(_incomingDirectory);
            _fileSystemService.CreateDirectory(_archiveDirectory);
            _fileSystemService.CreateDirectory(_uploadDirectory);
            _fileSystemService.CreateDirectory(_workingDirectory);

            return base.StartAsync(cancellationToken);
        }

    }
}
