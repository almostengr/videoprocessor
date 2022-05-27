using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services.FileSystem;
using Almostengr.VideoProcessor.Api.Services.Video;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Workers
{
    public class DashCamVideoWorker : BackgroundService
    {
        private readonly IDashCamVideoService _VideoService;
        private readonly AppSettings _appSettings;
        private readonly IFileSystemService _fileSystemService;
        private readonly ILogger<DashCamVideoWorker> _logger;
        private readonly string _incomingDirectory;
        private readonly string _archiveDirectory;
        private readonly string _uploadDirectory;
        private readonly string _workingDirectory;
        private readonly string _ffmpegInputFilePath;

        public DashCamVideoWorker(ILogger<DashCamVideoWorker> logger, IServiceScopeFactory factory)
        {
            _VideoService = factory.CreateScope().ServiceProvider.GetRequiredService<IDashCamVideoService>();
            _appSettings = factory.CreateScope().ServiceProvider.GetRequiredService<AppSettings>();
            _fileSystemService = factory.CreateScope().ServiceProvider.GetRequiredService<IFileSystemService>();
            _logger = logger;
            _incomingDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "incoming");
            _archiveDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "archive");
            _uploadDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "upload");
            _workingDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "working");
            _ffmpegInputFilePath = Path.Combine(_workingDirectory, VideoTextFiles.InputFile);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Random random = new();
            while (!stoppingToken.IsCancellationRequested)
            {
                string videoArchive = _VideoService.GetVideoArchivesInDirectory(_incomingDirectory)
                    .Where(x => x.StartsWith(".") == false)
                    .OrderBy(x => random.Next()).Take(1).FirstOrDefault();
                bool isDiskSpaceAvailable = _fileSystemService.IsDiskSpaceAvailable(_incomingDirectory, _appSettings.DiskSpaceThreshold);

                if (string.IsNullOrEmpty(videoArchive) || isDiskSpaceAvailable == false)
                {
                    await _VideoService.WorkerIdleAsync(stoppingToken);
                    continue;
                }

                try
                {
                    _logger.LogInformation($"Processing {videoArchive}");

                    _fileSystemService.DeleteDirectory(_workingDirectory);
                    _fileSystemService.CreateDirectory(_workingDirectory);

                    await _fileSystemService.ConfirmFileTransferCompleteAsync(videoArchive);

                    VideoPropertiesDto videoProperties = new();
                    videoProperties.SourceTarFilePath = videoArchive;
                    videoProperties.FfmpegInputFilePath = _ffmpegInputFilePath;
                    videoProperties.WorkingDirectory = _workingDirectory;
                    videoProperties.UploadDirectory = _uploadDirectory;
                    videoProperties.ArchiveDirectory = _archiveDirectory;

                    await _VideoService.ExtractTarFileAsync(videoArchive, _workingDirectory, stoppingToken);

                    _VideoService.PrepareFileNamesInDirectory(_workingDirectory);

                    await _VideoService.ConvertVideoFilesToMp4Async(_workingDirectory, stoppingToken);

                    _VideoService.CheckOrCreateFfmpegInputFile(_workingDirectory);

                    videoProperties.VideoFilter = _VideoService.GetFfmpegVideoFilters(videoProperties);
                    videoProperties.VideoFilter += _VideoService.GetDestinationFilter(videoProperties.WorkingDirectory);
                    videoProperties.VideoFilter += _VideoService.GetMajorRoadsFilter(videoProperties.WorkingDirectory);

                    await _VideoService.RenderVideoAsync(videoProperties, stoppingToken);

                    await _VideoService.CreateThumbnailsFromFinalVideoAsync(videoProperties, stoppingToken);

                    await _VideoService.CleanUpBeforeArchivingAsync(_workingDirectory);

                    await _VideoService.ArchiveDirectoryContentsAsync(
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
