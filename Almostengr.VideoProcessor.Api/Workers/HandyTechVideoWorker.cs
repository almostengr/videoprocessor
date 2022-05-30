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

namespace Almostengr.VideoProcessor.Workers
{
    public class HandyTechVideoWorker : BackgroundService
    {
        private readonly IHandyTechVideoService _videoService;
        private readonly AppSettings _appSettings;
        private readonly IFileSystemService _fileSystemService;
        private readonly ILogger<HandyTechVideoWorker> _logger;
        private readonly string _incomingDirectory;
        private readonly string _archiveDirectory;
        private readonly string _uploadDirectory;
        private readonly string _workingDirectory;

        public HandyTechVideoWorker(ILogger<HandyTechVideoWorker> logger, IServiceScopeFactory factory)
        {
            _videoService = factory.CreateScope().ServiceProvider.GetRequiredService<IHandyTechVideoService>();
            _appSettings = factory.CreateScope().ServiceProvider.GetRequiredService<AppSettings>();
            _fileSystemService = factory.CreateScope().ServiceProvider.GetRequiredService<IFileSystemService>();
            _logger = logger;
            _incomingDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "incoming");
            _archiveDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "archive");
            _uploadDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "upload");
            _workingDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "working");
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

                    await _videoService.AddAudioToTimelapseAsync(_workingDirectory, stoppingToken); // add audio to gopro timelapse files

                    await _videoService.ConvertVideoFilesToTsAsync(_workingDirectory, stoppingToken); // convert video files to TS format

                    _videoService.CheckOrCreateFfmpegInputFile(_workingDirectory);

                    videoProperties.VideoFilter = _videoService.GetFfmpegVideoFilters(videoProperties);
                    
                    await _videoService.RenderVideoAsync(videoProperties, stoppingToken);

                    await _videoService.CreateThumbnailsFromFinalVideoAsync(videoProperties, stoppingToken);

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
        }

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